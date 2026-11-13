using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using NUnit.Framework;
using StaadPro.Interop.Common;
using StaadPro.Interop.Entities;
using StaadPro.Interop.Enums;
using StaadPro.Interop.Extensions;
using StaadPro.Interop.Models.Geometry;

namespace StaadPro.Interop.Tests.Extensions
{
    [TestFixture]
    public sealed class GeometryFilteringAndConnectivityTests
    {
        private static Node N(int id, double x, double y = 0, double z = 0) => new Node(id, x, y, z);
        private static Beam B(Node a, Node b) => new Beam(1, a, b);

        [Test]
        public void RadiusUsesPlanMidpointRatherThanEndpointsSphereOrSegmentIntersection()
        {
            var through = B(N(1,-100,90),N(2,100,90));
            var band = B(N(3,2,80,4),N(4,4,80,4)); // midpoint (3,4), radius 5
            var crossingBand = B(N(5,1),N(6,20)); // crosses radius 5, midpoint outside
            var beams = new[] {through,band,crossingBand};
            Assert.That(beams.GetBeamsWithinRadius(0,0), Is.EqualTo(new[] {through}));
            Assert.That(beams.GetBeamsWithinRadius(5,5), Is.EqualTo(new[] {band}));
            Assert.That(new[] {band}.GetBeamsWithinRadius(0,0,N(99,3,-900,4)), Is.EqualTo(new[] {band}));
        }

        [TestCase(-1,2,"minRadius"),TestCase(double.NaN,2,"minRadius"),TestCase(double.PositiveInfinity,2,"minRadius"),
         TestCase(1,0,"maxRadius"),TestCase(0,double.NaN,"maxRadius"),TestCase(0,double.PositiveInfinity,"maxRadius"),
         TestCase(0,double.NegativeInfinity,"maxRadius")]
        public void InvalidRadialBandsRejectBeforeEnumeration(double min, double max, string parameter)
        {
            var input = new Once<Beam>(new Beam[0], () => Assert.Fail("Must reject before enumeration."));
            Assert.That(Assert.Throws<ArgumentOutOfRangeException>(()=>input.GetBeamsWithinRadius(min,max)).ParamName,Is.EqualTo(parameter));
        }

        [Test]
        public void RadialInputValidationNamesThePublicArgument()
        {
            Assert.That(Assert.Throws<ArgumentNullException>(()=>GeometryDiagnosticsExtensions.GetBeamsWithinRadius(null,0,1)).ParamName,Is.EqualTo("beamsCollection"));
            Assert.That(Assert.Throws<ArgumentException>(()=>new Beam[0].GetBeamsWithinRadius(0,1,N(1,double.NaN))).ParamName,Is.EqualTo("centre"));
            Assert.That(Assert.Throws<ArgumentException>(()=>new[]{new Beam()}.GetBeamsWithinRadius(0,1)).ParamName,Is.EqualTo("beamsCollection"));
        }

        [Test]
        public void RadialResultIsEagerReadOnlyOrderedAndRetainsRepeatedOriginalReferences()
        {
            var beam=B(N(1,2),N(2,4));
            var input=new Once<Beam>(new[]{beam,null,beam});
            var result=input.GetBeamsWithinRadius(3,3);
            beam.StartNode.X=900;
            Assert.That(result.ToArray(),Is.EqualTo(new[]{beam,beam}));Assert.That(result.First(),Is.SameAs(beam));
            Assert.Throws<NotSupportedException>(()=>((IList<Beam>)result).Clear());
            Assert.That(new Beam[]{null}.GetBeamsWithinRadius(0,1),Is.Empty);
        }

        [Test]
        public void RadialCentreIsCapturedBeforeEnumerationAndLaterInvalidInputIsNotHidden()
        {
            var centre=N(1,3);var beam=B(N(2,2),N(3,4));
            var input=new Once<Beam>(new[]{beam},()=>centre.X=100);
            Assert.That(input.GetBeamsWithinRadius(0,0,centre).Single(),Is.SameAs(beam));
            Assert.Throws<ArgumentException>(()=>new[]{beam,new Beam()}.GetBeamsWithinRadius(0,200));
            Assert.Throws<ApplicationException>(()=>Broken<Beam>(beam).GetBeamsWithinRadius(0,200));
        }

