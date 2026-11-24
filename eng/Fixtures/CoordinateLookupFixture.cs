using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using StaadPro.Interop.Entities;
using StaadPro.Interop.Extensions;

namespace PackageVerification
{
    internal static class CoordinateLookupFixture
    {
        internal static void Verify(XDocument docs)
        {
            var first = new Node(-7, 0.0001, -0.0001, 0.0001);
            var exact = new Node(99, 0, 0, 0);
            var input = new[] { first, exact };
            if (!ReferenceEquals(input.FirstNodeWithSameCoordinatesAs(exact), first) ||
                !ReferenceEquals(input.FirstNodeWithSameCoordinatesAs(0, 0, 0), first))
                throw new Exception("Coordinate first-match/axis-box semantics changed.");
            if (Array.Empty<Node>().FirstNodeWithSameCoordinatesAs(0, 0, 0) != null ||
                !ReferenceEquals(new[] { exact }.FirstNodeWithSameCoordinatesAs(exact), exact))
                throw new Exception("Empty/self coordinate lookup changed.");
            IEnumerable<Node> StopsAtMatch() { yield return exact; throw new Exception("Unvisited tail"); }
            if (!ReferenceEquals(StopsAtMatch().FirstNodeWithSameCoordinatesAs(0, 0, 0), exact))
                throw new Exception("Coordinate lookup failed to stop at the first match.");
            bool rejected = false;
            try { new[] { new Node(1, 50, double.NaN, 0), exact }.FirstNodeWithSameCoordinatesAs(exact); }
            catch (ArgumentException e) { rejected = e.ParamName == "nodeCollection"; }
            if (!rejected) throw new Exception("Visited candidate validation changed.");
            var methods = typeof(GeometryDiagnosticsExtensions).GetMethods().Where(m => m.Name == "FirstNodeWithSameCoordinatesAs").ToArray();
            if (methods.Length != 2) throw new Exception("Expected exactly two coordinate lookup overloads.");
            foreach (var method in methods)
            {
                var p = method.GetParameters(); bool node = p.Length == 2;
                if (method.ReturnType != typeof(Node) || p.Any(x => x.IsOptional || x.ParameterType.IsByRef) ||
                    method.GetCustomAttributes(typeof(ExtensionAttribute), false).Length != 1 ||
                    !p.Select(x => x.Name).SequenceEqual(node ? new[] { "nodeCollection", "node" } : new[] { "nodeCollection", "x", "y", "z" }) ||
                    !p.Select(x => x.ParameterType).SequenceEqual(node ? new[] { typeof(IEnumerable<Node>), typeof(Node) } :
                        new[] { typeof(IEnumerable<Node>), typeof(double), typeof(double), typeof(double) }))
                    throw new Exception("Coordinate lookup source contract changed.");
                string suffix = node ? ",StaadPro.Interop.Entities.Node)" : ",System.Double,System.Double,System.Double)";
                var entry = docs.Descendants("member").Single(x => ((string)x.Attribute("name")).StartsWith("M:" + typeof(GeometryDiagnosticsExtensions).FullName + ".FirstNodeWithSameCoordinatesAs(") && ((string)x.Attribute("name")).EndsWith(suffix));
                foreach (string tag in new[] { "summary", "param", "returns", "remarks", "exception", "example" })
                    if (entry.Element(tag) == null) throw new Exception("Coordinate lookup IntelliSense missing.");
            }
        }
    }
}
