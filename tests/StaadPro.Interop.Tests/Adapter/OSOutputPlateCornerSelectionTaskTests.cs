using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using PackageVerification;
using StaadPro.Interop.Adapters.Interfaces;
using StaadPro.Interop.Entities;
using StaadPro.Interop.Enums;
using StaadPro.Interop.Enums.Output;
using StaadPro.Interop.Models;
using StaadPro.Interop.Models.Output;

namespace StaadPro.Interop.Tests.Adapter
{
    [TestFixture, Apartment(ApartmentState.STA)]
    public sealed class OSOutputPlateCornerSelectionTaskTests
    {
        private static Plate Triangle(bool closed)
        {
            var a = new Node(1, 0, 0, 0);
            return new Plate(17, a, new Node(2, 2, 0, 0), new Node(3, 2, 2, 0),
                closed ? new Node(a.Id, a.X, a.Y, a.Z) : null);
        }

        private static Task<PlateCornerForcesResultSet> Query(IOSOutput output, bool allCorners)
        {
            var plate = Triangle(false);
            return allCorners ? output.GetPlateCornerForcesAsync(new LoadCase(3), plate)
                : output.GetPlateCornerForcesAsync(new ILoadCase[] { new LoadCase(3), new LoadCase(4) }, plate, plate.A);
        }

        [Test]
        public void CaseSelectionSnapshotsOnceAndPreservesOrderAndFamily()
        {
            var root = new OutputPlateCornerFixture.Root();
            var native = (OutputPlateCornerFixture.Native)root.Output;
            var plate = Triangle(false);
            var first = new LoadCase(4, "Initial");
            var second = new LoadCase(4, "Reference", LoadCaseType.ReferenceLoad);
            int enumerations = 0;
            bool enumerationFinished = false;
            IEnumerable<ILoadCase> Selection()
            {
                Assert.That(++enumerations, Is.EqualTo(1));
                yield return first;
                yield return new LoadCase(4, "Initial");
                yield return second;
                yield return new LoadCase(2, "Next");
                enumerationFinished = true;
            }

            native.Responses.Enqueue(false);
            native.Responses.Enqueue(true);
            native.Responses.Enqueue(true);
            native.OnCall = () =>
            {
                Assert.That(enumerationFinished, Is.True);
                first.Id = 70;
                second.Id = 80;
                plate.A.X = 900;
            };

            using (var wrapper = new OpenStaadWrapper(root))
            {
                var task = wrapper.Output.GetPlateCornerForcesAsync(Selection(), plate, new Node(1, 80, 0, 0));
                Assert.That(task.Status, Is.EqualTo(TaskStatus.RanToCompletion));
                var result = task.GetAwaiter().GetResult();

                Assert.That(result.Keys.Select(key => key.LoadCaseId), Is.EqualTo(new[] { 4, 4, 2 }));
                Assert.That(result.Keys.Select(key => key.LoadCaseType), Is.EqualTo(new[]
                    { LoadCaseType.PrimaryLoad, LoadCaseType.ReferenceLoad, LoadCaseType.PrimaryLoad }));
                Assert.That(result.Items.Select(item => item.Status), Is.EqualTo(new[]
                    { OutputQueryStatus.NativeFailure, OutputQueryStatus.Success, OutputQueryStatus.Success }));
                Assert.That(result[new PlateCornerForcesKey(4, 17, 1, LoadCaseType.ReferenceLoad)].Data.CornerPosition.X, Is.Zero);
                Assert.Throws<InvalidOperationException>(() => result.Find(new PlateCornerForcesKey(4, 17, 1)));
                Assert.That(native.Calls.Select(args => (int)args[2]), Is.EqualTo(new[] { 4, 4, 2 }));
                Assert.That(native.Threads, Is.All.EqualTo(Thread.CurrentThread.ManagedThreadId));
            }
        }

