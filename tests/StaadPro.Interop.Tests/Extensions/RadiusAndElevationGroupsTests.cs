using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using NUnit.Framework;
using StaadPro.Interop.Entities;
using StaadPro.Interop.Models;
using StaadPro.Interop.Services;

namespace StaadPro.Interop.Tests.Extensions
{
    [TestFixture]
    public sealed class RadiusAndElevationGroupsTests
    {
        private static Node N(double x = 0, double y = 0, double z = 0) => new Node(0, x, y, z);
        private static Beam At(double key, bool radius) => radius
            ? new Beam(N(key), N(0, 20, key)) : new Beam(N(0, key), N(8, key, 9));
        private static List<BeamGroup<double>> Group(IEnumerable<Beam> beams, bool radius, double tolerance = .001) => radius
            ? BeamGroupingService.GroupBeamsByRadius(beams, N(), tolerance)
            : BeamGroupingService.GroupBeamsByElevation(beams, tolerance);
        private sealed class Once<T> : IEnumerable<T>
        {
            private readonly IEnumerable<T> source;
            internal int Count;
            internal Once(IEnumerable<T> source) { this.source = source; }
            public IEnumerator<T> GetEnumerator()
            {
                if (++Count != 1) throw new ApplicationException("Enumerated twice.");
                return source.GetEnumerator();
            }
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        [TestCase(true)] [TestCase(false)]
        public void NullInputFailsAtInvocation(bool radius)
        {
            Assert.That(Assert.Throws<ArgumentNullException>(() => Group(null, radius)).ParamName, Is.EqualTo("beams"));
        }

        [Test]
        public void RadiusOriginIsRequiredAndQualifiedEvenForEmptyInput()
        {
            Assert.That(Assert.Throws<ArgumentNullException>(() => BeamGroupingService.GroupBeamsByRadius(Array.Empty<Beam>(), null)).ParamName, Is.EqualTo("origin"));
            foreach (Node origin in new[] { N(double.NaN), N(0, double.PositiveInfinity), N(0, 0, double.NegativeInfinity) })
                Assert.That(Assert.Throws<ArgumentException>(() => BeamGroupingService.GroupBeamsByRadius(Array.Empty<Beam>(), origin)).ParamName, Is.EqualTo("origin"));
        }

        [TestCase(true, -1d)] [TestCase(false, -1d)]
        [TestCase(true, double.NaN)] [TestCase(false, double.NaN)]
        [TestCase(true, double.PositiveInfinity)] [TestCase(false, double.PositiveInfinity)]
        [TestCase(true, double.NegativeInfinity)] [TestCase(false, double.NegativeInfinity)]
        public void LengthToleranceMustBeFiniteNonnegative(bool radius, double tolerance)
        {
            Assert.That(Assert.Throws<ArgumentOutOfRangeException>(() => Group(Array.Empty<Beam>(), radius, tolerance)).ParamName, Is.EqualTo("tolerance"));
        }

        [TestCase(true, 0)] [TestCase(false, 0)]
        [TestCase(true, 1)] [TestCase(false, 1)]
        [TestCase(true, 2)] [TestCase(false, 2)]
        public void MissingBeamOrEitherEndpointIsRejected(bool radius, int kind)
        {
            Beam bad = kind == 0 ? null : kind == 1 ? new Beam(null, N()) : new Beam(N(), null);
            Assert.That(Assert.Throws<ArgumentException>(() => Group(new[] { At(1, radius), bad }, radius)).ParamName, Is.EqualTo("beams"));
        }

