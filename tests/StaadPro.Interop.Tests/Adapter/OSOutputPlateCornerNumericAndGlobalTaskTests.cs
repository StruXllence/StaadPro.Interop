using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
using NUnit.Framework;
using PackageVerification;
using StaadPro.Interop.Entities;
using StaadPro.Interop.Enums;
using StaadPro.Interop.Enums.Output;
using StaadPro.Interop.Models;
using StaadPro.Interop.Models.Output;

namespace StaadPro.Interop.Tests.Adapter
{
    [TestFixture, Apartment(ApartmentState.STA)]
    public sealed class OSOutputPlateCornerNumericAndGlobalTaskTests
    {
        private static Plate InclinedPlate() =>
            new Plate(17, new Node(1, 1, 2, 3), new Node(2, 2, 0, 4), new Node(3, 2, 2, 5));

        [Test]
        public void NumericBatchPreservesFirstSeenOrderAndHydratesSuccessfulIncidenceOnce()
        {
            var root = new OutputPlateCornerFixture.Root();
            var geometry = (OutputPlateCornerFixture.Geometry)root.Geometry;
            var load = (OutputPlateCornerFixture.Load)root.Load;
            var native = (OutputPlateCornerFixture.Native)root.Output;
            load.CombinationIds = new[] { 3 };
            load.ReferenceIds = new[] { 3, 4 };
            native.Responses.Enqueue(true);
            native.Responses.Enqueue(0);
            int enumerations = 0;
            int apartment = Thread.CurrentThread.ManagedThreadId;
            bool captured = false;
            IEnumerable<int> Ids()
            {
                Assert.That(++enumerations, Is.EqualTo(1));
                foreach (int id in new[] { 4, 3, 7, 1, 4 })
                    yield return id;
                captured = true;
            }
            Action checkApartment = () =>
            {
                Assert.That(captured, Is.True);
                Assert.That(Thread.CurrentThread.ManagedThreadId, Is.EqualTo(apartment));
            };
            load.OnCall = checkApartment;
            geometry.OnCall = checkApartment;
            native.OnCall = checkApartment;

            using (var wrapper = new OpenStaadWrapper(root))
            {
                var task = wrapper.Output.GetPlateCornerForcesAsync(Ids(), 17, 2, wrapper.Geometry, wrapper.Load);
                Assert.That(task.Status, Is.EqualTo(TaskStatus.RanToCompletion));
                var result = task.Result;

                Assert.That(result.Keys.Select(k => k.LoadCaseId), Is.EqualTo(new[] { 4, 3, 7, 1 }));
                Assert.That(result.Keys.Select(k => k.LoadCaseType), Is.EqualTo(new LoadCaseType?[]
                    { LoadCaseType.ReferenceLoad, null, null, LoadCaseType.PrimaryLoad }));
                Assert.That(result.Items.Select(item => item.Status), Is.EqualTo(new[]
                    { OutputQueryStatus.Success, OutputQueryStatus.MetadataUnavailable,
                      OutputQueryStatus.MetadataUnavailable, OutputQueryStatus.NativeFailure }));
                Assert.That(result.Items[0].Data.CornerPosition.X, Is.EqualTo(2d));
                Assert.That(native.Calls.Select(call => (int)call[2]), Is.EqualTo(new[] { 4, 1 }));
                Assert.That(geometry.IncidenceCalls, Is.EqualTo(1));
                Assert.That(geometry.CoordinateCalls, Is.EqualTo(4));
            }
        }

        [Test]
        public void NumericBatchLeavesGeometryUntouchedForUnresolvedMetadata()
        {
            var root = new OutputPlateCornerFixture.Root();
            var load = (OutputPlateCornerFixture.Load)root.Load;
            load.ReferenceIds = new[] { 1 };
            using (var wrapper = new OpenStaadWrapper(root))
            {
                var result = wrapper.Output.GetPlateCornerForcesAsync(new[] { 1, 200 }, 17, 2, wrapper.Geometry, wrapper.Load)
                    .GetAwaiter().GetResult();

                Assert.That(result.Items.Select(item => item.Status), Is.All.EqualTo(OutputQueryStatus.MetadataUnavailable));
                Assert.That(result.Items.Select(item => item.LoadCaseType), Is.All.Null);
                Assert.That(((OutputPlateCornerFixture.Geometry)root.Geometry).IncidenceCalls, Is.Zero);
                Assert.That(((OutputPlateCornerFixture.Native)root.Output).Calls, Is.Empty);
            }
        }

        [Test]
        public void NumericBatchValidatesBeforeNativeReadsAndAcceptsEmptySelection()
        {
            var root = new OutputPlateCornerFixture.Root();
            using (var wrapper = new OpenStaadWrapper(root))
            {
                var output = wrapper.Output;
                var invalid = output.GetPlateCornerForcesAsync(new[] { 1, 0 }, 17, 2, wrapper.Geometry, wrapper.Load);
                var oversized = output.GetPlateCornerForcesAsync(Enumerable.Repeat(1, 100001), 17, 2, wrapper.Geometry, wrapper.Load);
                var empty = output.GetPlateCornerForcesAsync(Array.Empty<int>(), 17, 2, wrapper.Geometry, wrapper.Load);
                Assert.That(invalid.IsFaulted, Is.True);
                Assert.Throws<ArgumentOutOfRangeException>(() => invalid.GetAwaiter().GetResult());
                Assert.Throws<ArgumentException>(() => oversized.GetAwaiter().GetResult());
                Assert.That(empty.Result.Items, Is.Empty);
                Assert.That(((OutputPlateCornerFixture.Load)root.Load).Calls, Is.Empty);
                Assert.That(((OutputPlateCornerFixture.Geometry)root.Geometry).IncidenceCalls, Is.Zero);
                Assert.That(((OutputPlateCornerFixture.Native)root.Output).Calls, Is.Empty);
            }
        }

