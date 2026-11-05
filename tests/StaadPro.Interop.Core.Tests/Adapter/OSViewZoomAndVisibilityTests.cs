using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using StaadPro.Interop.Core.Entities;
using StaadPro.Interop.Core.Models;
using Native = PackageVerification.ViewFoundationFixture.Native;
using Root = PackageVerification.ViewFoundationFixture.Root;

namespace StaadPro.Interop.Core.Tests.Adapter
{
    [TestFixture]
    public class OSViewZoomAndVisibilityTests
    {
        [TestCase(1)]
        [TestCase(17)]
        [TestCase(int.MaxValue)]
        public void ShowMemberUsesOnlyThePositiveAnalyticalIdWithoutRequiringGeometry(int memberId)
        {
            var beam = new Beam { Id = memberId };
            var native = new Native();
            using (var wrapper = new OpenStaadWrapper(new Root { View = native }))
            {
                var result = wrapper.View.ShowMember(beam);
                Assert.That(native.Calls, Is.EqualTo(new[] { "ShowMember" }));
                Assert.That(native.ShownMemberIds, Is.EqualTo(new[] { memberId }));
                Assert.That(result.NativeFunction, Is.EqualTo("ShowMember"));
                Assert.That(beam.Id, Is.EqualTo(memberId));
                Assert.That(beam.StartNode, Is.Null);
                Assert.That(beam.EndNode, Is.Null);
                Assert.That(native.RotationAngles, Is.Empty);
            }
        }

        [Test]
        public void ShowMemberRejectsNullWithoutNativeAccess()
        {
            var native = new Native();
            using (var wrapper = new OpenStaadWrapper(new Root { View = native }))
            {
                var error = Assert.Throws<ArgumentNullException>(() => wrapper.View.ShowMember(null));
                Assert.That(error.ParamName, Is.EqualTo("beam"));
                Assert.That(native.Calls, Is.Empty);
            }
        }

        [TestCase(0)]
        [TestCase(-1)]
        [TestCase(int.MinValue)]
        public void ShowMemberRejectsNonpositiveIdsWithoutNativeAccess(int memberId)
        {
            var native = new Native();
            using (var wrapper = new OpenStaadWrapper(new Root { View = native }))
            {
                var error = Assert.Throws<ArgumentOutOfRangeException>(() => wrapper.View.ShowMember(new Beam { Id = memberId }));
                Assert.That(error.ParamName, Is.EqualTo("beam"));
                Assert.That(native.Calls, Is.Empty);
                Assert.That(native.ShownMemberIds, Is.Empty);
            }
        }

        [Test]
        public void ShowMemberSendsTheCapturedIdDespiteLaterChangesToTheCallerObject()
        {
            var beam = new Beam { Id = 17 };
            var native = new Native { OnCall = () => beam.Id = 41 };
            using (var wrapper = new OpenStaadWrapper(new Root { View = native }))
            {
                wrapper.View.ShowMember(beam);
                Assert.That(native.ShownMemberIds, Is.EqualTo(new[] { 17 }));
                native.OnCall = null;
                wrapper.View.ShowMember(beam);
                Assert.That(native.ShownMemberIds, Is.EqualTo(new[] { 17, 41 }));
                Assert.That(native.Calls, Is.EqualTo(new[] { "ShowMember", "ShowMember" }));
            }
        }

        [Test]
        public void ZoomAndVisibilityRequestsRemainSeparateAndDoNotInventRestoration()
        {
            var native = new Native();
            using (var wrapper = new OpenStaadWrapper(new Root { View = native }))
            {
                var view = wrapper.View;
                view.HideAllMembers();
                view.ZoomAll();
                view.ShowMember(new Beam { Id = 17 });
                view.ZoomExtentsMainView();
                view.ShowAllMembers();
                view.HideAllMembers();
                Assert.That(native.Calls, Is.EqualTo(new[]
                {
                    "HideAllMembers", "ZoomAll", "ShowMember", "ZoomExtentsMainView", "ShowAllMembers", "HideAllMembers"
                }));
                Assert.That(native.ShownMemberIds, Is.EqualTo(new[] { 17 }));
                Assert.That(native.RotationAngles, Is.Empty);
            }
        }

        [Test]
        public void ZoomVisibilityAndExistingRotationsShareTheSameAdapterGate()
        {
            int active = 0, maximum = 0;
            var native = new Native();
            native.OnCall = () =>
            {
                int current = Interlocked.Increment(ref active);
                maximum = Math.Max(maximum, current);
                Thread.Sleep(15);
                Interlocked.Decrement(ref active);
            };
            using (var wrapper = new OpenStaadWrapper(new Root { View = native }))
            {
                // Managed stand-in only: this test does not authorize worker-thread COM access.
                var view = wrapper.View;
                var beam = new Beam { Id = 17 };
                Action[] requests =
                {
                    () => view.ZoomAll(), () => view.ZoomExtentsMainView(),
                    () => view.ShowAllMembers(), () => view.HideAllMembers(),
                    () => view.ShowMember(beam), () => view.RotateUp(15.0)
                };
                Task.WaitAll(requests.Select(request => Task.Run(request)).ToArray());
                Assert.That(maximum, Is.EqualTo(1));
                Assert.That(native.Calls, Is.EquivalentTo(new[]
                {
                    "ZoomAll", "ZoomExtentsMainView", "ShowAllMembers", "HideAllMembers", "ShowMember", "RotateUp"
                }));
            }
        }
    }
}
