using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Xml.Linq;
using Microsoft.CSharp.RuntimeBinder;
using NUnit.Framework;
using StaadPro.Interop.Adapters.Interfaces;
using StaadPro.Interop.Adapters.Models;
using StaadPro.Interop.Enums.View;
using StaadPro.Interop.Models;
using StaadPro.Interop.Models.View;
using Native = PackageVerification.ViewFoundationFixture.Native;
using Root = PackageVerification.ViewFoundationFixture.Root;

namespace StaadPro.Interop.Tests.Adapter
{
    [TestFixture]
    public class OSViewFoundationTests
    {
        private static readonly string[] Names =
        {
            "ShowFront", "ShowBack", "ShowLeft", "ShowRight", "ShowPlan", "ShowBottom", "ShowIsometric", "RotateUp", "RotateDown", "RotateLeft", "RotateRight", "SpinLeft", "SpinRight"
        };

        private static IEnumerable<object[]> OrientationVersions
        {
            get
            {
                foreach (string name in Names)
                    foreach (int version in new[] { 24, 25, 26 })
                        yield return new object[] { name, version };
            }
        }

        private static bool IsAngular(string name) => name.StartsWith("Rotate") || name.StartsWith("Spin");

        private static ViewOperationResult Request(IOSView view, string name)
        {
            switch (name)
            {
                case "ShowFront": return view.ShowFront();
                case "ShowBack": return view.ShowBack();
                case "ShowLeft": return view.ShowLeft();
                case "ShowRight": return view.ShowRight();
                case "ShowPlan": return view.ShowPlan();
                case "ShowBottom": return view.ShowBottom();
                case "ShowIsometric": return view.ShowIsometric();
                case "RotateUp": return view.RotateUp(15.0);
                case "RotateDown": return view.RotateDown(15.0);
                case "RotateLeft": return view.RotateLeft(15.0);
                case "RotateRight": return view.RotateRight(15.0);
                case "SpinLeft": return view.SpinLeft(15.0);
                case "SpinRight": return view.SpinRight(15.0);
                default: throw new ArgumentOutOfRangeException(nameof(name));
            }
        }

        [TestCaseSource(nameof(OrientationVersions))]
        public void OrientationUsesCapturedViewAndCallerThreadWithoutInventedAcknowledgement(string name, int version)
        {
            var native = new Native();
            using (var wrapper = new OpenStaadWrapper(new Root { View = native, Major = version }))
            {
                Assert.That(native.Calls, Is.Empty, "Acquiring the wrapper must not change orientation.");
                var result = Request(wrapper.View, name);
                Assert.That(native.Calls, Is.EqualTo(new[] { name }));
                Assert.That(native.Threads.Single(), Is.EqualTo(Thread.CurrentThread.ManagedThreadId));
                Assert.That(result.NativeFunction, Is.EqualTo(name));
                Assert.That(result.Status, Is.EqualTo(ViewOperationStatus.InvokedWithoutAcknowledgement));
                Assert.That(result.IsSuccessful, Is.False);
                Assert.That(result.NativeReturnCode, Is.Null);
                Assert.That(native.RotationAngles, Is.EqualTo(IsAngular(name) ? new[] { 15.0 } : new double[0]));
            }
        }

        [TestCaseSource(nameof(Names))]
        public void OldViewBindingsRejectReplacementAndDisposalBeforeCallingNative(string name)
        {
            var native = new Native();
            var root = new Root { View = native };
            using (var wrapper = new OpenStaadWrapper(root))
            {
                var old = wrapper.View;
                var replacement = new Native();
                root.View = replacement;
                wrapper.RefreshComObjects();
                Assert.Throws<InvalidOperationException>(() => Request(old, name));
                Assert.That(native.Calls, Is.Empty);
                Assert.That(replacement.Calls, Is.Empty);
                var current = wrapper.View;
                wrapper.Dispose();
                Assert.Throws<InvalidOperationException>(() => Request(current, name));
                Assert.That(replacement.Calls, Is.Empty);
            }
        }

