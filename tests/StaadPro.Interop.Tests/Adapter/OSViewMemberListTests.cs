using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using StaadPro.Interop.Entities;
using StaadPro.Interop.Models;
using StaadPro.Interop.Models.View;
using Native = PackageVerification.ViewFoundationFixture.Native;
using Root = PackageVerification.ViewFoundationFixture.Root;

namespace StaadPro.Interop.Tests.Adapter
{
    [TestFixture]
    public class OSViewMemberListTests
    {
        private static ViewOperationResult Request(OpenStaadWrapper wrapper, bool show, IEnumerable<Beam> beams)
            => show ? wrapper.View.ShowMembers(beams) : wrapper.View.HideMembers(beams);

        [TestCase(true)]
        [TestCase(false)]
        public void CollectionsAreEnumeratedOnceAndPreserveOrderDuplicatesAndOwnedIds(bool show)
        {
            var beams = new[] { new Beam { Id = int.MaxValue }, new Beam { Id = 1 }, new Beam { Id = 1 } };
            var source = new Once(beams);
            var native = new Native { OnCall = () => { beams[0].Id = 23; beams[1] = new Beam { Id = 42 }; } };
            using (var wrapper = new OpenStaadWrapper(new Root { View = native }))
            {
                Request(wrapper, show, source);
                var batch = (show ? native.ShownMemberBatches : native.HiddenMemberBatches).Single();
                Assert.That(batch, Is.EqualTo(new[] { int.MaxValue, 1, 1 }));
                Assert.That(source.Enumerations, Is.EqualTo(1));
                Assert.That(source.Disposals, Is.EqualTo(1));
                Assert.That(native.Calls, Is.EqualTo(new[] { show ? "ShowMembers" : "HideMembers" }));
                Assert.That(native.ShownMemberIds, Is.Empty);
                Assert.That(native.HiddenMemberIds, Is.Empty);
            }
        }

        [TestCase(true)]
        [TestCase(false)]
        public void EmptySequencesForwardAnExactZeroCountArrayPair(bool show)
        {
            var native = new Native();
            using (var wrapper = new OpenStaadWrapper(new Root { View = native }))
            {
                Request(wrapper, show, new Beam[0]);
                Assert.That((show ? native.ShownMemberBatches : native.HiddenMemberBatches).Single(), Is.Empty);
                Assert.That(native.Calls.Count, Is.EqualTo(1));
            }
        }

        [TestCase(true)]
        [TestCase(false)]
        public void NullSequencesAndElementsRejectBeforeDispatch(bool show)
        {
            var native = new Native();
            using (var wrapper = new OpenStaadWrapper(new Root { View = native }))
            {
                Assert.That(Assert.Throws<ArgumentNullException>(() => Request(wrapper, show, null)).ParamName, Is.EqualTo("beams"));
                var source = new Once(new[] { new Beam { Id = 17 }, null });
                Assert.That(Assert.Throws<ArgumentException>(() => Request(wrapper, show, source)).ParamName, Is.EqualTo("beams"));
                Assert.That(source.Disposals, Is.EqualTo(1));
                Assert.That(native.Calls, Is.Empty);
            }
        }

        [TestCase(true, 0)]
        [TestCase(true, -1)]
        [TestCase(true, int.MinValue)]
        [TestCase(false, 0)]
        [TestCase(false, -1)]
        [TestCase(false, int.MinValue)]
        public void InvalidIdsDoNotPartiallyDispatch(bool show, int id)
        {
            var native = new Native();
            using (var wrapper = new OpenStaadWrapper(new Root { View = native }))
            {
                var source = new Once(new[] { new Beam { Id = 17 }, new Beam { Id = id } });
                Assert.That(Assert.Throws<ArgumentOutOfRangeException>(() => Request(wrapper, show, source)).ParamName, Is.EqualTo("beams"));
                Assert.That(source.Disposals, Is.EqualTo(1));
                Assert.That(native.Calls, Is.Empty);
            }
        }

