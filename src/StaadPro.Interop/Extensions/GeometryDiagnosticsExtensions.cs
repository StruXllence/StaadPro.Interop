using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using StaadPro.Interop.Entities;
using StaadPro.Interop.Helpers;

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

        /// <summary>Finds the nearest eligible node by three-dimensional Euclidean distance.</summary>
        /// <param name="nodesCollection">Non-null candidates enumerated once. Null entries are ignored. Each non-null
        /// node requires finite coordinates. Repeated IDs are allowed; input order resolves equal-distance ties.</param>
        /// <param name="referenceNode">Non-null node whose ID and finite coordinates are captured before enumeration.
        /// Coordinates must use the same model length units as the candidates.</param>
        /// <returns>The original nearest eligible node reference, or null for an empty sequence or no eligible node.
        /// A candidate with the reference ID and distance at most Constants.Tolerance (0.001) is excluded,
        /// preserving Node equality semantics; a differently numbered coincident node remains eligible.</returns>
        /// <remarks><para>Captures each candidate's ID/coordinates once and compares scaled Euclidean distances,
        /// avoiding squared-distance overflow/underflow and retaining ordering even when subtraction overflows.
        /// This is an O(n) scan with constant auxiliary storage. Equal computed distances retain the first candidate.
        /// The entire sequence is consumed, even after a zero-distance match, so invalid later geometry and enumerator
        /// failures are not hidden. No sorting, distance cutoff, unit conversion or native call is performed.</para>
        /// <para>Returns an existing mutable entity, not a clone. Later edits do not recompute the choice. Coordinate
        /// reads are not atomic across concurrent writers; callers must serialize edits. Reference exclusion compares
        /// captured values using the existing equality threshold; it does not reject every occurrence of the same ID.
        /// No STAAD session, geometry hydration, selection, merge or model update occurs.</para></remarks>
        /// <exception cref="ArgumentNullException">nodesCollection or referenceNode is null.</exception>
        /// <exception cref="ArgumentException">The reference or any non-null candidate has nonfinite coordinates.</exception>
        /// <example><code>
        /// var target = new StaadPro.Interop.Entities.Node(1, 0, 0, 0);
        /// var candidates = new[] { target, new StaadPro.Interop.Entities.Node(2, 2, 0, 0),
        ///     new StaadPro.Interop.Entities.Node(3, 1, 0, 0) };
        /// var closest = StaadPro.Interop.Extensions.GeometryDiagnosticsExtensions.GetClosestNode(candidates, target);
        /// System.Console.WriteLine(closest.Id);
        /// </code></example>
        public static Node GetClosestNode(this IEnumerable<Node> nodesCollection, Node referenceNode)
        {
            if (nodesCollection == null) throw new ArgumentNullException(nameof(nodesCollection));
            if (ReferenceEquals(referenceNode, null)) throw new ArgumentNullException(nameof(referenceNode));
            Point reference = Capture(referenceNode, nameof(referenceNode));
            Node best = null; DistanceKey bestDistance = default;
            foreach (Node node in nodesCollection)
            {
                if (ReferenceEquals(node, null)) continue;
                Point point = Capture(node, nameof(nodesCollection));
                DistanceKey distance = DistanceKey.Between(point, reference);
                if (point.Id == reference.Id && distance.CompareTo(new DistanceKey(Constants.Tolerance, 1)) <= 0) continue;
                if (ReferenceEquals(best, null) || distance.CompareTo(bestDistance) < 0)
                { best = node; bestDistance = distance; }
            }
            return best;
        }

        /// <summary>Finds the beam with the nearest start or end node to a reference node.</summary>
        /// <param name="beamsCollection">Non-null beam candidates enumerated once. Null entries are ignored. Every
        /// non-null beam requires both endpoint nodes with finite coordinates. Repeated beams remain eligible.</param>
        /// <param name="referenceNode">Non-null reference with finite coordinates captured before enumeration.
        /// All coordinates must use the same model length units.</param>
        /// <returns>The original beam whose nearer endpoint has the least three-dimensional Euclidean distance,
        /// or null for an empty/all-null sequence. Equal computed distances retain the first beam.</returns>
        /// <remarks><para>Measures endpoints, not the perpendicular distance to a segment, line or beam surface.
        /// A beam through the reference can lose to another beam with a closer endpoint. Coincident endpoints are
        /// valid candidates; beam/node IDs do not exclude a member. Uses scaled distance comparisons to avoid
        /// squared overflow/underflow and distinguish distances beyond Double.MaxValue.</para>
        /// <para>Reads endpoint references and finite coordinate snapshots once per candidate and performs an O(n)
        /// scan with constant auxiliary storage. Consumes the full sequence even after a zero-distance match.
        /// Invalid later incidence/coordinates or enumerator exceptions propagate without returning a partial choice.
        /// Returns an existing mutable beam, not cloned geometry; later edits do not recompute the selection.
        /// Concurrent edits must be serialized by the caller. No native session, hydration, selection, unit conversion,
        /// analysis or model mutation is performed.</para></remarks>
        /// <exception cref="ArgumentNullException">beamsCollection or referenceNode is null.</exception>
        /// <exception cref="ArgumentException">The reference or an endpoint has nonfinite coordinates, or a non-null
        /// beam lacks either endpoint.</exception>
        /// <example><code>
        /// var target = new StaadPro.Interop.Entities.Node(1, 0, 0, 0);
        /// var beam = new StaadPro.Interop.Entities.Beam(10,
        ///     new StaadPro.Interop.Entities.Node(2, 1, 0, 0), new StaadPro.Interop.Entities.Node(3, 4, 0, 0));
        /// var closest = StaadPro.Interop.Extensions.GeometryDiagnosticsExtensions.GetClosestBeam(new[] { beam }, target);
        /// System.Console.WriteLine(closest.Id);
        /// </code></example>
        public static Beam GetClosestBeam(this IEnumerable<Beam> beamsCollection, Node referenceNode)
        {
            if (beamsCollection == null) throw new ArgumentNullException(nameof(beamsCollection));
            if (ReferenceEquals(referenceNode, null)) throw new ArgumentNullException(nameof(referenceNode));
            Point reference = Capture(referenceNode, nameof(referenceNode));
            Beam best = null; DistanceKey bestDistance = default;
            foreach (Beam beam in beamsCollection)
            {
                if (ReferenceEquals(beam, null)) continue;
                Node start = beam.StartNode, end = beam.EndNode;
                if (ReferenceEquals(start, null) || ReferenceEquals(end, null))
                    throw new ArgumentException("Every non-null beam requires both endpoint nodes.", nameof(beamsCollection));
                Point a = Capture(start, nameof(beamsCollection)), b = Capture(end, nameof(beamsCollection));
                DistanceKey da = DistanceKey.Between(a, reference), db = DistanceKey.Between(b, reference);
                DistanceKey distance = da.CompareTo(db) <= 0 ? da : db;
                if (ReferenceEquals(best, null) || distance.CompareTo(bestDistance) < 0)
                { best = beam; bestDistance = distance; }
            }
            return best;
        }

        // Keep length as scale * factor so even lengths above Double.MaxValue can be ordered.
        private readonly struct DistanceKey : IComparable<DistanceKey>
        {
            private readonly double scale, factor;
            internal DistanceKey(double scale, double factor) { this.scale = scale; this.factor = factor; }
            internal static DistanceKey Between(Point a, Point b)
            {
                double x = Math.Abs(a.X - b.X), y = Math.Abs(a.Y - b.Y), z = Math.Abs(a.Z - b.Z);
                double multiplier = 1;
                if (double.IsInfinity(x) || double.IsInfinity(y) || double.IsInfinity(z))
                {
                    x = Math.Abs(a.X / 2 - b.X / 2); y = Math.Abs(a.Y / 2 - b.Y / 2); z = Math.Abs(a.Z / 2 - b.Z / 2);
                    multiplier = 2;
                }
                double scale = Math.Max(x, Math.Max(y, z));
                if (scale == 0) return default;
                double nx = x / scale, ny = y / scale, nz = z / scale;
                return new DistanceKey(scale, multiplier * Math.Sqrt(nx * nx + ny * ny + nz * nz));
            }
            public int CompareTo(DistanceKey other)
            {
                if (scale == 0 || other.scale == 0) return scale.CompareTo(other.scale);
                return scale <= other.scale
                    ? ((scale / other.scale) * factor).CompareTo(other.factor)
                    : factor.CompareTo((other.scale / scale) * other.factor);
            }
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
