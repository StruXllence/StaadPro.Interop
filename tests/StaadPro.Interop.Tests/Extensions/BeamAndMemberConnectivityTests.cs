using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using NUnit.Framework;
using StaadPro.Interop.Entities;
using StaadPro.Interop.Enums;
using StaadPro.Interop.Extensions;

namespace StaadPro.Interop.Tests.Extensions
{
    [TestFixture]
    public sealed class BeamAndMemberConnectivityTests
    {
        private static Node N(int id, double x = 0, double y = 0, double z = 0) => new Node(id, x, y, z);
        private static Beam B(Node a, Node b) => new Beam(a, b);
        private static Member M(Node a, Node b) => new Member(a, b);

        [Test]
        public void NullPublicArgumentsIdentifyTheirParameter()
        {
            var joint = N(1);
            Assert.That(Assert.Throws<ArgumentNullException>(() => BeamExtensions.IsConnectedToNode(null, joint)).ParamName, Is.EqualTo("beam"));
            Assert.That(Assert.Throws<ArgumentNullException>(() => B(joint, N(2, 2)).IsConnectedToNode(null)).ParamName, Is.EqualTo("node"));
            Assert.That(Assert.Throws<ArgumentNullException>(() => MemberExtensions.IsConnectedToMember(null, new Member())).ParamName, Is.EqualTo("member"));
            Assert.That(Assert.Throws<ArgumentNullException>(() => new Member().IsConnectedToMember(null)).ParamName, Is.EqualTo("targetMember"));
        }

        [TestCase(false)] [TestCase(true)]
        public void BeamNodeMatchesEitherEndpointAndUsesNodeIdentity(bool end)
        {
            var joint = N(7, 1, 2, 3);
            var beam = end ? B(N(1, 100), joint) : B(joint, N(1, 100));
            Assert.That(beam.IsConnectedToNode(N(7, 1, 2, 3)), Is.True);
            Assert.That(beam.IsConnectedToNode(N(8, 1, 2, 3)), Is.False);
            Assert.That(beam.IsConnectedToNode(N(7, 20)), Is.False);
        }

        [TestCase(0, 0, true)] [TestCase(.001, 0, true)]
        [TestCase(.00100001, 0, false)] [TestCase(.0008, .0008, false)]
        public void BothHelpersUseInclusiveThreeDimensionalEuclideanTolerance(double dx, double dy, bool expected)
        {
            var node = N(1);
            var other = N(1, dx, dy);
            Assert.That(B(node, N(2, 100)).IsConnectedToNode(other), Is.EqualTo(expected));
            var first = M(node, N(2, 100));
            var second = M(other, N(3, -100));
            Assert.That(first.IsConnectedToMember(second), Is.EqualTo(expected));
            Assert.That(second.IsConnectedToMember(first), Is.EqualTo(expected));
            Assert.That(first.IsConnectedToMember(second, MemberConnectivityScope.AnyNode), Is.EqualTo(expected));
        }

        [Test]
        public void InteriorBeamPointDoesNotBecomeAnEndpointConnection()
        {
            var beam = B(N(1, -10), N(2, 10));
            Assert.That(beam.IsConnectedToNode(N(1, 0)), Is.False);
            Assert.That(beam.IsConnectedToNode(N(3, 0)), Is.False);
        }

        [TestCase(false, false)] [TestCase(false, true)] [TestCase(true, false)] [TestCase(true, true)]
        public void MemberEndpointPairsAreSymmetricAndIndependentOfMemberIds(bool leftEnd, bool rightEnd)
        {
            var left = leftEnd ? M(N(1, 10), N(7, 1)) : M(N(7, 1), N(1, 10));
            var right = rightEnd ? M(N(2, -10), N(7, 1)) : M(N(7, 1), N(2, -10));
            Assert.That(left.IsConnectedToMember(right), Is.True);
            Assert.That(right.IsConnectedToMember(left), Is.True);
            Assert.That(left.IsConnectedToMember(left), Is.True);
        }

        [Test]
        public void AnyNodeScopeRecognizesSuppliedInteriorTJunctionsOnEitherSide()
        {
            var middle = N(7, 5);
            var longMember = M(N(1, 0), N(2, 10));
            longMember.Nodes.Add(middle);
            var spur = M(N(7, 5), N(3, 5, 4));
            Assert.That(longMember.IsConnectedToMember(spur), Is.False);
            Assert.That(longMember.IsConnectedToMember(spur, MemberConnectivityScope.AnyNode), Is.True);
            Assert.That(spur.IsConnectedToMember(longMember, MemberConnectivityScope.AnyNode), Is.True);
        }