        [TestCaseSource(nameof(Names))]
        public void ReplacementDuringNativeCallThrowsWithoutFallbackOrRetry(string name)
        {
            var native = new Native();
            var replacement = new Native();
            var root = new Root { View = native };
            using (var wrapper = new OpenStaadWrapper(root))
            {
                var api = wrapper.View;
                native.OnCall = () => { root.View = replacement; wrapper.RefreshComObjects(); };
                Assert.Throws<InvalidOperationException>(() => Request(api, name));
                Assert.That(native.Calls, Is.EqualTo(new[] { name }));
                Assert.That(replacement.Calls, Is.Empty);
            }
        }

        [TestCaseSource(nameof(Names))]
        public void DisposalDuringNativeCallThrowsWithoutReturningAReceiptOrRetrying(string name)
        {
            var native = new Native();
            using (var wrapper = new OpenStaadWrapper(new Root { View = native }))
            {
                native.OnCall = wrapper.Dispose;
                Assert.Throws<InvalidOperationException>(() => Request(wrapper.View, name));
                Assert.That(native.Calls, Is.EqualTo(new[] { name }));
            }
        }

        [TestCaseSource(nameof(Names))]
        public void OriginalNativeErrorPropagatesWithoutAnotherInstanceOrRetry(string name)
        {
            var error = new COMException("native UI error", unchecked((int)0x80004005));
            var native = new Native { OnCall = () => throw error };
            using (var wrapper = new OpenStaadWrapper(new Root { View = native }))
            {
                Assert.That(Assert.Throws<COMException>(() => Request(wrapper.View, name)), Is.SameAs(error));
                Assert.That(native.Calls, Is.EqualTo(new[] { name }));
            }
        }

        [TestCaseSource(nameof(Names))]
        public void MissingNativeMethodPropagatesBindingError(string name)
        {
            using (var wrapper = new OpenStaadWrapper(new Root { View = new object() }))
                Assert.Throws<RuntimeBinderException>(() => Request(wrapper.View, name));
        }

        [TestCaseSource(nameof(Names))]
        public void OrientationHasMatchingCompleteComparativeXmlAndNamedReturnType(string name)
        {
            var methods = typeof(IOSView).GetMethods().Where(m => !m.IsSpecialName).ToArray();
            Assert.That(methods.Select(m => m.Name), Is.EquivalentTo(Names));
            Assert.That(methods.Single(m => m.Name == name).ReturnType, Is.EqualTo(typeof(ViewOperationResult)));
            var parameters = methods.Single(m => m.Name == name).GetParameters();
            if (IsAngular(name))
            {
                Assert.That(parameters.Length, Is.EqualTo(1));
                Assert.That(parameters[0].ParameterType, Is.EqualTo(typeof(double)));
                Assert.That(parameters[0].Name, Is.EqualTo(name == "RotateUp" ? "angleDegrees" : "angle"));
                Assert.That(parameters[0].ParameterType.IsByRef, Is.False);
            }
            else
                Assert.That(parameters, Is.Empty);
            var xml = XDocument.Load(Path.ChangeExtension(typeof(IOSView).Assembly.Location, ".xml"));
            string suffix = IsAngular(name) ? "(System.Double)" : string.Empty;
            var contract = xml.Descendants("member").Single(m => (string)m.Attribute("name") == "M:" + typeof(IOSView).FullName + "." + name + suffix);
            var adapter = xml.Descendants("member").Single(m => (string)m.Attribute("name") == "M:" + typeof(OSViewAdapter).FullName + "." + name + suffix);
            Assert.That(contract.Elements().Select(x => x.ToString()), Is.EqualTo(adapter.Elements().Select(x => x.ToString())));
            foreach (string tag in new[] { "summary", "returns", "remarks", "example" })
                Assert.That(contract.Element(tag)?.Value, Is.Not.Null.And.Not.Empty);
            Assert.That(contract.Element("remarks").Value, Does.Contain("Unlike").Or.Contain("unlike"));
            Assert.That(contract.Descendants("exception").Count(), Is.EqualTo(IsAngular(name) ? 4 : 3));
            Assert.That(contract.Descendants().Attributes("cref").Any(a => a.Value.StartsWith("!:")), Is.False);
            Assert.That(contract.Descendants("code").Single().Value, Does.Contain("view." + name + "(" + (IsAngular(name) ? (name == "RotateUp" ? "angleDegrees" : "angle") : "") + ")"));
            Assert.That(contract.Elements("param").Select(p => (string)p.Attribute("name")),
                Is.EqualTo(IsAngular(name) ? new[] { name == "RotateUp" ? "angleDegrees" : "angle" } : new string[0]));
            if (IsAngular(name))
                Assert.That(contract.Element("param").Value, Does.Contain("degrees").And.Contain("negative").And.Contain("Zero"));
        }

