using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using NUnit.Framework;
using StaadPro.Interop.Entities;
using StaadPro.Interop.Extensions;

namespace StaadPro.Interop.Tests.Extensions
{
    [TestFixture]
    public sealed class CoordinateLookupTests
    {
        private static Node Find(bool nodeOverload, IEnumerable<Node> candidates, Node reference) => nodeOverload
            ? candidates.FirstNodeWithSameCoordinatesAs(reference)
            : candidates.FirstNodeWithSameCoordinatesAs(reference.X, reference.Y, reference.Z);
        private static Node N(int id = 1, double x = 0, double y = 0, double z = 0) => new Node(id, x, y, z);
        private sealed class Once : IEnumerable<Node>
        {
            private readonly IEnumerable<Node> values;
            private readonly Action start, finish;
            internal int Starts, Visits, Disposals;
            internal Once(IEnumerable<Node> values, Action start = null, Action finish = null)
            { this.values = values; this.start = start; this.finish = finish; }
            public IEnumerator<Node> GetEnumerator()
            {
                if (++Starts != 1) throw new InvalidOperationException("Re-enumeration");
                start?.Invoke();
                try { foreach (Node value in values) { Visits++; yield return value; } }
                finally { Disposals++; finish?.Invoke(); }
            }
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
        [TestCase(true)] [TestCase(false)]
        public void FirstMatchingOriginalReferenceWinsInsteadOfNearestOrMatchingId(bool nodeOverload)
        {
            var reference = N(9); var farSameId = N(9, 1); var first = N(-7, 0.0001, -0.0001, 0.0001); var nearest = N(55);
            var found = Find(nodeOverload, new[] { farSameId, first, nearest }, reference);
            Assert.That(found, Is.SameAs(first)); Assert.That(first.Equals(reference), Is.False);
            Assert.That(Node.DistanceBetweenNodes(first, reference), Is.GreaterThan(0.0001));
            Assert.That(Find(nodeOverload, new[] { reference, first }, reference), Is.SameAs(reference));
        }
        [TestCase(true)] [TestCase(false)]
        public void EveryAxisUsesTheExactInclusiveSourceTolerance(bool nodeOverload)
        {
            const double tolerance = 0.0001;
            double outside = BitConverter.Int64BitsToDouble(BitConverter.DoubleToInt64Bits(tolerance) + 1);
            for (int axis = 0; axis < 3; axis++)
                foreach (double sign in new[] { -1d, 1d })
                {
                    var inside = new double[3]; inside[axis] = sign * tolerance;
                    var beyond = new double[3]; beyond[axis] = sign * outside;
                    var match = N(4, inside[0], inside[1], inside[2]); var miss = N(4, beyond[0], beyond[1], beyond[2]);
                    Assert.That(Find(nodeOverload, new[] { match }, N()), Is.SameAs(match));
                    Assert.That(Find(nodeOverload, new[] { miss }, N()), Is.Null);
                }
        }
        [TestCase(true)] [TestCase(false)]
        public void EmptyAndNoMatchReturnNullAndDisposeOnce(bool nodeOverload)
        {
            var empty = new Once(Array.Empty<Node>()); Assert.That(Find(nodeOverload, empty, N()), Is.Null);
            Assert.That(empty.Starts, Is.EqualTo(1)); Assert.That(empty.Disposals, Is.EqualTo(1));
            var misses = new Once(new[] { N(1, 3), N(2, 4), N(3, 5) });
            Assert.That(Find(nodeOverload, misses, N()), Is.Null); Assert.That(misses.Visits, Is.EqualTo(3)); Assert.That(misses.Disposals, Is.EqualTo(1));
        }
        [TestCase(true)] [TestCase(false)]
        public void StopsAtFirstMatchWithoutObservingInvalidOrThrowingTail(bool nodeOverload)
        {
            var expected = N();
            IEnumerable<Node> Values() { yield return N(7, 1); yield return expected; throw new ApplicationException("Unvisited tail"); }
            var source = new Once(Values()); Assert.That(Find(nodeOverload, source, N()), Is.SameAs(expected));
            Assert.That(source.Visits, Is.EqualTo(2)); Assert.That(source.Disposals, Is.EqualTo(1));
            Assert.That(Find(nodeOverload, new[] { expected, null, N(2, double.NaN) }, N()), Is.SameAs(expected));
        }
        [TestCase(true)] [TestCase(false)]
        public void ReferenceCoordinatesAreCapturedBeforeEnumerationAndReturnedNodesRemainMutable(bool nodeOverload)
        {
            var reference = N(100); var candidate = N(200); var values = new List<Node> { candidate };
            var source = new Once(values, () => reference.X = 99, () => { candidate.X = 77; values.Clear(); });
            var result = Find(nodeOverload, source, reference);
            Assert.That(result, Is.SameAs(candidate)); Assert.That(result.X, Is.EqualTo(77)); Assert.That(values, Is.Empty);
            Assert.That(reference.X, Is.EqualTo(99)); Assert.That(source.Starts, Is.EqualTo(1)); Assert.That(source.Disposals, Is.EqualTo(1));
        }
        [TestCase(true)] [TestCase(false)]
        public void DraftRepeatedAndExtremeIdsHaveNoEffectOnCoordinateEligibility(bool nodeOverload)
        {
            foreach (int id in new[] { 0, -1, int.MinValue, int.MaxValue })
            {
                var candidate = N(id); Assert.That(Find(nodeOverload, new[] { candidate }, N(15)), Is.SameAs(candidate));
            }
            var far = N(1, 50); var match = N(1);
            Assert.That(Find(nodeOverload, new[] { far, far, match, match }, N(1)), Is.SameAs(match));
        }
        [TestCase(true)] [TestCase(false)]
        public void FiniteExtremesSignedZeroAndSubnormalDifferencesAreHandledWithoutRadiusArithmetic(bool nodeOverload)
        {
            var extreme = N(2, double.MaxValue, -double.MaxValue, double.Epsilon);
            Assert.That(Find(nodeOverload, new[] { extreme }, N(1, double.MaxValue, -double.MaxValue, 0)), Is.SameAs(extreme));
            Assert.That(Find(nodeOverload, new[] { N(1, double.MaxValue) }, N(1, -double.MaxValue)), Is.Null);
            Assert.That(Find(nodeOverload, new[] { N(2, -0d, double.Epsilon, -double.Epsilon) }, N()), Is.Not.Null);
            double nextLarge = BitConverter.Int64BitsToDouble(BitConverter.DoubleToInt64Bits(double.MaxValue) - 1);
            Assert.That(Find(nodeOverload, new[] { N(2, nextLarge) }, N(1, double.MaxValue)), Is.Null);
        }
        [TestCase(true)] [TestCase(false)]
        public void NullCollectionsAndVisitedNullEntriesProduceNamedArgumentErrors(bool nodeOverload)
        {
            Assert.That(Assert.Throws<ArgumentNullException>(() => Find(nodeOverload, null, N())).ParamName, Is.EqualTo("nodeCollection"));
            var source = new Once(new[] { N(3, 1), null, N() });
            Assert.That(Assert.Throws<ArgumentException>(() => Find(nodeOverload, source, N())).ParamName, Is.EqualTo("nodeCollection"));
            Assert.That(source.Visits, Is.EqualTo(2)); Assert.That(source.Disposals, Is.EqualTo(1));
        }
        [Test]
        public void NullReferenceNodeFailsBeforeEnumeratingEvenAnEmptySequence()
        {
            var source = new Once(Array.Empty<Node>());
            Assert.That(Assert.Throws<ArgumentNullException>(() => source.FirstNodeWithSameCoordinatesAs((Node)null)).ParamName, Is.EqualTo("node"));
            Assert.That(source.Starts, Is.Zero);
        }
        private static IEnumerable<TestCaseData> NonfiniteReferenceValues()
        {
            foreach (bool nodeOverload in new[] { true, false })
                foreach (int axis in new[] { 0, 1, 2 })
                    foreach (double value in new[] { double.NaN, double.PositiveInfinity, double.NegativeInfinity })
                        yield return new TestCaseData(nodeOverload, axis, value);
        }
        [TestCaseSource(nameof(NonfiniteReferenceValues))]
        public void NonfiniteReferenceCoordinatesFailBeforeInputEnumeration(bool nodeOverload, int axis, double value)
        {
            var coordinates = new double[3]; coordinates[axis] = value;
            var source = new Once(Array.Empty<Node>()); var reference = N(1, coordinates[0], coordinates[1], coordinates[2]);
            var error = nodeOverload ? Assert.Throws<ArgumentException>(() => Find(true, source, reference))
                : Assert.Throws<ArgumentOutOfRangeException>(() => Find(false, source, reference));
            Assert.That(error.ParamName, Is.EqualTo(nodeOverload ? "node" : new[] { "x", "y", "z" }[axis]));
            Assert.That(source.Starts, Is.Zero);
        }
        [TestCaseSource(nameof(NonfiniteReferenceValues))]
        public void EveryVisitedCandidateAxisIsValidatedBeforeComparison(bool nodeOverload, int axis, double value)
        {
            var coordinates = new double[3]; coordinates[0] = 100; coordinates[axis] = value;
            var source = new Once(new[] { N(1, coordinates[0], coordinates[1], coordinates[2]), N() });
            Assert.That(Assert.Throws<ArgumentException>(() => Find(nodeOverload, source, N())).ParamName, Is.EqualTo("nodeCollection"));
            Assert.That(source.Visits, Is.EqualTo(1)); Assert.That(source.Disposals, Is.EqualTo(1));
        }
        [TestCase(true)] [TestCase(false)]
        public void EnumerationErrorsBeforeMatchPropagateUnchangedAndDisposeAtInvocation(bool nodeOverload)
        {
            var failure = new ApplicationException("Source failed");
            IEnumerable<Node> Values() { yield return N(4, 5); throw failure; }
            var source = new Once(Values()); Assert.That(Assert.Throws<ApplicationException>(() => Find(nodeOverload, source, N())), Is.SameAs(failure));
            Assert.That(source.Disposals, Is.EqualTo(1));
        }
        [TestCase(true)] [TestCase(false)]
        public void FiniteRandomizedSearchMatchesTheOriginalFirstOrDefaultPredicate(bool nodeOverload)
        {
            var random = new Random(2411);
            for (int sample = 0; sample < 200; sample++)
            {
                var reference = N(1, random.NextDouble(), random.NextDouble(), random.NextDouble());
                var nodes = Enumerable.Range(0, 50).Select(i => N(i, reference.X + (random.NextDouble() - 0.5) * 0.0003,
                    reference.Y + (random.NextDouble() - 0.5) * 0.0003, reference.Z + (random.NextDouble() - 0.5) * 0.0003)).ToArray();
                var expected = nodes.FirstOrDefault(n => Math.Abs(n.X - reference.X) <= 0.0001 && Math.Abs(n.Y - reference.Y) <= 0.0001 && Math.Abs(n.Z - reference.Z) <= 0.0001);
                Assert.That(Find(nodeOverload, nodes, reference), Is.SameAs(expected));
            }
        }
        [Test]
        public void ExactTwoSourceSignaturesAndCompleteExpandedXmlAreShipped()
        {
            var methods = typeof(GeometryDiagnosticsExtensions).GetMethods().Where(m => m.Name == "FirstNodeWithSameCoordinatesAs").ToArray();
            Assert.That(methods, Has.Length.EqualTo(2));
            var xml = XDocument.Load(Path.ChangeExtension(typeof(GeometryDiagnosticsExtensions).Assembly.Location, ".xml"));
            foreach (var method in methods)
            {
                var p = method.GetParameters(); bool nodeOverload = p.Length == 2;
                Assert.That(method.ReturnType, Is.EqualTo(typeof(Node)));
                Assert.That(p.Select(x => x.Name), Is.EqualTo(nodeOverload ? new[] { "nodeCollection", "node" } : new[] { "nodeCollection", "x", "y", "z" }));
                Assert.That(p.Select(x => x.ParameterType), Is.EqualTo(nodeOverload ? new[] { typeof(IEnumerable<Node>), typeof(Node) } : new[] { typeof(IEnumerable<Node>), typeof(double), typeof(double), typeof(double) }));
                Assert.That(p.Any(x => x.IsOptional || x.ParameterType.IsByRef), Is.False);
                Assert.That(method.GetCustomAttributes(typeof(ExtensionAttribute), false), Has.Length.EqualTo(1));
                string suffix = nodeOverload ? ",StaadPro.Interop.Entities.Node)" : ",System.Double,System.Double,System.Double)";
                var entry = xml.Descendants("member").Single(x => ((string)x.Attribute("name")).StartsWith("M:" + typeof(GeometryDiagnosticsExtensions).FullName + ".FirstNodeWithSameCoordinatesAs(") && ((string)x.Attribute("name")).EndsWith(suffix));
                foreach (string tag in new[] { "summary", "param", "returns", "remarks", "exception", "example" }) Assert.That(entry.Element(tag), Is.Not.Null);
                Assert.That(entry.Elements("param").Select(x => (string)x.Attribute("name")), Is.EqualTo(p.Select(x => x.Name)));
                Assert.That(entry.Descendants().Any(x => x.Name == "include" || x.Name == "inheritdoc"), Is.False);
            }
        }
    }
}
