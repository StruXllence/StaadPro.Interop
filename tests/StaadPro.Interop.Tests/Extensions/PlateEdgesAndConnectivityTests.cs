using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using NUnit.Framework;
using StaadPro.Interop.Entities;
using StaadPro.Interop.Enums;
using StaadPro.Interop.Extensions;
using StaadPro.Interop.Models.Geometry;
namespace StaadPro.Interop.Tests.Extensions
{
    [TestFixture]
    public sealed class PlateEdgesAndConnectivityTests
    {
        private static Node N(int id,double x=0,double y=0,double z=0)=>new Node(id,x,y,z);
        private static Plate Triangle()=>new Plate(N(1),N(2,1),N(3,0,1));
        [Test]
        public void NullArgumentsFailAtInvocationAndIdentifyTheParameter()
        {
            Assert.That(Assert.Throws<ArgumentNullException>(()=>PlateExtensions.GetEdges(null)).ParamName,Is.EqualTo("plate"));
            Assert.That(Assert.Throws<ArgumentNullException>(()=>PlateExtensions.IsConnectedToPlate(null,new Plate())).ParamName,Is.EqualTo("plate"));
            Assert.That(Assert.Throws<ArgumentNullException>(()=>new Plate().IsConnectedToPlate(null)).ParamName,Is.EqualTo("targetPlate"));
        }
        [TestCase(-1)] [TestCase(2)] [TestCase(int.MaxValue)]
        public void UndefinedScopesFailBeforeMalformedGeometryIsRead(int scope)
        {var bad=new Plate(N(1,double.NaN),null,null);Assert.That(Assert.Throws<ArgumentOutOfRangeException>(()=>bad.IsConnectedToPlate(bad,(PlateConnectivityScope)scope)).ParamName,Is.EqualTo("scope"));}
        [TestCase(false)] [TestCase(true)]
        public void TriangleEdgesPreserveWindingAndClosingOriginalReferences(bool repeatFourth)
        {
            var a=N(1);var b=N(2,1);var c=N(3,0,1);var plate=new Plate(a,b,c,repeatFourth?N(1):null);var edges=plate.GetEdges().ToArray();
            Assert.That(edges.Length,Is.EqualTo(3));Assert.That(edges[0].Start,Is.SameAs(a));Assert.That(edges[0].End,Is.SameAs(b));Assert.That(edges[1].Start,Is.SameAs(b));Assert.That(edges[1].End,Is.SameAs(c));Assert.That(edges[2].Start,Is.SameAs(c));Assert.That(edges[2].End,Is.SameAs(a));
        }
        [Test]
        public void QuadrilateralEdgesUseAllFourRetainedCornersWithoutSorting()
        {
            var nodes=new[]{N(8),N(2,1),N(7,1,1),N(1,0,1)};var edges=new Plate(nodes[0],nodes[1],nodes[2],nodes[3]).GetEdges().ToArray();Assert.That(edges.Length,Is.EqualTo(4));
            for(int i=0;i<4;i++){Assert.That(edges[i].Start,Is.SameAs(nodes[i]));Assert.That(edges[i].End,Is.SameAs(nodes[(i+1)%4]));}
        }
        [TestCase(0)] [TestCase(1)] [TestCase(2)]
        public void SparseAndCollapsedLoopsRetainLiteralEdgeExtractionSemantics(int count)
        {
            var a=N(1);var b=N(2,1);var plate=count==0?new Plate():count==1?new Plate(null,a,null,N(1)):new Plate(null,a,null,b);var edges=plate.GetEdges().ToArray();Assert.That(edges.Length,Is.EqualTo(count));
            if(count==1){Assert.That(edges[0].Start,Is.SameAs(a));Assert.That(edges[0].End,Is.SameAs(a));}
            if(count==2){Assert.That(edges[0].Start,Is.SameAs(a));Assert.That(edges[0].End,Is.SameAs(b));Assert.That(edges[1].Start,Is.SameAs(b));Assert.That(edges[1].End,Is.SameAs(a));}
        }
        [Test]
        public void CornerAndEdgeExtractionShareFirstRetainedNontransitiveEquality()
        {
            var a=N(1);var b=N(1,.00075);var c=N(1,.0015);var plate=new Plate(a,b,c,a);var corners=plate.GetCornerLoop();var edges=plate.GetEdges().ToArray();
            Assert.That(corners.Count,Is.EqualTo(2));Assert.That(edges.Length,Is.EqualTo(2));Assert.That(edges[0].Start,Is.SameAs(corners[0]));Assert.That(edges[0].End,Is.SameAs(corners[1]));Assert.That(edges[0].End,Is.SameAs(c));
        }
        [Test]
        public void EdgeCollectionAndSlotsAreOwnedReadOnlyWhileNodesRemainOriginalReferences()
        {
            var plate=Triangle();var a=plate.A;var edges=plate.GetEdges();var first=edges.First();Assert.Throws<NotSupportedException>(()=>((IList<PlateEdge>)edges).Clear());Assert.Throws<NotSupportedException>(()=>((IList<PlateEdge>)edges)[0]=null);
            plate.A=N(9,9);a.X=7;Assert.That(edges.Count(),Is.EqualTo(3));Assert.That(edges.First(),Is.SameAs(first));Assert.That(first.Start,Is.SameAs(a));Assert.That(first.Start.X,Is.EqualTo(7));Assert.That(plate.A.Id,Is.EqualTo(9));
            Assert.That(typeof(PlateEdge).IsSealed,Is.True);Assert.That(typeof(PlateEdge).GetConstructors(),Is.Empty);Assert.That(typeof(PlateEdge).GetProperties().Any(p=>p.GetSetMethod(true)!=null),Is.False);
        }
        [TestCase(false,0)] [TestCase(false,1)] [TestCase(false,2)] [TestCase(true,0)] [TestCase(true,1)] [TestCase(true,2)]
        public void SharedEdgesAreSymmetricAcrossWindingAndClosure(bool reverse,int rotation)
        {
            var first=Triangle();var nodes=new[]{N(3,0,1),N(1),N(4,-1)};
            if(reverse)Array.Reverse(nodes);var second=new Plate(nodes[rotation],nodes[(rotation+1)%3],nodes[(rotation+2)%3]);
            Assert.That(first.IsConnectedToPlate(second),Is.True);Assert.That(second.IsConnectedToPlate(first),Is.True);
            Assert.That(first.IsConnectedToPlate(second,PlateConnectivityScope.AnyCorner),Is.True);Assert.That(second.IsConnectedToPlate(first,PlateConnectivityScope.AnyCorner),Is.True);
        }
        [Test]
        public void DiagonalOnEitherLoopIsNotAnEdgeEvenWhenBothNodeIdsArePresent()
        {
            var first=Triangle();var second=new Plate(N(1),N(4,0,-1),N(2,1),N(5,1,-1));
            Assert.That(first.IsConnectedToPlate(second),Is.False);Assert.That(second.IsConnectedToPlate(first),Is.False);
            Assert.That(first.IsConnectedToPlate(second,PlateConnectivityScope.AnyCorner),Is.True);Assert.That(second.IsConnectedToPlate(first,PlateConnectivityScope.AnyCorner),Is.True);
        }
        [Test]
        public void CornerOnlyNeighboursRequireAnyCornerAndSegmentCrossingsDoNotConnect()
        {
            var first=Triangle();var cornerOnly=new Plate(N(1),N(4,-1),N(5,0,-1));
            Assert.That(first.IsConnectedToPlate(cornerOnly),Is.False);Assert.That(first.IsConnectedToPlate(cornerOnly,PlateConnectivityScope.AnyCorner),Is.True);
            var coincidentOtherIds=new Plate(N(8),N(9,1),N(10,0,1));Assert.That(first.IsConnectedToPlate(coincidentOtherIds,PlateConnectivityScope.AnyCorner),Is.False);Assert.That(first.IsConnectedToPlate(coincidentOtherIds),Is.False);
            var crossing=new Plate(N(11,.5,-1),N(12,.5,2),N(13,2,0));Assert.That(first.IsConnectedToPlate(crossing),Is.False);Assert.That(first.IsConnectedToPlate(crossing,PlateConnectivityScope.AnyCorner),Is.False);
        }
        [TestCase(0)] [TestCase(1)] [TestCase(2)] [TestCase(3)]
        public void EmptyAndSelfConnectivityRespectTheSelectedLoopRule(int count)
        {
            var nodes=Enumerable.Range(1,count).Select(i=>N(i,i)).ToArray();var plate=new Plate(count>0?nodes[0]:null,count>1?nodes[1]:null,count>2?nodes[2]:null);
            Assert.That(plate.IsConnectedToPlate(plate),Is.EqualTo(count>=2));Assert.That(plate.IsConnectedToPlate(plate,PlateConnectivityScope.AnyCorner),Is.EqualTo(count>0));
            Assert.That(plate.IsConnectedToPlate(new Plate()),Is.False);Assert.That(plate.IsConnectedToPlate(new Plate(),PlateConnectivityScope.AnyCorner),Is.False);
        }
        [Test]
        public void OneCornerSelfEdgesNeverPromoteCornerContactToSharedEdge()
        {
            var a=N(1);var single=new Plate(a,a,a,a);var normal=Triangle();Assert.That(single.GetEdges().Single().Start,Is.SameAs(a));
            Assert.That(single.IsConnectedToPlate(normal),Is.False);Assert.That(normal.IsConnectedToPlate(single),Is.False);Assert.That(single.IsConnectedToPlate(normal,PlateConnectivityScope.AnyCorner),Is.True);
        }
        [Test]
        public void BothEdgeEndpointsCannotMatchOnlyOneCornerOfTheOtherLoop()
        {
            var left=new Plate(N(1),N(1,.0015),null);
            var right=new Plate(N(1,.00075),N(2,10),null);
            Assert.That(left.IsConnectedToPlate(right),Is.False);Assert.That(right.IsConnectedToPlate(left),Is.False);
            Assert.That(left.IsConnectedToPlate(right,PlateConnectivityScope.AnyCorner),Is.True);
        }
        [TestCase(.001,0,0,true)] [TestCase(.00100001,0,0,false)] [TestCase(.0008,.0008,0,false)] [TestCase(0,0,.001,true)]
        public void PlateConnectivityUsesInclusiveThreeDimensionalIdAndDistanceEquality(double x,double y,double z,bool expected)
        {
            var first=new Plate(N(1),N(2,10),null);var second=new Plate(N(1,x,y,z),N(2,10+x,y,z),null);
            Assert.That(first.IsConnectedToPlate(second),Is.EqualTo(expected));Assert.That(second.IsConnectedToPlate(first),Is.EqualTo(expected));
            Assert.That(new Plate(N(1),null,null).IsConnectedToPlate(new Plate(N(1,x,y,z),null,null),PlateConnectivityScope.AnyCorner),Is.EqualTo(expected));
        }
        [TestCase(0)] [TestCase(-1)]
        public void DraftIdsAndUnrelatedPlateIdsPreserveSuppliedNodeIdentity(int id)
        {
            var left=new Plate(99,N(id),N(id-1,1),null);var right=new Plate(99,N(id-1,1),N(id),null);Assert.That(left.IsConnectedToPlate(right),Is.True);right.Id=77;Assert.That(left.IsConnectedToPlate(right),Is.True);
            right.B.Id=3;Assert.That(left.IsConnectedToPlate(right),Is.False);
        }
        [Test]
        public void RepeatedIdsWithDistinctCoordinatesDoNotCollapseOrUseHashMembership()
        {
            var left=new Plate(N(1),N(1,10),N(2,0,1));var right=new Plate(N(1,10),N(1),N(3,0,-1));Assert.That(left.GetEdges().Count(),Is.EqualTo(3));Assert.That(left.IsConnectedToPlate(right),Is.True);
            right.A.X=20;Assert.That(left.IsConnectedToPlate(right),Is.False);Assert.That(left.IsConnectedToPlate(right,PlateConnectivityScope.AnyCorner),Is.True);
        }
        [TestCase(double.NaN)] [TestCase(double.PositiveInfinity)] [TestCase(double.NegativeInfinity)]
        public void NonfiniteCornersThrowEvenAfterMatchesOrOppositeEmptyLoops(double value)
        {
            var bad=Triangle();bad.D=N(1,0,0,value);Assert.That(Assert.Throws<ArgumentException>(()=>bad.GetEdges()).ParamName,Is.EqualTo("plate"));
            foreach(var scope in new[]{PlateConnectivityScope.SharedEdge,PlateConnectivityScope.AnyCorner})
            {
                Assert.That(Assert.Throws<ArgumentException>(()=>Triangle().IsConnectedToPlate(bad,scope)).ParamName,Is.EqualTo("targetPlate"));
                Assert.That(Assert.Throws<ArgumentException>(()=>bad.IsConnectedToPlate(new Plate(),scope)).ParamName,Is.EqualTo("plate"));
                Assert.That(Assert.Throws<ArgumentException>(()=>new Plate().IsConnectedToPlate(bad,scope)).ParamName,Is.EqualTo("targetPlate"));
            }
        }
        [Test]
        public void HugeFiniteAndSubnormalCoordinatesUseScaledComparisons()
        {
            var left=new Plate(N(1,double.MaxValue),N(2,-double.MaxValue),null);var equal=new Plate(N(2,-double.MaxValue),N(1,double.MaxValue),null);
            Assert.That(left.IsConnectedToPlate(equal),Is.True);equal.B.X=-double.MaxValue;Assert.That(left.IsConnectedToPlate(equal),Is.False);
            Assert.That(new Plate(N(1),N(2,1),null).IsConnectedToPlate(new Plate(N(1,double.Epsilon,double.Epsilon),N(2,1),null)),Is.True);
        }
        [Test]
        public void BooleanConnectivityIsImmediateWithoutMutationOrCachedAdjacency()
        {
            var left=Triangle();var right=new Plate(N(2,1),N(1),N(9,0,-1));bool connected=left.IsConnectedToPlate(right);right.A.Id=44;Assert.That(connected,Is.True);Assert.That(left.IsConnectedToPlate(right),Is.False);Assert.That(left.B.Id,Is.EqualTo(2));
        }
        [Test]
        public void PublicSignaturesDefaultsAndNamedEdgeDocumentationAreComplete()
        {
            var edge=typeof(PlateExtensions).GetMethod("GetEdges");Assert.That(edge.ReturnType,Is.EqualTo(typeof(IEnumerable<PlateEdge>)));Assert.That(edge.GetParameters().Select(p=>p.Name),Is.EqualTo(new[]{"plate"}));Assert.That(edge.GetParameters().Single().ParameterType,Is.EqualTo(typeof(Plate)));
            var connection=typeof(PlateExtensions).GetMethod("IsConnectedToPlate");Assert.That(connection.ReturnType,Is.EqualTo(typeof(bool)));Assert.That(connection.GetParameters().Select(p=>p.Name),Is.EqualTo(new[]{"plate","targetPlate","scope"}));Assert.That(connection.GetParameters().Select(p=>p.ParameterType),Is.EqualTo(new[]{typeof(Plate),typeof(Plate),typeof(PlateConnectivityScope)}));Assert.That(connection.GetParameters()[2].IsOptional,Is.True);Assert.That(connection.GetParameters()[2].DefaultValue,Is.EqualTo(PlateConnectivityScope.SharedEdge));Assert.That((int)PlateConnectivityScope.SharedEdge,Is.Zero);Assert.That((int)PlateConnectivityScope.AnyCorner,Is.EqualTo(1));
            var docs=XDocument.Load(Path.ChangeExtension(typeof(PlateExtensions).Assembly.Location,".xml"));
            foreach(var method in new[]{edge,connection})
            {string prefix="M:"+typeof(PlateExtensions).FullName+"."+method.Name+"(";var doc=docs.Descendants("member").Single(e=>((string)e.Attribute("name")).StartsWith(prefix));foreach(string tag in new[]{"summary","returns","remarks","exception","example"})Assert.That(doc.Element(tag),Is.Not.Null);Assert.That(doc.Elements("param").Select(p=>(string)p.Attribute("name")),Is.EqualTo(method.GetParameters().Select(p=>p.Name)));Assert.That(doc.Descendants().Any(e=>e.Name=="include"||e.Name=="inheritdoc"),Is.False);}
            foreach(var property in typeof(PlateEdge).GetProperties())
            {var doc=docs.Descendants("member").Single(e=>(string)e.Attribute("name")=="P:"+typeof(PlateEdge).FullName+"."+property.Name);Assert.That(doc.Element("summary"),Is.Not.Null);Assert.That(doc.Element("value"),Is.Not.Null);}
        }
    }
}
