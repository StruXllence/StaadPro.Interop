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
            foreach(var method in new[]{radialMethod,boundsMethod,connectivityMethod,beamNodeMethod,memberMethod})
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
