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
    public sealed class YZRangeSelectionTests
    {
        private static Node NodeAt(int id, double axisCoordinate, bool isZ, double other1 = 0, double other2 = 0) =>
            isZ ? new Node(id, other1, other2, axisCoordinate) : new Node(id, other1, axisCoordinate, other2);

        private static Beam BeamOnAxis(int id, double start, double end, bool isZ, double other = 0) =>
            new Beam(id, NodeAt(1, start, isZ, other, other), NodeAt(2, end, isZ, other, other));

        private static Plate PlateOnAxis(int id, double a, double b, double c, bool isZ, double? d = null, double other = 0) =>
            new Plate(
                id,
                NodeAt(1, a, isZ, other, other),
                NodeAt(2, b, isZ, other, other),
                NodeAt(3, c, isZ, other, other),
                d.HasValue ? NodeAt(4, d.Value, isZ, other, other) : null);

        private static HashSet<T> SelectRange<T>(IEnumerable<T> source, double min, double max, bool isZ) where T : IEntity =>
            isZ ? source.InZRange(min, max) : source.InYRange(min, max);

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

        private sealed class OtherEntity : IEntity { public int Id => 1; }

        [TestCase(false)]
        [TestCase(true)]
        public void RangeUsesWholeClosedExtentsInsteadOfMidpointsOrOverlap(bool isZ)
        {
            var accepted = BeamOnAxis(1, -2, 3, isZ);
            var reversed = BeamOnAxis(2, 3, -2, isZ);
            var crossing = BeamOnAxis(3, -3, 4, isZ);
            var touching = BeamOnAxis(4, -5, -2, isZ);
            var midpointInsideOnly = BeamOnAxis(5, -10, 11, isZ);
            var beams = SelectRange(new[] { accepted, reversed, crossing, touching, midpointInsideOnly }, -2, 3, isZ);
            Assert.That(beams.Count, Is.EqualTo(2));
            Assert.That(beams.Any(b => ReferenceEquals(b, accepted)), Is.True);
            Assert.That(beams.Any(b => ReferenceEquals(b, reversed)), Is.True);

            var triangle = PlateOnAxis(1, -2, 0, 3, isZ);
            var quad = PlateOnAxis(2, 3, -2, 1, isZ, 2);
            var escapesD = PlateOnAxis(3, 0, 1, 2, isZ, 3.0001);
            var plates = SelectRange(new[] { triangle, quad, escapesD }, -2, 3, isZ);
            Assert.That(plates.Count, Is.EqualTo(2));
            Assert.That(plates.Any(p => ReferenceEquals(p, triangle)), Is.True);
            Assert.That(plates.Any(p => ReferenceEquals(p, quad)), Is.True);

            var zeroWidth = SelectRange(new[] { BeamOnAxis(1, 2.5, 2.5, isZ), BeamOnAxis(2, 1, 2.5, isZ) }, 2.5, 2.5, isZ);
            Assert.That(zeroWidth.Single().Id, Is.EqualTo(1));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void BeamAndPlateSetsRetainFirstQualifyingReferenceByIdDespiteGlobalEquality(bool isZ)
        {
            var first = BeamOnAxis(7, 0, 1, isZ);
            var duplicate = BeamOnAxis(7, 1, 2, isZ);
            Assert.That(first.Equals(duplicate), Is.False);
            var set = SelectRange(new[] { BeamOnAxis(7, -3, -1, isZ), first, duplicate, first }, 0, 2, isZ);
            Assert.That(set.Count, Is.EqualTo(1));
            Assert.That(set.Single(), Is.SameAs(first));
            Assert.That(set.Contains(duplicate), Is.True);

            var plateFirst = PlateOnAxis(7, 0, 1, 2, isZ);
            var plateDuplicate = PlateOnAxis(7, 1, 1, 1, isZ);
            Assert.That(plateFirst.Equals(plateDuplicate), Is.False);
            Assert.That(SelectRange(new[] { plateFirst, plateDuplicate }, 0, 2, isZ).Single(), Is.SameAs(plateFirst));

            foreach (int id in new[] { 0, -1, int.MinValue, int.MaxValue })
                Assert.That(SelectRange(new[] { BeamOnAxis(id, 0, 1, isZ), BeamOnAxis(id, 1, 2, isZ) }, 0, 2, isZ).Count, Is.EqualTo(1));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void NodeSetUsesSourceIdAndOneMillionthDistanceRatherThanGlobalTolerance(bool isZ)
        {
            var a = NodeAt(1, 0, isZ);
            var near = NodeAt(1, 0.0000005, isZ);
            var separate = NodeAt(1, 0.0005, isZ);
            var anotherId = NodeAt(2, 0, isZ);
            Assert.That(a.Equals(separate), Is.True);
            var set = SelectRange(new[] { a, near, separate, anotherId }, -1, 1, isZ);
            Assert.That(set.Count, Is.EqualTo(3));
            Assert.That(set.Any(n => ReferenceEquals(n, a)), Is.True);
            Assert.That(set.Any(n => ReferenceEquals(n, near)), Is.False);
            Assert.That(set.Comparer.Equals(a, separate), Is.False);
            Assert.That(set.Comparer.Equals(a, NodeAt(1, 0.000001, isZ)), Is.True);
            Assert.That(set.Comparer.Equals(a, new Node(1, 0.0000007, 0.0000007, 0.0000007)), Is.False);
            Assert.That(set.Comparer.Equals(a, anotherId), Is.False);
            Assert.That(SelectRange(new[] { NodeAt(1, double.MaxValue, isZ), NodeAt(1, -double.MaxValue, isZ) }, double.NegativeInfinity, double.PositiveInfinity, isZ).Count, Is.EqualTo(2));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void RangeAllowsOpenEndedInfinitiesAndFiniteExtremeGeometry(bool isZ)
        {
            var nodes = new[] { NodeAt(1, -double.MaxValue, isZ), NodeAt(2, 0, isZ), NodeAt(3, double.MaxValue, isZ) };
            Assert.That(SelectRange(nodes, double.NegativeInfinity, double.PositiveInfinity, isZ).Count, Is.EqualTo(3));
            Assert.That(SelectRange(nodes, 0, double.PositiveInfinity, isZ).Count, Is.EqualTo(2));
            Assert.That(SelectRange(nodes, double.NegativeInfinity, 0, isZ).Count, Is.EqualTo(2));
            Assert.That(SelectRange(nodes, double.PositiveInfinity, double.PositiveInfinity, isZ), Is.Empty);
            Assert.That(SelectRange(nodes, double.NegativeInfinity, double.NegativeInfinity, isZ), Is.Empty);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void RangeValidatesBoundsAndDeclaredTypeBeforeEnumerating(bool isZ)
        {
            string minParam = isZ ? "zMin" : "yMin";
            string maxParam = isZ ? "zMax" : "yMax";
            var nodes = new Once<Node>(Array.Empty<Node>());
            Assert.That(Assert.Throws<ArgumentOutOfRangeException>(() => SelectRange(nodes, double.NaN, 1, isZ)).ParamName, Is.EqualTo(minParam));
            Assert.That(Assert.Throws<ArgumentOutOfRangeException>(() => SelectRange(nodes, 0, double.NaN, isZ)).ParamName, Is.EqualTo(maxParam));
            Assert.That(Assert.Throws<ArgumentOutOfRangeException>(() => SelectRange(nodes, 2, 1, isZ)).ParamName, Is.EqualTo(maxParam));
            Assert.That(nodes.Starts, Is.Zero);
            Assert.That(Assert.Throws<ArgumentNullException>(() => SelectRange((IEnumerable<Node>)null, 0, 1, isZ)).ParamName, Is.EqualTo("source"));

            var mixed = new Once<IEntity>(new IEntity[] { NodeAt(1, 0, isZ), BeamOnAxis(1, 0, 1, isZ) });
            var error = Assert.Throws<NotSupportedException>(() => SelectRange(mixed, 0, 1, isZ));
            Assert.That(error.Message, Does.StartWith(isZ ? "Z-range selection" : "Y-range selection"));
            Assert.That(mixed.Starts, Is.Zero);
            Assert.Throws<NotSupportedException>(() => SelectRange(Array.Empty<OtherEntity>(), 0, 1, isZ));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void RangeRejectsNullEntriesAndMissingRequiredIncidenceEvenAfterAnEarlyMatch(bool isZ)
        {
            Assert.Throws<ArgumentException>(() => SelectRange(new[] { NodeAt(1, 0, isZ), null }, 0, 1, isZ));
            Assert.Throws<ArgumentException>(() => SelectRange(new[] { BeamOnAxis(1, 0, 1, isZ), new Beam() }, 0, 1, isZ));
            Assert.Throws<ArgumentException>(() => SelectRange(new[] { new Beam(NodeAt(1, 0, isZ), null) }, 0, 1, isZ));
            Assert.Throws<ArgumentException>(() => SelectRange(new[] { new Plate(null, NodeAt(2, 0, isZ), NodeAt(3, 0, isZ)) }, 0, 1, isZ));
            Assert.Throws<ArgumentException>(() => SelectRange(new[] { new Plate(NodeAt(1, 0, isZ), null, NodeAt(3, 0, isZ)) }, 0, 1, isZ));
            Assert.Throws<ArgumentException>(() => SelectRange(new[] { new Plate(NodeAt(1, 0, isZ), NodeAt(2, 0, isZ), null) }, 0, 1, isZ));
            Assert.That(SelectRange(new[] { PlateOnAxis(1, 0, 0, 0, isZ) }, 0, 1, isZ).Count, Is.EqualTo(1));
        }

        private static IEnumerable<TestCaseData> AxisInvalidValues()
        {
            foreach (bool isZ in new[] { false, true })
                foreach (double value in new[] { double.NaN, double.PositiveInfinity, double.NegativeInfinity })
                    yield return new TestCaseData(isZ, value);
        }

        [TestCaseSource(nameof(AxisInvalidValues))]
        public void RangeRejectsParticipatingNonfiniteDataButDoesNotReadUnusedBeamPlateAxes(bool isZ, double value)
        {
            Assert.Throws<ArgumentException>(() => SelectRange(new[] { NodeAt(1, value, isZ) }, 0, 1, isZ));
            Assert.Throws<ArgumentException>(() => SelectRange(new[] { NodeAt(1, 50, isZ, value, 0) }, 0, 1, isZ));
            Assert.Throws<ArgumentException>(() => SelectRange(new[] { NodeAt(1, 0, isZ, 0, value) }, 0, 1, isZ));
            Assert.Throws<ArgumentException>(() => SelectRange(new[] { BeamOnAxis(1, value, 1, isZ) }, 0, 1, isZ));
            Assert.Throws<ArgumentException>(() => SelectRange(new[] { BeamOnAxis(1, 50, value, isZ) }, 0, 1, isZ));
            Assert.Throws<ArgumentException>(() => SelectRange(new[] { PlateOnAxis(1, 0, 1, 1, isZ, value) }, 0, 1, isZ));

            var beam = BeamOnAxis(1, 0, 1, isZ);
            var plate = PlateOnAxis(1, 0, 0, 1, isZ);
            beam.StartNode.X = value;
            plate.A.X = value;
            if (isZ)
            {
                beam.EndNode.Y = value;
                plate.C.Y = value;
            }
            else
            {
                beam.EndNode.Z = value;
                plate.C.Z = value;
            }
            Assert.That(SelectRange(new[] { beam }, 0, 1, isZ).Single(), Is.SameAs(beam));
            Assert.That(SelectRange(new[] { plate }, 0, 1, isZ).Single(), Is.SameAs(plate));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void SelectionEnumeratesOnceReturnsOwnedEditableSetAndDisposesOnFailure(bool isZ)
        {
            var first = BeamOnAxis(1, 0, 1, isZ);
            var input = new List<Beam> { first, BeamOnAxis(2, 5, 6, isZ) };
            var once = new Once<Beam>(input);
            var set = SelectRange(once, 0, 1, isZ);
            Assert.That(once.Starts, Is.EqualTo(1));
            Assert.That(once.Disposals, Is.EqualTo(1));

            input.Clear();
            first.StartNode.X = 99;
            first.StartNode.Y = 99;
            first.StartNode.Z = 99;
            Assert.That(set.Single(), Is.SameAs(first));
            set.Add(BeamOnAxis(3, -50, -40, isZ));
            Assert.That(set.Count, Is.EqualTo(2));

            var failure = new ApplicationException("source");
            IEnumerable<Node> Fail() { yield return NodeAt(1, 0, isZ); throw failure; }
            var failing = new Once<Node>(Fail());
            Assert.That(Assert.Throws<ApplicationException>(() => SelectRange(failing, 0, 1, isZ)), Is.SameAs(failure));
            Assert.That(failing.Disposals, Is.EqualTo(1));

            var empty = SelectRange(Array.Empty<Node>(), 0, 1, isZ);
            Assert.That(empty, Is.Empty);
            Assert.That(empty, Is.Not.SameAs(SelectRange(Array.Empty<Node>(), 0, 1, isZ)));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void ChangedAcceptedHashMetadataFailsBeforeASetEscapes(bool isZ)
        {
            var node = NodeAt(1, 0, isZ);
            IEnumerable<Node> NodeIdChanges() { yield return node; node.Id = 99; }
            Assert.Throws<ArgumentException>(() => SelectRange(NodeIdChanges(), 0, 1, isZ));

            node = NodeAt(1, 0, isZ);
            IEnumerable<Node> UnusedAxisChanges() { yield return node; node.X = 99; }
            Assert.Throws<ArgumentException>(() => SelectRange(UnusedAxisChanges(), 0, 1, isZ));

            var beam = BeamOnAxis(1, 0, 1, isZ);
            IEnumerable<Beam> BeamIdChanges() { yield return beam; beam.Id = 2; }
            Assert.Throws<ArgumentException>(() => SelectRange(BeamIdChanges(), 0, 1, isZ));

            var plate = PlateOnAxis(1, 0, 0, 1, isZ);
            IEnumerable<Plate> PlateIdChanges() { yield return plate; plate.Id = 2; }
            Assert.Throws<ArgumentException>(() => SelectRange(PlateIdChanges(), 0, 1, isZ));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void RandomizedFiniteSelectionMatchesOriginalAxisPredicateAndIdSetMeaning(bool isZ)
        {
            var random = new Random(isZ ? 2930 : 2929);
            Func<Node, double> coord = n => isZ ? n.Z : n.Y;
            for (int run = 0; run < 100; run++)
            {
                var beams = Enumerable.Range(0, 40).Select(i => BeamOnAxis(i % 9, random.Next(-3, 4), random.Next(-3, 4), isZ)).ToArray();
                var expectedBeams = beams.Where(b => Math.Min(coord(b.StartNode), coord(b.EndNode)) >= -1 && Math.Max(coord(b.StartNode), coord(b.EndNode)) <= 2)
                    .GroupBy(b => b.Id).Select(g => g.First()).ToArray();
                var actualBeams = SelectRange(beams, -1, 2, isZ);
                Assert.That(actualBeams.Count, Is.EqualTo(expectedBeams.Length));
                Assert.That(expectedBeams.All(b => actualBeams.Any(a => ReferenceEquals(a, b))), Is.True);

                var plates = Enumerable.Range(0, 30).Select(i => PlateOnAxis(i % 7, random.Next(-3, 4), random.Next(-3, 4), random.Next(-3, 4), isZ, (i % 2 == 0) ? (double?)random.Next(-3, 4) : null)).ToArray();
                var expectedPlates = plates.Where(p =>
                {
                    double min = Math.Min(coord(p.A), Math.Min(coord(p.B), coord(p.C)));
                    double max = Math.Max(coord(p.A), Math.Max(coord(p.B), coord(p.C)));
                    if (p.D != null) { min = Math.Min(min, coord(p.D)); max = Math.Max(max, coord(p.D)); }
                    return min >= -1 && max <= 2;
                }).GroupBy(p => p.Id).Select(g => g.First()).ToArray();
                var actualPlates = SelectRange(plates, -1, 2, isZ);
                Assert.That(actualPlates.Count, Is.EqualTo(expectedPlates.Length));
                Assert.That(expectedPlates.All(p => actualPlates.Any(a => ReferenceEquals(a, p))), Is.True);
            }
        }

        [Test]
        public void ExactTwoSourceContractsAndExpandedXmlAreShipped()
        {
            var methods = typeof(GeometryDiagnosticsExtensions).GetMethods()
                .Where(m => m.Name == "InYRange" || m.Name == "InZRange")
                .OrderBy(m => m.Name)
                .ToArray();
            Assert.That(methods, Has.Length.EqualTo(2));
            var xml = XDocument.Load(Path.ChangeExtension(typeof(GeometryDiagnosticsExtensions).Assembly.Location, ".xml"));
            foreach (var method in methods)
            {
                bool isZ = method.Name == "InZRange";
                var parameters = method.GetParameters();
                Assert.That(parameters.Select(p => p.Name), Is.EqualTo(isZ ? new[] { "source", "zMin", "zMax" } : new[] { "source", "yMin", "yMax" }));
                Assert.That(parameters.Any(p => p.IsOptional || p.ParameterType.IsByRef), Is.False);
                Assert.That(method.GetCustomAttributes(typeof(ExtensionAttribute), false), Has.Length.EqualTo(1));
                Assert.That(method.ReturnType.GetGenericTypeDefinition(), Is.EqualTo(typeof(HashSet<>)));
                Assert.That(method.GetGenericArguments().Single().GetGenericParameterConstraints(), Is.EqualTo(new[] { typeof(IEntity) }));
                string prefix = "M:" + typeof(GeometryDiagnosticsExtensions).FullName + "." + method.Name + new string((char)96, 2) + "1(";
                var entry = xml.Descendants("member").Single(e => ((string)e.Attribute("name")).StartsWith(prefix));
                foreach (string tag in new[] { "summary", "param", "returns", "remarks", "exception", "example" })
                    Assert.That(entry.Element(tag), Is.Not.Null);
                Assert.That(entry.Descendants().Any(e => e.Name == "include" || e.Name == "inheritdoc"), Is.False);
            }
        }
    }
}
