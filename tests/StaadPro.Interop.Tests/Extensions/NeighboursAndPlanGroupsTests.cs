using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using NUnit.Framework;
using StaadPro.Interop.Entities;
using StaadPro.Interop.Extensions;
using StaadPro.Interop.Models;
using StaadPro.Interop.Services;
namespace StaadPro.Interop.Tests.Extensions
{
 [TestFixture]
 public sealed class NeighboursAndPlanGroupsTests
 {
  private static Node N(int id,double x=0,double y=0,double z=0)=>new Node(id,x,y,z);
  private static Plate P(int id)=>new Plate(id,N(1),N(2,1),N(3,0,1));
  private static Plate Neighbour(int id)=>new Plate(id,N(2,1),N(1),N(4,0,-1));
  private static Beam Radial(double inward,double radius=1,double endRadius=2)
  {double a=inward*Math.PI/180;return new Beam(N(1,-radius*Math.Cos(a),0,-radius*Math.Sin(a)),N(2,-endRadius*Math.Cos(a),0,-endRadius*Math.Sin(a)));}
  private sealed class Once<T>:IEnumerable<T>
  {private readonly IEnumerable<T> source;public int Count;public Once(IEnumerable<T> source){this.source=source;}public IEnumerator<T> GetEnumerator(){if(++Count!=1)throw new Exception("enumerated twice");return source.GetEnumerator();}IEnumerator IEnumerable.GetEnumerator()=>GetEnumerator();}
  [Test]
  public void NullPublicArgumentsAreRejectedAtInvocation()
  {
   Assert.That(Assert.Throws<ArgumentNullException>(()=>PlateExtensions.GetPlatesSharingEdgeWith(null,Array.Empty<Plate>())).ParamName,Is.EqualTo("plate"));
   Assert.That(Assert.Throws<ArgumentNullException>(()=>P(1).GetPlatesSharingEdgeWith(null)).ParamName,Is.EqualTo("candidatePlates"));
   Assert.That(Assert.Throws<ArgumentNullException>(()=>BeamGroupingService.GroupBeamsByPlanAngle(null,N(0))).ParamName,Is.EqualTo("beams"));
   Assert.That(Assert.Throws<ArgumentNullException>(()=>BeamGroupingService.GroupBeamsByPlanAngle(Array.Empty<Beam>(),null)).ParamName,Is.EqualTo("origin"));
  }
  [Test]
  public void NeighboursPreserveOrderDuplicatesAndOriginalReferencesInReadOnlyStorage()
  {
   var plate=P(1);var first=Neighbour(7);var second=Neighbour(9);var source=new Once<Plate>(new[]{first,null,plate,second,first});
   var result=plate.GetPlatesSharingEdgeWith(source);Assert.That(result.Count(),Is.EqualTo(3));Assert.That(result.First(),Is.SameAs(first));Assert.That(result.Last(),Is.SameAs(first));Assert.That(result.ElementAt(1),Is.SameAs(second));Assert.That(source.Count,Is.EqualTo(1));
   Assert.Throws<NotSupportedException>(()=>((IList<Plate>)result).Clear());first.A=N(99,99);Assert.That(result.Count(),Is.EqualTo(3));Assert.That(result.First(),Is.SameAs(first));
  }
  [TestCase(0)] [TestCase(-1)] [TestCase(7)]
  public void SelfExclusionUsesCapturedPlateIdAndIgnoresExcludedGeometry(int id)
  {var plate=P(id);var same=new Plate(id,N(1,double.NaN),null,null);Assert.That(plate.GetPlatesSharingEdgeWith(new[]{same,plate,null}),Is.Empty);var other=Neighbour(id+1);Assert.That(plate.GetPlatesSharingEdgeWith(new[]{other}).Single(),Is.SameAs(other));}
  [Test]
  public void NeighboursRequireAnEdgeInBothLoopsRatherThanDiagonalOrCornerContact()
  {var p=P(1);var diagonal=new Plate(2,N(1),N(4,0,-1),N(2,1),N(5,1,-1));var corner=new Plate(3,N(1),N(8,-2),N(9,0,-2));var single=new Plate(4,N(1),null,null);var good=Neighbour(5);Assert.That(p.GetPlatesSharingEdgeWith(new[]{diagonal,corner,single,good}),Is.EqualTo(new[]{good}));}
  [TestCase(double.NaN)] [TestCase(double.PositiveInfinity)] [TestCase(double.NegativeInfinity)]
  public void EveryEligibleCandidateIsQualifiedEvenAfterMatchesOrOppositeEmptyGeometry(double value)
  {var p=P(1);var bad=new Plate(3,N(1,0,0,value),null,null);Assert.That(Assert.Throws<ArgumentException>(()=>p.GetPlatesSharingEdgeWith(new[]{Neighbour(2),bad})).ParamName,Is.EqualTo("candidatePlates"));Assert.Throws<ArgumentException>(()=>new Plate { Id = 1 }.GetPlatesSharingEdgeWith(new[]{bad}));p.D=N(1,value);Assert.That(Assert.Throws<ArgumentException>(()=>p.GetPlatesSharingEdgeWith(Array.Empty<Plate>())).ParamName,Is.EqualTo("plate"));}
  private static IEnumerable<Plate> PlatesThenFail(Exception failure){yield return Neighbour(2);throw failure;}
  private static IEnumerable<Beam> BeamsThenFail(Exception failure){yield return Radial(0);throw failure;}
  [Test]
  public void SourceEnumerationFailuresPropagateWithoutPartialReturn()
  {var failure=new ApplicationException("source failed");Assert.That(Assert.Throws<ApplicationException>(()=>P(1).GetPlatesSharingEdgeWith(PlatesThenFail(failure))),Is.SameAs(failure));Assert.That(Assert.Throws<ApplicationException>(()=>BeamGroupingService.GroupBeamsByPlanAngle(BeamsThenFail(failure),N(0))),Is.SameAs(failure));}
  [Test]
  public void NeighbourReferenceCaptureDoesNotFollowLaterSourceMutationsDuringEnumeration()
  {var p=P(1);var c=Neighbour(2);IEnumerable<Plate> Source(){yield return c;c.Id=99;c.A=N(77,77);}var result=p.GetPlatesSharingEdgeWith(Source());Assert.That(result.Single(),Is.SameAs(c));Assert.That(result.Single().Id,Is.EqualTo(99));}
  [Test]
  public void EmptySelectionsReturnOwnedEmptyCollections()
  {Assert.That(P(1).GetPlatesSharingEdgeWith(Array.Empty<Plate>()),Is.Empty);var a=BeamGroupingService.GroupBeamsByPlanAngle(Array.Empty<Beam>(),N(0));var b=BeamGroupingService.GroupBeamsByPlanAngle(Array.Empty<Beam>(),N(0));Assert.That(a,Is.Empty);Assert.That(a,Is.Not.SameAs(b));}
  [TestCase(-1d)] [TestCase(double.NaN)] [TestCase(double.PositiveInfinity)] [TestCase(double.NegativeInfinity)]
  public void AngularToleranceMustBeFiniteNonnegative(double tolerance)
  {Assert.That(Assert.Throws<ArgumentOutOfRangeException>(()=>BeamGroupingService.GroupBeamsByPlanAngle(Array.Empty<Beam>(),N(0),tolerance)).ParamName,Is.EqualTo("toleranceDegrees"));}
  [TestCase(0)] [TestCase(90)] [TestCase(180)] [TestCase(270)]
  public void BearingsUseEndpointToOriginXzConventionAndAscendingKeys(int expected)
  {Beam beam=expected==0?new Beam(N(1,-1),N(2,-2)):expected==90?new Beam(N(1,0,0,-1),N(2,0,0,-2)):expected==180?new Beam(N(1,1),N(2,2)):new Beam(N(1,0,0,1),N(2,0,0,2));var g=BeamGroupingService.GroupBeamsByPlanAngle(new[]{beam},N(0));Assert.That(g.Single().Key,Is.EqualTo(expected));Assert.That(g.Single().Beams.Single(),Is.SameAs(beam));}
  [Test]
  public void KeysSortAscendingAndMidpointRadiiSortWithinSeedGroupsWithStableTies()
  {var far=Radial(180,5,6);var near=Radial(180,1,2);var tie=Radial(180,1,2);var ninety=Radial(90);var zero=Radial(0);var source=new Once<Beam>(new[]{far,ninety,near,zero,tie,near});var g=BeamGroupingService.GroupBeamsByPlanAngle(source,N(0));Assert.That(g.Select(x=>x.Key),Is.EqualTo(new[]{0d,90d,180d}).Within(1e-10));Assert.That(g.Last().Beams.Count,Is.EqualTo(4));Assert.That(g.Last().Beams[0],Is.SameAs(near));Assert.That(g.Last().Beams[1],Is.SameAs(tie));Assert.That(g.Last().Beams[2],Is.SameAs(near));Assert.That(g.Last().Beams[3],Is.SameAs(far));Assert.That(source.Count,Is.EqualTo(1));}
  [Test]
  public void CircularWraparoundAndFirstSeedNontransitivityArePreserved()
  {var a=Radial(359.9);var b=Radial(.1);var c=Radial(.3);var g=BeamGroupingService.GroupBeamsByPlanAngle(new[]{a,b,c},N(0),.25);Assert.That(g.Count,Is.EqualTo(2));Assert.That(g.Single(x=>x.Beams.Contains(a)).Key,Is.EqualTo(359.9).Within(1e-10));Assert.That(g.Single(x=>x.Beams.Contains(a)).Beams,Does.Contain(b));Assert.That(g.Single(x=>x.Beams.Contains(c)).Beams.Count,Is.EqualTo(1));}
  [Test]
  public void EndpointBearingEligibilityRejectsOppositeAndTangentialRaysAndUndefinedOriginPoints()
  {var tangent=new Beam(N(1,1),N(2,0,0,1));var opposite=new Beam(N(1,1),N(2,-1));var atOrigin=new Beam(N(1),N(2,1));var verticalOrigin=new Beam(N(1,0,5),N(2,0,8));Assert.That(BeamGroupingService.GroupBeamsByPlanAngle(new[]{tangent,opposite,atOrigin,verticalOrigin},N(0)),Is.Empty);Assert.That(BeamGroupingService.GroupBeamsByPlanAngle(new[]{tangent,opposite,atOrigin},N(0),180).Single().Beams.Count,Is.EqualTo(2));}
  [Test]
  public void ZeroToleranceAndLargeFiniteToleranceHaveExplicitCircularSemantics()
  {var a=Radial(20);var b=Radial(20.1);Assert.That(BeamGroupingService.GroupBeamsByPlanAngle(new[]{a,b},N(0),0).Count,Is.EqualTo(2));Assert.That(BeamGroupingService.GroupBeamsByPlanAngle(new[]{a,b},N(0),double.MaxValue).Single().Beams.Count,Is.EqualTo(2));}
  [TestCase(90d,true)] [TestCase(89.999d,false)]
  public void EligibilityUsesInclusiveCircularToleranceAtExactRightAngles(double tolerance,bool expected)
  {var beam=new Beam(N(1,-1),N(2,0,0,-1));Assert.That(BeamGroupingService.GroupBeamsByPlanAngle(new[]{beam},N(0),tolerance).Any(),Is.EqualTo(expected));}
  [Test]
  public void TranslatedOriginAndEndpointReversalPreserveBearingAndMidpointMeaning()
  {var origin=N(0,10,100,20);var a=N(0,11,-100,20);var b=N(0,13,1000,20);var beam=new Beam(a,b);var reverse=new Beam(b,a);var groups=BeamGroupingService.GroupBeamsByPlanAngle(new[]{reverse,beam},origin,0);Assert.That(groups.Single().Key,Is.EqualTo(180));Assert.That(groups.Single().Beams[0],Is.SameAs(reverse));Assert.That(groups.Single().Beams[1],Is.SameAs(beam));}
  [TestCase(0)] [TestCase(1)] [TestCase(2)]
  public void MissingBeamOrEndpointThrowsInsteadOfBeingSkipped(int kind)
  {Beam beam=kind==0?null:kind==1?new Beam(null,N(2,1)):new Beam(N(1,1),null);Assert.That(Assert.Throws<ArgumentException>(()=>BeamGroupingService.GroupBeamsByPlanAngle(new[]{beam},N(0))).ParamName,Is.EqualTo("beams"));}
  [TestCase(double.NaN)] [TestCase(double.PositiveInfinity)] [TestCase(double.NegativeInfinity)]
  public void OriginAndAllEndpointCoordinatesMustBeFiniteEvenWhenYIsUnused(double value)
  {Assert.That(Assert.Throws<ArgumentException>(()=>BeamGroupingService.GroupBeamsByPlanAngle(Array.Empty<Beam>(),N(0,0,value))).ParamName,Is.EqualTo("origin"));var bad=Radial(45);bad.EndNode.Y=value;Assert.That(Assert.Throws<ArgumentException>(()=>BeamGroupingService.GroupBeamsByPlanAngle(new[]{Radial(0),bad},N(0))).ParamName,Is.EqualTo("beams"));}
  [Test]
  public void MidpointOrderingIgnoresHeightAndRetainsCapturedOriginalReferences()
  {var far=Radial(0,5,6);var near=Radial(0,1,2);near.StartNode.Y=1000;near.EndNode.Y=-2000;var g=BeamGroupingService.GroupBeamsByPlanAngle(new[]{far,near},N(0));Assert.That(g.Single().Beams.First(),Is.SameAs(near));double key=g[0].Key;near.StartNode.Z=50;Assert.That(g[0].Key,Is.EqualTo(key));Assert.That(g[0].Beams.First(),Is.SameAs(near));g[0].Beams.Clear();Assert.That(BeamGroupingService.GroupBeamsByPlanAngle(new[]{far},N(0)).Single().Beams.Count,Is.EqualTo(1));}
  [Test]
  public void FiniteHugeDistancesAndMidpointsDoNotOverflowIntoUnorderedInfinity()
  {var origin=N(0,-double.MaxValue);var near=new Beam(N(1,double.MaxValue/2),N(2,double.MaxValue/2));var far=new Beam(N(3,double.MaxValue),N(4,double.MaxValue));var g=BeamGroupingService.GroupBeamsByPlanAngle(new[]{far,near},origin);Assert.That(g.Single().Key,Is.EqualTo(180));Assert.That(g.Single().Beams[0],Is.SameAs(near));}
  [Test]
  public void SubnormalBearingsAndMidpointDistancesDoNotCollapseToZeroBySquaring()
  {var far=new Beam(N(1,-4*double.Epsilon),N(2,-4*double.Epsilon));var near=new Beam(N(3,-double.Epsilon),N(4,-double.Epsilon));var g=BeamGroupingService.GroupBeamsByPlanAngle(new[]{far,near},N(0));Assert.That(g.Single().Beams[0],Is.SameAs(near));Assert.That(g.Single().Key,Is.Zero);}
  [Test]
  public void CapturesSurviveSourceEditsAfterYieldAndOwnIndependentMutableLists()
  {var origin=N(0);var beam=Radial(0);IEnumerable<Beam> Source(){yield return beam;beam.StartNode.Z=9;origin.Z=50;}var g=BeamGroupingService.GroupBeamsByPlanAngle(Source(),origin);Assert.That(g.Single().Key,Is.Zero);Assert.That(g[0].Beams[0],Is.SameAs(beam));var a=new BeamGroup<double>();var b=new BeamGroup<double>();Assert.That(a.Beams,Is.Not.SameAs(b.Beams));a.Key=42;a.Beams=null;Assert.That(a.Key,Is.EqualTo(42));Assert.That(a.Beams,Is.Null);}
  [Test]
  public void ExactPublicSignaturesDefaultsAndExpandedXmlAreShipped()
  {var neighbour=typeof(PlateExtensions).GetMethod("GetPlatesSharingEdgeWith");Assert.That(neighbour.ReturnType,Is.EqualTo(typeof(IEnumerable<Plate>)));Assert.That(neighbour.GetParameters().Select(p=>p.Name),Is.EqualTo(new[]{"plate","candidatePlates"}));Assert.That(neighbour.GetParameters().Select(p=>p.ParameterType),Is.EqualTo(new[]{typeof(Plate),typeof(IEnumerable<Plate>)}));var group=typeof(BeamGroupingService).GetMethod("GroupBeamsByPlanAngle");Assert.That(group.ReturnType,Is.EqualTo(typeof(List<BeamGroup<double>>)));Assert.That(group.GetParameters().Select(p=>p.Name),Is.EqualTo(new[]{"beams","origin","toleranceDegrees"}));Assert.That(group.GetParameters().Select(p=>p.ParameterType),Is.EqualTo(new[]{typeof(IEnumerable<Beam>),typeof(Node),typeof(double)}));Assert.That(group.GetParameters()[2].DefaultValue,Is.EqualTo(.001d));Assert.That(group.GetParameters()[2].IsOptional,Is.True);
   var docs=XDocument.Load(Path.ChangeExtension(typeof(PlateExtensions).Assembly.Location,".xml"));foreach(var method in new[]{neighbour,group}){var prefix="M:"+method.DeclaringType.FullName+"."+method.Name+"(";var doc=docs.Descendants("member").Single(e=>((string)e.Attribute("name")).StartsWith(prefix));foreach(string tag in new[]{"summary","returns","remarks","exception","example"})Assert.That(doc.Element(tag),Is.Not.Null);Assert.That(doc.Elements("param").Select(p=>(string)p.Attribute("name")),Is.EqualTo(method.GetParameters().Select(p=>p.Name)));Assert.That(doc.Descendants().Any(e=>e.Name=="include"||e.Name=="inheritdoc"),Is.False);}
   foreach(string property in new[]{"Key","Beams"}){var doc=docs.Descendants("member").Single(e=>(string)e.Attribute("name")=="P:StaadPro.Interop.Models.BeamGroup`1."+property);Assert.That(doc.Element("summary"),Is.Not.Null);Assert.That(doc.Element("value"),Is.Not.Null);}}
 }
}
