using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using StaadPro.Interop.Entities;

namespace StaadPro.Interop.Extensions
{
    /// <summary>Provides detached, non-mutating geometry diagnostics over supplied entity collections.</summary>
    /// <remarks>These methods do not acquire a STAAD session or perform native calls. Coordinate/tolerance values
    /// must use the same model length units. Collections own their storage; returned entity references remain editable.</remarks>
    public static class GeometryDiagnosticsExtensions
    {
        /// <summary>Groups distinctly numbered nodes whose coordinates coincide with each group's first node.</summary>
        /// <param name="nodesCollection">Non-null node sequence, enumerated once. Null entries are ignored. Repeated IDs
        /// with identical coordinates are represented by their first instance; conflicting coordinates for one ID throw.</param>
        /// <param name="tolerance">Finite, nonnegative inclusive tolerance in model length units, applied separately to
        /// X, Y and Z. Default is 0.0001. Zero requires exact coordinate equality; this is not a Euclidean radius.</param>
        /// <returns>An owned, read-only outer collection of read-only groups containing at least two differently numbered
        /// original node references. Input/seed order is preserved; singleton groups are omitted.</returns>
        /// <remarks><para>Captures each non-null node's ID and finite coordinates before grouping. Starting with the first
        /// unassigned node, includes all still-unassigned nodes within tolerance of that seed on every axis, then marks
        /// them assigned. This preserves seed-based grouping, not transitive clustering: A near B and B near C does not
        /// place C with A unless C is also near A. Node equality/global equality tolerances are not used for grouping.</para>
        /// <para>The collection is enumerated once and results are materialized before return. Subsequent edits to input
        /// collections or coordinates do not recompute membership. Entity objects themselves are not cloned; coordinate
        /// capture is not atomic across simultaneous writers, so callers must coordinate edits. Null entries are skipped,
        /// but invalid coordinates or conflicting snapshots for one node number are not silently discarded.</para>
        /// <para>No model validation, node merge, renumbering, selection, unit conversion or native change is performed.
        /// Worst-case grouping is quadratic in the number of distinct IDs; choose appropriately sized detached batches.
        /// Enumerator exceptions propagate before a result is returned.</para></remarks>
        /// <exception cref="ArgumentNullException">nodesCollection is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">tolerance is negative, NaN or infinite.</exception>
        /// <exception cref="ArgumentException">A coordinate is nonfinite or one node ID has conflicting coordinates.</exception>
        /// <example><code>
        /// var nodes = new[] { new StaadPro.Interop.Entities.Node(1, 0, 0, 0),
        ///     new StaadPro.Interop.Entities.Node(2, 0.00001, 0, 0) };
        /// var groups = StaadPro.Interop.Extensions.GeometryDiagnosticsExtensions.GetDuplicateNodes(nodes);
        /// foreach (var group in groups) System.Console.WriteLine(group.Count());
        /// </code></example>
        public static IEnumerable<IEnumerable<Node>> GetDuplicateNodes(this IEnumerable<Node> nodesCollection, double tolerance = 0.0001)
        {
            if (nodesCollection == null) throw new ArgumentNullException(nameof(nodesCollection));
            ValidateTolerance(tolerance);
            var points = new List<Point>();
            var byId = new Dictionary<int, Point>();
            foreach (Node node in nodesCollection)
            {
                if (ReferenceEquals(node, null)) continue;
                Point point = Capture(node, nameof(nodesCollection));
                if (byId.TryGetValue(point.Id, out Point previous))
                {
                    if (point.X != previous.X || point.Y != previous.Y || point.Z != previous.Z)
                        throw new ArgumentException("One node number has conflicting coordinate snapshots.", nameof(nodesCollection));
                    continue;
                }
                byId.Add(point.Id, point); points.Add(point);
            }
            var assigned = new bool[points.Count];
            var groups = new List<IEnumerable<Node>>();
            for (int i = 0; i < points.Count; i++)
            {
                if (assigned[i]) continue;
                var group = new List<Node>(); Point seed = points[i];
                for (int j = i; j < points.Count; j++)
                {
                    Point candidate = points[j];
                    if (assigned[j] || Math.Abs(candidate.X - seed.X) > tolerance ||
                        Math.Abs(candidate.Y - seed.Y) > tolerance || Math.Abs(candidate.Z - seed.Z) > tolerance) continue;
                    assigned[j] = true; group.Add(candidate.Node);
                }
                if (group.Count > 1) groups.Add(new ReadOnlyCollection<Node>(group));
            }
            return new ReadOnlyCollection<IEnumerable<Node>>(groups);
        }

