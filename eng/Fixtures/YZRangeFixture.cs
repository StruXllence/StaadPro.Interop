using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using StaadPro.Interop.Common;
using StaadPro.Interop.Entities;
using StaadPro.Interop.Extensions;

namespace PackageVerification
{
    internal static class YZRangeFixture
    {
        internal static void Verify(XDocument docs)
        {
            var firstY = new Beam(7, new Node(1, double.NaN, 0, double.PositiveInfinity), new Node(2, 0, 1, 0));
            var duplicateY = new Beam(7, new Node(3, 0, 0.5, 0), new Node(4, 0, 1, 0));
            var crossingY = new Beam(8, new Node(5, 0, -1, 0), new Node(6, 0, 2, 0));
            var selectedY = new[] { firstY, duplicateY, crossingY }.InYRange(0, 1);
            if (selectedY.Count != 1 || !ReferenceEquals(selectedY.Single(), firstY) || !selectedY.Contains(duplicateY))
                throw new Exception("Y-range whole-extent/ID or off-axis semantics changed.");

            var firstZ = new Plate(9, new Node(1, double.NaN, double.NegativeInfinity, -2), new Node(2, 0, 0, 0), new Node(3, 0, 0, 2));
            var duplicateZ = new Plate(9, new Node(4, 0, 0, 1), new Node(5, 0, 0, 1), new Node(6, 0, 0, 1));
            var crossingZ = new Plate(10, new Node(7, 0, 0, -3), new Node(8, 0, 0, 0), new Node(9, 0, 0, 2));
            var selectedZ = new[] { firstZ, duplicateZ, crossingZ }.InZRange(-2, 2);
            if (selectedZ.Count != 1 || !ReferenceEquals(selectedZ.Single(), firstZ) || !selectedZ.Contains(duplicateZ))
                throw new Exception("Z-range whole-extent/ID or off-axis semantics changed.");

            var a = new Node(1, 0, 0, 0);
            var nearbyY = new Node(1, 0, 0.0005, 0);
            var nearbyZ = new Node(1, 0, 0, 0.0005);
            if (new[] { a, nearbyY }.InYRange(0, 1).Count != 2 || new[] { a, nearbyZ }.InZRange(0, 1).Count != 2)
                throw new Exception("Y/Z range node tolerance changed.");
            if (new[] { a }.InYRange(double.NegativeInfinity, double.PositiveInfinity).Count != 1 ||
                new[] { a }.InZRange(double.NegativeInfinity, double.PositiveInfinity).Count != 1)
                throw new Exception("Open-ended Y/Z range changed.");

            foreach (var method in typeof(GeometryDiagnosticsExtensions).GetMethods().Where(m => m.Name == "InYRange" || m.Name == "InZRange"))
            {
                bool isZ = method.Name == "InZRange";
                var p = method.GetParameters();
                if (!p.Select(x => x.Name).SequenceEqual(isZ ? new[] { "source", "zMin", "zMax" } : new[] { "source", "yMin", "yMax" }) ||
                    p.Any(x => x.IsOptional || x.ParameterType.IsByRef) ||
                    method.GetCustomAttributes(typeof(ExtensionAttribute), false).Length != 1 ||
                    method.ReturnType.GetGenericTypeDefinition() != typeof(HashSet<>) ||
                    !method.GetGenericArguments().Single().GetGenericParameterConstraints().SequenceEqual(new[] { typeof(IEntity) }))
                    throw new Exception("Y/Z range signature changed.");
                string prefix = "M:" + method.DeclaringType.FullName + "." + method.Name + new string((char)96, 2) + "1(";
                var entry = docs.Descendants("member").Single(e => ((string)e.Attribute("name")).StartsWith(prefix));
                foreach (string tag in new[] { "summary", "param", "returns", "remarks", "exception", "example" })
                    if (entry.Element(tag) == null) throw new Exception("Y/Z range documentation missing.");
            }
        }
    }
}