        [Test]
        public void CaseSelectionHydratesIncompleteIncidenceOnlyOnce()
        {
            var root = new OutputPlateCornerFixture.Root();
            var geometry = (OutputPlateCornerFixture.Geometry)root.Geometry;
            var native = (OutputPlateCornerFixture.Native)root.Output;
            using (var wrapper = new OpenStaadWrapper(root))
            {
                var plate = new Plate { Id = 17 };
                var cases = new ILoadCase[] { new LoadCase(3), new LoadCase(1) };
                var task = wrapper.Output.GetPlateCornerForcesAsync(cases, plate, new Node(3, -8, -8, -8));
                Assert.That(task.Status, Is.EqualTo(TaskStatus.RanToCompletion));
                var result = task.GetAwaiter().GetResult();

                Assert.That(geometry.IncidenceCalls, Is.EqualTo(1));
                Assert.That(geometry.CoordinateCalls, Is.EqualTo(4));
                Assert.That(native.Calls.Select(args => (int)args[1]), Is.EqualTo(new[] { 3, 3 }));
                Assert.That(result.Items.Select(item => item.Data.CornerPosition.X), Is.EqualTo(new[] { 2d, 2d }));
                Assert.That(plate.A, Is.Null);
            }
        }

        [TestCase(false)]
        [TestCase(true)]
        public void AllCornerSelectionUsesIncidenceOrderAndCoalescesAClosingNode(bool closed)
        {
            var root = new OutputPlateCornerFixture.Root();
            var native = (OutputPlateCornerFixture.Native)root.Output;
            native.Responses.Enqueue(true);
            native.Responses.Enqueue(0);
            native.Responses.Enqueue(true);
            using (var wrapper = new OpenStaadWrapper(root))
            {
                var task = wrapper.Output.GetPlateCornerForcesAsync(new LoadCase(3), Triangle(closed));
                Assert.That(task.Status, Is.EqualTo(TaskStatus.RanToCompletion));
                var result = task.GetAwaiter().GetResult();

                Assert.That(result.Keys.Select(key => key.CornerNodeId), Is.EqualTo(new[] { 1, 2, 3 }));
                Assert.That(result.FailedCount, Is.EqualTo(1));
                Assert.That(result.GetForCornerNode(2).Single().Status, Is.EqualTo(OutputQueryStatus.NativeFailure));
                Assert.That(native.Calls.Select(args => (int)args[1]), Is.EqualTo(new[] { 1, 2, 3 }));
                Assert.That(native.Threads, Is.All.EqualTo(Thread.CurrentThread.ManagedThreadId));
                Assert.That(((OutputPlateCornerFixture.Geometry)root.Geometry).IncidenceCalls, Is.Zero);
            }
        }

        [Test]
        public void InvalidSelectionsFaultBeforeNativeDispatchAndEmptyCaseSelectionIsEager()
        {
            var root = new OutputPlateCornerFixture.Root();
            using (var wrapper = new OpenStaadWrapper(root))
            {
                var plate = Triangle(false);
                var conflict = wrapper.Output.GetPlateCornerForcesAsync(
                    new ILoadCase[] { new LoadCase(3, "A"), new LoadCase(3, "B") }, plate, plate.A);
                var incomplete = wrapper.Output.GetPlateCornerForcesAsync(new LoadCase(3), new Plate { Id = 17 });
                var empty = wrapper.Output.GetPlateCornerForcesAsync(Array.Empty<ILoadCase>(), plate, plate.A);

                Assert.That(conflict.IsFaulted, Is.True);
                Assert.Throws<ArgumentException>(() => conflict.GetAwaiter().GetResult());
                Assert.That(incomplete.IsFaulted, Is.True);
                Assert.Throws<ArgumentException>(() => incomplete.GetAwaiter().GetResult());
                Assert.That(empty.Status, Is.EqualTo(TaskStatus.RanToCompletion));
                Assert.That(empty.Result.Items, Is.Empty);
                Assert.That(empty.Result.IsSuccessful, Is.True);
                Assert.That(((OutputPlateCornerFixture.Native)root.Output).Calls, Is.Empty);
                Assert.That(((OutputPlateCornerFixture.Geometry)root.Geometry).IncidenceCalls, Is.Zero);
            }
        }