        [TestCase(true, double.NaN)] [TestCase(false, double.NaN)]
        [TestCase(true, double.PositiveInfinity)] [TestCase(false, double.PositiveInfinity)]
        [TestCase(true, double.NegativeInfinity)] [TestCase(false, double.NegativeInfinity)]
        public void AllCoordinatesAreQualifiedIncludingUnusedAxesAndIneligibleLaterBeams(bool radius, double value)
        {
            foreach (bool start in new[] { true, false })
                for (int axis = 0; axis < 3; axis++)
                {
                    var bad = new Beam(N(1, 2, 3), N(4, 5, 6)); Node node = start ? bad.StartNode : bad.EndNode;
                    if (axis == 0) node.X = value; else if (axis == 1) node.Y = value; else node.Z = value;
                    Assert.That(Assert.Throws<ArgumentException>(() => Group(new[] { At(1, radius), bad }, radius)).ParamName, Is.EqualTo("beams"));
                }
        }

        [TestCase(true)] [TestCase(false)]
        public void ResultsSortKeysAndRetainInputOrderDuplicatesAndReferenceIdentity(bool radius)
        {
            Beam high = At(5, radius), first = At(2, radius), second = At(2, radius);
            var source = new Once<Beam>(new[] { high, first, second, first });
            var groups = Group(source, radius);
            Assert.That(source.Count, Is.EqualTo(1)); Assert.That(groups.Select(g => g.Key), Is.EqualTo(new[] { 2d, 5d }));
            Assert.That(groups[0].Beams.Count, Is.EqualTo(3));
            Assert.That(groups[0].Beams[0], Is.SameAs(first)); Assert.That(groups[0].Beams[1], Is.SameAs(second)); Assert.That(groups[0].Beams[2], Is.SameAs(first));
            Assert.That(groups[0].Beams, Is.Not.SameAs(groups[1].Beams));
        }

        [TestCase(true)] [TestCase(false)]
        public void FirstFixedSeedIsOrderDependentNontransitiveAndWinsAmbiguousMatches(bool radius)
        {
            var a = At(1, radius); var b = At(1.75, radius); var c = At(2.5, radius);
            var groups = Group(new[] { a, b, c }, radius, 1);
            Assert.That(groups.Select(g => g.Key), Is.EqualTo(new[] { 1d, 2.5d }));
            Assert.That(groups[0].Beams[1], Is.SameAs(b));
            var reversed = Group(new[] { c, b, a }, radius, 1);
            Assert.That(reversed.Single(g => g.Key == 2.5).Beams[1], Is.SameAs(b));
            var five = At(5, radius); var seven = At(7, radius); var six = At(6, radius);
            var ambiguous = Group(new[] { seven, five, six }, radius, 1);
            Assert.That(ambiguous.Single(g => g.Key == 7).Beams[1], Is.SameAs(six));
            Assert.That(ambiguous.Single(g => g.Key == 5).Beams.Count, Is.EqualTo(1));
        }

        [TestCase(true)] [TestCase(false)]
        public void EndpointAndSeedToleranceBoundariesAreInclusiveAndSeedsUseStartValue(bool radius)
        {
            Beam beam = radius ? new Beam(N(2), N(2.125)) : new Beam(N(0, 2), N(0, 2.125));
            Assert.That(Group(new[] { beam }, radius, .125).Single().Key, Is.EqualTo(2));
            Assert.That(Group(new[] { beam }, radius, .124), Is.Empty);
            Beam reversed = new Beam(beam.EndNode, beam.StartNode);
            Assert.That(Group(new[] { reversed }, radius, .125).Single().Key, Is.EqualTo(2.125));
            Assert.That(Group(new[] { At(2, radius), At(2.125, radius) }, radius, .125).Count, Is.EqualTo(1));
            Assert.That(Group(new[] { At(2, radius), At(2.125, radius) }, radius, .124).Count, Is.EqualTo(2));
        }

        [TestCase(true)] [TestCase(false)]
        public void ZeroToleranceRequiresExactComputedEqualityAndDefaultRemainsPointZeroZeroOne(bool radius)
        {
            Assert.That(Group(new[] { At(1, radius), At(1.0005, radius) }, radius).Count, Is.EqualTo(1));
            Assert.That(Group(new[] { At(1, radius), At(1.002, radius) }, radius).Count, Is.EqualTo(2));
            Assert.That(Group(new[] { At(1, radius), At(1.0005, radius) }, radius, 0).Count, Is.EqualTo(2));
            Assert.That(Group(new[] { At(1, radius), At(1, radius) }, radius, -0d).Count, Is.EqualTo(1));
        }

