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
            foreach(string name in new[]{"GetDuplicateNodes","GetZeroLengthBeams"})
            {
                var method=typeof(GeometryDiagnosticsExtensions).GetMethod(name);
                var parameters=method.GetParameters();Type element=name=="GetDuplicateNodes"?typeof(Node):typeof(Beam);
                Type expected=name=="GetDuplicateNodes"?typeof(IEnumerable<IEnumerable<Node>>):typeof(IEnumerable<Beam>);
                if(method.ReturnType!=expected||parameters.Length!=2||parameters[0].ParameterType!=typeof(IEnumerable<>).MakeGenericType(element)||parameters[0].Name!=(name=="GetDuplicateNodes"?"nodesCollection":"beamsCollection")||parameters[0].IsOptional||parameters[1].Name!="tolerance"||!parameters[1].IsOptional||!Equals(parameters[1].DefaultValue,.0001d))throw new Exception("Geometry diagnostic signature changed.");
                string id="M:StaadPro.Interop.Extensions.GeometryDiagnosticsExtensions."+name+"(System.Collections.Generic.IEnumerable{"+element.FullName+"},System.Double)";
                var entry=docs.Descendants("member").Single(x=>(string)x.Attribute("name")==id);
                foreach(string tag in new[]{"summary","returns","remarks","exception","example"})if(entry.Element(tag)==null)throw new Exception("Incomplete geometry diagnostic IntelliSense.");
                if(entry.Elements("param").Count()!=2||entry.Descendants().Any(x=>x.Name=="include"||x.Name=="inheritdoc"))throw new Exception("Unexpanded geometry diagnostic XML.");
            }
            var a=new Node(1,0,0,0);var b=new Node(2,.00001,0,0);var groups=new[]{a,b}.GetDuplicateNodes().Single();
            b.X=100;if(groups.Count()!=2||!ReferenceEquals(groups.Last(),b))throw new Exception("Diagnostic membership is deferred or identity was lost.");
            bool immutable=false;try{((IList<Node>)groups).Clear();}catch(NotSupportedException){immutable=true;}if(!immutable)throw new Exception("Diagnostic group is mutable.");
            var diagonal=new Beam(new Node(1,0,0,0),new Node(2,.4,.4,0));if(new[]{diagonal}.GetZeroLengthBeams(.5).Any())throw new Exception("Euclidean diagnostic became per-axis.");
            var tiny=new Beam(new Node(1,0,0,0),new Node(2,double.Epsilon,double.Epsilon,0));if(new[]{tiny}.GetZeroLengthBeams(double.Epsilon).Any())throw new Exception("Subnormal length underflowed into zero.");
            bool rejected=false;try{new[]{new Node(1,0,0,0),new Node(1,1,0,0)}.GetDuplicateNodes();}catch(ArgumentException){rejected=true;}if(!rejected)throw new Exception("Conflicting node identities disappeared.");
        }
    }
}
