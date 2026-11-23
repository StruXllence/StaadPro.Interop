using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using StaadPro.Interop.Entities;
using StaadPro.Interop.Extensions;

namespace PackageVerification
{
    internal static class NodeExtractionFixture
    {
        internal static void Verify(XDocument docs)
        {
            var a = new Node(8, double.NaN, 0, 0); var b = new Node(3, 0, 0, 0); var duplicate = new Node(8, 99, 0, 0);
            var beamNodes = new[] { new Beam(a, b), new Beam(duplicate, a) }.GetNodesFromBeams();
            if (beamNodes.Count != 2 || !ReferenceEquals(beamNodes[0], a) || !ReferenceEquals(beamNodes[1], b))
                throw new Exception("Beam extraction ID-only ordering changed.");
            var plateNodes = new[] { new Plate(null, a, b, duplicate), new Plate() }.GetNodesFromPlates();
            if (plateNodes.Count != 2 || !ReferenceEquals(plateNodes[0], a) || !ReferenceEquals(plateNodes[1], b))
                throw new Exception("Plate extraction raw slot/null semantics changed.");
            beamNodes.Clear(); if (plateNodes.Count != 2) throw new Exception("Extraction list storage is shared.");
            a.Id = 99; if (plateNodes[0].Id != 99) throw new Exception("Extraction unexpectedly cloned nodes.");
            bool missing = false; try { new[] { new Beam(a, null) }.GetNodesFromBeams(); } catch (ArgumentException) { missing = true; }
            if (!missing || new[] { new Plate() }.GetNodesFromPlates().Count != 0) throw new Exception("Endpoint/sparse plate validation changed.");
            foreach (string name in new[] { "GetNodesFromBeams", "GetNodesFromPlates" })
            {
                var method = typeof(GeometryDiagnosticsExtensions).GetMethod(name);
                var parameters = method.GetParameters(); bool beams = name.EndsWith("Beams", StringComparison.Ordinal);
                if (method.ReturnType != typeof(List<Node>) || parameters.Length != 1 || parameters[0].Name != (beams ? "beams" : "plates") ||
                    parameters[0].ParameterType != (beams ? typeof(IEnumerable<Beam>) : typeof(IEnumerable<Plate>)) || parameters[0].IsOptional ||
                    method.GetCustomAttributes(typeof(ExtensionAttribute), false).Length != 1)
                    throw new Exception("Node extraction signature changed.");
                var entry = docs.Descendants("member").Single(e => ((string)e.Attribute("name")).StartsWith("M:" + method.DeclaringType.FullName + "." + name + "("));
                foreach (string tag in new[] { "summary", "param", "returns", "remarks", "exception", "example" })
                    if (entry.Element(tag) == null) throw new Exception("Node extraction IntelliSense missing.");
                if (entry.Descendants().Any(e => e.Name == "include" || e.Name == "inheritdoc")) throw new Exception("Node extraction XML is unexpanded.");
            }
        }
    }
}
