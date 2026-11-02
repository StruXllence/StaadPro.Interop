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
        private static readonly string[] Names = { "ShowFront", "ShowBack", "ShowLeft" };

        private static IEnumerable<object[]> OrientationVersions
        {
            get
            {
                foreach (string name in Names)
                    foreach (int version in new[] { 24, 25, 26 })
                        yield return new object[] { name, version };
            }
        }

        private static ViewOperationResult Request(IOSView view, string name)
        {
            switch (name)
            {
                case "ShowFront": return view.ShowFront();
                case "ShowBack": return view.ShowBack();
                case "ShowLeft": return view.ShowLeft();
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
            Assert.That(methods.Single(m => m.Name == name).GetParameters(), Is.Empty);
            var xml = XDocument.Load(Path.ChangeExtension(typeof(IOSView).Assembly.Location, ".xml"));
            var contract = xml.Descendants("member").Single(m => (string)m.Attribute("name") == "M:" + typeof(IOSView).FullName + "." + name);
            var adapter = xml.Descendants("member").Single(m => (string)m.Attribute("name") == "M:" + typeof(OSViewAdapter).FullName + "." + name);
            Assert.That(contract.Elements().Select(x => x.ToString()), Is.EqualTo(adapter.Elements().Select(x => x.ToString())));
            foreach (string tag in new[] { "summary", "returns", "remarks", "example" })
                Assert.That(contract.Element(tag)?.Value, Is.Not.Null.And.Not.Empty);
            Assert.That(contract.Element("remarks").Value, Does.Contain("Unlike").Or.Contain("unlike"));
            Assert.That(contract.Descendants("exception").Count(), Is.EqualTo(3));
            Assert.That(contract.Descendants().Attributes("cref").Any(a => a.Value.StartsWith("!:")), Is.False);
            Assert.That(contract.Descendants("code").Single().Value, Does.Contain("view." + name + "()"));
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
