using System;
using System.Collections.Generic;
using StaadPro.Interop.Common;
using StaadPro.Interop.Entities;

namespace StaadPro.Interop.Services.Geometry.Detached
{
    internal static class XRangeSelectionService
    {
        internal static HashSet<T> Select<T>(IEnumerable<T> source, double xMin, double xMax) where T : IEntity
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (double.IsNaN(xMin)) throw new ArgumentOutOfRangeException(nameof(xMin));
            if (double.IsNaN(xMax) || xMax < xMin) throw new ArgumentOutOfRangeException(nameof(xMax));
            Type type = typeof(T);
            if (type != typeof(Node) && type != typeof(Beam) && type != typeof(Plate))
                throw new NotSupportedException("X-range selection requires Node, Beam or Plate as the declared generic type.");
            var accepted = new List<Entry<T>>();
            foreach (T entity in source)
            {
                if (ReferenceEquals(entity, null)) throw new ArgumentException("A selected entity entry is null.", nameof(source));
                double minimum, maximum;
                GeometryConnectivityService.NodeSnapshot point = null;
                if (type == typeof(Node))
                {
                    point = new GeometryConnectivityService.NodeSnapshot((Node)(object)entity, nameof(source));
                    minimum = maximum = point.X;
                }
                else if (type == typeof(Beam))
                {
                    var beam = (Beam)(object)entity; Node start = beam.StartNode, end = beam.EndNode;
                    double a = X(start), b = X(end); minimum = Math.Min(a, b); maximum = Math.Max(a, b);
                }
                else
                {
                    var plate = (Plate)(object)entity; Node a = plate.A, b = plate.B, c = plate.C, d = plate.D;
                    double ax = X(a), bx = X(b), cx = X(c);
                    minimum = Math.Min(ax, Math.Min(bx, cx)); maximum = Math.Max(ax, Math.Max(bx, cx));
                    if (!ReferenceEquals(d, null)) { double dx = X(d); minimum = Math.Min(minimum, dx); maximum = Math.Max(maximum, dx); }
                }
                if (minimum >= xMin && maximum <= xMax)
                    accepted.Add(new Entry<T>(entity, point == null ? entity.Id : point.Id, point));
            }
            // Avoid publishing a set whose hash-relevant metadata changed while input was enumerated.
            foreach (var entry in accepted) entry.RequireStable();
            var result = new HashSet<T>(RangeComparer<T>.Instance);
            foreach (var entry in accepted) result.Add(entry.Entity);
            foreach (var entry in accepted) entry.RequireStable();
            return result;
        }

        private static double X(Node node)
        {
            if (ReferenceEquals(node, null)) throw new ArgumentException("Required beam/plate incidence is missing.", "source");
            double value = node.X;
            if (double.IsNaN(value) || double.IsInfinity(value)) throw new ArgumentException("Participating X coordinates must be finite.", "source");
            return value;
        }
        private sealed class Entry<T> where T : IEntity
        {
            internal readonly T Entity;
            private readonly int id;
            private readonly GeometryConnectivityService.NodeSnapshot point;
            internal Entry(T entity, int id, GeometryConnectivityService.NodeSnapshot point) { Entity = entity; this.id = id; this.point = point; }
            internal void RequireStable()
            {
                if (Entity.Id != id) throw new ArgumentException("An accepted entity ID changed during selection.", "source");
                if (point == null) return;
                var node = (Node)(object)Entity;
                if (node.X != point.X || node.Y != point.Y || node.Z != point.Z)
                    throw new ArgumentException("Accepted node coordinates changed during selection.", "source");
            }
        }
        private sealed class RangeComparer<T> : IEqualityComparer<T> where T : IEntity
        {
            internal static readonly RangeComparer<T> Instance = new RangeComparer<T>();
            public int GetHashCode(T entity) => ReferenceEquals(entity, null) ? 0 : entity.Id.GetHashCode();
            public bool Equals(T left, T right)
            {
                if (ReferenceEquals(left, right)) return true;
                if (ReferenceEquals(left, null) || ReferenceEquals(right, null) || left.Id != right.Id) return false;
                if (typeof(T) != typeof(Node)) return true;
                var a = (Node)(object)left; var b = (Node)(object)right;
                double dx = a.X - b.X, dy = a.Y - b.Y, dz = a.Z - b.Z;
                // Retain this selection's original tighter node-set tolerance, independently of global Node equality.
                return Math.Sqrt(dx * dx + dy * dy + dz * dz) <= 0.000001;
            }
        }
    }
}
