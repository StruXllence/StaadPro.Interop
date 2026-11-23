using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using NUnit.Framework;
using StaadPro.Interop.Entities;
using StaadPro.Interop.Extensions;
using StaadPro.Interop.Models.Geometry;
using StaadPro.Interop.Services;

namespace StaadPro.Interop.Tests.Extensions
{
    [TestFixture]
    public sealed class UnwrappedAnglesAndUngroupedBeamsTests
    {
        private static Node N(int id,double x,double y=0,double z=0)=>new Node(id,x,y,z);
        private static Beam B(int id)=>new Beam { Id=id };
        private static EntityGroup<Beam> G(params Beam[] beams)=>new EntityGroup<Beam>{Entities=new HashSet<Beam>(beams)};

        [Test]
        public void AnglesUseOutwardXZRadiansAndRetainSortedNodeOrderAfterSuffixAdjustment()
        {
            var east=N(1,1,80,0);var north=N(2,0,-20,1);var west=N(3,-1,30,0);var south=N(4,0,9,-1);
            var full=BeamGroupingService.GetUnwrappedAngles(new[]{south,west,north,east},N(0,0));
            Assert.That(full.Select(a=>a.Node),Is.EqualTo(new[]{east,north,west,south}));
            Assert.That(full.Select(a=>a.AngleInRadians),Is.EqualTo(new[]{0d,Math.PI/2,Math.PI,3*Math.PI/2}));
            var above=N(5,1,4,1);var below=N(6,1,-8,-1);
            var seam=BeamGroupingService.GetUnwrappedAngles(new[]{below,above},N(0,0));
            Assert.That(seam.Select(a=>a.Node),Is.EqualTo(new[]{above,below}));
            Assert.That(seam.Select(a=>a.AngleInRadians),Is.EqualTo(new[]{Math.PI/4,-Math.PI/4}));
        }

        [Test]
        public void PiBoundaryIsStrictAndClosingGapIsNotRecentered()
        {
            var east=N(1,1);var west=N(2,-1);
            Assert.That(BeamGroupingService.GetUnwrappedAngles(new[]{west,east},N(0,0)).Select(a=>a.AngleInRadians),Is.EqualTo(new[]{0d,Math.PI}));
            var a=N(3,-1,0,-1);var b=N(4,0,0,-1);
            var values=BeamGroupingService.GetUnwrappedAngles(new[]{b,a},N(0,0));
            Assert.That(values.Select(x=>x.AngleInRadians),Is.EqualTo(new[]{5*Math.PI/4,3*Math.PI/2}));
        }

        [Test]
        public void OriginCapturedBeforeEnumerationAndMembershipSnapshotsDoNotFollowEdits()
        {
            var origin=N(0,2,3,4);var node=N(1,3,700,5);
            var input=new Once<Node>(new[]{node,node},()=>{origin.X=999;origin.Z=999;});
            var result=BeamGroupingService.GetUnwrappedAngles(input,origin);
            Assert.That(result,Has.Count.EqualTo(2));Assert.That(result[0].AngleInRadians,Is.EqualTo(Math.PI/4));
            Assert.That(result[0].Node,Is.SameAs(node));Assert.That(result[0],Is.Not.SameAs(result[1]));
            node.X=-900;node.Id=100;Assert.That(result[0].AngleInRadians,Is.EqualTo(Math.PI/4));
            result.RemoveAt(0);Assert.That(result,Has.Count.EqualTo(1)); // owned editable outer list
        }

        [Test]
        public void EqualBearingsPreserveInputOrderIncludingRepeatedAndDraftIds()
        {
            var a=N(0,1,5,1);var b=N(-2,2,8,2);var c=N(0,3,-2,3);
            var r=BeamGroupingService.GetUnwrappedAngles(new[]{c,a,b,a},N(-7,0));
            Assert.That(r.Select(x=>x.Node),Is.EqualTo(new[]{c,a,b,a}));
            Assert.That(r.All(x=>x.AngleInRadians==Math.PI/4),Is.True);
        }

