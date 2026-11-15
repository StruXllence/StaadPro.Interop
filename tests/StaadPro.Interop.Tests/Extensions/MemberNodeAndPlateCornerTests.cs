using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using StaadPro.Interop.Entities;
using StaadPro.Interop.Enums;
using StaadPro.Interop.Extensions;

namespace StaadPro.Interop.Tests.Extensions
{
    [TestFixture]
    public sealed class MemberNodeAndPlateCornerTests
    {
        private static Node N(int id, double x = 0, double y = 0, double z = 0) => new Node(id, x, y, z);
        private static Member M() => new Member(N(1), N(2, 10));

        [Test]
        public void NullArgumentsIdentifyThePublicParameter()
        {
            Assert.That(Assert.Throws<ArgumentNullException>(() => MemberExtensions.IsConnectedToNode(null, N(1))).ParamName, Is.EqualTo("member"));
            Assert.That(Assert.Throws<ArgumentNullException>(() => M().IsConnectedToNode(null)).ParamName, Is.EqualTo("node"));
            Assert.That(Assert.Throws<ArgumentNullException>(() => PlateExtensions.GetCornerLoop(null)).ParamName, Is.EqualTo("plate"));
        }

        [TestCase(0)] [TestCase(1)]
        public void MemberNodeScopeRecognizesEndpointIdentity(int scope)
        {
            var member = M();
            Assert.That(member.IsConnectedToNode(N(1), (MemberConnectivityScope)scope), Is.True);
            Assert.That(member.IsConnectedToNode(N(2, 10), (MemberConnectivityScope)scope), Is.True);
            Assert.That(member.IsConnectedToNode(N(3), (MemberConnectivityScope)scope), Is.False);
            Assert.That(member.IsConnectedToNode(N(1, 10), (MemberConnectivityScope)scope), Is.False);
        }

        [TestCase(0, 0, 0, true)] [TestCase(.001, 0, 0, true)]
        [TestCase(.00100001, 0, 0, false)] [TestCase(.0008, .0008, 0, false)]
        [TestCase(0, 0, .001, true)]
        public void MemberNodeUsesInclusiveThreeDimensionalEuclideanDistance(double x, double y, double z, bool expected)
        {
            foreach (var scope in new[] { MemberConnectivityScope.EndpointsOnly, MemberConnectivityScope.AnyNode })
                Assert.That(M().IsConnectedToNode(N(1, x, y, z), scope), Is.EqualTo(expected));
        }

        [Test]
        public void InteriorConnectionsRequireTheSuppliedNodeScopeAndCollectionMembership()
        {
            var member = M(); var interior = N(3, 5); member.Nodes.Add(interior);
            Assert.That(member.IsConnectedToNode(N(3, 5)), Is.False);
            Assert.That(member.IsConnectedToNode(N(3, 5), MemberConnectivityScope.AnyNode), Is.True);
            member.Nodes.Clear();
            Assert.That(member.IsConnectedToNode(N(1), MemberConnectivityScope.AnyNode), Is.False);
            Assert.That(member.IsConnectedToNode(N(1)), Is.True);
        }

        [Test]
        public void EachScopeIgnoresUnusedGeometryAndNeverHydratesIt()
        {
            var member = M(); member.Nodes = null; member.Beams = null;
            Assert.That(member.IsConnectedToNode(N(1)), Is.True);
            var detached = new Member { Nodes = new HashSet<Node> { N(3, 5) }, Beams = null };
            Assert.That(detached.IsConnectedToNode(N(3, 5), MemberConnectivityScope.AnyNode), Is.True);
            Assert.That(detached.StartNode, Is.Null);
        }

        [TestCase(-1)] [TestCase(2)] [TestCase(int.MaxValue)]
        public void UndefinedScopeThrowsBeforeReadingMalformedGeometry(int scope)
        {
            var member = new Member { Nodes = null };
            Assert.That(Assert.Throws<ArgumentOutOfRangeException>(() => member.IsConnectedToNode(N(1, double.NaN), (MemberConnectivityScope)scope)).ParamName, Is.EqualTo("scope"));
        }