        [Test]
        public void RadiusUsesTranslatedXzHypotenuseIgnoresHeightAndAllowsOriginProjection()
        {
            var origin = N(10, 100, 20);
            var chord = new Beam(N(13, -500, 24), N(7, 800, 24));
            var groups = BeamGroupingService.GroupBeamsByRadius(new[] { chord }, origin, 0);
            Assert.That(groups.Single().Key, Is.EqualTo(5)); Assert.That(groups[0].Beams[0], Is.SameAs(chord));
            var zero = new Beam(N(10, -2, 20), N(10, 2, 20));
            Assert.That(BeamGroupingService.GroupBeamsByRadius(new[] { zero }, origin, 0).Single().Key, Is.Zero);
        }

        [Test]
        public void ElevationUsesSignedYIgnoresPlanDirectionAndSkipsSlopingBeams()
        {
            var low = new Beam(N(-5, -3, 8), N(9, -3, -10)); var high = At(4, false);
            var slope = new Beam(N(0, 1), N(0, 2));
            var groups = BeamGroupingService.GroupBeamsByElevation(new[] { high, slope, low });
            Assert.That(groups.Select(g => g.Key), Is.EqualTo(new[] { -3d, 4d }));
            Assert.That(groups[0].Beams.Single(), Is.SameAs(low));
        }

        [TestCase(true)] [TestCase(false)]
        public void CoincidentDraftEndpointsAndEmptySelectionsHaveExplicitSemantics(bool radius)
        {
            var point = N(); point.Id = -1; var beam = new Beam(point, point) { Id = -2 };
            Assert.That(Group(new[] { beam }, radius).Single().Key, Is.Zero);
            var first = Group(Array.Empty<Beam>(), radius); var second = Group(Array.Empty<Beam>(), radius);
            Assert.That(first, Is.Empty); Assert.That(first, Is.Not.SameAs(second));
        }

        [TestCase(1e200)] [TestCase(1e-200)] [TestCase(double.Epsilon)] [TestCase(double.MaxValue)]
        public void ScaledRadiusAvoidsSquaringOverflowAndUnderflow(double value)
        {
            var groups = BeamGroupingService.GroupBeamsByRadius(new[] { new Beam(N(value), N(0, 0, value)) }, N(), 0);
            Assert.That(groups.Single().Key, Is.EqualTo(value));
        }

        [Test]
        public void ScaledTwoAxisRadiusAndTinyNonzeroGroupsRemainDistinct()
        {
            double huge = 1e200; var diagonal = new Beam(N(huge, 0, huge), N(-huge, 0, huge));
            Assert.That(BeamGroupingService.GroupBeamsByRadius(new[] { diagonal }, N(), 0).Single().Key / huge, Is.EqualTo(Math.Sqrt(2)).Within(1e-15));
            Assert.That(Group(new[] { At(double.Epsilon, true), At(2 * double.Epsilon, true) }, true, 0).Count, Is.EqualTo(2));
        }

        [Test]
        public void UnrepresentableRadiusThrowsInsteadOfCreatingInfiniteSeeds()
        {
            foreach (var pair in new[]
            {
                new { Origin = N(), Beam = new Beam(N(double.MaxValue, 0, double.MaxValue), N(1)) },
                new { Origin = N(-double.MaxValue), Beam = new Beam(N(double.MaxValue), N()) },
                new { Origin = N(), Beam = new Beam(N(1), N(double.MaxValue, 0, double.MaxValue)) }
            })
                Assert.That(Assert.Throws<ArgumentException>(() => BeamGroupingService.GroupBeamsByRadius(new[] { pair.Beam }, pair.Origin)).ParamName, Is.EqualTo("beams"));
        }