        [Test]
        public void EmptyInputsAreFreshAndCoincidentPointsKeepAtan2Convention()
        {
            var origin=N(0,0);var a=BeamGroupingService.GetUnwrappedAngles(new Node[0],origin);
            var b=BeamGroupingService.GetUnwrappedAngles(new Node[0],origin);
            Assert.That(a,Is.Empty);Assert.That(b,Is.Not.SameAs(a));
            var coincident=N(1,0,999,0);
            Assert.That(BeamGroupingService.GetUnwrappedAngles(new[]{coincident},origin).Single().AngleInRadians,Is.Zero);
            double negativeZero=BitConverter.Int64BitsToDouble(unchecked((long)0x8000000000000000));
            var signed=N(2,negativeZero,0,negativeZero);double expected=Math.Atan2(signed.Z-origin.Z,signed.X-origin.X);
            if(expected<0)expected+=2*Math.PI;
            Assert.That(BitConverter.DoubleToInt64Bits(BeamGroupingService.GetUnwrappedAngles(new[]{signed},origin).Single().AngleInRadians),
                Is.EqualTo(BitConverter.DoubleToInt64Bits(expected)));
        }

        [Test]
        public void ExtremeDifferencesUseFiniteDirectionAndSubnormalAnglesRemainDefined()
        {
            var origin=N(0,-double.MaxValue,0,-double.MaxValue);
            var diagonal=N(1,double.MaxValue,0,double.MaxValue/2);
            var r=BeamGroupingService.GetUnwrappedAngles(new[]{diagonal},origin).Single();
            Assert.That(r.AngleInRadians,Is.EqualTo(Math.Atan2(.75,1)).Within(1e-14));
            var tiny=N(2,double.Epsilon,0,double.Epsilon);
            Assert.That(BeamGroupingService.GetUnwrappedAngles(new[]{tiny},N(0,0)).Single().AngleInRadians,Is.EqualTo(Math.PI/4));
            var nearlyEast=N(3,1,0,-double.Epsilon);
            Assert.That(BeamGroupingService.GetUnwrappedAngles(new[]{nearlyEast},N(0,0)).Single().AngleInRadians,Is.EqualTo(2*Math.PI));
        }

        [Test]
        public void AllAnglesStillRepresentTheirOriginalDirectionModuloFullTurns()
        {
            var random=new Random(23011);var origin=N(0,3,4,-5);
            var nodes=Enumerable.Range(1,200).Select(i=>N(i,random.Next(-100,101),random.Next(-50,51),random.Next(-100,101))).ToArray();
            var sorted=nodes.OrderBy(n=>{double a=Math.Atan2(n.Z-origin.Z,n.X-origin.X);return a<0?a+2*Math.PI:a;}).ToArray();
            var result=BeamGroupingService.GetUnwrappedAngles(new Once<Node>(nodes),origin);
            Assert.That(result.Select(r=>r.Node),Is.EqualTo(sorted));
            foreach(var item in result)
            {
                double raw=Math.Atan2(item.Node.Z-origin.Z,item.Node.X-origin.X);
                Assert.That(Math.Cos(item.AngleInRadians),Is.EqualTo(Math.Cos(raw)).Within(2e-14));
                Assert.That(Math.Sin(item.AngleInRadians),Is.EqualTo(Math.Sin(raw)).Within(2e-14));
            }
            for(int i=1;i<result.Count;i++)Assert.That(Math.Abs(result[i].AngleInRadians-result[i-1].AngleInRadians),Is.LessThanOrEqualTo(Math.PI));
        }

        [Test]
        public void NullInputsMissingNodesAndEnumeratorFailuresRemainExplicit()
        {
            Assert.That(Assert.Throws<ArgumentNullException>(()=>BeamGroupingService.GetUnwrappedAngles(null,N(0,0))).ParamName,Is.EqualTo("nodes"));
            Assert.That(Assert.Throws<ArgumentNullException>(()=>BeamGroupingService.GetUnwrappedAngles(new Node[0],null)).ParamName,Is.EqualTo("origin"));
            Assert.That(Assert.Throws<ArgumentException>(()=>BeamGroupingService.GetUnwrappedAngles(new[]{N(1,1),null},N(0,0))).ParamName,Is.EqualTo("nodes"));
            Assert.Throws<ApplicationException>(()=>BeamGroupingService.GetUnwrappedAngles(Broken(N(1,1)),N(0,0)));
        }

