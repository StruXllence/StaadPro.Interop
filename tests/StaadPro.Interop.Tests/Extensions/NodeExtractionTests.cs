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
    public sealed class NodeExtractionTests
    {
        private static Node N(int id, double x = 0, double y = 0, double z = 0) => new Node(id, x, y, z);
        private sealed class Once<T> : IEnumerable<T>
        {
            private readonly IEnumerable<T> source;
            internal int Count;
            internal bool Disposed;
            internal Once(IEnumerable<T> source) { this.source = source; }
            public IEnumerator<T> GetEnumerator()
            {
                if (++Count != 1) throw new ApplicationException("Enumerated twice.");
                try { foreach (T item in source) yield return item; }
                finally { Disposed = true; }
            }
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        [Test]
        public void NullSequencesFailAtInvocationWithExactParameterNames()
        {
            Assert.That(Assert.Throws<ArgumentNullException>(() => GeometryDiagnosticsExtensions.GetNodesFromBeams(null)).ParamName, Is.EqualTo("beams"));
            Assert.That(Assert.Throws<ArgumentNullException>(() => GeometryDiagnosticsExtensions.GetNodesFromPlates(null)).ParamName, Is.EqualTo("plates"));
        }

        [Test]
        public void BeamExtractionUsesIdOnlyAndPreservesFirstEndpointReferencesAndOrder()
        {
            Node first = N(8), end = N(3), conflict = N(8, 900), coincident = N(5);
            var source = new Once<Beam>(new[] { new Beam(first, end), new Beam(conflict, coincident), new Beam(end, first) });
            var nodes = source.GetNodesFromBeams();
            Assert.That(nodes.Select(n => n.Id), Is.EqualTo(new[] { 8, 3, 5 }));
            Assert.That(nodes[0], Is.SameAs(first)); Assert.That(nodes[1], Is.SameAs(end)); Assert.That(nodes[2], Is.SameAs(coincident));
            Assert.That(source.Count, Is.EqualTo(1)); Assert.That(source.Disposed, Is.True);
        }

        [Test]
        public void PlateExtractionUsesRawAbcdSlotsSkipsNullsAndKeepsFirstIdAcrossPlates()
        {
            Node a = N(8), b = N(3), conflict = N(8, 900), coincident = N(5), extra = N(1);
            var first = new Plate(a, null, b, a); var second = new Plate(conflict, coincident, extra, b);
            var source = new Once<Plate>(new[] { first, second, first });
            var nodes = source.GetNodesFromPlates();
            Assert.That(nodes.Select(n => n.Id), Is.EqualTo(new[] { 8, 3, 5, 1 }));
            Assert.That(nodes[0], Is.SameAs(a)); Assert.That(nodes[1], Is.SameAs(b));
            Assert.That(nodes[2], Is.SameAs(coincident)); Assert.That(nodes[3], Is.SameAs(extra));
            Assert.That(source.Count, Is.EqualTo(1)); Assert.That(source.Disposed, Is.True);
        }

        [TestCase(0)] [TestCase(-1)] [TestCase(int.MinValue)] [TestCase(int.MaxValue)]
        public void EveryIntegerIdIsAcceptedAndDeduplicatedWithoutPositiveIdRules(int id)
        {
            Node first = N(id), duplicate = N(id, 100);
            var beams = new[] { new Beam(first, duplicate) }.GetNodesFromBeams();
            var plates = new[] { new Plate(null, first, duplicate) }.GetNodesFromPlates();
            Assert.That(beams.Count, Is.EqualTo(1)); Assert.That(beams[0], Is.SameAs(first));
            Assert.That(plates.Count, Is.EqualTo(1)); Assert.That(plates[0], Is.SameAs(first));
        }

        [TestCase(double.NaN)] [TestCase(double.PositiveInfinity)] [TestCase(double.NegativeInfinity)]
        public void UnusedCoordinatesAreNotValidatedOrUsedForEquality(double value)
        {
            var first = N(1, value, value, value); var duplicate = N(1, 4, 5, 6); var other = N(2, value, value, value);
            var beams = new[] { new Beam(first, duplicate), new Beam(other, first) }.GetNodesFromBeams();
            var plates = new[] { new Plate(first, duplicate, other, first) }.GetNodesFromPlates();
            Assert.That(beams.Count, Is.EqualTo(2)); Assert.That(beams[0], Is.SameAs(first)); Assert.That(beams[1], Is.SameAs(other));
            Assert.That(plates.Count, Is.EqualTo(2)); Assert.That(plates[0], Is.SameAs(first)); Assert.That(plates[1], Is.SameAs(other));
        }

        [TestCase(0)] [TestCase(1)] [TestCase(2)]
        public void MissingBeamOrEitherEndpointIsRejectedEvenAfterPriorMatches(int kind)
        {
            var a = N(1); var b = N(2); var valid = new Beam(a, b);
            Beam bad = kind == 0 ? null : kind == 1 ? new Beam(null, a) : new Beam(a, null);
            var source = new Once<Beam>(new[] { valid, valid, bad });
            Assert.That(Assert.Throws<ArgumentException>(() => source.GetNodesFromBeams()).ParamName, Is.EqualTo("beams"));
            Assert.That(source.Disposed, Is.True);
        }

        [Test]
        public void NullPlateEntryFailsEvenAfterItsPriorCornerIdsWereCollected()
        {
            var plate = new Plate(N(1), N(2), N(3)); var source = new Once<Plate>(new[] { plate, plate, null });
            Assert.That(Assert.Throws<ArgumentException>(() => source.GetNodesFromPlates()).ParamName, Is.EqualTo("plates"));
            Assert.That(source.Disposed, Is.True);
        }

        [TestCase(0)] [TestCase(1)] [TestCase(2)] [TestCase(3)]
        public void EachSinglePresentCornerSlotIsValidAndNotReinterpretedAsPlateGeometry(int slot)
        {
            var plate = new Plate(); var node = N(7);
            if (slot == 0) plate.A = node; else if (slot == 1) plate.B = node; else if (slot == 2) plate.C = node; else plate.D = node;
            var nodes = new[] { plate }.GetNodesFromPlates(); Assert.That(nodes.Count, Is.EqualTo(1)); Assert.That(nodes[0], Is.SameAs(node));
        }

        [Test]
        public void EmptySequencesAndEmptyPlatesReturnIndependentMutableLists()
        {
            var beams = Array.Empty<Beam>().GetNodesFromBeams(); var moreBeams = Array.Empty<Beam>().GetNodesFromBeams();
            var plates = new[] { new Plate(), new Plate() }.GetNodesFromPlates(); var morePlates = Array.Empty<Plate>().GetNodesFromPlates();
            Assert.That(beams, Is.Empty); Assert.That(plates, Is.Empty); Assert.That(morePlates, Is.Empty);
            Assert.That(beams, Is.Not.SameAs(moreBeams)); Assert.That(plates, Is.Not.SameAs(morePlates));
            beams.Add(N(1)); plates.Add(N(2)); Assert.That(moreBeams, Is.Empty); Assert.That(morePlates, Is.Empty);
        }

        [Test]
        public void CoincidentAndRepeatedReferencesRetainIdOnlyFirstOccurrenceSemantics()
        {
            var same = N(1); var distinct = N(2);
            var beams = new[] { new Beam(same, same), new Beam(distinct, same) }.GetNodesFromBeams();
            var plates = new[] { new Plate(same, same, distinct, same) }.GetNodesFromPlates();
            Assert.That(beams.Count, Is.EqualTo(2)); Assert.That(beams[0], Is.SameAs(same)); Assert.That(beams[1], Is.SameAs(distinct));
            Assert.That(plates.Count, Is.EqualTo(2)); Assert.That(plates[0], Is.SameAs(same)); Assert.That(plates[1], Is.SameAs(distinct));
        }

        [TestCase(true)] [TestCase(false)]
        public void CapturedIdKeysDoNotFollowMutableNodeHashOrIdChanges(bool beams)
        {
            var first = N(1); var second = N(2); var oldIdDuplicate = N(1, 99);
            IEnumerable<Beam> BeamSource()
            {
                yield return new Beam(first, second);
                first.Id = 3;
                yield return new Beam(first, oldIdDuplicate);
            }
            IEnumerable<Plate> PlateSource()
            {
                yield return new Plate(first, second, null);
                first.Id = 3;
                yield return new Plate(first, oldIdDuplicate, null);
            }
            var result = beams ? BeamSource().GetNodesFromBeams() : PlateSource().GetNodesFromPlates();
            Assert.That(result.Count, Is.EqualTo(3)); Assert.That(result[0], Is.SameAs(first)); Assert.That(result[1], Is.SameAs(second));
            Assert.That(result[2], Is.SameAs(first)); Assert.That(result.Any(n => ReferenceEquals(n, oldIdDuplicate)), Is.False);
            Assert.That(result.Select(n => n.Id), Is.EqualTo(new[] { 3, 2, 3 }));
        }

        [Test]
        public void MembershipAndListsRemainOwnedAfterSourceSlotEditsAndFurtherEnumeration()
        {
            var a = N(1); var b = N(2); var c = N(3); var beam = new Beam(a, b); var plate = new Plate(a, null, c);
            var beamInput = new List<Beam> { beam };
            IEnumerable<Beam> BeamSource() { yield return beamInput[0]; beamInput[0] = new Beam(c, c); }
            IEnumerable<Plate> PlateSource() { yield return plate; plate.A = b; plate.C = b; }
            var fromBeams = BeamSource().GetNodesFromBeams(); var fromPlates = PlateSource().GetNodesFromPlates();
            Assert.That(fromBeams[0], Is.SameAs(a)); Assert.That(fromBeams[1], Is.SameAs(b));
            Assert.That(fromPlates[0], Is.SameAs(a)); Assert.That(fromPlates[1], Is.SameAs(c));
            a.X = 50; Assert.That(fromBeams[0].X, Is.EqualTo(50)); Assert.That(fromPlates[0].X, Is.EqualTo(50));
            fromBeams.Clear(); Assert.That(fromPlates.Count, Is.EqualTo(2)); Assert.That(plate.A, Is.SameAs(b));
            Assert.That(beamInput[0].StartNode, Is.SameAs(c));
        }

        [TestCase(true)] [TestCase(false)]
        public void SourceFailuresPropagateAtInvocationWithoutPartialResultsAndDisposeEnumerators(bool beams)
        {
            var failure = new ApplicationException("source");
            IEnumerable<Beam> BeamSource() { yield return new Beam(N(1), N(2)); throw failure; }
            IEnumerable<Plate> PlateSource() { yield return new Plate(N(1), N(2), N(3)); throw failure; }
            if (beams)
            {
                var source = new Once<Beam>(BeamSource()); Assert.That(Assert.Throws<ApplicationException>(() => source.GetNodesFromBeams()), Is.SameAs(failure)); Assert.That(source.Disposed, Is.True);
            }
            else
            {
                var source = new Once<Plate>(PlateSource()); Assert.That(Assert.Throws<ApplicationException>(() => source.GetNodesFromPlates()), Is.SameAs(failure)); Assert.That(source.Disposed, Is.True);
            }
        }

        [TestCase(true)] [TestCase(false)]
        public void ExactSourceFunctionNamesParametersReturnTypesAndExpandedXmlAreShipped(bool beams)
        {
            string name = beams ? "GetNodesFromBeams" : "GetNodesFromPlates";
            var method = typeof(GeometryDiagnosticsExtensions).GetMethod(name); var parameter = method.GetParameters().Single();
            Assert.That(method.ReturnType, Is.EqualTo(typeof(List<Node>))); Assert.That(parameter.Name, Is.EqualTo(beams ? "beams" : "plates"));
            Assert.That(parameter.ParameterType, Is.EqualTo(beams ? typeof(IEnumerable<Beam>) : typeof(IEnumerable<Plate>)));
            Assert.That(parameter.IsOptional, Is.False); Assert.That(method.GetCustomAttributes(typeof(ExtensionAttribute), false), Has.Length.EqualTo(1));
            var docs = XDocument.Load(Path.ChangeExtension(typeof(GeometryDiagnosticsExtensions).Assembly.Location, ".xml"));
            var entry = docs.Descendants("member").Single(e => ((string)e.Attribute("name")).StartsWith("M:" + method.DeclaringType.FullName + "." + name + "("));
            foreach (string tag in new[] { "summary", "param", "returns", "remarks", "exception", "example" }) Assert.That(entry.Element(tag), Is.Not.Null);
            Assert.That((string)entry.Element("param").Attribute("name"), Is.EqualTo(parameter.Name));
            Assert.That(entry.Descendants().Any(e => e.Name == "include" || e.Name == "inheritdoc"), Is.False);
        }
    }
}