        [Test]
        public void FiniteElevationDifferencesBeyondDoubleRangeDoNotMatchAnyFiniteTolerance()
        {
            var opposite = new Beam(N(0, -double.MaxValue), N(0, double.MaxValue));
            Assert.That(BeamGroupingService.GroupBeamsByElevation(new[] { opposite }, double.MaxValue), Is.Empty);
            var groups = BeamGroupingService.GroupBeamsByElevation(new[] { At(-double.MaxValue, false), At(double.MaxValue, false) }, double.MaxValue);
            Assert.That(groups.Count, Is.EqualTo(2));
            Assert.That(groups[0].Key, Is.EqualTo(-double.MaxValue));
        }

        [TestCase(true)] [TestCase(false)]
        public void CapturedValuesSurviveEnumeratorMutationsAndResultsOwnTheirLists(bool radius)
        {
            var origin = N(); var beam = At(2, radius);
            IEnumerable<Beam> Source()
            {
                yield return beam;
                beam.StartNode.X = 90; beam.StartNode.Y = 90; beam.EndNode.Y = -90;
                origin.X = 40;
            }
            var source = new Once<Beam>(Source());
            var groups = radius ? BeamGroupingService.GroupBeamsByRadius(source, origin) : BeamGroupingService.GroupBeamsByElevation(source);
            Assert.That(source.Count, Is.EqualTo(1)); Assert.That(groups.Single().Key, Is.EqualTo(2));
            Assert.That(groups[0].Beams.Single(), Is.SameAs(beam));
            var fresh = Group(new[] { At(2, radius) }, radius); groups[0].Key = -7; groups[0].Beams.Clear(); groups.Clear();
            Assert.That(fresh.Single().Key, Is.EqualTo(2)); Assert.That(fresh[0].Beams.Count, Is.EqualTo(1));
        }

        [TestCase(true)] [TestCase(false)]
        public void EnumerationFailurePropagatesWithoutPartialReturn(bool radius)
        {
            var failure = new ApplicationException("source");
            IEnumerable<Beam> Source() { yield return At(2, radius); throw failure; }
            Assert.That(Assert.Throws<ApplicationException>(() => Group(Source(), radius)), Is.SameAs(failure));
        }

        [TestCase(true)] [TestCase(false)]
        public void ExactOriginalSignaturesDefaultsAndExpandedIntelliSenseAreAvailable(bool radius)
        {
            var method = typeof(BeamGroupingService).GetMethod(radius ? "GroupBeamsByRadius" : "GroupBeamsByElevation");
            Assert.That(method.ReturnType, Is.EqualTo(typeof(List<BeamGroup<double>>)));
            Assert.That(method.GetParameters().Select(p => p.Name), Is.EqualTo(radius ? new[] { "beams", "origin", "tolerance" } : new[] { "beams", "tolerance" }));
            Assert.That(method.GetParameters().Select(p => p.ParameterType), Is.EqualTo(radius ? new[] { typeof(IEnumerable<Beam>), typeof(Node), typeof(double) } : new[] { typeof(IEnumerable<Beam>), typeof(double) }));
            Assert.That(method.GetParameters().Last().IsOptional, Is.True); Assert.That(method.GetParameters().Last().DefaultValue, Is.EqualTo(.001));
            Assert.That(method.GetParameters().Take(method.GetParameters().Length - 1).Any(p => p.IsOptional), Is.False);
            var docs = XDocument.Load(Path.ChangeExtension(typeof(BeamGroupingService).Assembly.Location, ".xml"));
            var entry = docs.Descendants("member").Single(e => ((string)e.Attribute("name")).StartsWith("M:" + method.DeclaringType.FullName + "." + method.Name + "("));
            foreach (string tag in new[] { "summary", "returns", "remarks", "exception", "example" }) Assert.That(entry.Element(tag), Is.Not.Null);
            Assert.That(entry.Elements("param").Select(e => (string)e.Attribute("name")), Is.EqualTo(method.GetParameters().Select(p => p.Name)));
            Assert.That(entry.Descendants().Any(e => e.Name == "include" || e.Name == "inheritdoc"), Is.False);
        }
    }
}