        [Test]
        public void AnyNodeUsesExactlyNodesWithoutAddingEndpointsOrConstituentBeams()
        {
            var joint = N(7, 5);
            var left = M(N(1), joint);
            var right = M(N(7, 5), N(3, 9));
            left.Nodes = new HashSet<Node> { N(99, 100) };
            left.Beams.Add(B(N(1), joint));
            Assert.That(left.IsConnectedToMember(right), Is.True);
            Assert.That(left.IsConnectedToMember(right, MemberConnectivityScope.AnyNode), Is.False);
            var suppliedOnly = new Member { Nodes = new HashSet<Node> { N(7, 5) } };
            Assert.That(suppliedOnly.IsConnectedToMember(right, MemberConnectivityScope.AnyNode), Is.True);
        }

        [Test]
        public void EndpointScopeIgnoresUnusedNodeAndBeamCollections()
        {
            var left = M(N(1), N(7, 5));
            var right = M(N(7, 5), N(3, 9));
            left.Nodes = null;
            right.Nodes = new HashSet<Node> { null, N(99, double.NaN) };
            left.Beams = null;
            Assert.That(left.IsConnectedToMember(right), Is.True);
        }

        [Test]
        public void AnyNodeScopeIgnoresUnusedEndpointGeometry()
        {
            var left = M(N(1, double.NaN), N(2, double.PositiveInfinity));
            left.Nodes = new HashSet<Node> { N(7, 5) };
            var right = new Member { Nodes = new HashSet<Node> { N(7, 5) } };
            Assert.That(left.IsConnectedToMember(right, MemberConnectivityScope.AnyNode), Is.True);
        }

        [TestCase(MemberConnectivityScope.EndpointsOnly)] [TestCase(MemberConnectivityScope.AnyNode)]
        public void AllSelectedGeometryIsValidatedBeforeAnEarlyMatch(MemberConnectivityScope scope)
        {
            var left = M(N(7), N(2, 2));
            var right = M(N(7), N(3, 3));
            if (scope == MemberConnectivityScope.EndpointsOnly) right.EndNode.Z = double.NaN;
            else right.Nodes.Add(N(99, double.NaN));
            Assert.That(Assert.Throws<ArgumentException>(() => left.IsConnectedToMember(right, scope)).ParamName, Is.EqualTo("targetMember"));
            Assert.Throws<ArgumentException>(() => right.IsConnectedToMember(left, scope));
        }

        [TestCase(false)] [TestCase(true)]
        public void BeamRequiresBothEndpointsEvenWhenTheOtherEndpointMatches(bool missingStart)
        {
            var joint = N(1);
            var beam = missingStart ? B(null, joint) : B(joint, null);
            Assert.That(Assert.Throws<ArgumentException>(() => beam.IsConnectedToNode(joint)).ParamName, Is.EqualTo("beam"));
        }

        [TestCase(double.NaN)] [TestCase(double.PositiveInfinity)] [TestCase(double.NegativeInfinity)]
        public void NonfiniteParticipatingCoordinatesAreRejectedOnEveryAxis(double value)
        {
            for (int axis = 0; axis < 3; axis++)
            {
                var bad = N(1, axis == 0 ? value : 0, axis == 1 ? value : 0, axis == 2 ? value : 0);
                var good = N(1);
                Assert.That(Assert.Throws<ArgumentException>(() => B(good, N(2, 3)).IsConnectedToNode(bad)).ParamName, Is.EqualTo("node"));
                Assert.Throws<ArgumentException>(() => B(good, bad).IsConnectedToNode(good));
                Assert.Throws<ArgumentException>(() => M(good, N(2, 3)).IsConnectedToMember(M(good, bad)));
                var right = new Member { Nodes = new HashSet<Node> { good, bad } };
                Assert.Throws<ArgumentException>(() => new Member().IsConnectedToMember(right, MemberConnectivityScope.AnyNode));
            }
        }

        [Test]
        public void MissingSelectedMemberIncidenceAndCollectionsIdentifyTheMember()
        {
            var valid = M(N(1), N(2, 2));
            Assert.That(Assert.Throws<ArgumentException>(() => new Member().IsConnectedToMember(valid)).ParamName, Is.EqualTo("member"));
            Assert.That(Assert.Throws<ArgumentException>(() => valid.IsConnectedToMember(new Member())).ParamName, Is.EqualTo("targetMember"));
            var missing = new Member { Nodes = null };
            Assert.That(Assert.Throws<ArgumentException>(() => missing.IsConnectedToMember(valid, MemberConnectivityScope.AnyNode)).ParamName, Is.EqualTo("member"));
            Assert.That(Assert.Throws<ArgumentException>(() => valid.IsConnectedToMember(missing, MemberConnectivityScope.AnyNode)).ParamName, Is.EqualTo("targetMember"));
        }

