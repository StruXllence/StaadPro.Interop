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
    internal static class CoincidenceXRangeFixture
    {
        internal static void Verify(XDocument docs)
        {
            var a = new Node(1, 0, 0, 0); var b = new Node(99, 0.0001, -0.0001, 0.0001);
            if (!a.HasSameCoordinatesAs(b) || ((Node)null).HasSameCoordinatesAs(null)) throw new Exception("Coincidence/null semantics changed.");
            var first = new Beam(7, a, new Node(2, 1, 0, 0));
            var duplicate = new Beam(7, new Node(3, 0.5, 0, 0), new Node(4, 1, 0, 0));
            var crossing = new Beam(8, new Node(5, -1, 0, 0), new Node(6, 2, 0, 0));
            var selected = new[] { first, duplicate, crossing }.InXRange(0, 1);
            if (selected.Count != 1 || !ReferenceEquals(selected.Single(), first) || !selected.Contains(duplicate)) throw new Exception("Range whole-extent/ID semantics changed.");
            var nearby = new Node(1, 0.0005, 0, 0);
            if (!a.Equals(nearby) || new[] { a, nearby }.InXRange(0, 1).Count != 2) throw new Exception("Range node tolerance changed.");
            if (new[] { a }.InXRange(double.NegativeInfinity, double.PositiveInfinity).Count != 1) throw new Exception("Open-ended range changed.");
            foreach (var method in typeof(GeometryDiagnosticsExtensions).GetMethods().Where(m => m.Name == "HasSameCoordinatesAs" || m.Name == "InXRange"))
            {
                bool range = method.Name == "InXRange"; var p = method.GetParameters();
                if (!p.Select(x => x.Name).SequenceEqual(range ? new[] { "source", "xMin", "xMax" } : new[] { "left", "right" }) ||
                    p.Any(x => x.IsOptional || x.ParameterType.IsByRef) || method.GetCustomAttributes(typeof(ExtensionAttribute), false).Length != 1 ||
                    (range ? method.ReturnType.GetGenericTypeDefinition() != typeof(HashSet<>) || !method.GetGenericArguments().Single().GetGenericParameterConstraints().SequenceEqual(new[] { typeof(IEntity) }) : method.ReturnType != typeof(bool)))
                    throw new Exception("Coincidence/range signature changed.");
                string prefix = "M:" + method.DeclaringType.FullName + "." + method.Name + (range ? new string((char)96, 2) + "1(" : "(");
                var entry = docs.Descendants("member").Single(e => ((string)e.Attribute("name")).StartsWith(prefix));
                foreach (string tag in new[] { "summary", "param", "returns", "remarks", "exception", "example" })
                    if (entry.Element(tag) == null) throw new Exception("Coincidence/range documentation missing.");
            }
        }
    }
}