        [Test]
        public void MissingSelectedIncidenceThrowsEvenAfterAnEndpointMatch()
        {
            Assert.That(Assert.Throws<ArgumentException>(() => new Member(N(1), null).IsConnectedToNode(N(1))).ParamName, Is.EqualTo("member"));
            var member = M(); member.Nodes = null;
            Assert.That(Assert.Throws<ArgumentException>(() => member.IsConnectedToNode(N(1), MemberConnectivityScope.AnyNode)).ParamName, Is.EqualTo("member"));
            member.Nodes = new HashSet<Node> { N(1), null };
            Assert.That(Assert.Throws<ArgumentException>(() => member.IsConnectedToNode(N(1), MemberConnectivityScope.AnyNode)).ParamName, Is.EqualTo("member"));
        }

        [TestCase(double.NaN)] [TestCase(double.PositiveInfinity)] [TestCase(double.NegativeInfinity)]
        public void NonfiniteSelectedAndTargetCoordinatesCannotHideBehindMatchesOrEmptySets(double invalid)
        {
            var member = M(); member.EndNode.Z = invalid;
            Assert.Throws<ArgumentException>(() => member.IsConnectedToNode(N(1)));
            member = M(); member.Nodes.Add(N(3, 5, invalid));
            Assert.Throws<ArgumentException>(() => member.IsConnectedToNode(N(1), MemberConnectivityScope.AnyNode));
            member.Nodes.Clear();
            Assert.That(Assert.Throws<ArgumentException>(() => member.IsConnectedToNode(N(1, invalid), MemberConnectivityScope.AnyNode)).ParamName, Is.EqualTo("node"));
        }

        [Test]
        public void AnyNodeObservesMutableHashKeysAndRepeatedDraftIdsByCurrentValues()
        {
            var first = N(17, 50); var second = N(18, 5);
            var member = new Member { Nodes = new HashSet<Node> { first, second } };
            first.Id = 0; second.Id = 0;
            Assert.That(member.IsConnectedToNode(N(0, 5), MemberConnectivityScope.AnyNode), Is.True);
            Assert.That(member.IsConnectedToNode(N(0, 20), MemberConnectivityScope.AnyNode), Is.False);
            second.Id = -2;
            Assert.That(member.IsConnectedToNode(N(-2, 5), MemberConnectivityScope.AnyNode), Is.True);
        }

        [Test]
        public void MemberNodeDistanceHandlesFiniteHugeAndSubnormalCoordinates()
        {
            var member = new Member(N(1, double.MaxValue), N(2));
            Assert.That(member.IsConnectedToNode(N(1, double.MaxValue)), Is.True);
            Assert.That(member.IsConnectedToNode(N(1, -double.MaxValue)), Is.False);
            Assert.That(M().IsConnectedToNode(N(1, double.Epsilon, double.Epsilon)), Is.True);
        }

        [Test]
        public void MemberNodeBooleanIsAnImmediateNonmutatingObservation()
        {
            var member = M(); var node = N(1); bool result = member.IsConnectedToNode(node);
            node.X = 10;
            Assert.That(result, Is.True);
            Assert.That(member.IsConnectedToNode(node), Is.False);
            Assert.That(member.StartNode.X, Is.Zero);
            Assert.That(member.Nodes.Count, Is.EqualTo(2));
        }

        [TestCase(false)] [TestCase(true)]
        public void TriangleOmitsAbsentOrEquivalentFourthCornerWithoutClosingTheLoop(bool repeat)
        {
            var a = N(1); var b = N(2, 1); var c = N(3, 0, 1);
            var loop = new Plate(a, b, c, repeat ? N(1) : null).GetCornerLoop();
            Assert.That(loop.Count, Is.EqualTo(3));
            Assert.That(loop[0], Is.SameAs(a)); Assert.That(loop[1], Is.SameAs(b)); Assert.That(loop[2], Is.SameAs(c));
        }

