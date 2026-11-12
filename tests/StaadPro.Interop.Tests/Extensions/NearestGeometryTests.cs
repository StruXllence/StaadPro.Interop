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
    public sealed class NearestGeometryTests
    {
        private static Node N(int id,double x,double y=0,double z=0)=>new Node(id,x,y,z);
        private static Beam B(int id,Node a,Node b)=>new Beam(id,a,b);
        [Test]
        public void NullArgumentsIdentifyPublicParameter()
        {
            Assert.That(Assert.Throws<ArgumentNullException>(()=>GeometryDiagnosticsExtensions.GetClosestNode(null,N(1,0))).ParamName,Is.EqualTo("nodesCollection"));
            Assert.That(Assert.Throws<ArgumentNullException>(()=>GeometryDiagnosticsExtensions.GetClosestBeam(null,N(1,0))).ParamName,Is.EqualTo("beamsCollection"));
            Assert.That(Assert.Throws<ArgumentNullException>(()=>new Node[0].GetClosestNode(null)).ParamName,Is.EqualTo("referenceNode"));
            Assert.That(Assert.Throws<ArgumentNullException>(()=>new Beam[0].GetClosestBeam(null)).ParamName,Is.EqualTo("referenceNode"));
        }
        [Test]
        public void NullEntriesAndEmptyInputsHaveNoCandidate()
        {
            var reference=N(1,0);
            Assert.That(new Node[0].GetClosestNode(reference),Is.Null);Assert.That(new Node[]{null,reference,null}.GetClosestNode(reference),Is.Null);
            Assert.That(new Beam[0].GetClosestBeam(reference),Is.Null);Assert.That(new Beam[]{null,null}.GetClosestBeam(reference),Is.Null);
        }
        [Test]
        public void ReferenceEqualityUsesNumberAndInclusiveEuclideanThreshold()
        {
            var reference=N(1,0);var excluded=N(1,.001);var farCopy=N(1,.00101);var otherId=N(2,0);
            Assert.That(new[]{reference,excluded}.GetClosestNode(reference),Is.Null);
            Assert.That(new[]{farCopy}.GetClosestNode(reference),Is.SameAs(farCopy));
            Assert.That(new[]{reference,excluded,farCopy,otherId}.GetClosestNode(reference),Is.SameAs(otherId));
            var diagonal=N(1,.0008,.0008);Assert.That(new[]{diagonal}.GetClosestNode(reference),Is.SameAs(diagonal));
        }
        [Test]
        public void ClosestNodeUsesAllThreeAxesAndRetainsOriginalIdentity()
        {
            var target=N(1,0);var diagonal=N(2,2,2,2);var nearest=N(3,0,0,3);var distant=N(4,0,4);
            Assert.That(new[]{diagonal,null,distant,nearest}.GetClosestNode(target),Is.SameAs(nearest));
        }
        [Test]
        public void BeamDistanceUsesNearestEndpointRatherThanSegmentProjection()
        {
            var target=N(1,0);var through=B(1,N(2,-100),N(3,100));var near=B(2,N(4,1,1),N(5,20));
            Assert.That(new[]{through,null,near}.GetClosestBeam(target),Is.SameAs(near));
            Assert.That(new[]{B(3,N(6,10),N(7,.5)),near}.GetClosestBeam(target).Id,Is.EqualTo(3));
        }
        [Test]
        public void BeamCoincidenceIsNotExcludedByNodeOrBeamNumber()
        {
            var reference=N(1,0);var beam=B(1,reference,N(2,100));
            Assert.That(new[]{beam}.GetClosestBeam(reference),Is.SameAs(beam));
        }
        [Test]
        public void EqualDistancesRetainFirstNodeAndBeamIncludingDifferentAxisScales()
        {
            var reference=N(1,0);var a=N(2,3,4);var b=N(3,0,0,-5);
            Assert.That(new[]{a,b}.GetClosestNode(reference),Is.SameAs(a));Assert.That(new[]{b,a}.GetClosestNode(reference),Is.SameAs(b));
            var x=B(1,a,N(4,20));var y=B(2,N(5,20),b);
            Assert.That(new[]{x,y}.GetClosestBeam(reference),Is.SameAs(x));Assert.That(new[]{y,x}.GetClosestBeam(reference),Is.SameAs(y));
        }
        [TestCase(0)] [TestCase(1)] [TestCase(2)]
        public void NonfiniteCandidateCoordinatesFailEvenAfterCoincidentMatch(int axis)
        {
            foreach(double bad in new[]{double.NaN,double.PositiveInfinity,double.NegativeInfinity})
            {
                var reference=N(1,0);var invalid=N(3,axis==0?bad:0,axis==1?bad:0,axis==2?bad:0);var zero=N(2,0);
                Assert.That(Assert.Throws<ArgumentException>(()=>new[]{zero,invalid}.GetClosestNode(reference)).ParamName,Is.EqualTo("nodesCollection"));
                Assert.That(Assert.Throws<ArgumentException>(()=>new[]{B(1,zero,zero),B(2,zero,invalid)}.GetClosestBeam(reference)).ParamName,Is.EqualTo("beamsCollection"));
            }
        }
        [TestCase(double.NaN)] [TestCase(double.PositiveInfinity)] [TestCase(double.NegativeInfinity)]
        public void InvalidReferenceRejectsBeforeEnumerationEvenForEmptyInput(double bad)
        {
            for(int axis=0;axis<3;axis++)
            {
                var target=N(1,axis==0?bad:0,axis==1?bad:0,axis==2?bad:0);var nodes=new Once<Node>(new Node[0]);var beams=new Once<Beam>(new Beam[0]);
                Assert.That(Assert.Throws<ArgumentException>(()=>nodes.GetClosestNode(target)).ParamName,Is.EqualTo("referenceNode"));
                Assert.Throws<ArgumentException>(()=>beams.GetClosestBeam(target));Assert.That(nodes.Count+beams.Count,Is.Zero);
            }
        }
        [TestCase(true)] [TestCase(false)]
        public void MissingEndpointFailsEvenAfterZeroDistance(bool startMissing)
        {
            var reference=N(1,0);var invalid=new Beam(startMissing?null:N(2,0),startMissing?N(2,0):null);
            Assert.Throws<ArgumentException>(()=>new[]{B(1,reference,reference),invalid}.GetClosestBeam(reference));
        }
        [Test]
        public void InputsAreEnumeratedOnceAndReferenceCoordinatesAreCapturedBeforeEnumeration()
        {
            var reference=N(1,0);var near=N(2,1);var far=N(3,99);var nodes=new Once<Node>(Mutate(reference,new[]{near,far}));
            Assert.That(nodes.GetClosestNode(reference),Is.SameAs(near));Assert.That(nodes.Count,Is.EqualTo(1));
            reference.X=0;var a=B(1,near,near);var b=B(2,far,far);var beams=new Once<Beam>(Mutate(reference,new[]{a,b}));
            Assert.That(beams.GetClosestBeam(reference),Is.SameAs(a));Assert.That(beams.Count,Is.EqualTo(1));
        }
        [Test]
        public void EnumeratorFailuresPropagateWithoutEarlyReturnAtZero()
        {
            var error=new InvalidOperationException("enumerator failure");var reference=N(1,0);var coincident=N(2,0);
            Assert.That(Assert.Throws<InvalidOperationException>(()=>Fail(coincident,error).GetClosestNode(reference)),Is.SameAs(error));
            Assert.That(Assert.Throws<InvalidOperationException>(()=>Fail(B(1,reference,reference),error).GetClosestBeam(reference)),Is.SameAs(error));
        }
        [Test]
        public void HugeDistancesRemainOrderedWhenSquaresOrCoordinateSubtractionOverflow()
        {
            var target=N(1,-double.MaxValue);var far=N(2,double.MaxValue);var nearer=N(3,double.MaxValue/2);
            Assert.That(new[]{far,nearer}.GetClosestNode(target),Is.SameAs(nearer));
            Assert.That(new[]{B(1,far,far),B(2,nearer,nearer)}.GetClosestBeam(target).Id,Is.EqualTo(2));
            target=N(1,0);far=N(2,double.MaxValue,double.MaxValue);nearer=N(3,double.MaxValue,0);
            Assert.That(new[]{far,nearer}.GetClosestNode(target),Is.SameAs(nearer));
        }
        [Test]
        public void SubnormalDistancesDoNotCollapseIntoZeroTies()
        {
            var target=N(1,0);var far=N(2,2*double.Epsilon);var near=N(3,double.Epsilon);
            Assert.That(new[]{far,near}.GetClosestNode(target),Is.SameAs(near));
            var diagonal=N(4,double.Epsilon,double.Epsilon);
            Assert.That(new[]{diagonal,near}.GetClosestNode(target),Is.SameAs(near));
            Assert.That(new[]{B(1,diagonal,diagonal),B(2,near,near)}.GetClosestBeam(target).Id,Is.EqualTo(2));
        }
        [Test]
        public void SelectionMatchesDirectEuclideanOrderingForModerateRandomGeometry()
        {
            var random=new Random(26431);
            for(int batch=0;batch<50;batch++)
            {
                var target=N(-1,random.Next(-10,10),random.Next(-10,10),random.Next(-10,10));
                var nodes=Enumerable.Range(1,30).Select(i=>N(i,random.NextDouble()*100,random.NextDouble()*100,random.NextDouble()*100)).ToArray();
                Func<Node,double> square=n=>Math.Pow(n.X-target.X,2)+Math.Pow(n.Y-target.Y,2)+Math.Pow(n.Z-target.Z,2);
                Assert.That(nodes.GetClosestNode(target),Is.SameAs(nodes.OrderBy(square).First()));
                var beams=Enumerable.Range(0,15).Select(i=>B(i,nodes[2*i],nodes[2*i+1])).ToArray();
                Assert.That(beams.GetClosestBeam(target),Is.SameAs(beams.OrderBy(b=>Math.Min(square(b.StartNode),square(b.EndNode))).First()));
            }
        }
        private static IEnumerable<T> Mutate<T>(Node reference,IEnumerable<T> items){reference.X=100;foreach(var item in items)yield return item;}
        private static IEnumerable<T> Fail<T>(T first,Exception error){yield return first;throw error;}
        private sealed class Once<T>:IEnumerable<T>
        {
            private readonly IEnumerable<T> items;internal int Count;
            internal Once(IEnumerable<T> items){this.items=items;}
            public IEnumerator<T> GetEnumerator(){if(++Count!=1)throw new Exception("enumerated twice");return items.GetEnumerator();}
            IEnumerator IEnumerable.GetEnumerator()=>GetEnumerator();
        }
    }
}
