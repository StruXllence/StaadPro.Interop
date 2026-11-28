using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using PackageVerification;
using StaadPro.Interop.Entities;
using StaadPro.Interop.Enums.Output;
using StaadPro.Interop.Models;
using StaadPro.Interop.Models.Output;

namespace StaadPro.Interop.Tests.Adapter
{
    [TestFixture, Apartment(ApartmentState.STA)]
    public sealed class OSOutputPlateCornerBatchTaskTests
    {
        private static Plate Triangle(int id = 17)
        {
            return new Plate(id, new Node(1, 0, 0, 0), new Node(2, 2, 0, 0), new Node(3, 2, 2, 0));
        }

        [Test]
        public void CasePlateBatchCapturesInputsAndExpandsInCasePlateCornerOrder()
        {
            var root = new OutputPlateCornerFixture.Root();
            var native = (OutputPlateCornerFixture.Native)root.Output;
            var first = Triangle(19);
            var second = Triangle();
            var loadCase = new LoadCase(3);
            int caseEnumerations = 0, plateEnumerations = 0;
            IEnumerable<ILoadCase> Cases()
            {
                Assert.That(++caseEnumerations, Is.EqualTo(1));
                yield return loadCase;
                yield return new LoadCase(1);
                yield return new LoadCase(3);
            }
            IEnumerable<Plate> Plates()
            {
                Assert.That(++plateEnumerations, Is.EqualTo(1));
                yield return first;
                yield return second;
                yield return Triangle(19);
            }

            native.Responses.Enqueue(0);
            native.OnCall = () => { loadCase.Id = 20; second.Id = 30; second.A.X = 70; };
            using (var wrapper = new OpenStaadWrapper(root))
            {
                var task = wrapper.Output.GetPlateCornerForcesAsync(Cases(), Plates());
                Assert.That(task.Status, Is.EqualTo(TaskStatus.RanToCompletion));
                var result = task.Result;

                Assert.That(result.Keys.Select(k => k.LoadCaseId), Is.EqualTo(new[] { 3, 3, 3, 3, 3, 3, 1, 1, 1, 1, 1, 1 }));
                Assert.That(result.Keys.Select(k => k.PlateId), Is.EqualTo(new[] { 19, 19, 19, 17, 17, 17, 19, 19, 19, 17, 17, 17 }));
                Assert.That(result.Keys.Select(k => k.CornerNodeId), Is.EqualTo(new[] { 1, 2, 3, 1, 2, 3, 1, 2, 3, 1, 2, 3 }));
                Assert.That(result.Items[0].Status, Is.EqualTo(OutputQueryStatus.NativeFailure));
                Assert.That(result.Items[3].Data.CornerPosition.X, Is.Zero);
                Assert.That(native.Calls.Count, Is.EqualTo(12));
                Assert.That(native.Threads, Is.All.EqualTo(Thread.CurrentThread.ManagedThreadId));
                Assert.That(((OutputPlateCornerFixture.Geometry)root.Geometry).IncidenceCalls, Is.Zero);
            }
        }

        [Test]
        public void CasePlateBatchEnforcesExpansionLimitBeforeDeduplicationAndReusesValidation()
        {
            var root = new OutputPlateCornerFixture.Root();
            var plate = Triangle();
            var loadCase = new LoadCase(3);
            using (var wrapper = new OpenStaadWrapper(root))
            {
                var oversized = wrapper.Output.GetPlateCornerForcesAsync(
                    Enumerable.Repeat<ILoadCase>(loadCase, 500), Enumerable.Repeat(plate, 67));
                var invalid = wrapper.Output.GetPlateCornerForcesAsync(
                    new ILoadCase[] { loadCase }, new[] { new Plate { Id = 17 } });
                Assert.That(oversized.IsFaulted, Is.True);
                Assert.That(Assert.Throws<ArgumentException>(() => oversized.GetAwaiter().GetResult()).Message,
                    Does.Contain("100000 outcomes"));
                Assert.Throws<ArgumentException>(() => invalid.GetAwaiter().GetResult());
                Assert.That(wrapper.Output.GetPlateCornerForcesAsync(Array.Empty<ILoadCase>(), new[] { plate }).Result.Items, Is.Empty);
                Assert.That(((OutputPlateCornerFixture.Native)root.Output).Calls, Is.Empty);
            }
        }

