using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using StaadPro.Interop.Entities;
using StaadPro.Interop.Extensions;
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