        [TestCase(false)]
        [TestCase(true)]
        public void SecondReadTransportFaultRetainsOriginalExceptionAndNoPartialSet(bool allCorners)
        {
            var root = new OutputPlateCornerFixture.Root();
            var native = (OutputPlateCornerFixture.Native)root.Output;
            var error = new COMException("Native read failed", unchecked((int)0x800706BA));
            native.OnCall = () => { if (native.Calls.Count == 2) throw error; };
            using (var wrapper = new OpenStaadWrapper(root))
            {
                var task = Query(wrapper.Output, allCorners);
                Assert.That(task.IsFaulted, Is.True);
                Assert.That(Assert.Throws<COMException>(() => task.GetAwaiter().GetResult()), Is.SameAs(error));
                Assert.That(native.Calls, Has.Count.EqualTo(2));
            }
        }

        [Test]
        public void AllCornersAcrossCasesPreserveFamilyOrderAndNativeFailure()
        {
            var root = new OutputPlateCornerFixture.Root();
            var native = (OutputPlateCornerFixture.Native)root.Output;
            var a = new Node(1, 0, 0, 0);
            var plate = new Plate(17, a, new Node(2, 2, 0, 0), new Node(3, 2, 2, 0), new Node(4, 0, 2, 0));
            var primary = new LoadCase(5, "Dead");
            var reference = new LoadCase(5, "Reference", LoadCaseType.ReferenceLoad);
            int enumerations = 0;
            IEnumerable<ILoadCase> Cases()
            {
                Assert.That(++enumerations, Is.EqualTo(1));
                yield return primary;
                yield return new LoadCase(5, "Dead");
                yield return reference;
            }

            native.Responses.Enqueue(0);
            native.OnCall = () => { primary.Id = 99; a.X = 400; };
            using (var wrapper = new OpenStaadWrapper(root))
            {
                var task = wrapper.Output.GetPlateCornerForcesAsync(Cases(), plate);
                Assert.That(task.Status, Is.EqualTo(TaskStatus.RanToCompletion));
                var result = task.GetAwaiter().GetResult();

                Assert.That(result.Keys.Select(k => k.CornerNodeId), Is.EqualTo(new[] { 1, 2, 3, 4, 1, 2, 3, 4 }));
                Assert.That(result.Keys.Take(4).Select(k => k.LoadCaseType), Is.All.EqualTo(LoadCaseType.PrimaryLoad));
                Assert.That(result.Keys.Skip(4).Select(k => k.LoadCaseType), Is.All.EqualTo(LoadCaseType.ReferenceLoad));
                Assert.That(result.Items[0].Status, Is.EqualTo(OutputQueryStatus.NativeFailure));
                Assert.That(result.FailedCount, Is.EqualTo(1));
                Assert.That(result.Items[4].Data.CornerPosition.X, Is.Zero);
                Assert.That(native.Calls.Select(args => (int)args[2]), Is.All.EqualTo(5));
                Assert.That(native.Threads, Is.All.EqualTo(Thread.CurrentThread.ManagedThreadId));
            }
        }

        [Test]
        public void AllCornersAcrossPlatesSnapshotIncidenceAndReadDistinctRequestsInOrder()
        {
            var root = new OutputPlateCornerFixture.Root();
            var native = (OutputPlateCornerFixture.Native)root.Output;
            var first = Triangle(true);
            var second = new Plate(42, new Node(1, 0, 0, 0), new Node(5, 2, 0, 0), new Node(7, 2, 3, 0));
            int enumerations = 0;
            IEnumerable<Plate> Plates()
            {
                Assert.That(++enumerations, Is.EqualTo(1));
                yield return first;
                yield return second;
                yield return Triangle(true);
            }

            native.OnCall = () => { second.Id = 99; second.C.X = 400; };
            using (var wrapper = new OpenStaadWrapper(root))
            {
                var task = wrapper.Output.GetPlateCornerForcesAsync(new LoadCase(3), Plates());
                Assert.That(task.Status, Is.EqualTo(TaskStatus.RanToCompletion));
                var result = task.GetAwaiter().GetResult();

                Assert.That(result.Keys.Select(k => k.PlateId), Is.EqualTo(new[] { 17, 17, 17, 42, 42, 42 }));
                Assert.That(result.Keys.Select(k => k.CornerNodeId), Is.EqualTo(new[] { 1, 2, 3, 1, 5, 7 }));
                Assert.That(result[new PlateCornerForcesKey(3, 42, 7, LoadCaseType.PrimaryLoad)].Data.CornerPosition.X, Is.EqualTo(2d));
                Assert.That(native.Calls.Select(args => (int)args[0]), Is.EqualTo(new[] { 17, 17, 17, 42, 42, 42 }));
                Assert.That(native.Threads, Is.All.EqualTo(Thread.CurrentThread.ManagedThreadId));
                Assert.That(((OutputPlateCornerFixture.Geometry)root.Geometry).IncidenceCalls, Is.Zero);
            }
        }

