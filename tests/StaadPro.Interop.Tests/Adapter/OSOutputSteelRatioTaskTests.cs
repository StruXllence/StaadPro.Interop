using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
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
    [TestFixture, NonParallelizable]
    public sealed class OSOutputSteelRatioTaskTests
    {
        private const string NativeMethod = "GetMemberSteelDesignRatio";

        private static Beam NewBeam(int id, int startNodeId = 1) =>
            new Beam(id, new Node(startNodeId, 0, 0, 0), new Node(2, 2, 0, 0));

        private static OpenStaadWrapper Wrap(OutputWorkflowFixture.Native output) =>
            new OpenStaadWrapper(new OutputWorkflowFixture.Root { Output = output });

        [TestCase(true, 0d, OutputQueryStatus.Success)]
        [TestCase(true, -999d, OutputQueryStatus.MemberNotDesigned)]
        [TestCase(true, -1d, OutputQueryStatus.AnalysisNotPerformed)]
        [TestCase(false, double.NaN, OutputQueryStatus.NativeFailure)]
        public void SingleTaskReturnsTheNativeObservationOnTheCallingThread(bool nativeStatus, double ratio, OutputQueryStatus expected)
        {
            var native = new OutputWorkflowFixture.Native();
            native.Responses[NativeMethod] = new Queue<object>(new object[] { nativeStatus });
            native.Scalars["ratio"] = ratio;
            using (var wrapper = Wrap(native))
            {
                var task = wrapper.Output.GetMemberSteelUtilizationRatioAsync(17);
                Assert.That(task.Status, Is.EqualTo(TaskStatus.RanToCompletion));
                Assert.That(native.Calls, Is.EqualTo(new[] { NativeMethod }));
                Assert.That(native.Arguments.Single(), Is.EqualTo(new object[] { 17 }));
                Assert.That(native.Threads.Single(), Is.EqualTo(Thread.CurrentThread.ManagedThreadId));
                var result = task.GetAwaiter().GetResult();
                Assert.That(result.EntityId, Is.EqualTo(17));
                Assert.That(result.Status, Is.EqualTo(expected));
                if (result.IsSuccessful)
                    Assert.That(result.Data, Is.EqualTo(ratio));
                else
                    Assert.Throws<InvalidOperationException>(() => { var unused = result.Data; });
            }
        }

        [Test]
        public void BatchUsesCapturedRequestOrderAndRetainsUnsuccessfulStatusesWithoutWritingCaches()
        {
            var first = NewBeam(24);
            var second = NewBeam(11);
            var third = NewBeam(42);
            var beams = new HashSet<Beam> { first, second, third };
            int[] requested = beams.Select(beam => beam.Id).ToArray();
            int notifications = 0;
            foreach (var beam in beams)
            {
                beam.UtilizationRatio = 0.125;
                beam.PropertyChanged += (sender, args) => notifications++;
            }

            var native = new OutputWorkflowFixture.Native();
            native.Responses[NativeMethod] = new Queue<object>(new object[] { false, true, true });
            native.OnCall = operation =>
            {
                // Changing membership after dispatch starts must not change the captured request.
                beams.Clear();
                native.Scalars["ratio"] = native.Calls.Count == 2 ? -999d : 0.875d;
            };
            using (var wrapper = Wrap(native))
            {
                var task = wrapper.Output.GetMemberSteelUtilizationRatiosAsync(beams);
                Assert.That(task.Status, Is.EqualTo(TaskStatus.RanToCompletion));
                Assert.That(native.Calls, Is.EqualTo(new[] { NativeMethod, NativeMethod, NativeMethod }));
                Assert.That(native.Threads, Is.All.EqualTo(Thread.CurrentThread.ManagedThreadId));
                Assert.That(native.Arguments.Select(args => (int)args.Single()), Is.EqualTo(requested));
                var result = task.GetAwaiter().GetResult();
                Assert.That(result.Items.Select(item => item.EntityId), Is.EqualTo(requested.Select(id => (int?)id)));
                Assert.That(result.Items.Select(item => item.Status), Is.EqualTo(new[]
                {
                    OutputQueryStatus.NativeFailure, OutputQueryStatus.MemberNotDesigned, OutputQueryStatus.Success
                }));
                Assert.That(result.SucceededCount, Is.EqualTo(1));
                Assert.That(result.FailedCount, Is.EqualTo(2));
                Assert.That(result.Items[2].Data, Is.EqualTo(0.875));
                Assert.That(new[] { first, second, third }.Select(beam => beam.UtilizationRatio), Is.All.EqualTo(0.125));
                Assert.That(notifications, Is.Zero);
                Assert.That(((OutputWorkflowFixture.Load)wrapper.RawLoad).Calls, Is.Zero);
            }
        }

        [Test]
        public void InvalidInputsFaultBeforeCallingTheNativeReceiverAndEmptyBatchSucceeds()
        {
            var native = new OutputWorkflowFixture.Native();
            using (var wrapper = Wrap(native))
            {
                var single = wrapper.Output.GetMemberSteelUtilizationRatioAsync(0);
                Assert.That(single.IsFaulted, Is.True);
                Assert.That(Assert.Throws<ArgumentOutOfRangeException>(() => single.GetAwaiter().GetResult()).ParamName, Is.EqualTo("beamId"));
                var missing = wrapper.Output.GetMemberSteelUtilizationRatiosAsync(null);
                Assert.That(missing.IsFaulted, Is.True);
                Assert.That(Assert.Throws<ArgumentNullException>(() => missing.GetAwaiter().GetResult()).ParamName, Is.EqualTo("beams"));
                var invalid = wrapper.Output.GetMemberSteelUtilizationRatiosAsync(new HashSet<Beam> { NewBeam(17), NewBeam(-1) });
                Assert.That(Assert.Throws<ArgumentOutOfRangeException>(() => invalid.GetAwaiter().GetResult()).ParamName, Is.EqualTo("beams"));
                var nullBeam = wrapper.Output.GetMemberSteelUtilizationRatiosAsync(new HashSet<Beam> { NewBeam(17), null });
                Assert.That(Assert.Throws<ArgumentNullException>(() => nullBeam.GetAwaiter().GetResult()).ParamName, Is.EqualTo("beams"));
                var duplicate = wrapper.Output.GetMemberSteelUtilizationRatiosAsync(new HashSet<Beam> { NewBeam(17), NewBeam(17, 3) });
                Assert.That(Assert.Throws<ArgumentException>(() => duplicate.GetAwaiter().GetResult()).ParamName, Is.EqualTo("beams"));

                var empty = wrapper.Output.GetMemberSteelUtilizationRatiosAsync(new HashSet<Beam>());
                Assert.That(empty.Status, Is.EqualTo(TaskStatus.RanToCompletion));
                Assert.That(empty.Result.IsSuccessful, Is.True);
                Assert.That(empty.Result.Items, Is.Empty);
                Assert.That(native.Calls, Is.Empty);
            }
        }

        [Test]
        public void ChangedBeamIdentityFaultsTheBatchWithoutContinuingOrWritingTheCache()
        {
            var first = NewBeam(17);
            var second = NewBeam(18);
            var beams = new HashSet<Beam> { first, second };
            var changing = beams.First();
            int capturedId = changing.Id;
            var native = new OutputWorkflowFixture.Native { OnCall = operation => changing.Id = 99 };
            using (var wrapper = Wrap(native))
            {
                var task = wrapper.Output.GetMemberSteelUtilizationRatiosAsync(beams);
                Assert.That(task.IsFaulted, Is.True);
                Assert.Throws<InvalidOperationException>(() => task.GetAwaiter().GetResult());
                Assert.That(native.Calls, Has.Count.EqualTo(1));
                Assert.That(native.Arguments.Single(), Is.EqualTo(new object[] { capturedId }));
                Assert.That(first.UtilizationRatio, Is.Null);
                Assert.That(second.UtilizationRatio, Is.Null);
            }
        }

        [Test]
        public void MalformedSuccessfulValueFaultsTheBatchBeforeAnyLaterQuery()
        {
            var native = new OutputWorkflowFixture.Native();
            native.OnCall = operation => native.Scalars["ratio"] = native.Calls.Count == 2 ? "0.75" : (object)0.75;
            var beams = new HashSet<Beam> { NewBeam(17), NewBeam(18), NewBeam(19) };
            using (var wrapper = Wrap(native))
            {
                var task = wrapper.Output.GetMemberSteelUtilizationRatiosAsync(beams);
                Assert.That(task.IsFaulted, Is.True);
                Assert.Throws<InvalidOperationException>(() => task.GetAwaiter().GetResult());
                Assert.That(native.Calls, Has.Count.EqualTo(2));
                Assert.That(beams.Select(beam => beam.UtilizationRatio), Is.All.Null);
            }
        }

        [TestCase(false)]
        [TestCase(true)]
        public void TransportExceptionsFaultTheTaskAndKeepTheirOriginalIdentity(bool batch)
        {
            var expected = new COMException("Ratio query transport failure", unchecked((int)0x80010108));
            var native = new OutputWorkflowFixture.Native { OnCall = operation => throw expected };
            using (var wrapper = Wrap(native))
            {
                Task task = batch
                    ? (Task)wrapper.Output.GetMemberSteelUtilizationRatiosAsync(new HashSet<Beam> { NewBeam(17), NewBeam(18) })
                    : wrapper.Output.GetMemberSteelUtilizationRatioAsync(17);
                Assert.That(task.IsFaulted, Is.True);
                Assert.That(Assert.Throws<COMException>(() => task.GetAwaiter().GetResult()), Is.SameAs(expected));
                Assert.That(native.Calls, Has.Count.EqualTo(1));
            }
        }

        [Test]
        public void ReplacedDuringDispatchFaultsTheSingleTaskAndStaleBindingFaultsAnEmptyBatch()
        {
            var native = new OutputWorkflowFixture.Native();
            var root = new OutputWorkflowFixture.Root { Output = native };
            using (var wrapper = new OpenStaadWrapper(root))
            {
                var stale = wrapper.Output;
                var replacement = new OutputWorkflowFixture.Native();
                native.OnCall = operation =>
                {
                    root.Output = replacement;
                    wrapper.RefreshComObjects();
                };
                var changed = stale.GetMemberSteelUtilizationRatioAsync(17);
                Assert.That(changed.IsFaulted, Is.True);
                Assert.Throws<InvalidOperationException>(() => changed.GetAwaiter().GetResult());
                var empty = stale.GetMemberSteelUtilizationRatiosAsync(new HashSet<Beam>());
                Assert.That(empty.IsFaulted, Is.True);
                Assert.Throws<InvalidOperationException>(() => empty.GetAwaiter().GetResult());
                Assert.That(replacement.Calls, Is.Empty);
            }
        }

        [TestCase(false)]
        [TestCase(true)]
        public void ContractsShipMatchingExpandedDocumentationAndStatusReturningSignatures(bool batch)
        {
            string name = batch ? "GetMemberSteelUtilizationRatiosAsync" : "GetMemberSteelUtilizationRatioAsync";
            Type parameterType = batch ? typeof(HashSet<Beam>) : typeof(int);
            Type returnType = batch ? typeof(Task<OutputBatchResult<double>>) : typeof(Task<OutputQueryResult<double>>);
            string parameterName = batch ? "beams" : "beamId";
            string xmlParameter = batch ? "System.Collections.Generic.HashSet{" + typeof(Beam).FullName + "}" : "System.Int32";
            var xml = XDocument.Load(Path.ChangeExtension(typeof(IOSOutput).Assembly.Location, ".xml"));
            XElement Documentation(Type declaringType)
            {
                var method = declaringType.GetMethod(name, new[] { parameterType });
                Assert.That(method, Is.Not.Null);
                Assert.That(method.ReturnType, Is.EqualTo(returnType));
                Assert.That(method.GetParameters().Single().Name, Is.EqualTo(parameterName));
                Assert.That(method.GetParameters().Single().IsOptional, Is.False);
                string id = "M:" + declaringType.FullName + "." + name + "(" + xmlParameter + ")";
                return xml.Descendants("member").Single(e => (string)e.Attribute("name") == id);
            }
            var contract = Documentation(typeof(IOSOutput));
            var adapter = Documentation(typeof(OSOutputAdapter));
            Assert.That(string.Concat(contract.Elements()), Is.EqualTo(string.Concat(adapter.Elements())));
            foreach (string tag in new[] { "summary", "param", "returns", "remarks", "exception", "example" })
                Assert.That(contract.Element(tag), Is.Not.Null);
            Assert.That(contract.Descendants().Any(e => e.Name == "include" || e.Name == "inheritdoc"), Is.False);
        }
    }
}
