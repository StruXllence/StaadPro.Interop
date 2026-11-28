using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
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
    public sealed class OSOutputPlateCornerLocalTaskTests
    {
        private static Plate CreatePlate() =>
            new Plate(17, new Node(1, 0, 0, 0), new Node(2, 2, 0, 0), new Node(3, 2, 2, 0));

        private static Task<OutputQueryResult<PlateCornerMemberForces>> StartQuery(
            IOSOutput output, bool realMember, Plate plate, Node start, Node end) =>
            realMember
                ? output.GetPlateCornerForcesAsync(new LoadCase(1), plate, start, new Beam(18, start, end))
                : output.GetPlateCornerForcesAsync(new LoadCase(1), plate, start, end);

        [Test]
        public void ConceptualDirectionUsesTheCanonicalCornerAndRetainsGlobalSource()
        {
            var root = new OutputPlateCornerFixture.Root();
            var native = (OutputPlateCornerFixture.Native)root.Output;
            var plate = CreatePlate();

            using (var wrapper = new OpenStaadWrapper(root))
            {
                var task = wrapper.Output.GetPlateCornerForcesAsync(
                    new LoadCase(1), plate, new Node(2, 99, 99, 99), otherMemberNode: plate.A);

                Assert.That(task.Status, Is.EqualTo(TaskStatus.RanToCompletion));
                var result = task.GetAwaiter().GetResult();
                var data = result.Data;
                Assert.That(data.MemberId, Is.Null);
                Assert.That(data.MemberEnd, Is.Null);
                Assert.That(data.BetaAngleDegrees, Is.Zero);
                Assert.That(data.ReferencePoint, Is.EqualTo(new Point3D(2, 0, 0)));
                Assert.That(data.Key, Is.EqualTo(new PlateCornerForcesKey(1, 17, 2, LoadCaseType.PrimaryLoad)));
                Assert.That(data.XAxis, Is.EqualTo(new Vector3D(-1, 0, 0)));
                Assert.That(data.Components.ForceVector, Is.EqualTo(new Vector3D(-1.25, 2.25, -3.25)));
                Assert.That(data.Components.MomentVector, Is.EqualTo(new Vector3D(-4.25, 5.25, -6.25)));
                Assert.That(data.Source.Components.ForceVector, Is.EqualTo(new Vector3D(1.25, 2.25, 3.25)));
                Assert.That(data.Source.Components.MomentVector, Is.EqualTo(new Vector3D(4.25, 5.25, 6.25)));
                Assert.That(native.Calls.Single(), Is.EqualTo(new object[] { 17, 2, 1 }));
                Assert.That(native.Threads.Single(), Is.EqualTo(Thread.CurrentThread.ManagedThreadId));
            }
        }

        [Test]
        public void RealMemberAtEndUsesItsOriginalDirectionAndCapturedBeta()
        {
            var root = new OutputPlateCornerFixture.Root();
            var native = (OutputPlateCornerFixture.Native)root.Output;
            var plate = CreatePlate();
            var member = new Beam(18, plate.A, plate.B) { BetaAngle = 90 };
            native.OnCall = () => member.BetaAngle = 0;

            using (var wrapper = new OpenStaadWrapper(root))
            {
                var task = wrapper.Output.GetPlateCornerForcesAsync(new LoadCase(1), plate, plate.B, member);
                Assert.That(task.Status, Is.EqualTo(TaskStatus.RanToCompletion));
                var data = task.GetAwaiter().GetResult().Data;

                Assert.That(data.MemberId, Is.EqualTo(18));
                Assert.That(data.MemberEnd, Is.EqualTo(MemberEnd.End));
                Assert.That(data.BetaAngleDegrees, Is.EqualTo(90d));
                Assert.That(data.ReferencePoint, Is.EqualTo(new Point3D(2, 0, 0)));
                Assert.That(data.XAxis, Is.EqualTo(new Vector3D(1, 0, 0)));
                Assert.That((data.Components.ForceVector - new Vector3D(1.25, 3.25, -2.25)).Length, Is.LessThan(1e-12));
                Assert.That((data.Components.MomentVector - new Vector3D(4.25, 6.25, -5.25)).Length, Is.LessThan(1e-12));
                Assert.That(native.Calls.Single(), Is.EqualTo(new object[] { 17, 2, 1 }));
                Assert.That(native.Threads.Single(), Is.EqualTo(Thread.CurrentThread.ManagedThreadId));
            }
        }

        [TestCase(false)]
        [TestCase(true)]
        public void IdOnlyPlateHydratesAndProjectsOnCallingApartment(bool realMember)
        {
            var root = new OutputPlateCornerFixture.Root();
            var geometry = (OutputPlateCornerFixture.Geometry)root.Geometry;
            var native = (OutputPlateCornerFixture.Native)root.Output;
            var plate = CreatePlate();
            int owner = Thread.CurrentThread.ManagedThreadId;
            geometry.OnCall = () => Assert.That(Thread.CurrentThread.ManagedThreadId, Is.EqualTo(owner));

            using (var wrapper = new OpenStaadWrapper(root))
            {
                var task = StartQuery(wrapper.Output, realMember, new Plate { Id = 17 }, plate.A, plate.B);
                Assert.That(task.Status, Is.EqualTo(TaskStatus.RanToCompletion));
                var result = task.GetAwaiter().GetResult();

                Assert.That(result.Data.ReferencePoint, Is.EqualTo(new Point3D(0, 0, 0)));
                Assert.That(result.Data.Components.ForceVector, Is.EqualTo(new Vector3D(1.25, 2.25, 3.25)));
                Assert.That(geometry.IncidenceCalls, Is.EqualTo(1));
                Assert.That(geometry.CoordinateCalls, Is.EqualTo(4));
                Assert.That(native.Threads.Single(), Is.EqualTo(owner));
            }
        }

        [TestCase(false)]
        [TestCase(true)]
        public void MissingPlateDuringHydrationPreservesNotFoundWithoutForceRead(bool realMember)
        {
            var root = new OutputPlateCornerFixture.Root();
            var geometry = (OutputPlateCornerFixture.Geometry)root.Geometry;
            geometry.IncidenceStatus = -4001;
            var plate = CreatePlate();

            using (var wrapper = new OpenStaadWrapper(root))
            {
                var task = StartQuery(wrapper.Output, realMember, new Plate { Id = 17 }, plate.A, plate.B);
                Assert.That(task.Status, Is.EqualTo(TaskStatus.RanToCompletion));
                var result = task.GetAwaiter().GetResult();

                Assert.That(result.Status, Is.EqualTo(OutputQueryStatus.NotFound));
                Assert.That(result.NativeFunction, Is.EqualTo("GetPlateIncidence"));
                Assert.That(result.NativeReturnCode, Is.EqualTo(-4001));
                Assert.That(result.CornerNodeId, Is.EqualTo(1));
                Assert.That(geometry.CoordinateCalls, Is.Zero);
                Assert.That(((OutputPlateCornerFixture.Native)root.Output).Calls, Is.Empty);
            }
        }

        [TestCase(false, false)]
        [TestCase(true, false)]
        [TestCase(false, 0)]
        [TestCase(true, 0)]
        public void OrdinaryNativeFailureSettlesWithStatusAndNoPayload(bool realMember, object status)
        {
            var root = new OutputPlateCornerFixture.Root();
            var native = (OutputPlateCornerFixture.Native)root.Output;
            native.Status = status;
            native.ReplaceArray = _ => "Unqualified output";
            var plate = CreatePlate();

            using (var wrapper = new OpenStaadWrapper(root))
            {
                var task = StartQuery(wrapper.Output, realMember, plate, plate.A, plate.B);
                Assert.That(task.Status, Is.EqualTo(TaskStatus.RanToCompletion));
                var result = task.GetAwaiter().GetResult();

                Assert.That(result.Status, Is.EqualTo(OutputQueryStatus.NativeFailure));
                Assert.That(result.ManagedOperation, Is.EqualTo("GetPlateCornerForcesInMemberLocalCoordinates"));
                Assert.That(result.EntityId, Is.EqualTo(17));
                Assert.That(result.LoadCaseId, Is.EqualTo(1));
                Assert.That(result.CornerNodeId, Is.EqualTo(1));
                Assert.That(result.LoadCaseType, Is.EqualTo(LoadCaseType.PrimaryLoad));
                Assert.That(result.NativeBooleanReturn, Is.EqualTo(status is bool ? (bool?)false : null));
                Assert.That(result.NativeReturnCode, Is.EqualTo(status is int ? (int?)0 : null));
                Assert.Throws<InvalidOperationException>(() => { _ = result.Data; });
            }
        }

        [TestCase(false)]
        [TestCase(true)]
        public void InvalidLocalDirectionOrEndCoordinatesFaultBeforeForceRead(bool realMember)
        {
            var root = new OutputPlateCornerFixture.Root();
            var plate = CreatePlate();

            using (var wrapper = new OpenStaadWrapper(root))
            {
                var task = realMember
                    ? wrapper.Output.GetPlateCornerForcesAsync(new LoadCase(1), plate, plate.A,
                        new Beam(18, new Node(1, 0, 1, 0), plate.B))
                    : wrapper.Output.GetPlateCornerForcesAsync(new LoadCase(1), plate, plate.A,
                        new Node(99, 0, 0, 0));

                Assert.That(task.IsFaulted, Is.True);
                Assert.Throws<ArgumentException>(() => task.GetAwaiter().GetResult());
                Assert.That(((OutputPlateCornerFixture.Native)root.Output).Calls, Is.Empty);
            }
        }

        [TestCase(false)]
        [TestCase(true)]
        public void MalformedSuccessAndTransportFailureFaultWithOriginalError(bool realMember)
        {
            var root = new OutputPlateCornerFixture.Root();
            var native = (OutputPlateCornerFixture.Native)root.Output;
            var plate = CreatePlate();
            var failure = new COMException("Native force read failed.", unchecked((int)0x800706BA));
            native.WriteCount = 5;

            using (var wrapper = new OpenStaadWrapper(root))
            {
                var malformed = StartQuery(wrapper.Output, realMember, plate, plate.A, plate.B);
                Assert.That(malformed.IsFaulted, Is.True);
                Assert.Throws<InvalidOperationException>(() => malformed.GetAwaiter().GetResult());

                native.OnCall = () => throw failure;
                var failed = StartQuery(wrapper.Output, realMember, plate, plate.A, plate.B);
                Assert.That(failed.IsFaulted, Is.True);
                Assert.That(Assert.Throws<COMException>(() => failed.GetAwaiter().GetResult()), Is.SameAs(failure));
            }
        }
    }
}