        [Test]
        public void RadialArithmeticPreservesHugeMidpointsSubnormalDeltasAndOverflowingDistances()
        {
            var huge=B(N(1,double.MaxValue),N(2,double.MaxValue));
            Assert.That(new[]{huge}.GetBeamsWithinRadius(double.MaxValue,double.MaxValue).Single(),Is.SameAs(huge));
            Assert.That(new[]{huge}.GetBeamsWithinRadius(0,double.MaxValue,N(3,-double.MaxValue)),Is.Empty);
            var opposite=B(N(1,-double.MaxValue),N(2,double.MaxValue));
            Assert.That(new[]{opposite}.GetBeamsWithinRadius(0,0).Single(),Is.SameAs(opposite));
            var tiny=B(N(1,double.Epsilon),N(2,2*double.Epsilon));
            Assert.That(new[]{tiny}.GetBeamsWithinRadius(2*double.Epsilon,2*double.Epsilon).Single(),Is.SameAs(tiny));
            var diagonal=B(N(1,double.Epsilon,0,double.Epsilon),N(2,double.Epsilon,0,double.Epsilon));
            Assert.That(new[]{diagonal}.GetBeamsWithinRadius(0,double.Epsilon),Is.Empty);
        }

        [Test]
        public void RadialModerateDataAgreesWithIndependentSquaredBandPredicate()
        {
            var random=new Random(13579);
            var beams=Enumerable.Range(1,250).Select(i=>B(N(i,random.Next(-50,51),random.Next(-10,11),random.Next(-50,51)),
                N(1000+i,random.Next(-50,51),random.Next(-10,11),random.Next(-50,51)))).ToArray();
            var expected=beams.Where(b=>{double x=(b.StartNode.X+b.EndNode.X)/2,z=(b.StartNode.Z+b.EndNode.Z)/2;return x*x+z*z>=100&&x*x+z*z<=400;}).ToArray();
            Assert.That(beams.GetBeamsWithinRadius(10,20),Is.EqualTo(expected));
        }

        [Test]
        public void BoundsUseAllNodeBeamAndTriangleOrQuadrilateralCornerCoordinates()
        {
            var a=N(1,-5,3,9);var b=N(2,7,-2,-4);var c=N(3,2,10,1);var d=N(4,-8,-6,12);
            var nodeBounds=new Once<Node>(new[]{a,null,b,a}).GetBoundingBox();
            AssertBounds(nodeBounds,-5,7,-2,3,-4,9);
            AssertBounds(new[]{B(a,b),B(c,d)}.GetBoundingBox(),-8,7,-6,10,-4,12);
            AssertBounds(new[]{new Plate(1,a,b,c)}.GetBoundingBox(),-5,7,-2,10,-4,9);
            AssertBounds(new[]{new Plate(1,a,b,c,d)}.GetBoundingBox(),-8,7,-6,10,-4,12);
            a.X=500;Assert.That(nodeBounds.XMin,Is.EqualTo(-5));
        }

        [Test]
        public void BoundsAllowPointsAndExtremeFiniteCoordinatesWithoutSubtractingExtents()
        {
            AssertBounds(new[]{N(1,2,3,4)}.GetBoundingBox(),2,2,3,3,4,4);
            AssertBounds(new[]{N(1,-double.MaxValue),N(2,double.MaxValue)}.GetBoundingBox(),-double.MaxValue,double.MaxValue,0,0,0,0);
            AssertBounds(new[]{N(1,double.Epsilon)}.GetBoundingBox(),double.Epsilon,double.Epsilon,0,0,0,0);
        }