        [TestCase(false)]
        [TestCase(true)]
        public void NumericBatchRejectsForeignOrStaleBindingsBeforeDispatch(bool stale)
        {
            var root = new OutputPlateCornerFixture.Root();
            var other = new OutputPlateCornerFixture.Root();
            using (var wrapper = new OpenStaadWrapper(root))
            using (var foreign = new OpenStaadWrapper(other))
            {
                var geometry = stale ? wrapper.Geometry : foreign.Geometry;
                if (stale) wrapper.RefreshComObjects();

                var task = wrapper.Output.GetPlateCornerForcesAsync(new[] { 1 }, 17, 2, geometry, wrapper.Load);
                Assert.That(task.IsFaulted, Is.True);
                Assert.Throws<ArgumentException>(() => task.GetAwaiter().GetResult());
                Assert.That(((OutputPlateCornerFixture.Load)root.Load).Calls, Is.Empty);
                Assert.That(((OutputPlateCornerFixture.Geometry)root.Geometry).IncidenceCalls, Is.Zero);
                Assert.That(((OutputPlateCornerFixture.Native)root.Output).Calls, Is.Empty);
            }
        }

        [Test]
        public void GlobalAliasReturnsAllNativeComponentsAndUsesCanonicalPoint()
        {
            var root = new OutputPlateCornerFixture.Root();
            var native = (OutputPlateCornerFixture.Native)root.Output;
            var plate = InclinedPlate();
            using (var wrapper = new OpenStaadWrapper(root))
            {
                var task = wrapper.Output.GetPlateCornerForcesInGlobalCoordinatesAsync(
                    new LoadCase(1, "Supplied"), plate, new Node(1, 90, 90, 90));
                Assert.That(task.Status, Is.EqualTo(TaskStatus.RanToCompletion));
                var result = task.Result;
                var f = result.Data.Components;

                Assert.That(new[] { f.Fx, f.Fy, f.Fz, f.Mx, f.My, f.Mz },
                    Is.EqualTo(new[] { 1.25, 2.25, 3.25, 4.25, 5.25, 6.25 }));
                Assert.That(result.Data.CornerPosition, Is.EqualTo(new Point3D(1, 2, 3)));
                Assert.That(result.Data.Key, Is.EqualTo(new PlateCornerForcesKey(1, 17, 1, LoadCaseType.PrimaryLoad)));
                Assert.That(native.Threads.Single(), Is.EqualTo(Thread.CurrentThread.ManagedThreadId));
                Assert.That(((OutputPlateCornerFixture.Load)root.Load).Calls, Is.Empty);
            }
        }

        [Test]
        public void GlobalAliasRetainsNotFoundForCornerMissingAfterHydration()
        {
            var root = new OutputPlateCornerFixture.Root();
            using (var wrapper = new OpenStaadWrapper(root))
            {
                var task = wrapper.Output.GetPlateCornerForcesInGlobalCoordinatesAsync(
                    new LoadCase(1), new Plate { Id = 17 }, new Node(99, 0, 0, 0));
                Assert.That(task.Status, Is.EqualTo(TaskStatus.RanToCompletion));
                Assert.That(task.Result.Status, Is.EqualTo(OutputQueryStatus.NotFound));
                Assert.That(task.Result.CornerNodeId, Is.EqualTo(99));
                Assert.That(((OutputPlateCornerFixture.Geometry)root.Geometry).IncidenceCalls, Is.EqualTo(1));
                Assert.That(((OutputPlateCornerFixture.Native)root.Output).Calls, Is.Empty);
            }
        }

        [Test]
        public void GlobalAliasPreservesOrdinaryNativeFailureThroughTheTask()
        {
            var root = new OutputPlateCornerFixture.Root();
            ((OutputPlateCornerFixture.Native)root.Output).Status = false;
            var plate = InclinedPlate();
            using (var wrapper = new OpenStaadWrapper(root))
            {
                var result = wrapper.Output.GetPlateCornerForcesInGlobalCoordinatesAsync(new LoadCase(1), plate, plate.A)
                    .GetAwaiter().GetResult();
                Assert.That(result.Status, Is.EqualTo(OutputQueryStatus.NativeFailure));
                Assert.That(result.NativeBooleanReturn, Is.False);
                Assert.Throws<InvalidOperationException>(() => { _ = result.Data; });
            }
        }

        [TestCase(false)]
        [TestCase(true)]
        public void BothTasksExposeMalformedSuccessAndOriginalTransportFault(bool numeric)
        {
            var root = new OutputPlateCornerFixture.Root();
            var native = (OutputPlateCornerFixture.Native)root.Output;
            var plate = InclinedPlate();
            var error = new COMException("Native read failed", unchecked((int)0x800706BA));
            using (var wrapper = new OpenStaadWrapper(root))
            {
                Task Query() => numeric
                    ? (Task)wrapper.Output.GetPlateCornerForcesAsync(new[] { 1 }, 17, 1, wrapper.Geometry, wrapper.Load)
                    : wrapper.Output.GetPlateCornerForcesInGlobalCoordinatesAsync(new LoadCase(1), plate, plate.A);

                native.WriteCount = 5;
                var malformed = Query();
                Assert.That(malformed.IsFaulted, Is.True);
                Assert.Throws<InvalidOperationException>(() => malformed.GetAwaiter().GetResult());

                native.OnCall = () => throw error;
                var invalid = Query();
                Assert.That(invalid.IsFaulted, Is.True);
                Assert.That(Assert.Throws<COMException>(() => invalid.GetAwaiter().GetResult()), Is.SameAs(error));
            }
        }
    }
}
