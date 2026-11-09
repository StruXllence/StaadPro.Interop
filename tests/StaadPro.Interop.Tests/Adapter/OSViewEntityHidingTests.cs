using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using StaadPro.Interop.Adapters.Interfaces;
using StaadPro.Interop.Entities;
using StaadPro.Interop.Models;
using StaadPro.Interop.Models.View;
using Native = PackageVerification.ViewFoundationFixture.Native;
using Root = PackageVerification.ViewFoundationFixture.Root;

namespace StaadPro.Interop.Tests.Adapter
{
    [TestFixture]
    public class OSViewEntityHidingTests
    {
        private static ViewOperationResult Request(IOSView view, string name, int id)
        {
            switch (name)
            {
                case "HidePlate": return view.HidePlate(new Plate { Id = id });
                case "HideSolid": return view.HideSolid(id);
                case "HideSurface": return view.HideSurface(id);
                default: throw new ArgumentOutOfRangeException(nameof(name));
            }
        }

        [TestCase("HidePlate", 1)]
        [TestCase("HidePlate", 17)]
        [TestCase("HidePlate", int.MaxValue)]
        [TestCase("HideSolid", 1)]
        [TestCase("HideSolid", 17)]
        [TestCase("HideSolid", int.MaxValue)]
        [TestCase("HideSurface", 1)]
        [TestCase("HideSurface", 17)]
        [TestCase("HideSurface", int.MaxValue)]
        public void PositiveIdsUseOnlyTheCorrectNativeRouteWithUnchangedInt32Values(string name, int id)
        {
            var native = new Native();
            using (var wrapper = new OpenStaadWrapper(new Root { View = native }))
            {
                var receipt = Request(wrapper.View, name, id);
                Assert.That(native.Calls, Is.EqualTo(new[] { name }));
                Assert.That(native.HiddenPlateIds, Is.EqualTo(name == "HidePlate" ? new[] { id } : new int[0]));
                Assert.That(native.HiddenSolidIds, Is.EqualTo(name == "HideSolid" ? new[] { id } : new int[0]));
                Assert.That(native.HiddenSurfaceIds, Is.EqualTo(name == "HideSurface" ? new[] { id } : new int[0]));
                Assert.That(native.HiddenMemberIds, Is.Empty); Assert.That(native.ShownMemberIds, Is.Empty);
                Assert.That(native.HiddenMemberBatches, Is.Empty); Assert.That(native.ShownMemberBatches, Is.Empty);
                Assert.That(native.RotationAngles, Is.Empty); Assert.That(receipt.NativeFunction, Is.EqualTo(name));
            }
        }

        [TestCase("HidePlate", 0, "plate")]
        [TestCase("HidePlate", -1, "plate")]
        [TestCase("HidePlate", int.MinValue, "plate")]
        [TestCase("HideSolid", 0, "solidId")]
        [TestCase("HideSolid", -1, "solidId")]
        [TestCase("HideSolid", int.MinValue, "solidId")]
        [TestCase("HideSurface", 0, "surfaceId")]
        [TestCase("HideSurface", -1, "surfaceId")]
        [TestCase("HideSurface", int.MinValue, "surfaceId")]
        public void NonpositiveIdentifiersFailBeforeNativeDispatchAndRetainPublicParameterNames(string name, int id, string parameter)
        {
            var native = new Native();
            using (var wrapper = new OpenStaadWrapper(new Root { View = native }))
            {
                Assert.That(Assert.Throws<ArgumentOutOfRangeException>(() => Request(wrapper.View, name, id)).ParamName, Is.EqualTo(parameter));
                Assert.That(native.Calls, Is.Empty);
                Assert.DoesNotThrow(() => Request(wrapper.View, name, 1));
                Assert.That(native.Calls.Count, Is.EqualTo(1));
            }
        }

