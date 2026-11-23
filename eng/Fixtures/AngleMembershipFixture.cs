using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using StaadPro.Interop.Entities;
using StaadPro.Interop.Extensions;
using StaadPro.Interop.Models.Geometry;
using StaadPro.Interop.Services;

namespace PackageVerification
{
    internal static class AngleMembershipFixture
    {
        internal static void Verify(XDocument docs)
        {
            var up = new Node(1, 1, 0, 1); var down = new Node(2, 1, 0, -1);
            var angles = BeamGroupingService.GetUnwrappedAngles(new[] { down, up }, new Node(0, 0, 0, 0));
            if (angles.Count != 2 || !ReferenceEquals(angles[0].Node, up) || !ReferenceEquals(angles[1].Node, down) ||
                angles[0].AngleInRadians != Math.PI / 4 || angles[1].AngleInRadians != -Math.PI / 4)
                throw new Exception("Outward radians, sorted node order or suffix adjustment changed.");
            up.X = -99;
            if (angles[0].AngleInRadians != Math.PI / 4) throw new Exception("Angle result is a live coordinate projection.");
            var huge = BeamGroupingService.GetUnwrappedAngles(new[] { new Node(1, double.MaxValue, 0, double.MaxValue / 2) },
                new Node(0, -double.MaxValue, 0, -double.MaxValue)).Single();
            if (Math.Abs(huge.AngleInRadians - Math.Atan2(.75, 1)) > 1e-14) throw new Exception("Finite extreme direction overflowed.");
            var supplied = new Beam(7, new Node(1, 0, 0, 0), new Node(2, 1, 0, 0));
            var grouped = new Beam(7, new Node(90, 100, 0, 0), new Node(91, 101, 0, 0));
            var other = new Beam { Id = 8 }; var group = new EntityGroup<Beam>();
            group.Entities.Add(grouped);
            var remainder = new[] { supplied, other, other }.GetBeamsWithNoGroupAssignment(new[] { group });
            if (supplied.Equals(grouped) || remainder.Count() != 2 || !ReferenceEquals(remainder.First(), other))
                throw new Exception("Detached assignment must use IDs, preserve duplicates and retain references.");
            group.Entities.Clear(); other.Id = 7;
            if (remainder.Count() != 2) throw new Exception("Detached membership remained deferred.");
            bool readOnly = false; try { ((IList<Beam>)remainder).Clear(); } catch (NotSupportedException) { readOnly = true; }
            if (!readOnly) throw new Exception("Detached result storage is editable.");
            var angleMethod = typeof(BeamGroupingService).GetMethod("GetUnwrappedAngles");
            var groupMethod = typeof(GeometryDiagnosticsExtensions).GetMethod("GetBeamsWithNoGroupAssignment");
            if (angleMethod.ReturnType != typeof(List<UnwrappedNodeAngle>) ||
                !angleMethod.GetParameters().Select(p => p.ParameterType).SequenceEqual(new[] { typeof(IEnumerable<Node>), typeof(Node) }) ||
                !angleMethod.GetParameters().Select(p => p.Name).SequenceEqual(new[] { "nodes", "origin" }) ||
                groupMethod.ReturnType != typeof(IEnumerable<Beam>) ||
                !groupMethod.GetParameters().Select(p => p.ParameterType).SequenceEqual(new[] { typeof(IEnumerable<Beam>), typeof(IEnumerable<EntityGroup<Beam>>) }) ||
                !groupMethod.GetParameters().Select(p => p.Name).SequenceEqual(new[] { "beamsCollection", "groups" }))
                throw new Exception("New detached method signature changed.");
            foreach (var method in new[] { angleMethod, groupMethod })
            {
                if (method.GetParameters().Any(p => p.IsOptional || p.ParameterType.IsByRef)) throw new Exception("New public defaults/reference outputs added.");
                var entry = docs.Descendants("member").Single(e => ((string)e.Attribute("name")).StartsWith("M:" + method.DeclaringType.FullName + "." + method.Name + "("));
                foreach (string tag in new[] { "summary", "returns", "remarks", "exception", "example" })
                    if (entry.Element(tag) == null) throw new Exception("New detached method XML missing.");
                if (!entry.Elements("param").Select(e => (string)e.Attribute("name")).SequenceEqual(method.GetParameters().Select(p => p.Name)) ||
                    entry.Descendants().Any(e => e.Name == "include" || e.Name == "inheritdoc"))
                    throw new Exception("New detached XML parameters/expansion changed.");
            }
            var model = typeof(UnwrappedNodeAngle);
            if (!model.IsSealed || model.GetConstructors().Length != 0 ||
                model.GetProperties().Any(p => p.GetSetMethod(true) != null) ||
                !model.GetProperties().Select(p => p.Name).OrderBy(n => n).SequenceEqual(new[] { "AngleInRadians", "Node" }))
                throw new Exception("Named angle result must be immutable without tuple fields.");
            if (!docs.Descendants("member").Any(e => (string)e.Attribute("name") == "T:" + model.FullName && e.Element("summary") != null))
                throw new Exception("Named angle result XML missing.");
            foreach (var property in model.GetProperties(BindingFlags.Instance | BindingFlags.Public))
                if (!docs.Descendants("member").Any(e => (string)e.Attribute("name") == "P:" + model.FullName + "." + property.Name && e.Element("summary") != null && e.Element("value") != null))
                    throw new Exception("Named angle property XML missing.");
        }
    }
}