        [Test]
        public void QuadrilateralRetainsOriginalWindingWithoutSortingIdsOrCoordinates()
        {
            var nodes = new[] { N(4, 1, 1), N(1), N(3, 0, 1), N(2, 1) };
            var loop = new Plate(nodes[0], nodes[1], nodes[2], nodes[3]).GetCornerLoop();
            for (int i = 0; i < 4; i++) Assert.That(loop[i], Is.SameAs(nodes[i]));
        }

        [Test]
        public void SparseAndCollapsedPlatesRemainExtractionResultsRatherThanValidityClaims()
        {
            Assert.That(new Plate().GetCornerLoop(), Is.Empty);
            var node = N(0);
            var sparse = new Plate(null, node, null, N(0)).GetCornerLoop();
            Assert.That(sparse.Count, Is.EqualTo(1)); Assert.That(sparse[0], Is.SameAs(node));
            Assert.That(new Plate(node, node, node, node).GetCornerLoop().Count, Is.EqualTo(1));
            Assert.That(new Plate(N(1), N(2, 1), N(3, 2)).GetCornerLoop().Count, Is.EqualTo(3)); // collinear
        }

        [TestCase(.001, 0, 0, 1)] [TestCase(.00100001, 0, 0, 2)]
        [TestCase(.0008, .0008, 0, 2)] [TestCase(0, 0, .001, 1)]
        public void CornerDeduplicationUsesIdAndInclusiveThreeDimensionalDistance(double x, double y, double z, int count)
        {
            var a = N(1); var b = N(1, x, y, z);
            Assert.That(new Plate(a, b, null).GetCornerLoop().Count, Is.EqualTo(count));
            b.Id = 2;
            Assert.That(new Plate(a, b, null).GetCornerLoop().Count, Is.EqualTo(2));
        }

        [Test]
        public void NontransitiveEqualityComparesOnlyRetainedRepresentatives()
        {
            var a = N(1); var b = N(1, .00075); var c = N(1, .0015);
            var loop = new Plate(a, b, c, a).GetCornerLoop();
            Assert.That(loop.Count, Is.EqualTo(2));
            Assert.That(loop[0], Is.SameAs(a)); Assert.That(loop[1], Is.SameAs(c));
        }

        [TestCase(double.NaN)] [TestCase(double.PositiveInfinity)] [TestCase(double.NegativeInfinity)]
        public void InvalidPresentCornersThrowEvenWhenLastOrRepeated(double invalid)
        {
            var plate = new Plate(N(1), N(2, 1), N(3, 0, 1), N(1, 0, 0, invalid));
            Assert.That(Assert.Throws<ArgumentException>(() => plate.GetCornerLoop()).ParamName, Is.EqualTo("plate"));
            Assert.Throws<ArgumentException>(() => new Plate(null, N(1, invalid), null).GetCornerLoop());
        }

        [Test]
        public void CornerListOwnsReadOnlyMembershipButRetainsLiveNodeReferences()
        {
            var a = N(1); var b = N(2, 1); var plate = new Plate(a, b, N(3, 0, 1));
            var loop = plate.GetCornerLoop();
            Assert.Throws<NotSupportedException>(() => ((IList<Node>)loop).Clear());
            Assert.Throws<NotSupportedException>(() => ((IList<Node>)loop)[0] = b);
            plate.A = N(4, 5); a.X = 8; b.Id = a.Id; b.X = a.X;
            Assert.That(loop.Count, Is.EqualTo(3)); Assert.That(loop[0], Is.SameAs(a));
            Assert.That(loop[0].X, Is.EqualTo(8));
            Assert.That(plate.A.Id, Is.EqualTo(4));
            Assert.Throws<NotSupportedException>(() => ((IList<Node>)new Plate().GetCornerLoop()).Add(a));
        }

        [Test]
        public void CornerIdentityRetainsDraftIdsAndHandlesHugeAndSubnormalDistances()
        {
            var a = N(-1, double.MaxValue); var b = N(-1, -double.MaxValue);
            Assert.That(new Plate(a, b, N(-1, double.MaxValue)).GetCornerLoop().Count, Is.EqualTo(2));
            Assert.That(new Plate(N(0), N(0, double.Epsilon, double.Epsilon), null).GetCornerLoop().Count, Is.EqualTo(1));
        }
    }
}