        [Test]
        public void NullPlateFailsBeforeNativeAccess()
        {
            var native = new Native();
            using (var wrapper = new OpenStaadWrapper(new Root { View = native }))
            {
                Assert.That(Assert.Throws<ArgumentNullException>(() => wrapper.View.HidePlate(null)).ParamName, Is.EqualTo("plate"));
                Assert.That(native.Calls, Is.Empty);
            }
        }

        [Test]
        public void PlateNeedsOnlyItsIdAndIsNotModifiedByTheAdapter()
        {
            var plate = new Plate { Id = 17 };
            int notifications = 0; plate.PropertyChanged += (_, __) => notifications++;
            var native = new Native();
            using (var wrapper = new OpenStaadWrapper(new Root { View = native }))
            {
                wrapper.View.HidePlate(plate);
                Assert.That(plate.Id, Is.EqualTo(17));
                Assert.That(new[] { plate.A, plate.B, plate.C, plate.D }, Is.All.Null);
                Assert.That(notifications, Is.Zero); Assert.That(native.HiddenPlateIds, Is.EqualTo(new[] { 17 }));
            }
        }

        [Test]
        public void PlateIdentityIsCapturedBeforeNativeCallbackAndObservedFreshlyOnTheNextRequest()
        {
            var plate = new Plate { Id = 17 };
            var native = new Native { OnCall = () => plate.Id = 41 };
            using (var wrapper = new OpenStaadWrapper(new Root { View = native }))
            {
                wrapper.View.HidePlate(plate); native.OnCall = null; wrapper.View.HidePlate(plate);
                Assert.That(native.HiddenPlateIds, Is.EqualTo(new[] { 17, 41 }));
                Assert.That(native.Calls, Is.EqualTo(new[] { "HidePlate", "HidePlate" }));
            }
        }

        [TestCase("HidePlate")]
        [TestCase("HideSolid")]
        [TestCase("HideSurface")]
        public void RepeatedCallsAreSeparateRequestsAndDifferentWrappersRemainIsolated(string name)
        {
            var left = new Native(); var right = new Native();
            using (var first = new OpenStaadWrapper(new Root { View = left }))
            using (var second = new OpenStaadWrapper(new Root { View = right }))
            {
                var one = Request(first.View, name, 17);
                Request(second.View, name, 41);
                var two = Request(first.View, name, 17);
                Assert.That(left.Calls, Is.EqualTo(new[] { name, name })); Assert.That(right.Calls, Is.EqualTo(new[] { name }));
                Assert.That(one, Is.Not.SameAs(two));
                var leftIds = name == "HidePlate" ? left.HiddenPlateIds : name == "HideSolid" ? left.HiddenSolidIds : left.HiddenSurfaceIds;
                var rightIds = name == "HidePlate" ? right.HiddenPlateIds : name == "HideSolid" ? right.HiddenSolidIds : right.HiddenSurfaceIds;
                Assert.That(leftIds, Is.EqualTo(new[] { 17, 17 })); Assert.That(rightIds, Is.EqualTo(new[] { 41 }));
            }
        }

        [Test]
        public void EntityHidingUsesTheSamePerAdapterGateAsExistingViewRequests()
        {
            int active = 0, maximum = 0;
            var native = new Native { OnCall = () =>
            {
                maximum = Math.Max(maximum, Interlocked.Increment(ref active));
                Thread.Sleep(15); Interlocked.Decrement(ref active);
            } };
            using (var wrapper = new OpenStaadWrapper(new Root { View = native }))
            {
                // Managed stand-in only; real native COM calls still require the established caller apartment.
                var view = wrapper.View;
                Task.WaitAll(Task.Run(() => view.HidePlate(new Plate { Id = 17 })), Task.Run(() => view.HideSolid(41)),
                    Task.Run(() => view.HideSurface(63)), Task.Run(() => view.ShowFront()), Task.Run(() => view.HideMember(new Beam { Id = 79 })));
                Assert.That(maximum, Is.EqualTo(1));
                Assert.That(native.Calls, Is.EquivalentTo(new[] { "HidePlate", "HideSolid", "HideSurface", "ShowFront", "HideMember" }));
            }
        }
    }
}
