using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using StaadPro.Interop.Entities;
using StaadPro.Interop.Extensions;
namespace StaadPro.Interop.Tests.Extensions
{
    [TestFixture]
    public sealed class GeometryDiagnosticsTests
    {
        private static readonly double[] BadTolerances={-1,double.NaN,double.PositiveInfinity,double.NegativeInfinity};
        private static Node N(int id,double x,double y=0,double z=0)=>new Node(id,x,y,z);
        [TestCaseSource(nameof(BadTolerances))]
        public void InvalidTolerancesRejectBeforeEnumeration(double tolerance)
        {
            var nodes=new Once<Node>(new[]{N(1,0)});var beams=new Once<Beam>(new Beam[0]);
            Assert.That(Assert.Throws<ArgumentOutOfRangeException>(()=>nodes.GetDuplicateNodes(tolerance)).ParamName,Is.EqualTo("tolerance"));
            Assert.Throws<ArgumentOutOfRangeException>(()=>beams.GetZeroLengthBeams(tolerance));Assert.That(nodes.Count+beams.Count,Is.Zero);
        }
        [Test]
        public void NullCollectionsAndMissingEndpointsIdentifyPublicInputs()
        {
            Assert.That(Assert.Throws<ArgumentNullException>(()=>GeometryDiagnosticsExtensions.GetDuplicateNodes(null)).ParamName,Is.EqualTo("nodesCollection"));
            Assert.That(Assert.Throws<ArgumentNullException>(()=>GeometryDiagnosticsExtensions.GetZeroLengthBeams(null)).ParamName,Is.EqualTo("beamsCollection"));
            Assert.That(Assert.Throws<ArgumentException>(()=>new[]{new Beam()}.GetZeroLengthBeams()).ParamName,Is.EqualTo("beamsCollection"));
        }
        [Test]
        public void DuplicateGroupsUseInclusivePerAxisSeedPartitionWithoutTransitiveClustering()
        {
            var a=N(1,0);var b=N(2,.75,.75,.75);var c=N(3,1.5);var d=N(4,1.5);
            var groups=new[]{a,b,c,d}.GetDuplicateNodes(1).Select(g=>g.ToArray()).ToArray();
            Assert.That(groups,Has.Length.EqualTo(2));Assert.That(groups[0],Is.EqualTo(new[]{a,b}));Assert.That(groups[1],Is.EqualTo(new[]{c,d}));
            Assert.That(new[]{N(1,0),N(2,1,1,1)}.GetDuplicateNodes(1).Single().Count(),Is.EqualTo(2));
        }
        [Test]
        public void NodeNumbersAreStableKeysAndConflictingCopiesDoNotSilentlyDisappear()
        {
            var a=N(1,0);var b=N(2,0);var groups=new[]{a,a,N(1,0),null,b}.GetDuplicateNodes(0).Single().ToArray();
            Assert.That(groups,Has.Length.EqualTo(2));Assert.That(groups[0],Is.SameAs(a));Assert.That(groups[1],Is.SameAs(b));
            Assert.That(Assert.Throws<ArgumentException>(()=>new[]{N(1,0),N(1,1)}.GetDuplicateNodes()).ParamName,Is.EqualTo("nodesCollection"));
        }
        [Test]
        public void ResultsAreMaterializedOnceReadOnlyAndRetainOriginalReferences()
        {
            var a=N(1,0);var b=N(2,0);var nodes=new Once<Node>(new[]{a,b});var result=nodes.GetDuplicateNodes();
            Assert.That(nodes.Count,Is.EqualTo(1));b.X=100;b.Id=999;
            Assert.That(result.Single().Last(),Is.SameAs(b));Assert.That(result.Single().Count(),Is.EqualTo(2));
            Assert.Throws<NotSupportedException>(()=>((IList<IEnumerable<Node>>)result).Clear());
            Assert.Throws<NotSupportedException>(()=>((IList<Node>)result.Single()).Clear());
            Assert.That(result.Count(),Is.EqualTo(1));Assert.That(nodes.Count,Is.EqualTo(1));
        }
        [TestCase(double.NaN)] [TestCase(double.PositiveInfinity)] [TestCase(double.NegativeInfinity)]
        public void NonfiniteCoordinatesAreErrorsForBothDiagnostics(double value)
        {
            foreach(int axis in new[]{0,1,2})
            {
                var bad=N(1,axis==0?value:0,axis==1?value:0,axis==2?value:0);
                Assert.Throws<ArgumentException>(()=>new[]{bad}.GetDuplicateNodes());
                Assert.Throws<ArgumentException>(()=>new[]{new Beam(bad,N(2,0))}.GetZeroLengthBeams());
            }
        }
        [Test]
        public void NullEntriesAndEmptyCollectionsProduceNoFalseDiagnostic()
        {
            Assert.That(new Node[]{null}.GetDuplicateNodes(),Is.Empty);Assert.That(new Beam[]{null}.GetZeroLengthBeams(),Is.Empty);
            Assert.That(new Node[0].GetDuplicateNodes(),Is.Empty);Assert.That(new Beam[0].GetZeroLengthBeams(),Is.Empty);
            Assert.That(new[]{N(1,0)}.GetDuplicateNodes(),Is.Empty);
        }
        [Test]
        public void ZeroLengthUsesEuclideanDistanceRatherThanPerAxisThreshold()
        {
            var within=new Beam(1,N(1,0),N(2,.3,.4));var outside=new Beam(2,N(3,0),N(4,.4,.4));
            var result=new[]{within,outside}.GetZeroLengthBeams(.5).ToArray();Assert.That(result,Is.EqualTo(new[]{within}));
        }
        [Test]
        public void LargeFiniteDifferencesAndSubnormalLengthsDoNotOverflowOrUnderflowIntoWrongResults()
        {
            var far=new Beam(N(1,double.MaxValue),N(2,-double.MaxValue));Assert.That(new[]{far}.GetZeroLengthBeams(double.MaxValue),Is.Empty);
            var diagonal=new Beam(N(1,0),N(2,double.MaxValue/2,double.MaxValue/2));Assert.That(new[]{diagonal}.GetZeroLengthBeams(double.MaxValue).Single(),Is.SameAs(diagonal));
            var tiny=new Beam(N(1,0),N(2,double.Epsilon,double.Epsilon));Assert.That(new[]{tiny}.GetZeroLengthBeams(double.Epsilon),Is.Empty);
            Assert.That(new[]{tiny}.GetZeroLengthBeams(2*double.Epsilon).Single(),Is.SameAs(tiny));
        }
        [Test]
        public void ZeroToleranceRequiresExactCoincidenceAndSkipsNoRepeatedBeam()
        {
            var zero=new Beam(N(1,0),N(2,0));var nonzero=new Beam(N(3,0),N(4,double.Epsilon));
            var source=new Once<Beam>(new[]{zero,null,zero,nonzero});var result=source.GetZeroLengthBeams(0);
            zero.EndNode.X=1;Assert.That(result.Count(),Is.EqualTo(2));Assert.That(result.First(),Is.SameAs(zero));Assert.That(source.Count,Is.EqualTo(1));
            Assert.Throws<NotSupportedException>(()=>((IList<Beam>)result).Clear());
        }
        [Test]
        public void DefaultThresholdAndNegativeZeroAreConsistent()
        {
            Assert.That(new[]{N(1,0),N(2,.0001)}.GetDuplicateNodes().Single().Count(),Is.EqualTo(2));
            Assert.That(new[]{new Beam(N(1,0),N(2,.0001))}.GetZeroLengthBeams().Count(),Is.EqualTo(1));
            Assert.That(new[]{N(1,-0d),N(2,0)}.GetDuplicateNodes(-0d).Single().Count(),Is.EqualTo(2));
        }
        [Test]
        public void EnumeratorFailuresPropagateBeforeAnyResultIsReturned()
        {
            var error=new InvalidOperationException("input failure");
            IEnumerable<Node> Nodes(){yield return N(1,0);throw error;}
            IEnumerable<Beam> Beams(){yield return new Beam(N(1,0),N(2,0));throw error;}
            Assert.That(Assert.Throws<InvalidOperationException>(()=>Nodes().GetDuplicateNodes()),Is.SameAs(error));
            Assert.That(Assert.Throws<InvalidOperationException>(()=>Beams().GetZeroLengthBeams()),Is.SameAs(error));
        }
        private sealed class Once<T> : IEnumerable<T>
        {
            private readonly IEnumerable<T> values;internal int Count;internal Once(IEnumerable<T> values){this.values=values;}
            public IEnumerator<T> GetEnumerator(){if(++Count>1)throw new Exception("Second enumeration");return values.GetEnumerator();}
            IEnumerator IEnumerable.GetEnumerator()=>GetEnumerator();
        }
    }
}
