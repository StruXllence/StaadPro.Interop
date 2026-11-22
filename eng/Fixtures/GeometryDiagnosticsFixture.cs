using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using StaadPro.Interop.Entities;
using StaadPro.Interop.Enums;
using StaadPro.Interop.Extensions;
using StaadPro.Interop.Common;
using StaadPro.Interop.Models.Geometry;
using StaadPro.Interop.Models;
using StaadPro.Interop.Services;
namespace PackageVerification
{
    internal static class GeometryDiagnosticsFixture
    {
        internal static void Verify(string xmlPath)
        {
            var docs=XDocument.Load(xmlPath);
            foreach(string name in new[]{"GetDuplicateNodes","GetZeroLengthBeams","GetClosestNode","GetClosestBeam"})
            {
                var method=typeof(GeometryDiagnosticsExtensions).GetMethod(name);
                bool nearest=name.StartsWith("GetClosest"); bool nodes=name=="GetDuplicateNodes"||name=="GetClosestNode";
                var parameters=method.GetParameters();Type element=nodes?typeof(Node):typeof(Beam);
                Type expected=nearest?element:name=="GetDuplicateNodes"?typeof(IEnumerable<IEnumerable<Node>>):typeof(IEnumerable<Beam>);
                if(method.ReturnType!=expected||parameters.Length!=2||parameters[0].ParameterType!=typeof(IEnumerable<>).MakeGenericType(element)||parameters[0].Name!=(nodes?"nodesCollection":"beamsCollection")||parameters[0].IsOptional)throw new Exception("Geometry diagnostic signature changed.");
                if(nearest){if(parameters[1].Name!="referenceNode"||parameters[1].ParameterType!=typeof(Node)||parameters[1].IsOptional)throw new Exception("Nearest-geometry reference contract changed.");}
                else if(parameters[1].Name!="tolerance"||!parameters[1].IsOptional||!Equals(parameters[1].DefaultValue,.0001d))throw new Exception("Diagnostic tolerance changed.");
                string id="M:StaadPro.Interop.Extensions.GeometryDiagnosticsExtensions."+name+"(System.Collections.Generic.IEnumerable{"+element.FullName+"},"+(nearest?typeof(Node).FullName:"System.Double")+")";
                var entry=docs.Descendants("member").Single(x=>(string)x.Attribute("name")==id);
                foreach(string tag in new[]{"summary","returns","remarks","exception","example"})if(entry.Element(tag)==null)throw new Exception("Incomplete geometry diagnostic IntelliSense.");
                if(entry.Elements("param").Count()!=2||entry.Descendants().Any(x=>x.Name=="include"||x.Name=="inheritdoc"))throw new Exception("Unexpanded geometry diagnostic XML.");
            }
            var radialMethod=typeof(GeometryDiagnosticsExtensions).GetMethod("GetBeamsWithinRadius");
            var rp=radialMethod.GetParameters();
            if(radialMethod.ReturnType!=typeof(IEnumerable<Beam>)||!rp.Select(p=>p.ParameterType).SequenceEqual(new[]{typeof(IEnumerable<Beam>),typeof(double),typeof(double),typeof(Node)})||!rp.Select(p=>p.Name).SequenceEqual(new[]{"beamsCollection","minRadius","maxRadius","centre"})||rp.Take(3).Any(p=>p.IsOptional)||!rp[3].IsOptional||rp[3].DefaultValue!=null)throw new Exception("Radial filter signature changed.");
            var boundsMethod=typeof(GeometryDiagnosticsExtensions).GetMethod("GetBoundingBox");
            if(!boundsMethod.IsGenericMethodDefinition||boundsMethod.ReturnType!=typeof(AxisAlignedBounds)||boundsMethod.GetGenericArguments().Length!=1||!boundsMethod.GetGenericArguments()[0].GetGenericParameterConstraints().Contains(typeof(IEntity))||boundsMethod.GetParameters().Length!=1||boundsMethod.GetParameters()[0].Name!="source"||boundsMethod.GetParameters()[0].IsOptional)throw new Exception("Named bounds signature changed.");
            var connectivityMethod=typeof(BeamExtensions).GetMethod("IsConnectedToBeam");
            if(connectivityMethod.ReturnType!=typeof(bool)||!connectivityMethod.GetParameters().Select(p=>p.ParameterType).SequenceEqual(new[]{typeof(Beam),typeof(Beam)})||!connectivityMethod.GetParameters().Select(p=>p.Name).SequenceEqual(new[]{"beam","targetBeam"})||connectivityMethod.GetParameters().Any(p=>p.IsOptional))throw new Exception("Beam connectivity signature changed.");
            var beamNodeMethod=typeof(BeamExtensions).GetMethod("IsConnectedToNode");
            if(beamNodeMethod.ReturnType!=typeof(bool)||!beamNodeMethod.GetParameters().Select(p=>p.ParameterType).SequenceEqual(new[]{typeof(Beam),typeof(Node)})||!beamNodeMethod.GetParameters().Select(p=>p.Name).SequenceEqual(new[]{"beam","node"})||beamNodeMethod.GetParameters().Any(p=>p.IsOptional))throw new Exception("Beam-node connectivity signature changed.");
            var memberMethod=typeof(MemberExtensions).GetMethod("IsConnectedToMember");
            var memberParameters=memberMethod.GetParameters();
            if(memberMethod.ReturnType!=typeof(bool)||!memberParameters.Select(p=>p.ParameterType).SequenceEqual(new[]{typeof(Member),typeof(Member),typeof(MemberConnectivityScope)})||!memberParameters.Select(p=>p.Name).SequenceEqual(new[]{"member","targetMember","scope"})||memberParameters.Take(2).Any(p=>p.IsOptional)||!memberParameters[2].IsOptional||!Equals(memberParameters[2].DefaultValue,MemberConnectivityScope.EndpointsOnly)||(int)MemberConnectivityScope.EndpointsOnly!=0||(int)MemberConnectivityScope.AnyNode!=1)throw new Exception("Member connectivity contract changed.");
            var memberNodeMethod=typeof(MemberExtensions).GetMethod("IsConnectedToNode");
            var mnp=memberNodeMethod.GetParameters();
            if(memberNodeMethod.ReturnType!=typeof(bool)||!mnp.Select(p=>p.ParameterType).SequenceEqual(new[]{typeof(Member),typeof(Node),typeof(MemberConnectivityScope)})||!mnp.Select(p=>p.Name).SequenceEqual(new[]{"member","node","scope"})||mnp.Take(2).Any(p=>p.IsOptional)||!mnp[2].IsOptional||!Equals(mnp[2].DefaultValue,MemberConnectivityScope.EndpointsOnly))throw new Exception("Member-node contract changed.");
            var cornerMethod=typeof(PlateExtensions).GetMethod("GetCornerLoop");
            var cp=cornerMethod.GetParameters();
            if(cornerMethod.ReturnType!=typeof(IReadOnlyList<Node>)||cp.Length!=1||cp[0].Name!="plate"||cp[0].ParameterType!=typeof(Plate)||cp[0].IsOptional)throw new Exception("Corner loop contract changed.");
            var edgesMethod=typeof(PlateExtensions).GetMethod("GetEdges");
            var ep=edgesMethod.GetParameters();
            if(edgesMethod.ReturnType!=typeof(IEnumerable<PlateEdge>)||ep.Length!=1||ep[0].Name!="plate"||ep[0].ParameterType!=typeof(Plate)||ep[0].IsOptional)throw new Exception("Named plate-edge signature changed.");
            var plateMethod=typeof(PlateExtensions).GetMethod("IsConnectedToPlate");
            var pp=plateMethod.GetParameters();
            if(plateMethod.ReturnType!=typeof(bool)||!pp.Select(p=>p.ParameterType).SequenceEqual(new[]{typeof(Plate),typeof(Plate),typeof(PlateConnectivityScope)})||!pp.Select(p=>p.Name).SequenceEqual(new[]{"plate","targetPlate","scope"})||pp.Take(2).Any(p=>p.IsOptional)||!pp[2].IsOptional||!Equals(pp[2].DefaultValue,PlateConnectivityScope.SharedEdge)||(int)PlateConnectivityScope.SharedEdge!=0||(int)PlateConnectivityScope.AnyCorner!=1)throw new Exception("Plate connectivity/default changed.");
            if(!typeof(PlateEdge).IsSealed||typeof(PlateEdge).GetConstructors().Length!=0||typeof(PlateEdge).GetProperties().Any(p=>p.GetSetMethod(true)!=null||p.PropertyType!=typeof(Node))||!typeof(PlateEdge).GetProperties().Select(p=>p.Name).OrderBy(n=>n).SequenceEqual(new[]{"End","Start"}))throw new Exception("Plate edge endpoint slots are not immutable/named.");
            foreach(var edgeProperty in typeof(PlateEdge).GetProperties())
            {
                var edgeDoc=docs.Descendants("member").Single(e=>(string)e.Attribute("name")=="P:"+typeof(PlateEdge).FullName+"."+edgeProperty.Name);
                if(edgeDoc.Element("summary")==null||edgeDoc.Element("value")==null)throw new Exception("Plate-edge endpoint IntelliSense missing.");
            }
            var neighbourMethod=typeof(PlateExtensions).GetMethod("GetPlatesSharingEdgeWith");var np=neighbourMethod.GetParameters();
            if(neighbourMethod.ReturnType!=typeof(IEnumerable<Plate>)||!np.Select(p=>p.Name).SequenceEqual(new[]{"plate","candidatePlates"})||!np.Select(p=>p.ParameterType).SequenceEqual(new[]{typeof(Plate),typeof(IEnumerable<Plate>)})||np.Any(p=>p.IsOptional))throw new Exception("Neighbour signature changed.");
            var planMethod=typeof(BeamGroupingService).GetMethod("GroupBeamsByPlanAngle");var gp=planMethod.GetParameters();
            if(planMethod.ReturnType!=typeof(List<BeamGroup<double>>)||!gp.Select(p=>p.Name).SequenceEqual(new[]{"beams","origin","toleranceDegrees"})||!gp.Select(p=>p.ParameterType).SequenceEqual(new[]{typeof(IEnumerable<Beam>),typeof(Node),typeof(double)})||gp.Take(2).Any(p=>p.IsOptional)||!gp[2].IsOptional||!Equals(gp[2].DefaultValue,.001d))throw new Exception("Plan-angle signature/default changed.");
            var radiusMethod=typeof(BeamGroupingService).GetMethod("GroupBeamsByRadius");
            var elevationMethod=typeof(BeamGroupingService).GetMethod("GroupBeamsByElevation");
            foreach(var method in new[]{radiusMethod,elevationMethod})
            {
                bool radius=method==radiusMethod;var parameters=method.GetParameters();
                if(method.ReturnType!=typeof(List<BeamGroup<double>>)||!parameters.Select(p=>p.Name).SequenceEqual(radius?new[]{"beams","origin","tolerance"}:new[]{"beams","tolerance"})||!parameters.Select(p=>p.ParameterType).SequenceEqual(radius?new[]{typeof(IEnumerable<Beam>),typeof(Node),typeof(double)}:new[]{typeof(IEnumerable<Beam>),typeof(double)})||!parameters.Last().IsOptional||!Equals(parameters.Last().DefaultValue,.001d)||parameters.Take(parameters.Length-1).Any(p=>p.IsOptional))throw new Exception("Radius/elevation signature/default changed.");
            }
            foreach(string property in new[]{"Key","Beams"}){var groupDoc=docs.Descendants("member").Single(e=>(string)e.Attribute("name")=="P:StaadPro.Interop.Models.BeamGroup`1."+property);if(groupDoc.Element("summary")==null||groupDoc.Element("value")==null)throw new Exception("Beam-group IntelliSense missing.");}
            foreach(var method in new[]{radialMethod,boundsMethod,connectivityMethod,beamNodeMethod,memberMethod,memberNodeMethod,cornerMethod,edgesMethod,plateMethod,neighbourMethod,planMethod,radiusMethod,elevationMethod})
            {
                string prefix="M:"+method.DeclaringType.FullName+"."+method.Name;
                var doc=docs.Descendants("member").Single(e=>((string)e.Attribute("name")).StartsWith(prefix+"(")||((string)e.Attribute("name")).StartsWith(prefix+"``1("));
                foreach(string tag in new[]{"summary","returns","remarks","exception","example"})if(doc.Element(tag)==null)throw new Exception("Missing new geometry IntelliSense.");
                if(!doc.Elements("param").Select(e=>(string)e.Attribute("name")).SequenceEqual(method.GetParameters().Select(p=>p.Name))||doc.Descendants().Any(e=>e.Name=="include"||e.Name=="inheritdoc"))throw new Exception("Geometry IntelliSense parameter mismatch.");
            }
            if(!typeof(AxisAlignedBounds).IsSealed||typeof(AxisAlignedBounds).GetConstructors().Length!=0||!typeof(AxisAlignedBounds).GetProperties().Select(p=>p.Name).OrderBy(n=>n).SequenceEqual(new[]{"XMax","XMin","YMax","YMin","ZMax","ZMin"})||typeof(AxisAlignedBounds).GetProperties().Any(p=>p.GetSetMethod(true)!=null||p.PropertyType!=typeof(double)))throw new Exception("Bounds snapshot must be named and immutable.");
            foreach(var property in typeof(AxisAlignedBounds).GetProperties())
            {
                var doc=docs.Descendants("member").Single(e=>(string)e.Attribute("name")=="P:"+typeof(AxisAlignedBounds).FullName+"."+property.Name);
                if(doc.Element("summary")==null||doc.Element("value")==null)throw new Exception("Bounds property IntelliSense is missing.");
            }
            var crossing=new Beam(new Node(11,-100,70,0),new Node(12,100,70,0));
            var radial=new Beam[]{null,crossing,crossing}.GetBeamsWithinRadius(0,0);
            crossing.StartNode.X=-200;
            if(radial.Count()!=2||!ReferenceEquals(radial.First(),crossing))throw new Exception("Radial filter did not materialize XZ midpoint membership.");
            bool radialReadOnly=false;try{((IList<Beam>)radial).Clear();}catch(NotSupportedException){radialReadOnly=true;}if(!radialReadOnly)throw new Exception("Radial storage is mutable.");
            var hugeBeam=new Beam(new Node(1,double.MaxValue,0,0),new Node(2,double.MaxValue,0,0));
            if(!new[]{hugeBeam}.GetBeamsWithinRadius(double.MaxValue,double.MaxValue).Any())throw new Exception("Finite huge midpoint overflowed.");
            var triangle=new Plate(new Node(1,-2,3,9),new Node(2,7,-5,4),new Node(3,1,6,-8));
            var bounds=new[]{triangle}.GetBoundingBox();triangle.A.X=500;
            if(bounds.XMin!=-2||bounds.XMax!=7||bounds.YMin!=-5||bounds.YMax!=6||bounds.ZMin!=-8||bounds.ZMax!=9)throw new Exception("Bounds did not capture all triangle corners.");
            var joint=new Node(19,1,2,3);var left=new Beam(new Node(1,0,0,0),joint);
            var right=new Beam(new Node(19,1,2,3),new Node(2,20,0,0));
            if(!left.IsConnectedToBeam(right))throw new Exception("Equivalent endpoints did not connect.");
            right.StartNode.Id=99;if(left.IsConnectedToBeam(right))throw new Exception("Coincidence with another ID connected.");
            bool malformedRejected=false;try{left.IsConnectedToBeam(new Beam(joint,null));}catch(ArgumentException){malformedRejected=true;}if(!malformedRejected)throw new Exception("An early match hid missing incidence.");
            var endpoint=new Node(7,5,0,0);
            var endpointBeam=new Beam(new Node(1,0,0,0),endpoint);
            if(!endpointBeam.IsConnectedToNode(new Node(7,5,0,0))||endpointBeam.IsConnectedToNode(new Node(8,5,0,0)))throw new Exception("Beam-node equality changed.");
            bool invalidEndpoint=false;try{new Beam(endpoint,null).IsConnectedToNode(endpoint);}catch(ArgumentException){invalidEndpoint=true;}if(!invalidEndpoint)throw new Exception("Beam-node early match hid missing incidence.");
            var host=new Member(new Node(1,0,0,0),new Node(2,10,0,0));host.Nodes.Add(endpoint);
            var spur=new Member(new Node(7,5,0,0),new Node(3,5,4,0));
            if(host.IsConnectedToMember(spur)||!host.IsConnectedToMember(spur,MemberConnectivityScope.AnyNode)||!spur.IsConnectedToMember(host,MemberConnectivityScope.AnyNode))throw new Exception("Member T-junction scope changed.");
            host.Nodes=new HashSet<Node>();if(host.IsConnectedToMember(spur,MemberConnectivityScope.AnyNode))throw new Exception("Empty selected Nodes gained implicit endpoints.");
            var mutableJoint=new Node(11,5,0,0);host.Nodes.Add(mutableJoint);mutableJoint.Id=7;
            if(!host.IsConnectedToMember(spur,MemberConnectivityScope.AnyNode))throw new Exception("Member connectivity depended on stale node hashing.");
            spur.Nodes.Add(null);bool invalidMember=false;try{host.IsConnectedToMember(spur,MemberConnectivityScope.AnyNode);}catch(ArgumentException){invalidMember=true;}if(!invalidMember)throw new Exception("Member early match hid null selected incidence.");
            bool invalidScope=false;try{new Member().IsConnectedToMember(new Member(),(MemberConnectivityScope)999);}catch(ArgumentOutOfRangeException){invalidScope=true;}if(!invalidScope)throw new Exception("Undefined member scope accepted.");
            var nodeHost=new Member(new Node(1,0,0,0),new Node(2,10,0,0));nodeHost.Nodes.Add(new Node(3,5,0,0));
            if(nodeHost.IsConnectedToNode(new Node(3,5,0,0))||!nodeHost.IsConnectedToNode(new Node(3,5,0,0),MemberConnectivityScope.AnyNode))throw new Exception("Member-node scope changed.");
            var key=new Node(8,5,0,0);nodeHost.Nodes=new HashSet<Node>{key};key.Id=3;
            if(!nodeHost.IsConnectedToNode(new Node(3,5,0,0),MemberConnectivityScope.AnyNode))throw new Exception("Member-node depended on stale hashing.");
            bool badTarget=false;nodeHost.Nodes.Clear();try{nodeHost.IsConnectedToNode(new Node(1,double.NaN,0,0),MemberConnectivityScope.AnyNode);}catch(ArgumentException){badTarget=true;}if(!badTarget)throw new Exception("Empty selection hid invalid target.");
            var cornerA=new Node(1,0,0,0);var cornerC=new Node(1,.0015,0,0);
            var cornerPlate=new Plate(cornerA,new Node(1,.00075,0,0),cornerC,new Node(1,0,0,0));var loop=cornerPlate.GetCornerLoop();
            if(loop.Count!=2||!ReferenceEquals(loop[0],cornerA)||!ReferenceEquals(loop[1],cornerC))throw new Exception("Corner winding/first-representative equality changed.");
            bool cornerReadOnly=false;try{((IList<Node>)loop).Clear();}catch(NotSupportedException){cornerReadOnly=true;}if(!cornerReadOnly)throw new Exception("Corner storage is mutable.");
            cornerPlate.A=new Node(9,9,0,0);cornerA.Y=7;if(loop.Count!=2||!ReferenceEquals(loop[0],cornerA)||loop[0].Y!=7)throw new Exception("Corner ownership changed.");
            if(new Plate().GetCornerLoop().Count!=0||new Plate(null,new Node(1,0,0,0),null,new Node(1,0,0,0)).GetCornerLoop().Count!=1)throw new Exception("Sparse corners changed.");
            bool invalidCorner=false;try{new Plate(new Node(1,0,0,0),null,null,new Node(1,double.NaN,0,0)).GetCornerLoop();}catch(ArgumentException){invalidCorner=true;}if(!invalidCorner)throw new Exception("A repeated corner hid nonfinite coordinates.");
            var edgeA=new Node(1,0,0,0);var edgeB=new Node(2,1,0,0);var edgeC=new Node(3,0,1,0);
            var edgePlate=new Plate(edgeA,edgeB,edgeC,new Node(1,0,0,0));var edgeSequence=edgePlate.GetEdges();var edgeArray=edgeSequence.ToArray();
            if(edgeArray.Length!=3||!ReferenceEquals(edgeArray[0].Start,edgeA)||!ReferenceEquals(edgeArray[0].End,edgeB)||!ReferenceEquals(edgeArray[2].End,edgeA))throw new Exception("Plate edge winding/closure changed.");
            bool readOnlyEdges=false;try{((IList<PlateEdge>)edgeSequence).Clear();}catch(NotSupportedException){readOnlyEdges=true;}if(!readOnlyEdges)throw new Exception("Edge collection is mutable.");
            var edgeNeighbour=new Plate(new Node(2,1,0,0),new Node(1,0,0,0),new Node(9,0,-1,0));
            if(!edgePlate.IsConnectedToPlate(edgeNeighbour)||!edgeNeighbour.IsConnectedToPlate(edgePlate))throw new Exception("Reversed edges did not connect symmetrically.");
            var diagonalNeighbour=new Plate(new Node(1,0,0,0),new Node(4,0,-1,0),new Node(2,1,0,0),new Node(5,1,-1,0));
            if(edgePlate.IsConnectedToPlate(diagonalNeighbour)||diagonalNeighbour.IsConnectedToPlate(edgePlate)||!edgePlate.IsConnectedToPlate(diagonalNeighbour,PlateConnectivityScope.AnyCorner))throw new Exception("Target diagonal was counted as an edge.");
            var oneCorner=new Plate(edgeA,null,null);if(oneCorner.GetEdges().Count()!=1||oneCorner.IsConnectedToPlate(edgePlate)||!oneCorner.IsConnectedToPlate(edgePlate,PlateConnectivityScope.AnyCorner))throw new Exception("Sparse self-edge connectivity changed.");
            edgePlate.A=new Node(99,99,0,0);edgeA.Z=7;if(!ReferenceEquals(edgeSequence.First(),edgeArray[0])||!ReferenceEquals(edgeArray[0].Start,edgeA)||edgeArray[0].Start.Z!=7)throw new Exception("Edge materialization/reference ownership changed.");
            bool badPlate=false;try{new Plate().IsConnectedToPlate(new Plate(new Node(1,double.NaN,0,0),null,null));}catch(ArgumentException){badPlate=true;}if(!badPlate)throw new Exception("An empty loop hid malformed opposite geometry.");
            bool badPlateScope=false;try{new Plate().IsConnectedToPlate(new Plate(),(PlateConnectivityScope)999);}catch(ArgumentOutOfRangeException){badPlateScope=true;}if(!badPlateScope)throw new Exception("Undefined plate scope accepted.");
            var neighbourhoodReference=new Plate(1,new Node(1,0,0,0),new Node(2,1,0,0),new Node(3,0,1,0));
            var neighbourhoodMatch=new Plate(2,new Node(2,1,0,0),new Node(1,0,0,0),new Node(4,0,-1,0));
            var neighbours=neighbourhoodReference.GetPlatesSharingEdgeWith(new[]{null,neighbourhoodReference,neighbourhoodMatch,neighbourhoodMatch});
            if(neighbours.Count()!=2||!ReferenceEquals(neighbours.First(),neighbourhoodMatch)||!ReferenceEquals(neighbours.Last(),neighbourhoodMatch))throw new Exception("Neighbour identity/order/duplicates changed.");
            bool neighbourReadOnly=false;try{((IList<Plate>)neighbours).Clear();}catch(NotSupportedException){neighbourReadOnly=true;}if(!neighbourReadOnly)throw new Exception("Neighbour collection is mutable.");
            if(neighbourhoodReference.GetPlatesSharingEdgeWith(new[]{new Plate(1,new Node(1,double.NaN,0,0),null,null)}).Any())throw new Exception("Same-ID self exclusion changed.");
            var planFar=new Beam(new Node(1,5,0,0),new Node(2,6,0,0));var planNear=new Beam(new Node(3,1,0,0),new Node(4,2,0,0));
            var planGroups=BeamGroupingService.GroupBeamsByPlanAngle(new[]{planFar,planNear,planNear},new Node(0,0,0,0));
            if(planGroups.Count!=1||planGroups[0].Key!=180||planGroups[0].Beams.Count!=3||!ReferenceEquals(planGroups[0].Beams[0],planNear)||!ReferenceEquals(planGroups[0].Beams[2],planFar))throw new Exception("Inward bearing/midpoint ordering changed.");
            planNear.StartNode.Z=8;if(planGroups[0].Key!=180||!ReferenceEquals(planGroups[0].Beams[0],planNear))throw new Exception("Plan grouping became deferred.");
            if(BeamGroupingService.GroupBeamsByPlanAngle(new[]{new Beam(new Node(0,0,0,0),new Node(1,1,0,0))},new Node(0,0,0,0)).Any())throw new Exception("Undefined origin bearing was grouped.");
            var chord=new Beam(new Node(1,3,50,4),new Node(2,-3,-50,4));
            var radiusGroups=BeamGroupingService.GroupBeamsByRadius(new[]{chord,chord},new Node(0,0,100,0));
            if(radiusGroups.Count!=1||radiusGroups[0].Key!=5||radiusGroups[0].Beams.Count!=2||!ReferenceEquals(radiusGroups[0].Beams[1],chord))throw new Exception("XZ radius/group occurrence semantics changed.");
            var low=new Beam(new Node(1,0,-2,0),new Node(2,8,-2,9));var high=new Beam(new Node(3,0,3,0),new Node(4,8,3,9));
            var elevationGroups=BeamGroupingService.GroupBeamsByElevation(new[]{high,low,low});
            if(elevationGroups.Count!=2||elevationGroups[0].Key!=-2||elevationGroups[0].Beams.Count!=2||!ReferenceEquals(elevationGroups[1].Beams[0],high))throw new Exception("Signed elevation/stable grouping changed.");
            chord.StartNode.X=100;if(radiusGroups[0].Key!=5||radiusGroups[0].Beams.Count!=2)throw new Exception("Radius membership became deferred.");
            bool unrepresentable=false;try{BeamGroupingService.GroupBeamsByRadius(new[]{new Beam(new Node(1,double.MaxValue,0,double.MaxValue),new Node(2,1,0,0))},new Node(0,0,0,0));}catch(ArgumentException){unrepresentable=true;}if(!unrepresentable)throw new Exception("Unrepresentable radius was accepted.");
            var a=new Node(1,0,0,0);var b=new Node(2,.00001,0,0);var groups=new[]{a,b}.GetDuplicateNodes().Single();
            b.X=100;if(groups.Count()!=2||!ReferenceEquals(groups.Last(),b))throw new Exception("Diagnostic membership is deferred or identity was lost.");
            bool immutable=false;try{((IList<Node>)groups).Clear();}catch(NotSupportedException){immutable=true;}if(!immutable)throw new Exception("Diagnostic group is mutable.");
            var diagonal=new Beam(new Node(1,0,0,0),new Node(2,.4,.4,0));if(new[]{diagonal}.GetZeroLengthBeams(.5).Any())throw new Exception("Euclidean diagnostic became per-axis.");
            var tiny=new Beam(new Node(1,0,0,0),new Node(2,double.Epsilon,double.Epsilon,0));if(new[]{tiny}.GetZeroLengthBeams(double.Epsilon).Any())throw new Exception("Subnormal length underflowed into zero.");
            var target=new Node(1,0,0,0);var near=new Node(2,1,0,0);var far=new Node(3,3,0,0);
            if(!ReferenceEquals(new[]{target,far,near}.GetClosestNode(target),near))throw new Exception("Closest-node choice or reference exclusion changed.");
            if(new[]{new Node(1,.001,0,0)}.GetClosestNode(target)!=null)throw new Exception("Reference equality threshold changed.");
            var through=new Beam(new Node(4,-100,0,0),new Node(5,100,0,0));var nearby=new Beam(near,far);
            if(!ReferenceEquals(new[]{through,nearby}.GetClosestBeam(target),nearby))throw new Exception("Closest beam uses segment distance.");
            var origin=new Node(1,-double.MaxValue,0,0);var hugeFar=new Node(2,double.MaxValue,0,0);var hugeNear=new Node(3,double.MaxValue/2,0,0);
            if(!ReferenceEquals(new[]{hugeFar,hugeNear}.GetClosestNode(origin),hugeNear))throw new Exception("Huge nearest-node distances collapsed.");
            var subNear=new Node(2,double.Epsilon,0,0);var subFar=new Node(3,double.Epsilon,double.Epsilon,0);
            if(!ReferenceEquals(new[]{subFar,subNear}.GetClosestNode(target),subNear))throw new Exception("Subnormal nearest-node distances collapsed.");
            bool rejected=false;try{new[]{new Node(1,0,0,0),new Node(1,1,0,0)}.GetDuplicateNodes();}catch(ArgumentException){rejected=true;}if(!rejected)throw new Exception("Conflicting node identities disappeared.");
        }
    }
}