        [TestCase(0.0)]
        [TestCase(15.25)]
        [TestCase(-15.25)]
        [TestCase(360.0)]
        [TestCase(-720.0)]
        [TestCase(double.Epsilon)]
        [TestCase(double.MaxValue)]
        [TestCase(double.MinValue)]
        public void RotationPreservesFiniteSignedDegreesWithoutNormalization(double angleDegrees)
        {
            var native = new Native();
            using (var wrapper = new OpenStaadWrapper(new Root { View = native }))
            {
                var adapter = (OSViewAdapter)wrapper.View;
                var result = adapter.RotateUp(angleDegrees);
                Assert.That(native.Calls, Is.EqualTo(new[] { "RotateUp" }));
                Assert.That(native.RotationAngles.Single(), Is.EqualTo(angleDegrees));
                Assert.That(BitConverter.DoubleToInt64Bits(native.RotationAngles.Single()),
                    Is.EqualTo(BitConverter.DoubleToInt64Bits(angleDegrees)));
                Assert.That(result.Status, Is.EqualTo(ViewOperationStatus.InvokedWithoutAcknowledgement));
            }
        }

        [TestCase(double.NaN)]
        [TestCase(double.PositiveInfinity)]
        [TestCase(double.NegativeInfinity)]
        public void RotationRejectsNonfiniteDegreesBeforeNativeDispatch(double angleDegrees)
        {
            var native = new Native();
            using (var wrapper = new OpenStaadWrapper(new Root { View = native }))
            {
                var error = Assert.Throws<ArgumentOutOfRangeException>(() => wrapper.View.RotateUp(angleDegrees));
                Assert.That(error.ParamName, Is.EqualTo("angleDegrees"));
                Assert.That(native.Calls, Is.Empty);
                Assert.That(native.RotationAngles, Is.Empty);
            }
        }

        [Test]
        public void RepeatedRotationsRemainSeparateRequestsAndPreserveNegativeZero()
        {
            var native = new Native();
            using (var wrapper = new OpenStaadWrapper(new Root { View = native }))
            {
                double negativeZero = BitConverter.Int64BitsToDouble(long.MinValue);
                foreach (double angle in new[] { 15.0, -15.0, negativeZero })
                    wrapper.View.RotateUp(angle);

                Assert.That(native.Calls, Is.EqualTo(new[] { "RotateUp", "RotateUp", "RotateUp" }));
                Assert.That(native.RotationAngles, Is.EqualTo(new[] { 15.0, -15.0, negativeZero }));
                Assert.That(BitConverter.DoubleToInt64Bits(native.RotationAngles[2]), Is.EqualTo(long.MinValue));
            }
        }

        [Test]
        public void NullViewBindingsAreRejected()
        {
            Assert.Throws<ArgumentNullException>(() => new OSViewAdapter(null));
            using (var wrapper = new OpenStaadWrapper(new Root()))
                Assert.Throws<ArgumentNullException>(() => new OSViewAdapter(wrapper));
        }
    }
}