        /// <summary>Finds beams whose endpoint Euclidean distance is at most the supplied tolerance.</summary>
        /// <param name="beamsCollection">Non-null beam sequence, enumerated once. Null beams are ignored; every non-null
        /// beam requires initialized start/end nodes with finite coordinates. Input order and repeated beams are retained.</param>
        /// <param name="tolerance">Finite, nonnegative inclusive Euclidean length threshold in model length units.
        /// Default is 0.0001. Zero requires coincident endpoints.</param>
        /// <returns>An owned, read-only materialized collection of the original matching beam references.</returns>
        /// <remarks><para>Captures each beam's endpoint references once and reads finite coordinate values before testing.
        /// Uses a scaled Euclidean comparison to avoid squared-length overflow/underflow, including subnormal deltas.
        /// Unlike GetDuplicateNodes, this threshold is a distance, not a per-axis box. Missing incidence or nonfinite
        /// geometry throws rather than being treated as an acceptable zero-length member.</para>
        /// <para>The sequence is enumerated once and the result is complete before return. Later mutations do not
        /// recompute membership; returned beams/nodes remain caller-owned mutable objects. Coordinate reads are not
        /// atomic across concurrent writers. Enumerator exceptions propagate without returning a partial result.
        /// No node/beam hydration, model analysis, deletion, repair, unit conversion or native call occurs.</para></remarks>
        /// <exception cref="ArgumentNullException">beamsCollection is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">tolerance is negative, NaN or infinite.</exception>
        /// <exception cref="ArgumentException">A non-null beam has missing incidence or nonfinite coordinates.</exception>
        /// <example><code>
        /// var start = new StaadPro.Interop.Entities.Node(1, 0, 0, 0);
        /// var end = new StaadPro.Interop.Entities.Node(2, 0, 0, 0);
        /// var beams = new[] { new StaadPro.Interop.Entities.Beam(1, start, end) };
        /// var invalid = StaadPro.Interop.Extensions.GeometryDiagnosticsExtensions.GetZeroLengthBeams(beams);
        /// System.Console.WriteLine(invalid.Count());
        /// </code></example>
        public static IEnumerable<Beam> GetZeroLengthBeams(this IEnumerable<Beam> beamsCollection, double tolerance = 0.0001)
        {
            if (beamsCollection == null) throw new ArgumentNullException(nameof(beamsCollection));
            ValidateTolerance(tolerance);
            var result = new List<Beam>();
            foreach (Beam beam in beamsCollection)
            {
                if (ReferenceEquals(beam, null)) continue;
                Node start = beam.StartNode, end = beam.EndNode;
                if (ReferenceEquals(start, null) || ReferenceEquals(end, null))
                    throw new ArgumentException("Every non-null beam requires both endpoint nodes.", nameof(beamsCollection));
                Point a = Capture(start, nameof(beamsCollection)), b = Capture(end, nameof(beamsCollection));
                double x = Math.Abs(a.X - b.X), y = Math.Abs(a.Y - b.Y), z = Math.Abs(a.Z - b.Z);
                double scale = Math.Max(x, Math.Max(y, z));
                if (scale == 0) { result.Add(beam); continue; }
                if (scale > tolerance) continue;
                double nx = x / scale, ny = y / scale, nz = z / scale;
                if (Math.Sqrt(nx * nx + ny * ny + nz * nz) <= tolerance / scale) result.Add(beam);
            }
            return new ReadOnlyCollection<Beam>(result);
        }

        private static void ValidateTolerance(double tolerance)
        {
            if (tolerance < 0 || double.IsNaN(tolerance) || double.IsInfinity(tolerance))
                throw new ArgumentOutOfRangeException(nameof(tolerance));
        }
        private static Point Capture(Node node, string parameter)
        {
            int id = node.Id; double x = node.X, y = node.Y, z = node.Z;
            if (!Finite(x) || !Finite(y) || !Finite(z)) throw new ArgumentException("Coordinates must be finite.", parameter);
            return new Point(node, id, x, y, z);
        }
        private static bool Finite(double value) => !double.IsNaN(value) && !double.IsInfinity(value);
        private sealed class Point
        {
            internal Point(Node node, int id, double x, double y, double z) { Node = node; Id = id; X = x; Y = y; Z = z; }
            internal readonly Node Node; internal readonly int Id; internal readonly double X, Y, Z;
        }
    }
}