        [Test]
        public void BoundsRejectNullEmptyUnsupportedAndMalformedInputs()
        {
            Assert.That(Assert.Throws<ArgumentNullException>(()=>GeometryDiagnosticsExtensions.GetBoundingBox<Node>(null)).ParamName,Is.EqualTo("source"));
            Assert.Throws<InvalidOperationException>(()=>new Node[0].GetBoundingBox());
            Assert.Throws<InvalidOperationException>(()=>new Node[]{null}.GetBoundingBox());
            Assert.Throws<InvalidOperationException>(()=>new IEntity[]{N(1,0)}.GetBoundingBox());
            Assert.Throws<InvalidOperationException>(()=>new Once<Member>(new Member[0],()=>Assert.Fail("Unsupported T must reject before enumeration.")).GetBoundingBox());
            Assert.Throws<ArgumentException>(()=>new[]{new Beam()}.GetBoundingBox());
            Assert.Throws<ArgumentException>(()=>new[]{new Plate(null,N(1,0),N(2,0))}.GetBoundingBox());
            Assert.Throws<ArgumentException>(()=>new[]{new Plate(N(1,0),null,N(2,0))}.GetBoundingBox());
            Assert.Throws<ArgumentException>(()=>new[]{new Plate(N(1,0),N(2,0),null)}.GetBoundingBox());
            Assert.Throws<ApplicationException>(()=>Broken(N(1,0)).GetBoundingBox());
        }

        [TestCase(double.NaN),TestCase(double.PositiveInfinity),TestCase(double.NegativeInfinity)]
        public void AllOperationsRejectNonfiniteParticipatingGeometry(double value)
        {
            foreach(int axis in new[]{0,1,2})
            {
                var bad=N(9,axis==0?value:0,axis==1?value:0,axis==2?value:0);
                var good=N(1,0);var valid=B(good,N(2,1));
                Assert.Throws<ArgumentException>(()=>new[]{B(good,bad)}.GetBeamsWithinRadius(0,1));
                Assert.Throws<ArgumentException>(()=>new[]{valid}.GetBeamsWithinRadius(0,1,bad));
                Assert.Throws<ArgumentException>(()=>new[]{bad}.GetBoundingBox());
                Assert.Throws<ArgumentException>(()=>new[]{B(good,bad)}.GetBoundingBox());
                Assert.Throws<ArgumentException>(()=>new[]{new Plate(good,N(2,0),N(3,0),bad)}.GetBoundingBox());
                Assert.Throws<ArgumentException>(()=>valid.IsConnectedToBeam(B(good,bad)));
            }
        }

        [TestCase(false,false),TestCase(false,true),TestCase(true,false),TestCase(true,true)]
        public void BeamConnectivityMatchesEveryEndpointPairAndIsSymmetric(bool leftEnd,bool rightEnd)
        {
            var shared=N(17,1,2,3);var copy=N(17,1,2,3);
            var a=leftEnd?B(N(1,100),shared):B(shared,N(1,100));
            var b=rightEnd?B(N(2,-100),copy):B(copy,N(2,-100));
            Assert.That(a.IsConnectedToBeam(b),Is.True);Assert.That(b.IsConnectedToBeam(a),Is.True);
            Assert.That(a.IsConnectedToBeam(a),Is.True);
        }

        [TestCase(0,0,true),TestCase(.001,0,true),TestCase(.00100001,0,false),TestCase(.0008,.0008,false)]
        public void ConnectivityUsesInclusiveEuclideanToleranceAndMatchingNodeId(double dx,double dy,bool expected)
        {
            var node=N(1,0);var candidate=N(1,dx,dy);var beam=B(node,N(2,50));
            Assert.That(beam.IsConnectedToBeam(B(candidate,N(3,60))),Is.EqualTo(expected));
            candidate.Id=900;Assert.That(beam.IsConnectedToBeam(B(candidate,N(3,60))),Is.False);
        }

        [Test]
        public void ConnectivityRejectsSegmentCrossingsAndKeepsDraftEqualityWithoutNativeIdRestrictions()
        {
            var beam=B(N(1,-2),N(2,2));
            Assert.That(beam.IsConnectedToBeam(B(N(3,0,-2),N(4,0,2))),Is.False);
            var draft=B(N(0,0),N(-1,1));
            Assert.That(draft.IsConnectedToBeam(B(N(0,0),N(-2,2))),Is.True);
            Assert.That(B(N(1,-double.MaxValue),N(2,0)).IsConnectedToBeam(B(N(1,double.MaxValue),N(3,1))),Is.False);
            Assert.That(B(N(1,double.Epsilon),N(2,1)).IsConnectedToBeam(B(N(1,2*double.Epsilon),N(3,2))),Is.True);
        }