        [TestCase(double.NaN),TestCase(double.PositiveInfinity),TestCase(double.NegativeInfinity)]
        public void AllNodeAndOriginCoordinatesMustBeFiniteEvenUnusedY(double value)
        {
            for(int axis=0;axis<3;axis++)
            {
                var bad=N(1,axis==0?value:0,axis==1?value:0,axis==2?value:0);
                Assert.That(Assert.Throws<ArgumentException>(()=>BeamGroupingService.GetUnwrappedAngles(new[]{N(2,1),bad},N(0,0))).ParamName,Is.EqualTo("nodes"));
                var empty=new Once<Node>(new Node[0],()=>Assert.Fail("Origin must be qualified first."));
                Assert.That(Assert.Throws<ArgumentException>(()=>BeamGroupingService.GetUnwrappedAngles(empty,bad)).ParamName,Is.EqualTo("origin"));
            }
        }

        [Test]
        public void DetachedMembershipUsesOnlyBeamIdDespiteGeometrySensitivePackageEquality()
        {
            var supplied=new Beam(7,N(1,0),N(2,1));var grouped=new Beam(7,N(90,100),N(91,101));
            Assert.That(supplied.Equals(grouped),Is.False);
            var other=new Beam(8,N(1,0),N(2,1));
            var result=new[]{supplied,other}.GetBeamsWithNoGroupAssignment(new[]{G(grouped)});
            Assert.That(result.Single(),Is.SameAs(other));
            Assert.That(new[]{B(7)}.GetBeamsWithNoGroupAssignment(new[]{G(B(7))}),Is.Empty);
        }

        [Test]
        public void UngroupedFilterPreservesOrderDuplicatesNullSkippingAndNoGeometryRequirements()
        {
            var a=B(1);var b=B(2);var c=B(3);
            var groups=new Once<EntityGroup<Beam>>(new[]{null,G(null,B(1)),G(B(1))});
            var result=new Once<Beam>(new[]{b,null,a,c,b}).GetBeamsWithNoGroupAssignment(groups);
            Assert.That(result.ToArray(),Is.EqualTo(new[]{b,c,b}));Assert.That(result.First(),Is.SameAs(b));
            Assert.Throws<NotSupportedException>(()=>((IList<Beam>)result).Clear());
            Assert.That(new Beam[0].GetBeamsWithNoGroupAssignment(new EntityGroup<Beam>[0]),Is.Empty);
            Assert.That(new Beam[]{null}.GetBeamsWithNoGroupAssignment(new EntityGroup<Beam>[0]),Is.Empty);
        }

        [Test]
        public void CapturedGroupIdsIgnoreLaterEditsAndStaleHashBuckets()
        {
            var grouped=B(1);var g=G(grouped);grouped.Id=7; // hash bucket now stale, enumeration remains valid
            var candidate=B(7);var ungrouped=B(8);
            var beams=new Once<Beam>(new[]{candidate,ungrouped},()=>{grouped.Id=9;g.Entities.Clear();});
            var result=beams.GetBeamsWithNoGroupAssignment(new[]{g});
            Assert.That(result.Single(),Is.SameAs(ungrouped));
            candidate.Id=8;ungrouped.Id=7;
            Assert.That(result.Single(),Is.SameAs(ungrouped)); // no deferred re-evaluation
        }

        [Test]
        public void ZeroNegativeAndMaximumDraftIdsParticipateInMembership()
        {
            var zero=B(0);var negative=B(-1);var maximum=B(int.MaxValue);
            Assert.That(new[]{zero,negative,maximum}.GetBeamsWithNoGroupAssignment(new[]{G(B(0),B(-1))}).Single(),Is.SameAs(maximum));
            Assert.That(new[]{zero,negative}.GetBeamsWithNoGroupAssignment(new EntityGroup<Beam>[0]),Is.EqualTo(new[]{zero,negative}));
        }