        [Test]
        public void AllCornerSelectionsValidateIncidenceAndConflictsBeforeDispatch()
        {
            var root = new OutputPlateCornerFixture.Root();
            var plate = Triangle(false);
            using (var wrapper = new OpenStaadWrapper(root))
            {
                var caseConflict = wrapper.Output.GetPlateCornerForcesAsync(
                    new ILoadCase[] { new LoadCase(3, "A"), new LoadCase(3, "B") }, plate);
                var incomplete = wrapper.Output.GetPlateCornerForcesAsync(
                    new ILoadCase[] { new LoadCase(3) }, new Plate { Id = 17 });
                var conflictingPlate = Triangle(false);
                conflictingPlate.A.X += 1;
                var plateConflict = wrapper.Output.GetPlateCornerForcesAsync(new LoadCase(3),
                    new[] { plate, conflictingPlate });

                foreach (var task in new[] { caseConflict, incomplete, plateConflict })
                {
                    Assert.That(task.IsFaulted, Is.True);
                    Assert.Throws<ArgumentException>(() => task.GetAwaiter().GetResult());
                }

                Assert.That(wrapper.Output.GetPlateCornerForcesAsync(Array.Empty<ILoadCase>(), plate).Result.IsSuccessful, Is.True);
                Assert.That(wrapper.Output.GetPlateCornerForcesAsync(new LoadCase(3), Array.Empty<Plate>()).Result.Items, Is.Empty);
                Assert.That(((OutputPlateCornerFixture.Native)root.Output).Calls, Is.Empty);
                Assert.That(((OutputPlateCornerFixture.Geometry)root.Geometry).IncidenceCalls, Is.Zero);
            }
        }

        [TestCase(true)]
        [TestCase(false)]
        public void AllCornerExpansionChecksLimitBeforeDeduplicatingOrReading(bool byCase)
        {
            var root = new OutputPlateCornerFixture.Root();
            var plate = Triangle(false);
            var loadCase = new LoadCase(3);
            using (var wrapper = new OpenStaadWrapper(root))
            {
                var task = byCase
                    ? wrapper.Output.GetPlateCornerForcesAsync(Enumerable.Repeat<ILoadCase>(loadCase, 33334), plate)
                    : wrapper.Output.GetPlateCornerForcesAsync(loadCase, Enumerable.Repeat(plate, 33334));
                Assert.That(task.IsFaulted, Is.True);
                Assert.That(Assert.Throws<ArgumentException>(() => task.GetAwaiter().GetResult()).Message, Does.Contain("100000 outcomes"));
                Assert.That(((OutputPlateCornerFixture.Native)root.Output).Calls, Is.Empty);
            }
        }

        [TestCase(true)]
        [TestCase(false)]
        public void AllCornerSelectionFaultRetainsOriginalTransportError(bool byCase)
        {
            var root = new OutputPlateCornerFixture.Root();
            var native = (OutputPlateCornerFixture.Native)root.Output;
            var failure = new COMException("Native read failed", unchecked((int)0x800706BA));
            native.OnCall = () => { if (native.Calls.Count == 2) throw failure; };
            var plate = Triangle(false);
            var loadCase = new LoadCase(3);
            using (var wrapper = new OpenStaadWrapper(root))
            {
                var task = byCase
                    ? wrapper.Output.GetPlateCornerForcesAsync(new ILoadCase[] { loadCase }, plate)
                    : wrapper.Output.GetPlateCornerForcesAsync(loadCase, new[] { plate });
                Assert.That(task.IsFaulted, Is.True);
                Assert.That(Assert.Throws<COMException>(() => task.GetAwaiter().GetResult()), Is.SameAs(failure));
                Assert.That(native.Calls, Has.Count.EqualTo(2));
            }
        }
    }
}
