using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using NUnit.Framework;
using StaadPro.Interop.Common;
using StaadPro.Interop.Entities;
using StaadPro.Interop.Extensions;

namespace StaadPro.Interop.Tests.Extensions
{
    [TestFixture]
    public sealed class CoincidenceAndXRangeTests
    {
        private static Node N(int id = 1, double x = 0, double y = 0, double z = 0) => new Node(id, x, y, z);
        private static Beam B(int id, double start, double end) => new Beam(id, N(1, start), N(2, end));
        private static Plate P(int id, double a, double b, double c, double? d = null) => new Plate(id, N(1, a), N(2, b), N(3, c), d.HasValue ? N(4, d.Value) : null);
        private sealed class Once<T> : IEnumerable<T>
        {
            private readonly IEnumerable<T> values;
            internal int Starts, Disposals;
            internal Once(IEnumerable<T> values) { this.values = values; }
            public IEnumerator<T> GetEnumerator()
            {
                if (++Starts != 1) throw new ApplicationException("Re-enumerated");
                try { foreach (T value in values) yield return value; } finally { Disposals++; }
            }
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
        [Test]
        public void CoincidenceUsesInclusiveAxisBoxWithoutIdsAndKeepsNullFalsePrecedence()
        {
            var a = N(); var b = N(99, 0.0001, -0.0001, 0.0001);
            Assert.That(a.HasSameCoordinatesAs(b), Is.True); Assert.That(b.HasSameCoordinatesAs(a), Is.True);
            Assert.That(Node.DistanceBetweenNodes(a, b), Is.GreaterThan(0.0001)); Assert.That(a.Equals(b), Is.False);
            Assert.That(a.HasSameCoordinatesAs(a), Is.True); Assert.That(((Node)null).HasSameCoordinatesAs(null), Is.False);
            Assert.That(((Node)null).HasSameCoordinatesAs(N(1, double.NaN)), Is.False);
            Assert.That(N(1, double.NaN).HasSameCoordinatesAs(null), Is.False);
        }
        [TestCase(0)] [TestCase(1)] [TestCase(2)]
        public void CoincidenceRejectsTheNextRepresentableValueBeyondEitherAxisBoundary(int axis)
        {
            double outside = BitConverter.Int64BitsToDouble(BitConverter.DoubleToInt64Bits(0.0001) + 1);
            foreach (double sign in new[] { -1d, 1d })
            {
                var v = new double[3]; v[axis] = sign * outside;
                Assert.That(N().HasSameCoordinatesAs(N(2, v[0], v[1], v[2])), Is.False);
            }
        }
        private static IEnumerable<TestCaseData> InvalidPairCoordinates()
        {
            foreach (bool left in new[] { true, false }) foreach (int axis in new[] { 0, 1, 2 })
                foreach (double v in new[] { double.NaN, double.PositiveInfinity, double.NegativeInfinity }) yield return new TestCaseData(left, axis, v);
        }
        [TestCaseSource(nameof(InvalidPairCoordinates))]
        public void CoincidenceValidatesAllCoordinatesOnNonNullInputs(bool left, int axis, double value)
        {
            var v = new double[3]; v[axis] = value; var bad = N(1, v[0], v[1], v[2]);
            Assert.That(Assert.Throws<ArgumentException>(() => (left ? bad : N()).HasSameCoordinatesAs(left ? N() : bad)).ParamName, Is.EqualTo(left ? "left" : "right"));
            Assert.Throws<ArgumentException>(() => bad.HasSameCoordinatesAs(bad));
        }
        [Test]
        public void CoincidenceHandlesZeroSubnormalsAndHugeFiniteDataWithoutMutation()
        {
            var huge = N(-1, double.MaxValue, -double.MaxValue, -0d);
            Assert.That(huge.HasSameCoordinatesAs(N(2, double.MaxValue, -double.MaxValue, double.Epsilon)), Is.True);
            Assert.That(N(1, double.MaxValue).HasSameCoordinatesAs(N(1, -double.MaxValue)), Is.False);
            Assert.That(N(0, -0d, double.Epsilon, -double.Epsilon).HasSameCoordinatesAs(N()), Is.True);
            Assert.That(huge.Id, Is.EqualTo(-1)); Assert.That(huge.X, Is.EqualTo(double.MaxValue));
        }
        [Test]
        public void XRangeUsesWholeClosedExtentsInsteadOfMidpointsOrIntersection()
        {
            var accepted = B(1, 0, 2); var reversed = B(2, 2, 0); var crossing = B(3, -1, 3); var touching = B(4, -1, 0);
            var beams = new[] { accepted, reversed, crossing, touching }.InXRange(0, 2);
            Assert.That(beams.Count, Is.EqualTo(2)); Assert.That(beams.Any(b => ReferenceEquals(b, accepted)), Is.True);
            Assert.That(beams.Any(b => ReferenceEquals(b, reversed)), Is.True);
            var triangle = P(1, 0, 1, 2); var quad = P(2, 2, 0, 1, 2); var escapes = P(3, 0, 1, 2, 3);
            Assert.That(new[] { triangle, quad, escapes }.InXRange(0, 2).Count, Is.EqualTo(2));
            var zeroWidth = new[] { B(1, 1, 1), B(2, 0, 1) }.InXRange(1, 1);
            Assert.That(zeroWidth.Single().Id, Is.EqualTo(1));
        }
        [Test]
        public void BeamAndPlateSetsRetainFirstQualifyingReferenceByIdDespiteGlobalGeometryEquality()
        {
            var a = B(7, 0, 1); var b = B(7, 1, 2); Assert.That(a.Equals(b), Is.False);
            var set = new[] { B(7, -2, -1), a, b, a }.InXRange(0, 2);
            Assert.That(set.Count, Is.EqualTo(1)); Assert.That(set.Single(), Is.SameAs(a)); Assert.That(set.Contains(b), Is.True);
            var p = P(7, 0, 1, 2); var duplicate = P(7, 1, 1, 1); Assert.That(p.Equals(duplicate), Is.False);
            Assert.That(new[] { p, duplicate }.InXRange(0, 2).Single(), Is.SameAs(p));
            foreach (int id in new[] { 0, -1, int.MinValue, int.MaxValue }) Assert.That(new[] { B(id, 0, 1), B(id, 1, 2) }.InXRange(0, 2).Count, Is.EqualTo(1));
        }
        [Test]
        public void NodeSetUsesSourceIdAndOneMillionthDistanceRatherThanGlobalToleranceOrCoincidence()
        {
            var a = N(1); var near = N(1, 0.0000005); var separate = N(1, 0.0005); var anotherId = N(2);
            Assert.That(a.Equals(separate), Is.True);
            var set = new[] { a, near, separate, anotherId }.InXRange(-1, 1);
            Assert.That(set.Count, Is.EqualTo(3)); Assert.That(set.Any(n => ReferenceEquals(n, a)), Is.True);
            Assert.That(set.Any(n => ReferenceEquals(n, near)), Is.False); Assert.That(set.Comparer.Equals(a, separate), Is.False);
            Assert.That(set.Comparer.Equals(a, N(1, 0.000001)), Is.True);
            Assert.That(set.Comparer.Equals(a, N(1, 0.0000007, 0.0000007, 0.0000007)), Is.False);
            Assert.That(set.Comparer.Equals(a, anotherId), Is.False);
            Assert.That(new[] { N(1, double.MaxValue), N(1, -double.MaxValue) }.InXRange(double.NegativeInfinity, double.PositiveInfinity).Count, Is.EqualTo(2));
        }
        [Test]
        public void RangeAllowsOpenEndedInfinitiesAndFiniteExtremeGeometry()
        {
            var nodes = new[] { N(1, -double.MaxValue), N(2, 0), N(3, double.MaxValue) };
            Assert.That(nodes.InXRange(double.NegativeInfinity, double.PositiveInfinity).Count, Is.EqualTo(3));
            Assert.That(nodes.InXRange(0, double.PositiveInfinity).Count, Is.EqualTo(2));
            Assert.That(nodes.InXRange(double.NegativeInfinity, 0).Count, Is.EqualTo(2));
            Assert.That(nodes.InXRange(double.PositiveInfinity, double.PositiveInfinity), Is.Empty);
            Assert.That(nodes.InXRange(double.NegativeInfinity, double.NegativeInfinity), Is.Empty);
        }
        [Test]
        public void RangeValidatesBoundsAndDeclaredTypeBeforeEnumerating()
        {
            var nodes = new Once<Node>(Array.Empty<Node>());
            Assert.That(Assert.Throws<ArgumentOutOfRangeException>(() => nodes.InXRange(double.NaN, 1)).ParamName, Is.EqualTo("xMin"));
            Assert.That(Assert.Throws<ArgumentOutOfRangeException>(() => nodes.InXRange(0, double.NaN)).ParamName, Is.EqualTo("xMax"));
            Assert.Throws<ArgumentOutOfRangeException>(() => nodes.InXRange(2, 1)); Assert.That(nodes.Starts, Is.Zero);
            Assert.That(Assert.Throws<ArgumentNullException>(() => ((IEnumerable<Node>)null).InXRange(0, 1)).ParamName, Is.EqualTo("source"));
            var mixed = new Once<IEntity>(new IEntity[] { N(), B(1, 0, 1) }); Assert.Throws<NotSupportedException>(() => mixed.InXRange(0, 1)); Assert.That(mixed.Starts, Is.Zero);
            Assert.Throws<NotSupportedException>(() => Array.Empty<OtherEntity>().InXRange(0, 1));
        }
        private sealed class OtherEntity : IEntity { public int Id => 1; }
        [Test]
        public void RangeRejectsNullEntriesAndMissingRequiredIncidenceEvenAfterAnEarlyMatch()
        {
            Assert.Throws<ArgumentException>(() => new[] { N(), null }.InXRange(0, 1));
            Assert.Throws<ArgumentException>(() => new[] { B(1, 0, 1), new Beam() }.InXRange(0, 1));
            Assert.Throws<ArgumentException>(() => new[] { new Beam(N(), null) }.InXRange(0, 1));
            Assert.Throws<ArgumentException>(() => new[] { new Plate(null, N(), N()) }.InXRange(0, 1));
            Assert.Throws<ArgumentException>(() => new[] { new Plate(N(), null, N()) }.InXRange(0, 1));
            Assert.Throws<ArgumentException>(() => new[] { new Plate(N(), N(), null) }.InXRange(0, 1));
            Assert.That(new[] { P(1, 0, 0, 0) }.InXRange(0, 1).Count, Is.EqualTo(1));
        }
        [TestCase(double.NaN)] [TestCase(double.PositiveInfinity)] [TestCase(double.NegativeInfinity)]
        public void RangeRejectsParticipatingNonfiniteDataButDoesNotReadBeamPlateYOrZ(double value)
        {
            Assert.Throws<ArgumentException>(() => new[] { N(1, value) }.InXRange(0, 1));
            Assert.Throws<ArgumentException>(() => new[] { N(1, 50, value) }.InXRange(0, 1));
            Assert.Throws<ArgumentException>(() => new[] { N(1, 0, 0, value) }.InXRange(0, 1));
            Assert.Throws<ArgumentException>(() => new[] { B(1, value, 1) }.InXRange(0, 1));
            Assert.Throws<ArgumentException>(() => new[] { P(1, 0, 1, 1, value) }.InXRange(0, 1));
            var beam = B(1, 0, 1); beam.StartNode.Y = value; beam.EndNode.Z = value;
            var plate = P(1, 0, 0, 1); plate.A.Y = value; plate.C.Z = value;
            Assert.That(new[] { beam }.InXRange(0, 1).Single(), Is.SameAs(beam)); Assert.That(new[] { plate }.InXRange(0, 1).Single(), Is.SameAs(plate));
        }
        [Test]
        public void SelectionEnumeratesOnceReturnsOwnedEditableSetAndDisposesOnFailure()
        {
            var first = B(1, 0, 1); var input = new List<Beam> { first, B(2, 5, 6) }; var once = new Once<Beam>(input);
            var set = once.InXRange(0, 1); Assert.That(once.Starts, Is.EqualTo(1)); Assert.That(once.Disposals, Is.EqualTo(1));
            input.Clear(); first.StartNode.X = 99; Assert.That(set.Single(), Is.SameAs(first)); set.Add(B(3, -50, -40)); Assert.That(set.Count, Is.EqualTo(2));
            var failure = new ApplicationException("source"); IEnumerable<Node> Fail() { yield return N(); throw failure; }
            var failing = new Once<Node>(Fail()); Assert.That(Assert.Throws<ApplicationException>(() => failing.InXRange(0, 1)), Is.SameAs(failure)); Assert.That(failing.Disposals, Is.EqualTo(1));
            var empty = Array.Empty<Node>().InXRange(0, 1); Assert.That(empty, Is.Empty); Assert.That(empty, Is.Not.SameAs(Array.Empty<Node>().InXRange(0, 1)));
        }
        [Test]
        public void ChangedAcceptedHashMetadataFailsBeforeASetEscapes()
        {
            var node = N(); IEnumerable<Node> NodeIdChanges() { yield return node; node.Id = 99; }
            Assert.Throws<ArgumentException>(() => NodeIdChanges().InXRange(0, 1)); node = N();
            IEnumerable<Node> CoordinateChanges() { yield return node; node.Y = 99; }
            Assert.Throws<ArgumentException>(() => CoordinateChanges().InXRange(0, 1));
            var beam = B(1, 0, 1); IEnumerable<Beam> BeamIdChanges() { yield return beam; beam.Id = 2; }
            Assert.Throws<ArgumentException>(() => BeamIdChanges().InXRange(0, 1));
            var plate = P(1, 0, 0, 1); IEnumerable<Plate> PlateIdChanges() { yield return plate; plate.Id = 2; }
            Assert.Throws<ArgumentException>(() => PlateIdChanges().InXRange(0, 1));
        }
        [Test]
        public void RandomizedFiniteSelectionMatchesOriginalAxisPredicateAndIdSetMeaning()
        {
            var random = new Random(2728);
            for (int run = 0; run < 100; run++)
            {
                var beams = Enumerable.Range(0, 40).Select(i => B(i % 9, random.Next(-3, 4), random.Next(-3, 4))).ToArray();
                var expected = beams.Where(b => Math.Min(b.StartNode.X, b.EndNode.X) >= -1 && Math.Max(b.StartNode.X, b.EndNode.X) <= 2).GroupBy(b => b.Id).Select(g => g.First()).ToArray();
                var actual = beams.InXRange(-1, 2); Assert.That(actual.Count, Is.EqualTo(expected.Length));
                Assert.That(expected.All(b => actual.Any(a => ReferenceEquals(a, b))), Is.True);
                var a = N(1, random.NextDouble(), random.NextDouble(), random.NextDouble()); var b = N(2, a.X + (random.NextDouble() - 0.5) * 0.0003, a.Y, a.Z);
                Assert.That(a.HasSameCoordinatesAs(b), Is.EqualTo(Math.Abs(a.X - b.X) <= 0.0001 && Math.Abs(a.Y - b.Y) <= 0.0001 && Math.Abs(a.Z - b.Z) <= 0.0001));
            }
        }
        [Test]
        public void ExactTwoSourceContractsAndExpandedXmlAreShipped()
        {
            var methods = typeof(GeometryDiagnosticsExtensions).GetMethods().Where(m => m.Name == "HasSameCoordinatesAs" || m.Name == "InXRange").ToArray(); Assert.That(methods, Has.Length.EqualTo(2));
            var xml = XDocument.Load(Path.ChangeExtension(typeof(GeometryDiagnosticsExtensions).Assembly.Location, ".xml"));
            foreach (var method in methods)
            {
                var parameters = method.GetParameters(); bool range = method.Name == "InXRange";
                Assert.That(parameters.Select(p => p.Name), Is.EqualTo(range ? new[] { "source", "xMin", "xMax" } : new[] { "left", "right" }));
                Assert.That(parameters.Any(p => p.IsOptional || p.ParameterType.IsByRef), Is.False); Assert.That(method.GetCustomAttributes(typeof(ExtensionAttribute), false), Has.Length.EqualTo(1));
                if (range) { Assert.That(method.ReturnType.GetGenericTypeDefinition(), Is.EqualTo(typeof(HashSet<>))); Assert.That(method.GetGenericArguments().Single().GetGenericParameterConstraints(), Is.EqualTo(new[] { typeof(IEntity) })); }
                else { Assert.That(method.ReturnType, Is.EqualTo(typeof(bool))); Assert.That(parameters.Select(p => p.ParameterType), Is.EqualTo(new[] { typeof(Node), typeof(Node) })); }
                var entry = xml.Descendants("member").Single(e => ((string)e.Attribute("name")).StartsWith("M:" + typeof(GeometryDiagnosticsExtensions).FullName + "." + method.Name + (range ? new string((char)96, 2) + "1(" : "(")));
                foreach (string tag in new[] { "summary", "param", "returns", "remarks", "exception", "example" }) Assert.That(entry.Element(tag), Is.Not.Null);
                Assert.That(entry.Descendants().Any(e => e.Name == "include" || e.Name == "inheritdoc"), Is.False);
            }
        }
    }
}