        [Test]
        public void ConnectivityNullAndMissingInputsHaveSpecificPublicParameterNames()
        {
            var a=N(1,0);var valid=B(a,N(2,1));
            Assert.That(Assert.Throws<ArgumentNullException>(()=>BeamExtensions.IsConnectedToBeam(null,valid)).ParamName,Is.EqualTo("beam"));
            Assert.That(Assert.Throws<ArgumentNullException>(()=>valid.IsConnectedToBeam(null)).ParamName,Is.EqualTo("targetBeam"));
            Assert.That(Assert.Throws<ArgumentException>(()=>valid.IsConnectedToBeam(new Beam())).ParamName,Is.EqualTo("targetBeam"));
            Assert.That(Assert.Throws<ArgumentException>(()=>new Beam().IsConnectedToBeam(valid)).ParamName,Is.EqualTo("beam"));
        }

        [Test]
        public void BoundsModelAndThreePublicContractsHaveCompleteExpandedXml()
        {
            var assembly=typeof(Node).Assembly;var xml=XDocument.Load(Path.ChangeExtension(assembly.Location,".xml"));
            var methods=new[]{typeof(GeometryDiagnosticsExtensions).GetMethod("GetBeamsWithinRadius"),
                typeof(GeometryDiagnosticsExtensions).GetMethod("GetBoundingBox"),typeof(BeamExtensions).GetMethod("IsConnectedToBeam")};
            foreach(var method in methods)
            {
                Assert.That(method.GetParameters().Any(p=>p.IsOut||p.ParameterType.IsByRef),Is.False);
                var docs=xml.Descendants("member").Single(e=>((string)e.Attribute("name")).StartsWith("M:"+method.DeclaringType.FullName+"."+method.Name+(method.IsGenericMethod?"``1(":"(")));
                foreach(string tag in new[]{"summary","param","returns","remarks","exception","example"})
                    Assert.That(docs.Element(tag),Is.Not.Null,method.Name+"/"+tag);
                Assert.That(docs.Elements("param").Select(e=>(string)e.Attribute("name")),Is.EqualTo(method.GetParameters().Select(p=>p.Name)));
                Assert.That(docs.Descendants().Any(e=>e.Name=="include"||e.Name=="inheritdoc"),Is.False);
            }
            var bounds=typeof(AxisAlignedBounds);Assert.That(bounds.IsSealed,Is.True);Assert.That(bounds.GetConstructors(),Is.Empty);
            Assert.That(bounds.GetProperties().Select(p=>p.Name),Is.EquivalentTo(new[]{"XMin","XMax","YMin","YMax","ZMin","ZMax"}));
            Assert.That(bounds.GetProperties().All(p=>p.PropertyType==typeof(double)&&p.GetSetMethod(true)==null),Is.True);
            foreach(var property in bounds.GetProperties())
            {
                var doc=xml.Descendants("member").Single(e=>(string)e.Attribute("name")=="P:"+bounds.FullName+"."+property.Name);
                Assert.That(doc.Element("summary"),Is.Not.Null);Assert.That(doc.Element("value"),Is.Not.Null);
            }
        }

        private static void AssertBounds(AxisAlignedBounds b,params double[] expected)
            => Assert.That(new[]{b.XMin,b.XMax,b.YMin,b.YMax,b.ZMin,b.ZMax},Is.EqualTo(expected));
        private static IEnumerable<T> Broken<T>(T first) {yield return first;throw new ApplicationException("source failed");}
        private sealed class Once<T>:IEnumerable<T>
        {
            private readonly IEnumerable<T> values;private readonly Action before;private bool enumerated;
            internal Once(IEnumerable<T> values,Action before=null){this.values=values;this.before=before;}
            public IEnumerator<T> GetEnumerator(){if(enumerated)throw new InvalidOperationException("Repeated enumeration.");enumerated=true;before?.Invoke();return values.GetEnumerator();}
            IEnumerator IEnumerable.GetEnumerator()=>GetEnumerator();
        }
    }
}
