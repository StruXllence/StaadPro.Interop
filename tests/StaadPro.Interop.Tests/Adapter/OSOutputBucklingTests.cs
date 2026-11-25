using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Xml.Linq;
using Microsoft.CSharp.RuntimeBinder;
using NUnit.Framework;
using PackageVerification;
using StaadPro.Interop.Adapters.Interfaces;
using StaadPro.Interop.Adapters.Models;
using StaadPro.Interop.Entities;
using StaadPro.Interop.Enums.Output;
using StaadPro.Interop.Models;
using StaadPro.Interop.Models.Output;

namespace StaadPro.Interop.Tests.Adapter
{
    [TestFixture]
    public sealed class OSOutputBucklingTests
    {
        private static OpenStaadWrapper Wrap(OutputAdvancedFixture.Native native, int version = 26) =>
            new OpenStaadWrapper(new OutputAdvancedFixture.Root { Output = native, Major = version });
        private static void Response(OutputAdvancedFixture.Native native, string name, object value) =>
            native.Responses[name] = new Queue<object>(new[] { value });
        private static IEnumerable<TestCaseData> InvalidFactorValues()
        {
            foreach (object value in new object[] { null, 2, 2L, 2f, 2m, true, "2.5", new double[1],
                double.NaN, double.PositiveInfinity, double.NegativeInfinity })
                yield return new TestCaseData(new[] { value }).SetName("FactorRejects_" + (value == null ? "null" : value.GetType().Name + "_" + value));
        }
        private static IEnumerable<TestCaseData> InvalidStatuses()
        {
            foreach (object value in new object[] { null, -1, 2, 1d, 1f, 1m, "1", uint.MaxValue })
                yield return new TestCaseData(new[] { value }).SetName("BucklingRejectsStatus_" + (value == null ? "null" : value.GetType().Name + "_" + value));
        }
        [TestCase(24)]
        [TestCase(25)]
        [TestCase(26)]
        public void FactorAndNodeOverloadUseCommonRoutesWithUnchangedArgumentsAndCallerThread(int version)
        {
            var native = new OutputAdvancedFixture.Native();
            using (var wrapper = Wrap(native, version))
            {
                var factor = wrapper.Output.GetBucklingFactor(3);
                var displacement = wrapper.Output.GetBucklingModeDisplacements(2, new Node { Id = 17 });
                Assert.That(factor.Data, Is.EqualTo(2.5));
                Assert.That(factor.NativeFunction, Is.EqualTo("GetBucklingFactor"));
                Assert.That(factor.NativeBooleanReturn, Is.True);
                Assert.That(factor.NativeReturnCode, Is.Null);
                Assert.That(displacement.Data.NativeModeNumber, Is.EqualTo(2));
                Assert.That(displacement.Data.NodeId, Is.EqualTo(17));
                Assert.That(displacement.EntityId, Is.EqualTo(17));
                Assert.That(displacement.NativeReturnCode, Is.EqualTo(1));
                Assert.That(native.Calls, Is.EqualTo(new[] { "GetBucklingFactor", "GetBucklingModeDisplacementAtNode" }));
                Assert.That(native.Arguments[0], Is.EqualTo(new object[] { 3 }));
                Assert.That(native.Arguments[1], Is.EqualTo(new object[] { 2, 17 }));
                Assert.That(native.Threads, Is.All.EqualTo(Thread.CurrentThread.ManagedThreadId));
            }
        }
        [TestCase(0d)]
        [TestCase(-0d)]
        [TestCase(-2.5d)]
        [TestCase(double.MaxValue)]
        [TestCase(double.MinValue)]
        [TestCase(double.Epsilon)]
        public void FactorPreservesFiniteWrittenMagnitudeAndSign(double value)
        {
            var native = new OutputAdvancedFixture.Native();
            native.Scalars["bucklingFactor"] = value;
            using (var wrapper = Wrap(native))
            {
                double result = wrapper.Output.GetBucklingFactor(1).Data;
                Assert.That(BitConverter.DoubleToInt64Bits(result), Is.EqualTo(BitConverter.DoubleToInt64Bits(value)));
            }
        }
        [TestCaseSource(nameof(InvalidFactorValues))]
        public void FactorRejectsUnwrittenMalformedAndNonDoubleSuccess(object value)
        {
            var native = new OutputAdvancedFixture.Native(); native.Scalars["bucklingFactor"] = value;
            using (var wrapper = Wrap(native))
                Assert.Throws<InvalidOperationException>(() => wrapper.Output.GetBucklingFactor(1));
            Assert.That(native.Calls, Has.Count.EqualTo(1));
        }
        [Test]
        public void FactorSentinelDetectsAnEntirelyUnwrittenSuccess()
        {
            var native = new OutputAdvancedFixture.Native { WriteOutputs = false };
            using (var wrapper = Wrap(native))
                Assert.Throws<InvalidOperationException>(() => wrapper.Output.GetBucklingFactor(1));
        }
        [TestCaseSource(nameof(InvalidStatuses))]
        public void BothBucklingQueriesRejectUnsupportedStatusWithoutRetry(object status)
        {
            foreach (bool factor in new[] { true, false })
            {
                var native = new OutputAdvancedFixture.Native();
                Response(native, factor ? "GetBucklingFactor" : "GetBucklingModeDisplacementAtNode", status);
                using (var wrapper = Wrap(native))
                {
                    if (factor) Assert.Throws<InvalidOperationException>(() => wrapper.Output.GetBucklingFactor(1));
                    else Assert.Throws<InvalidOperationException>(() => wrapper.Output.GetBucklingModeDisplacements(1, new Node { Id = 17 }));
                }
                Assert.That(native.Calls, Has.Count.EqualTo(1));
            }
        }
        [TestCase(true)]
        [TestCase(false)]
        public void ExactIntegralSuccessEvidenceRemainsSeparateFromFactorData(bool factor)
        {
            foreach (object status in new object[] { (short)1, 1, 1L, 1u, true })
            {
                var native = new OutputAdvancedFixture.Native();
                Response(native, factor ? "GetBucklingFactor" : "GetBucklingModeDisplacementAtNode", status);
                using (var wrapper = Wrap(native))
                {
                    var result = factor ? (object)wrapper.Output.GetBucklingFactor(1) :
                        wrapper.Output.GetBucklingModeDisplacements(1, new Node { Id = 17 });
                    var type = result.GetType();
                    Assert.That(type.GetProperty("IsSuccessful").GetValue(result), Is.True);
                    Assert.That(type.GetProperty("NativeBooleanReturn").GetValue(result), Is.EqualTo(status is bool ? (object)true : null));
                    Assert.That(type.GetProperty("NativeReturnCode").GetValue(result), Is.EqualTo(status is bool ? null : (object)1));
                }
            }
        }
        [TestCase(true)]
        [TestCase(false)]
        public void NativeFailureTakesPrecedenceOverAnyWrittenOrMalformedData(bool factor)
        {
            foreach (object status in new object[] { false, 0, (short)0, 0L })
            {
                var native = new OutputAdvancedFixture.Native { ReplaceArray = (role, array) => null };
                native.Scalars["bucklingFactor"] = "malformed";
                Response(native, factor ? "GetBucklingFactor" : "GetBucklingModeDisplacementAtNode", status);
                using (var wrapper = Wrap(native))
                {
                    if (factor)
                    {
                        var result = wrapper.Output.GetBucklingFactor(1);
                        Assert.That(result.Status, Is.EqualTo(OutputQueryStatus.NativeFailure));
                        Assert.Throws<InvalidOperationException>(() => { var ignored = result.Data; });
                    }
                    else
                    {
                        var result = wrapper.Output.GetBucklingModeDisplacements(1, new Node { Id = 17 });
                        Assert.That(result.Status, Is.EqualTo(OutputQueryStatus.NativeFailure));
                        Assert.That(result.EntityId, Is.EqualTo(17));
                        Assert.Throws<InvalidOperationException>(() => { var ignored = result.Data; });
                    }
                }
                Assert.That(native.Calls, Has.Count.EqualTo(1));
            }
        }
        [Test]
        public void InvalidArgumentsFailBeforeNativeAccessAndNodeCoordinatesAreUnused()
        {
            var native = new OutputAdvancedFixture.Native();
            using (var wrapper = Wrap(native))
            {
                foreach (int mode in new[] { 0, -1, int.MinValue })
                    Assert.That(Assert.Throws<ArgumentOutOfRangeException>(() => wrapper.Output.GetBucklingFactor(mode)).ParamName, Is.EqualTo("bucklingMode"));
                Assert.That(Assert.Throws<ArgumentNullException>(() => wrapper.Output.GetBucklingModeDisplacements(1, (Node)null)).ParamName, Is.EqualTo("node"));
                Assert.Throws<ArgumentOutOfRangeException>(() => wrapper.Output.GetBucklingModeDisplacements(-1, new Node { Id = 17 }));
                foreach (int id in new[] { 0, -1, int.MinValue })
                    Assert.Throws<ArgumentOutOfRangeException>(() => wrapper.Output.GetBucklingModeDisplacements(1, new Node { Id = id }));
                Assert.That(native.Calls, Is.Empty);
                var literalIndex = wrapper.Output.GetBucklingModeDisplacements(0, new Node { Id = 17, X = double.NaN, Y = double.PositiveInfinity, Z = double.NegativeInfinity });
                Assert.That(literalIndex.Data.NativeModeNumber, Is.Zero);
                Assert.That(native.Arguments.Single(), Is.EqualTo(new object[] { 0, 17 }));
            }
        }
        [Test]
        public void NodeOverloadCopiesAllComponentsAndCapturesIdBeforeMutableCallbacks()
        {
            var native = new OutputAdvancedFixture.Native();
            var buffer = Array.CreateInstance(typeof(double), new[] { 6 }, new[] { -3 });
            for (int i = 0; i < 6; i++) buffer.SetValue((double)(i - 2), i - 3);
            native.ReplaceArray = (role, array) => buffer;
            var node = new Node { Id = 17 };
            native.OnCall = op => node.Id = 99;
            using (var wrapper = Wrap(native))
            {
                var result = wrapper.Output.GetBucklingModeDisplacements(1, node);
                buffer.SetValue(12345d, -3);
                Assert.That(result.Data.NodeId, Is.EqualTo(17)); Assert.That(result.EntityId, Is.EqualTo(17));
                Assert.That(result.Data.Translation.X, Is.EqualTo(-2)); Assert.That(result.Data.Translation.Y, Is.EqualTo(-1));
                Assert.That(result.Data.Translation.Z, Is.Zero); Assert.That(result.Data.Rotation.X, Is.EqualTo(1));
                Assert.That(result.Data.Rotation.Y, Is.EqualTo(2)); Assert.That(result.Data.Rotation.Z, Is.EqualTo(3));
                Assert.That(native.Arguments.Single(), Is.EqualTo(new object[] { 1, 17 }));
                Assert.That(node.Id, Is.EqualTo(99));
            }
        }
        [Test]
        public void NodeOverloadReusesExactShapeFiniteAndWrittenOutputChecks()
        {
            var malformed = new object[] { null, 0d, new double[5], new double[7], new double[1,6],
                new float[6], new object[6], new int[6],
                new[] { 0d, 0, 0, 0, 0, double.NaN }, new[] { 0d, 0, 0, 0, 0, double.PositiveInfinity } };
            foreach (object value in malformed)
            {
                var native = new OutputAdvancedFixture.Native { ReplaceArray = (role, array) => value };
                using (var wrapper = Wrap(native))
                    Assert.Throws<InvalidOperationException>(() => wrapper.Output.GetBucklingModeDisplacements(1, new Node { Id = 17 }));
            }
            var unwritten = new OutputAdvancedFixture.Native { WriteOutputs = false };
            using (var wrapper = Wrap(unwritten))
                Assert.Throws<InvalidOperationException>(() => wrapper.Output.GetBucklingModeDisplacements(1, new Node { Id = 17 }));
        }
        [TestCase(true)]
        [TestCase(false)]
        public void StaleDisposedAndReplacedDuringCallBindingsCannotPublishData(bool factor)
        {
            var native = new OutputAdvancedFixture.Native(); var root = new OutputAdvancedFixture.Root { Output = native };
            var wrapper = new OpenStaadWrapper(root); var stale = wrapper.Output;
            void Call(IOSOutput output)
            {
                if (factor) output.GetBucklingFactor(1);
                else output.GetBucklingModeDisplacements(1, new Node { Id = 17 });
            }
            root.Output = new OutputAdvancedFixture.Native(); wrapper.RefreshComObjects();
            Assert.Throws<InvalidOperationException>(() => Call(stale)); Assert.That(native.Calls, Is.Empty);
            var current = wrapper.Output; wrapper.Dispose(); Assert.Throws<InvalidOperationException>(() => Call(current));
            var replacementNative = new OutputAdvancedFixture.Native(); root.Output = replacementNative;
            using (var rebound = new OpenStaadWrapper(root))
            {
                replacementNative.OnCall = op => { root.Output = new OutputAdvancedFixture.Native(); rebound.RefreshComObjects(); };
                Assert.Throws<InvalidOperationException>(() => Call(rebound.Output));
                Assert.That(replacementNative.Calls, Has.Count.EqualTo(1));
            }
        }
        [TestCase(true)]
        [TestCase(false)]
        public void OriginalTransportAndMissingSignatureErrorsAreNotConvertedOrRetried(bool factor)
        {
            var error = new COMException("buckling native failure", unchecked((int)0x80004005));
            var native = new OutputAdvancedFixture.Native { OnCall = op => throw error };
            using (var wrapper = Wrap(native))
            {
                if (factor) Assert.That(Assert.Throws<COMException>(() => wrapper.Output.GetBucklingFactor(1)), Is.SameAs(error));
                else Assert.That(Assert.Throws<COMException>(() => wrapper.Output.GetBucklingModeDisplacements(1, new Node { Id = 17 })), Is.SameAs(error));
                native.OnCall = null;
                if (factor) Assert.DoesNotThrow(() => wrapper.Output.GetBucklingFactor(1));
                else Assert.DoesNotThrow(() => wrapper.Output.GetBucklingModeDisplacements(1, new Node { Id = 17 }));
            }
            Assert.That(native.Calls, Has.Count.EqualTo(2));
            using (var missing = new OpenStaadWrapper(new OutputAdvancedFixture.Root { Output = new object() }))
            {
                if (factor) Assert.Throws<RuntimeBinderException>(() => missing.Output.GetBucklingFactor(1));
                else Assert.Throws<RuntimeBinderException>(() => missing.Output.GetBucklingModeDisplacements(1, new Node { Id = 17 }));
            }
        }
        [Test]
        public void BothSourcePortsUseNamedManagedReturnsAndMatchingExpandedXml()
        {
            var xml = XDocument.Load(Path.ChangeExtension(typeof(IOSOutput).Assembly.Location, ".xml"));
            var names = new[] { "GetBucklingFactor", "GetBucklingModeDisplacements" };
            var types = new[] { new[] { typeof(int) }, new[] { typeof(int), typeof(Node) } };
            var parameterNames = new[] { new[] { "bucklingMode" }, new[] { "nativeBucklingModeNumber", "node" } };
            var returns = new[] { typeof(OutputQueryResult<double>), typeof(OutputQueryResult<ModeDisplacements>) };
            for (int i = 0; i < names.Length; i++)
            {
                var method = typeof(IOSOutput).GetMethod(names[i], types[i]);
                Assert.That(method.ReturnType, Is.EqualTo(returns[i]));
                Assert.That(method.GetParameters().Select(p => p.Name), Is.EqualTo(parameterNames[i]));
                Assert.That(method.GetParameters().Any(p => p.ParameterType.IsByRef || p.IsOptional), Is.False);
                string suffix = "." + names[i] + "(" + string.Join(",", types[i].Select(t => t.FullName)) + ")";
                var contract = xml.Descendants("member").Single(e => (string)e.Attribute("name") == "M:" + typeof(IOSOutput).FullName + suffix);
                var implementation = xml.Descendants("member").Single(e => (string)e.Attribute("name") == "M:" + typeof(OSOutputAdapter).FullName + suffix);
                Assert.That(string.Concat(contract.Elements()), Is.EqualTo(string.Concat(implementation.Elements())));
                Assert.That(contract.Descendants().Any(e => e.Name == "include" || e.Name == "inheritdoc"), Is.False);
                foreach (string tag in new[] { "summary", "param", "returns", "remarks", "exception", "example" })
                    Assert.That(contract.Element(tag), Is.Not.Null);
            }
        }
    }
}