        [Test]
        public void EmptyAnyNodeCollectionsReturnFalseButDoNotHideInvalidOtherSelection()
        {
            var empty = new Member();
            var valid = M(N(1), N(2, 2));
            Assert.That(empty.IsConnectedToMember(valid, MemberConnectivityScope.AnyNode), Is.False);
            Assert.That(valid.IsConnectedToMember(empty, MemberConnectivityScope.AnyNode), Is.False);
            Assert.That(empty.IsConnectedToMember(empty, MemberConnectivityScope.AnyNode), Is.False);
            var invalid = new Member { Nodes = new HashSet<Node> { null } };
            Assert.Throws<ArgumentException>(() => empty.IsConnectedToMember(invalid, MemberConnectivityScope.AnyNode));
            Assert.Throws<ArgumentException>(() => invalid.IsConnectedToMember(empty, MemberConnectivityScope.AnyNode));
        }

        [TestCase(-1)] [TestCase(2)] [TestCase(int.MaxValue)]
        public void InvalidScopeRejectsBeforeReadingMalformedGeometry(int scope)
        {
            Assert.That(Assert.Throws<ArgumentOutOfRangeException>(() => new Member().IsConnectedToMember(new Member(), (MemberConnectivityScope)scope)).ParamName, Is.EqualTo("scope"));
        }

        [Test]
        public void MutatedHashKeysAndRepeatedIdsUseCurrentSnapshotValues()
        {
            var interior = N(7, 5);
            var left = new Member { Nodes = new HashSet<Node> { interior } };
            interior.Id = 77;
            var right = new Member { Nodes = new HashSet<Node> { N(77, 5), N(77, 100) } };
            Assert.That(left.IsConnectedToMember(right, MemberConnectivityScope.AnyNode), Is.True);
            interior.X = 60;
            Assert.That(left.IsConnectedToMember(right, MemberConnectivityScope.AnyNode), Is.False);
            interior.X = 100;
            Assert.That(left.IsConnectedToMember(right, MemberConnectivityScope.AnyNode), Is.True);
        }

        [TestCase(0)] [TestCase(-7)]
        public void DraftIdsRetainDetachedEqualitySemantics(int id)
        {
            var joint = N(id);
            Assert.That(B(joint, N(2, 2)).IsConnectedToNode(N(id)), Is.True);
            Assert.That(M(joint, N(2, 2)).IsConnectedToMember(M(N(id), N(3, 3))), Is.True);
        }

        [Test]
        public void HugeDifferencesAndSubnormalDeltasRespectTheExistingTolerance()
        {
            var huge = N(1, double.MaxValue);
            var far = N(1, -double.MaxValue);
            Assert.That(B(huge, N(2)).IsConnectedToNode(far), Is.False);
            Assert.That(M(huge, N(2)).IsConnectedToMember(M(far, N(3))), Is.False);
            var tiny = N(1, double.Epsilon, double.Epsilon);
            Assert.That(B(N(1), N(2, 2)).IsConnectedToNode(tiny), Is.True);
            Assert.That(M(N(1), N(2, 2)).IsConnectedToMember(M(tiny, N(3, 3))), Is.True);
        }

        [Test]
        public void PublicSignaturesDefaultsAndCompiledXmlAreComplete()
        {
            var beam = typeof(BeamExtensions).GetMethod("IsConnectedToNode");
            Assert.That(beam.ReturnType, Is.EqualTo(typeof(bool)));
            Assert.That(beam.GetParameters().Select(p => p.Name), Is.EqualTo(new[]{"beam","node"}));
            Assert.That(beam.GetParameters().Select(p => p.ParameterType), Is.EqualTo(new[]{typeof(Beam),typeof(Node)}));
            Assert.That(beam.GetParameters().Any(p => p.IsOptional), Is.False);
            var member = typeof(MemberExtensions).GetMethod("IsConnectedToMember");
            Assert.That(member.ReturnType, Is.EqualTo(typeof(bool)));
            Assert.That(member.GetParameters().Select(p => p.Name), Is.EqualTo(new[]{"member","targetMember","scope"}));
            Assert.That(member.GetParameters().Select(p => p.ParameterType), Is.EqualTo(new[]{typeof(Member),typeof(Member),typeof(MemberConnectivityScope)}));
            Assert.That(member.GetParameters()[2].IsOptional, Is.True);
            Assert.That(member.GetParameters()[2].DefaultValue, Is.EqualTo(MemberConnectivityScope.EndpointsOnly));
            var docs = XDocument.Load(Path.ChangeExtension(typeof(Beam).Assembly.Location, ".xml"));
            foreach (var method in new[]{beam,member})
            {
                var prefix = "M:" + method.DeclaringType.FullName + "." + method.Name + "(";
                var doc = docs.Descendants("member").Single(e => ((string)e.Attribute("name")).StartsWith(prefix));
                foreach (string tag in new[]{"summary","returns","remarks","exception","example"}) Assert.That(doc.Element(tag), Is.Not.Null);
                Assert.That(doc.Elements("param").Select(e => (string)e.Attribute("name")), Is.EqualTo(method.GetParameters().Select(p => p.Name)));
                Assert.That(doc.Descendants().Any(e => e.Name == "include" || e.Name == "inheritdoc"), Is.False);
            }
        }
    }
}