        [Test]
        public void NullArgumentsNullEntitySetsAndSourceFailuresAreValidatedBeforeReturn()
        {
            Assert.That(Assert.Throws<ArgumentNullException>(()=>GeometryDiagnosticsExtensions.GetBeamsWithNoGroupAssignment(null,new EntityGroup<Beam>[0])).ParamName,Is.EqualTo("beamsCollection"));
            Assert.That(Assert.Throws<ArgumentNullException>(()=>new Beam[0].GetBeamsWithNoGroupAssignment(null)).ParamName,Is.EqualTo("groups"));
            Assert.That(Assert.Throws<ArgumentException>(()=>new Beam[0].GetBeamsWithNoGroupAssignment(new[]{new EntityGroup<Beam>{Entities=null}})).ParamName,Is.EqualTo("groups"));
            Assert.Throws<ApplicationException>(()=>new Beam[0].GetBeamsWithNoGroupAssignment(Broken(G(B(1)))));
            Assert.Throws<ApplicationException>(()=>Broken(B(2)).GetBeamsWithNoGroupAssignment(new EntityGroup<Beam>[0]));
        }

        [Test]
        public void NamedAngleResultAndBothPublicContractsHaveCompleteExpandedDocumentation()
        {
            var angle=typeof(BeamGroupingService).GetMethod("GetUnwrappedAngles");
            Assert.That(angle.ReturnType,Is.EqualTo(typeof(List<UnwrappedNodeAngle>)));
            Assert.That(angle.GetParameters().Select(p=>p.Name),Is.EqualTo(new[]{"nodes","origin"}));
            Assert.That(angle.GetParameters().Select(p=>p.ParameterType),Is.EqualTo(new[]{typeof(IEnumerable<Node>),typeof(Node)}));
            var filter=typeof(GeometryDiagnosticsExtensions).GetMethod("GetBeamsWithNoGroupAssignment");
            Assert.That(filter.ReturnType,Is.EqualTo(typeof(IEnumerable<Beam>)));
            Assert.That(filter.GetParameters().Select(p=>p.Name),Is.EqualTo(new[]{"beamsCollection","groups"}));
            Assert.That(filter.GetParameters().Select(p=>p.ParameterType),Is.EqualTo(new[]{typeof(IEnumerable<Beam>),typeof(IEnumerable<EntityGroup<Beam>>)}));
            var xml=XDocument.Load(Path.ChangeExtension(typeof(Node).Assembly.Location,".xml"));
            foreach(var method in new[]{angle,filter})
            {
                Assert.That(method.GetParameters().Any(p=>p.IsOptional||p.ParameterType.IsByRef||p.IsOut),Is.False);
                var doc=xml.Descendants("member").Single(e=>((string)e.Attribute("name")).StartsWith("M:"+method.DeclaringType.FullName+"."+method.Name+"("));
                foreach(string tag in new[]{"summary","param","returns","remarks","exception","example"})Assert.That(doc.Element(tag),Is.Not.Null);
                Assert.That(doc.Elements("param").Select(e=>(string)e.Attribute("name")),Is.EqualTo(method.GetParameters().Select(p=>p.Name)));
                Assert.That(doc.Descendants().Any(e=>e.Name=="include"||e.Name=="inheritdoc"),Is.False);
            }
            var type=typeof(UnwrappedNodeAngle);Assert.That(type.IsSealed,Is.True);Assert.That(type.GetConstructors(),Is.Empty);
            Assert.That(type.GetProperties().Select(p=>p.Name),Is.EquivalentTo(new[]{"Node","AngleInRadians"}));
            Assert.That(type.GetProperties().All(p=>p.GetSetMethod(true)==null),Is.True);
            foreach(var property in type.GetProperties())
            {
                var doc=xml.Descendants("member").Single(e=>(string)e.Attribute("name")=="P:"+type.FullName+"."+property.Name);
                Assert.That(doc.Element("summary"),Is.Not.Null);Assert.That(doc.Element("value"),Is.Not.Null);
            }
        }

        private static IEnumerable<T> Broken<T>(T item){yield return item;throw new ApplicationException("source failed");}
        private sealed class Once<T>:IEnumerable<T>
        {
            private readonly IEnumerable<T> values;private readonly Action before;private bool used;
            internal Once(IEnumerable<T> values,Action before=null){this.values=values;this.before=before;}
            public IEnumerator<T> GetEnumerator(){if(used)throw new ApplicationException("repeated enumeration");used=true;before?.Invoke();return values.GetEnumerator();}
            IEnumerator IEnumerable.GetEnumerator()=>GetEnumerator();
        }
    }
}
