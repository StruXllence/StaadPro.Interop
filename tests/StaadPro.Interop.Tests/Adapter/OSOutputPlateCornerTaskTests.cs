using System;
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
    public sealed class OSOutputPlateCornerTaskTests
    {
        private static Plate CreatePlate() =>
            new Plate(17, new Node(1, 1, 2, 3), new Node(2, 2, 0, 0), new Node(3, 2, 2, 0));

        private static Task<OutputQueryResult<PlateCornerForces>> Query(IOSOutput output, bool byId) =>
            byId ? output.GetPlateCornerForcesAsync(1, 17, 1)
                : output.GetPlateCornerForcesAsync(new LoadCase(1, "Supplied title"), CreatePlate(), new Node(1, 99, 99, 99));

        [TestCase(false)]
        [TestCase(true)]
        public void QueryCompletesOnCallerApartmentAndUsesCanonicalCorner(bool byId)
        {
            var root = new OutputPlateCornerFixture.Root();
            var native = (OutputPlateCornerFixture.Native)root.Output;
            var geometry = (OutputPlateCornerFixture.Geometry)root.Geometry;
            var load = (OutputPlateCornerFixture.Load)root.Load;
            int owner = Thread.CurrentThread.ManagedThreadId;
            Action checkThread = () => Assert.That(Thread.CurrentThread.ManagedThreadId, Is.EqualTo(owner));
            load.OnCall = checkThread;
            geometry.OnCall = checkThread;
            native.OnCall = checkThread;

            using (var wrapper = new OpenStaadWrapper(root))
            {
                var task = Query(wrapper.Output, byId);
                Assert.That(task.Status, Is.EqualTo(TaskStatus.RanToCompletion));
                var result = task.GetAwaiter().GetResult();

                Assert.That(native.Calls.Single(), Is.EqualTo(new object[] { 17, 1, 1 }));
                Assert.That(result.Status, Is.EqualTo(OutputQueryStatus.Success));
                Assert.That(result.EntityId, Is.EqualTo(17));
                Assert.That(result.LoadCaseId, Is.EqualTo(1));
                Assert.That(result.CornerNodeId, Is.EqualTo(1));
                Assert.That(result.LoadCaseType, Is.EqualTo(LoadCaseType.PrimaryLoad));
                Assert.That(result.Data.Components.Fx, Is.EqualTo(1.25));
                Assert.That(result.Data.CornerPosition.X, Is.EqualTo(byId ? 0 : 1));
                Assert.That(geometry.IncidenceCalls, Is.EqualTo(byId ? 1 : 0));
                Assert.That(geometry.CoordinateCalls, Is.EqualTo(byId ? 4 : 0));
                Assert.That(load.Calls.Any(), Is.EqualTo(byId));
            }
        }

        [TestCase(false, false)]
        [TestCase(true, false)]
        [TestCase(false, 0)]
        [TestCase(true, 0)]
        public void NativeFailureCompletesTaskAndDiscardsAnyForceBuffer(bool byId, object status)
        {
            var root = new OutputPlateCornerFixture.Root();
            var native = (OutputPlateCornerFixture.Native)root.Output;
            native.Status = status;
            native.ReplaceArray = _ => "Unqualified output";

            using (var wrapper = new OpenStaadWrapper(root))
            {
                var task = Query(wrapper.Output, byId);
                Assert.That(task.Status, Is.EqualTo(TaskStatus.RanToCompletion));
                var result = task.GetAwaiter().GetResult();
                Assert.That(result.Status, Is.EqualTo(OutputQueryStatus.NativeFailure));
                Assert.That(result.CornerNodeId, Is.EqualTo(1));
                Assert.Throws<InvalidOperationException>(() => { _ = result.Data; });
                Assert.That(native.Calls, Has.Count.EqualTo(1));
            }
        }

        [TestCase(false)]
        [TestCase(true)]
        public void MissingOrAmbiguousCaseMetadataCompletesWithoutGeometryOrOutput(bool ambiguous)
        {
            var root = new OutputPlateCornerFixture.Root();
            var load = (OutputPlateCornerFixture.Load)root.Load;
            if (ambiguous)
                load.ReferenceIds = new[] { 1 };
            else
                load.PrimaryIds = Array.Empty<int>();

            using (var wrapper = new OpenStaadWrapper(root))
            {
                var task = Query(wrapper.Output, byId: true);
                Assert.That(task.Status, Is.EqualTo(TaskStatus.RanToCompletion));
                var result = task.GetAwaiter().GetResult();
                Assert.That(result.Status, Is.EqualTo(OutputQueryStatus.MetadataUnavailable));
                Assert.That(result.LoadCaseType, Is.Null);
                Assert.That(((OutputPlateCornerFixture.Geometry)root.Geometry).IncidenceCalls, Is.Zero);
                Assert.That(((OutputPlateCornerFixture.Native)root.Output).Calls, Is.Empty);
            }
        }

        [Test]
        public void ResolvedNonCornerCompletesNotFoundBeforeForceExtraction()
        {
            var root = new OutputPlateCornerFixture.Root();
            using (var wrapper = new OpenStaadWrapper(root))
            {
                var task = wrapper.Output.GetPlateCornerForcesAsync(1, 17, 200);
                Assert.That(task.Status, Is.EqualTo(TaskStatus.RanToCompletion));
                var result = task.GetAwaiter().GetResult();
                Assert.That(result.Status, Is.EqualTo(OutputQueryStatus.NotFound));
                Assert.That(result.CornerNodeId, Is.EqualTo(200));
                Assert.That(((OutputPlateCornerFixture.Native)root.Output).Calls, Is.Empty);
            }
        }

        [TestCase(false)]
        [TestCase(true)]
        public void MalformedSuccessAndTransportErrorsFaultTheTask(bool byId)
        {
            var root = new OutputPlateCornerFixture.Root();
            var native = (OutputPlateCornerFixture.Native)root.Output;
            var error = new COMException("Native transport failure", unchecked((int)0x800706BA));
            native.WriteCount = 5;

            using (var wrapper = new OpenStaadWrapper(root))
            {
                var incomplete = Query(wrapper.Output, byId);
                Assert.That(incomplete.IsFaulted, Is.True);
                Assert.Throws<InvalidOperationException>(() => incomplete.GetAwaiter().GetResult());

                native.OnCall = () => throw error;
                var failed = Query(wrapper.Output, byId);
                Assert.That(failed.IsFaulted, Is.True);
                Assert.That(Assert.Throws<COMException>(() => failed.GetAwaiter().GetResult()), Is.SameAs(error));

                native.OnCall = null;
                native.WriteCount = 6;
                Assert.That(Query(wrapper.Output, byId).GetAwaiter().GetResult().IsSuccessful, Is.True);
            }
        }

        [Test]
        public void CallerValidationFaultsTasksBeforeNativeWork()
        {
            var root = new OutputPlateCornerFixture.Root();
            var plate = CreatePlate();
            using (var wrapper = new OpenStaadWrapper(root))
            {
                var badEntity = wrapper.Output.GetPlateCornerForcesAsync(loadCase: null, plate: plate, cornerNode: plate.A);
                var badId = wrapper.Output.GetPlateCornerForcesAsync(1, 17, 0);
                Assert.That(badEntity.IsFaulted, Is.True);
                Assert.That(badId.IsFaulted, Is.True);
                Assert.That(Assert.Throws<ArgumentNullException>(() => badEntity.GetAwaiter().GetResult()).ParamName, Is.EqualTo("loadCase"));
                Assert.That(Assert.Throws<ArgumentOutOfRangeException>(() => badId.GetAwaiter().GetResult()).ParamName, Is.EqualTo("cornerNodeId"));
                Assert.That(((OutputPlateCornerFixture.Load)root.Load).Calls, Is.Empty);
                Assert.That(((OutputPlateCornerFixture.Native)root.Output).Calls, Is.Empty);
            }
        }

        [TestCase(false)]
        [TestCase(true)]
        public void ReplacedOutputInsideNativeCallFaultsTheTask(bool byId)
        {
            var root = new OutputPlateCornerFixture.Root();
            var native = (OutputPlateCornerFixture.Native)root.Output;
            using (var wrapper = new OpenStaadWrapper(root))
            {
                native.OnCall = () =>
                {
                    root.Output = new OutputPlateCornerFixture.Native();
                    wrapper.RefreshComObjects();
                };
                var result = Query(wrapper.Output, byId);
                Assert.That(result.IsFaulted, Is.True);
                Assert.Throws<InvalidOperationException>(() => result.GetAwaiter().GetResult());
                Assert.That(native.Calls, Has.Count.EqualTo(1));
                Assert.That(((OutputPlateCornerFixture.Native)root.Output).Calls, Is.Empty);
            }
        }

        private static Task<OutputQueryResult<PlateCornerForces>> QueryWithAdapters(
            IOSOutput output, IOSGeometry geometry, IOSLoad load, bool explicitFamily) =>
            explicitFamily
                ? output.GetPlateCornerForcesAsync(1, 17, 1, LoadCaseType.PrimaryLoad, geometry, load)
                : output.GetPlateCornerForcesAsync(1, 17, 1, geometry, load);

        [TestCase(false)]
        [TestCase(true)]
        public void ExplicitAdaptersFinishLookupAndExtractionOnTheCallingApartment(bool explicitFamily)
        {
            var root = new OutputPlateCornerFixture.Root();
            var load = (OutputPlateCornerFixture.Load)root.Load;
            var geometry = (OutputPlateCornerFixture.Geometry)root.Geometry;
            var native = (OutputPlateCornerFixture.Native)root.Output;
            int apartment = Thread.CurrentThread.ManagedThreadId;
            Action checkThread = () => Assert.That(Thread.CurrentThread.ManagedThreadId, Is.EqualTo(apartment));
            load.OnCall = checkThread;
            geometry.OnCall = checkThread;
            native.OnCall = checkThread;

            using (var wrapper = new OpenStaadWrapper(root))
            {
                var task = QueryWithAdapters(wrapper.Output, wrapper.Geometry, wrapper.Load, explicitFamily);
                Assert.That(task.Status, Is.EqualTo(TaskStatus.RanToCompletion));
                var result = task.GetAwaiter().GetResult();
                Assert.That(result.IsSuccessful, Is.True);
                Assert.That(result.Data.Key, Is.EqualTo(new PlateCornerForcesKey(1, 17, 1, LoadCaseType.PrimaryLoad)));
                Assert.That(result.Data.Components.Fx, Is.EqualTo(1.25));
                Assert.That(geometry.IncidenceCalls, Is.EqualTo(1));
                Assert.That(geometry.CoordinateCalls, Is.EqualTo(4));
                Assert.That(native.Calls.Single(), Is.EqualTo(new object[] { 17, 1, 1 }));
            }
        }

        [TestCase(LoadCaseType.PrimaryLoad, "GetPrimaryLoadCaseCount")]
        [TestCase(LoadCaseType.LoadCombination, "GetLoadCombinationCaseCount")]
        [TestCase(LoadCaseType.ReferenceLoad, "GetReferenceLoadCaseCount")]
        public void SpecifiedFamilyDisambiguatesMetadataWithoutAddingANativeSelector(LoadCaseType family, string countMethod)
        {
            var root = new OutputPlateCornerFixture.Root();
            var load = (OutputPlateCornerFixture.Load)root.Load;
            var native = (OutputPlateCornerFixture.Native)root.Output;
            load.CombinationIds = new[] { 1 };
            load.ReferenceIds = new[] { 1 };

            using (var wrapper = new OpenStaadWrapper(root))
            {
                var ambiguous = QueryWithAdapters(wrapper.Output, wrapper.Geometry, wrapper.Load, false);
                Assert.That(ambiguous.Status, Is.EqualTo(TaskStatus.RanToCompletion));
                Assert.That(ambiguous.Result.Status, Is.EqualTo(OutputQueryStatus.MetadataUnavailable));
                Assert.That(ambiguous.Result.LoadCaseType, Is.Null);
                Assert.That(native.Calls, Is.Empty);
                Assert.That(((OutputPlateCornerFixture.Geometry)root.Geometry).IncidenceCalls, Is.Zero);
                load.Calls.Clear();

                var result = wrapper.Output.GetPlateCornerForcesAsync(1, 17, 1, family, wrapper.Geometry, wrapper.Load)
                    .GetAwaiter().GetResult();

                Assert.That(result.IsSuccessful, Is.True);
                Assert.That(result.LoadCaseType, Is.EqualTo(family));
                Assert.That(load.Calls, Does.Contain(countMethod));
                Assert.That(load.Calls.Where(c => c.EndsWith("CaseCount", StringComparison.Ordinal)), Is.All.EqualTo(countMethod));
                Assert.That(native.Calls.Single(), Is.EqualTo(new object[] { 17, 1, 1 }));
            }
        }

        [Test]
        public void MissingSpecifiedFamilyKeepsIdentityAndDoesNotFallBackToPrimary()
        {
            var root = new OutputPlateCornerFixture.Root();
            using (var wrapper = new OpenStaadWrapper(root))
            {
                var task = wrapper.Output.GetPlateCornerForcesAsync(1, 17, 1, LoadCaseType.ReferenceLoad, wrapper.Geometry, wrapper.Load);
                Assert.That(task.Status, Is.EqualTo(TaskStatus.RanToCompletion));
                var result = task.Result;
                Assert.That(result.Status, Is.EqualTo(OutputQueryStatus.MetadataUnavailable));
                Assert.That(result.LoadCaseId, Is.EqualTo(1));
                Assert.That(result.EntityId, Is.EqualTo(17));
                Assert.That(result.CornerNodeId, Is.EqualTo(1));
                Assert.That(result.LoadCaseType, Is.EqualTo(LoadCaseType.ReferenceLoad));
                Assert.That(((OutputPlateCornerFixture.Load)root.Load).Calls, Does.Not.Contain("GetPrimaryLoadCaseCount"));
                Assert.That(((OutputPlateCornerFixture.Geometry)root.Geometry).IncidenceCalls, Is.Zero);
                Assert.That(((OutputPlateCornerFixture.Native)root.Output).Calls, Is.Empty);
            }
        }

        [TestCase(false)]
        [TestCase(true)]
        public void NullForeignAndStaleAdaptersFaultBeforeReadingMetadata(bool explicitFamily)
        {
            var root = new OutputPlateCornerFixture.Root();
            using (var wrapper = new OpenStaadWrapper(root))
            using (var foreign = new OpenStaadWrapper(new OutputPlateCornerFixture.Root()))
            {
                var geometry = wrapper.Geometry;
                var load = wrapper.Load;
                var noGeometry = QueryWithAdapters(wrapper.Output, null, load, explicitFamily);
                var noLoad = QueryWithAdapters(wrapper.Output, geometry, null, explicitFamily);
                Assert.That(noGeometry.IsFaulted && noLoad.IsFaulted, Is.True);
                Assert.That(Assert.Throws<ArgumentNullException>(() => noGeometry.GetAwaiter().GetResult()).ParamName, Is.EqualTo("geometry"));
                Assert.That(Assert.Throws<ArgumentNullException>(() => noLoad.GetAwaiter().GetResult()).ParamName, Is.EqualTo("load"));
                Assert.Throws<ArgumentException>(() => QueryWithAdapters(wrapper.Output, foreign.Geometry, load, explicitFamily).GetAwaiter().GetResult());
                Assert.Throws<ArgumentException>(() => QueryWithAdapters(wrapper.Output, geometry, foreign.Load, explicitFamily).GetAwaiter().GetResult());
                Assert.That(((OutputPlateCornerFixture.Load)root.Load).Calls, Is.Empty);

                root.Geometry = new OutputPlateCornerFixture.Geometry();
                root.Load = new OutputPlateCornerFixture.Load();
                wrapper.RefreshComObjects();

                var staleGeometry = QueryWithAdapters(wrapper.Output, geometry, wrapper.Load, explicitFamily);
                var staleLoad = QueryWithAdapters(wrapper.Output, wrapper.Geometry, load, explicitFamily);
                Assert.That(staleGeometry.IsFaulted && staleLoad.IsFaulted, Is.True);
                Assert.Throws<ArgumentException>(() => staleGeometry.GetAwaiter().GetResult());
                Assert.Throws<ArgumentException>(() => staleLoad.GetAwaiter().GetResult());
                Assert.That(((OutputPlateCornerFixture.Load)root.Load).Calls, Is.Empty);
                Assert.That(((OutputPlateCornerFixture.Geometry)root.Geometry).IncidenceCalls, Is.Zero);
                Assert.That(((OutputPlateCornerFixture.Native)root.Output).Calls, Is.Empty);
            }
        }

        [TestCase(LoadCaseType.RepeatLoad)]
        [TestCase((LoadCaseType)0)]
        public void UnsupportedFamilyFaultsWithoutNativeDispatch(LoadCaseType family)
        {
            var root = new OutputPlateCornerFixture.Root();
            using (var wrapper = new OpenStaadWrapper(root))
            {
                var task = wrapper.Output.GetPlateCornerForcesAsync(1, 17, 1, family, wrapper.Geometry, wrapper.Load);
                Assert.That(task.IsFaulted, Is.True);
                Assert.Throws<ArgumentOutOfRangeException>(() => task.GetAwaiter().GetResult());
                Assert.That(((OutputPlateCornerFixture.Load)root.Load).Calls, Is.Empty);
                Assert.That(((OutputPlateCornerFixture.Native)root.Output).Calls, Is.Empty);
            }
        }

        [TestCase(false)]
        [TestCase(true)]
        public void ExplicitAdapterTaskStopsAfterAParticipatingBindingChanges(bool changeLoad)
        {
            var root = new OutputPlateCornerFixture.Root();
            using (var wrapper = new OpenStaadWrapper(root))
            {
                if (changeLoad)
                    ((OutputPlateCornerFixture.Load)root.Load).OnCall = () =>
                    {
                        root.Load = new OutputPlateCornerFixture.Load();
                        wrapper.RefreshComObjects();
                    };
                else
                    ((OutputPlateCornerFixture.Geometry)root.Geometry).OnCall = () =>
                    {
                        root.Geometry = new OutputPlateCornerFixture.Geometry();
                        wrapper.RefreshComObjects();
                    };

                var task = QueryWithAdapters(wrapper.Output, wrapper.Geometry, wrapper.Load, changeLoad);
                Assert.That(task.IsFaulted, Is.True);
                Assert.Throws<InvalidOperationException>(() => task.GetAwaiter().GetResult());
                Assert.That(((OutputPlateCornerFixture.Native)root.Output).Calls, Is.Empty);
            }
        }

        [TestCase(false)]
        [TestCase(true)]
        public void ExplicitAdapterTaskPreservesNativeFailureAndOriginalTransportError(bool explicitFamily)
        {
            var root = new OutputPlateCornerFixture.Root();
            var native = (OutputPlateCornerFixture.Native)root.Output;
            var original = new COMException("Native dispatch failed", unchecked((int)0x800706BA));

            using (var wrapper = new OpenStaadWrapper(root))
            {
                native.Status = false;
                native.ReplaceArray = _ => "This buffer must be discarded";
                var rejected = QueryWithAdapters(wrapper.Output, wrapper.Geometry, wrapper.Load, explicitFamily);
                Assert.That(rejected.Status, Is.EqualTo(TaskStatus.RanToCompletion));
                Assert.That(rejected.Result.Status, Is.EqualTo(OutputQueryStatus.NativeFailure));
                Assert.Throws<InvalidOperationException>(() => { _ = rejected.Result.Data; });

                native.OnCall = () => throw original;
                var failed = QueryWithAdapters(wrapper.Output, wrapper.Geometry, wrapper.Load, explicitFamily);
                Assert.That(failed.IsFaulted, Is.True);
                Assert.That(Assert.Throws<COMException>(() => failed.GetAwaiter().GetResult()), Is.SameAs(original));
            }
        }
    }
}