        [Test]
        public void SparseRequestsRetainFirstOrderAndResolveSuccessfulIncidenceOnce()
        {
            var root = new OutputPlateCornerFixture.Root();
            var native = (OutputPlateCornerFixture.Native)root.Output;
            var geometry = (OutputPlateCornerFixture.Geometry)root.Geometry;
            var plate = new Plate { Id = 17 };
            var first = new PlateCornerForcesRequest(new LoadCase(2), plate, new Node(3, -3, -4, -5));
            var second = new PlateCornerForcesRequest(new LoadCase(1), plate, new Node(2, -9, 0, 0));
            int enumerations = 0;
            IEnumerable<PlateCornerForcesRequest> Requests()
            {
                Assert.That(++enumerations, Is.EqualTo(1));
                yield return first;
                yield return second;
                yield return first;
                yield return new PlateCornerForcesRequest(new LoadCase(1), plate, new Node(99, 0, 0, 0));
            }

            native.Responses.Enqueue(true);
            native.Responses.Enqueue(false);
            using (var wrapper = new OpenStaadWrapper(root))
            {
                var task = wrapper.Output.GetPlateCornerForcesAsync(Requests());
                Assert.That(task.Status, Is.EqualTo(TaskStatus.RanToCompletion));
                var result = task.Result;

                Assert.That(result.Keys.Select(k => k.CornerNodeId), Is.EqualTo(new[] { 3, 2, 99 }));
                Assert.That(result.Items.Select(item => item.Status),
                    Is.EqualTo(new[] { OutputQueryStatus.Success, OutputQueryStatus.NativeFailure, OutputQueryStatus.NotFound }));
                Assert.That(result.Items[0].Data.CornerPosition.X, Is.EqualTo(2d));
                Assert.That(geometry.IncidenceCalls, Is.EqualTo(1));
                Assert.That(geometry.CoordinateCalls, Is.EqualTo(4));
                Assert.That(native.Calls.Select(args => (int)args[2]), Is.EqualTo(new[] { 2, 1 }));
                Assert.That(native.Threads, Is.All.EqualTo(Thread.CurrentThread.ManagedThreadId));
            }
        }

        [TestCase(-4001, OutputQueryStatus.NotFound)]
        [TestCase(-1, OutputQueryStatus.NativeFailure)]
        public void SparseBatchPreservesNativeIncidenceStatusAndContinues(int status, OutputQueryStatus expected)
        {
            var root = new OutputPlateCornerFixture.Root();
            ((OutputPlateCornerFixture.Geometry)root.Geometry).IncidenceStatus = status;
            var native = (OutputPlateCornerFixture.Native)root.Output;
            var loadCase = new LoadCase(1);
            var full = Triangle(19);
            var requests = new[]
            {
                new PlateCornerForcesRequest(loadCase, new Plate { Id = 17 }, full.A),
                new PlateCornerForcesRequest(loadCase, full, full.B)
            };
            using (var wrapper = new OpenStaadWrapper(root))
            {
                var result = wrapper.Output.GetPlateCornerForcesAsync(requests).GetAwaiter().GetResult();

                Assert.That(result.Items[0].Status, Is.EqualTo(expected));
                Assert.That(result.Items[0].NativeReturnCode, Is.EqualTo(status));
                Assert.That(result.Items[1].IsSuccessful, Is.True);
                Assert.That(native.Calls.Count, Is.EqualTo(1));
            }
        }

        [Test]
        public void SparseBatchRejectsOversizedInvalidAndConflictingRequestsBeforeDispatch()
        {
            var root = new OutputPlateCornerFixture.Root();
            var plate = Triangle();
            var request = new PlateCornerForcesRequest(new LoadCase(1, "A"), plate, plate.A);
            var conflict = new PlateCornerForcesRequest(new LoadCase(1, "B"), plate, plate.A);
            using (var wrapper = new OpenStaadWrapper(root))
            {
                var oversized = wrapper.Output.GetPlateCornerForcesAsync(Enumerable.Repeat(request, 100001));
                var invalid = wrapper.Output.GetPlateCornerForcesAsync(new[] { request, null });
                var inconsistent = wrapper.Output.GetPlateCornerForcesAsync(new[] { request, conflict });
                var missing = wrapper.Output.GetPlateCornerForcesAsync(requests: null);
                foreach (var task in new[] { oversized, invalid, inconsistent })
                {
                    Assert.That(task.IsFaulted, Is.True);
                    Assert.Throws<ArgumentException>(() => task.GetAwaiter().GetResult());
                }
                Assert.Throws<ArgumentNullException>(() => missing.GetAwaiter().GetResult());
                Assert.That(wrapper.Output.GetPlateCornerForcesAsync(Array.Empty<PlateCornerForcesRequest>()).Result.Items, Is.Empty);
                Assert.That(((OutputPlateCornerFixture.Native)root.Output).Calls, Is.Empty);
            }
        }

        [TestCase(false)]
        [TestCase(true)]
        public void TransportFailureFaultsTaskWithOriginalException(bool sparse)
        {
            var root = new OutputPlateCornerFixture.Root();
            var native = (OutputPlateCornerFixture.Native)root.Output;
            var failure = new COMException("Native read failed", unchecked((int)0x800706BA));
            native.OnCall = () => { if (native.Calls.Count == 2) throw failure; };
            var plate = Triangle();
            var loadCase = new LoadCase(3);
            using (var wrapper = new OpenStaadWrapper(root))
            {
                var task = sparse
                    ? wrapper.Output.GetPlateCornerForcesAsync(new[]
                        { new PlateCornerForcesRequest(loadCase, plate, plate.A), new PlateCornerForcesRequest(loadCase, plate, plate.B) })
                    : wrapper.Output.GetPlateCornerForcesAsync(new ILoadCase[] { loadCase }, new[] { plate });
                Assert.That(task.IsFaulted, Is.True);
                Assert.That(Assert.Throws<COMException>(() => task.GetAwaiter().GetResult()), Is.SameAs(failure));
                Assert.That(native.Calls, Has.Count.EqualTo(2));
            }
        }
    }
}