        [TestCase(true)]
        [TestCase(false)]
        public void EnumeratorFailuresPropagateWithoutPartialNativeCalls(bool show)
        {
            var error = new InvalidOperationException("enumeration failed");
            var native = new Native();
            using (var wrapper = new OpenStaadWrapper(new Root { View = native }))
            {
                Assert.That(Assert.Throws<InvalidOperationException>(() => Request(wrapper, show, Fail(error))), Is.SameAs(error));
                Assert.That(native.Calls, Is.Empty);
            }
        }

        [TestCase(true)]
        [TestCase(false)]
        public void ReplacementDuringEnumerationRejectsTheCapturedAdapter(bool show)
        {
            var native = new Native();
            var root = new Root { View = native };
            using (var wrapper = new OpenStaadWrapper(root))
            {
                var view = wrapper.View;
                var replacement = new Native();
                var sequence = Replace(() => { root.View = replacement; wrapper.RefreshComObjects(); });
                Assert.Throws<InvalidOperationException>(() => { if (show) view.ShowMembers(sequence); else view.HideMembers(sequence); });
                Assert.That(native.Calls, Is.Empty);
                Assert.That(replacement.Calls, Is.Empty);
            }
        }

        [TestCase(1)]
        [TestCase(17)]
        [TestCase(int.MaxValue)]
        public void HideMemberCapturesOnlyTheIdBeforeNativeCallback(int id)
        {
            var beam = new Beam { Id = id };
            var native = new Native { OnCall = () => beam.Id = 42 };
            using (var wrapper = new OpenStaadWrapper(new Root { View = native }))
            {
                wrapper.View.HideMember(beam);
                Assert.That(native.HiddenMemberIds, Is.EqualTo(new[] { id }));
                Assert.That(beam.StartNode, Is.Null);
                Assert.That(beam.EndNode, Is.Null);
            }
        }

        [Test]
        public void HideMemberRejectsNullAndNonpositiveIdsBeforeDispatch()
        {
            var native = new Native();
            using (var wrapper = new OpenStaadWrapper(new Root { View = native }))
            {
                Assert.That(Assert.Throws<ArgumentNullException>(() => wrapper.View.HideMember(null)).ParamName, Is.EqualTo("beam"));
                foreach (int id in new[] { 0, -1, int.MinValue })
                    Assert.That(Assert.Throws<ArgumentOutOfRangeException>(() => wrapper.View.HideMember(new Beam { Id = id })).ParamName, Is.EqualTo("beam"));
                Assert.That(native.Calls, Is.Empty);
            }
        }

        [Test]
        public void ListAndSingleRequestsShareTheExistingViewGate()
        {
            int active = 0, maximum = 0;
            var native = new Native { OnCall = () => {
                maximum = Math.Max(maximum, Interlocked.Increment(ref active));
                Thread.Sleep(15);
                Interlocked.Decrement(ref active);
            } };
            using (var wrapper = new OpenStaadWrapper(new Root { View = native }))
            {
                var view = wrapper.View;
                var beam = new Beam { Id = 17 };
                // Managed stand-in only: native COM calls still require the established caller apartment.
                Task.WaitAll(Task.Run(() => view.ShowMembers(new[] { beam })),
                    Task.Run(() => view.HideMember(beam)), Task.Run(() => view.HideMembers(new[] { beam })),
                    Task.Run(() => view.ShowFront()));
                Assert.That(maximum, Is.EqualTo(1));
                Assert.That(native.Calls, Is.EquivalentTo(new[] { "ShowMembers", "HideMember", "HideMembers", "ShowFront" }));
            }
        }

        private static IEnumerable<Beam> Fail(Exception error)
        { yield return new Beam { Id = 17 }; throw error; }

        private static IEnumerable<Beam> Replace(Action replace)
        { replace(); yield return new Beam { Id = 17 }; }

        private sealed class Once : IEnumerable<Beam>
        {
            private readonly IEnumerable<Beam> beams;
            internal int Enumerations, Disposals;
            internal Once(IEnumerable<Beam> beams) { this.beams = beams; }
            public IEnumerator<Beam> GetEnumerator()
            {
                if (++Enumerations != 1) throw new InvalidOperationException("Enumerated twice.");
                try { foreach (var beam in beams) yield return beam; }
                finally { Disposals++; }
            }
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
    }
}
