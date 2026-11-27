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
    }
}
