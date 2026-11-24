using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using StaadPro.Interop.Entities;
using StaadPro.Interop.Common;
using StaadPro.Interop.Models.Geometry;
using StaadPro.Interop.Helpers;

namespace StaadPro.Interop.Extensions
{
    /// <summary>Provides detached, non-mutating geometry comparisons and selection over supplied entities.</summary>
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


        /// <summary>Filters beams by their midpoint's inclusive radial band in the XZ plane.</summary>
        /// <param name="beamsCollection">Non-null sequence enumerated once. Null beams are skipped; every other beam
        /// must have both endpoint nodes with finite X, Y and Z coordinates. Order and repeated references are preserved.</param>
        /// <param name="minRadius">Finite nonnegative inclusive inner radius in the supplied model length units.</param>
        /// <param name="maxRadius">Finite inclusive outer radius, greater than or equal to minRadius, in the same units.</param>
        /// <param name="centre">Optional radial centre; null uses X=0 and Z=0. A supplied node must have finite coordinates.
        /// Its X/Z values are captured before enumeration. Y does not affect radial membership.</param>
        /// <returns>An owned, read-only, fully materialized collection of original matching beam references.</returns>
        /// <remarks><para>Measures the XZ distance from centre to the arithmetic midpoint of each beam's endpoints.
        /// This is a plan-view midpoint test, not a three-dimensional sphere, nearest-endpoint search, segment clipping,
        /// or a requirement that both endpoints lie in the band. A beam crossing the centre can match a zero-radius band
        /// even when both endpoints are far away. Height, axes and units are not inferred or converted.</para>
        /// <para>Uses overflow-aware midpoint arithmetic and scaled distance comparisons for huge and subnormal finite values.
        /// Boundaries are inclusive with no extra tolerance. Ordinary floating-point rounding still applies.
        /// Validates the complete sequence even after a match; malformed incidence/coordinates and enumerator failures
        /// throw rather than returning partial results. Null input/radii/centre are checked before enumeration.</para>
        /// <para>Enumerates once and freezes membership before return. Original entities remain mutable, while result storage
        /// is read-only. Capture is not atomic across concurrent writers; coordinate edits must be coordinated.
        /// No native calls, geometry hydration, selection, merging, analysis, save or model mutation occur.</para></remarks>
        /// <exception cref="ArgumentNullException">beamsCollection is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">A radius is nonfinite, negative, or maxRadius is smaller than minRadius.</exception>
        /// <exception cref="ArgumentException">The centre has nonfinite coordinates, or a non-null beam has missing/nonfinite endpoints.</exception>
        /// <example><code>
        /// var a = new StaadPro.Interop.Entities.Node(1, -10, 4, 0);
        /// var b = new StaadPro.Interop.Entities.Node(2, 10, 4, 0);
        /// var beams = new[] { new StaadPro.Interop.Entities.Beam(1, a, b) };
        /// var atCentre = StaadPro.Interop.Extensions.GeometryDiagnosticsExtensions.GetBeamsWithinRadius(beams, 0, 0);
        /// System.Console.WriteLine(atCentre.Count()); // midpoint at origin in XZ, despite Y=4
        /// </code></example>
        public static IEnumerable<Beam> GetBeamsWithinRadius(this IEnumerable<Beam> beamsCollection,
            double minRadius, double maxRadius, Node centre = null)
        {
            if (beamsCollection == null) throw new ArgumentNullException(nameof(beamsCollection));
            if (!Finite(minRadius) || minRadius < 0) throw new ArgumentOutOfRangeException(nameof(minRadius));
            if (!Finite(maxRadius) || maxRadius < minRadius) throw new ArgumentOutOfRangeException(nameof(maxRadius));
            Point origin = ReferenceEquals(centre, null) ? new Point(null, 0, 0, 0, 0) : Capture(centre, nameof(centre));
            var radialOrigin = new Point(null, 0, origin.X, 0, origin.Z);
            var minimum = new DistanceKey(minRadius, 1);
            var maximum = new DistanceKey(maxRadius, 1);
            var matches = new List<Beam>();
            foreach (Beam beam in beamsCollection)
            {
                if (ReferenceEquals(beam, null)) continue;
                Node start = beam.StartNode, end = beam.EndNode;
                if (ReferenceEquals(start, null) || ReferenceEquals(end, null))
                    throw new ArgumentException("Every non-null beam requires both endpoint nodes.", nameof(beamsCollection));
                Point a = Capture(start, nameof(beamsCollection)), b = Capture(end, nameof(beamsCollection));
                var midpoint = new Point(null, 0, Midpoint(a.X, b.X), 0, Midpoint(a.Z, b.Z));
                DistanceKey radius = DistanceKey.Between(midpoint, radialOrigin);
                if (radius.CompareTo(minimum) >= 0 && radius.CompareTo(maximum) <= 0) matches.Add(beam);
            }
            return new ReadOnlyCollection<Beam>(matches);
        }

        /// <summary>Captures the axis-aligned coordinate bounds of supplied nodes, beam endpoints or plate corners.</summary>
        /// <typeparam name="T">Exactly Node, Beam or Plate. A sequence declared as IEntity or another generic entity type is not supported.</typeparam>
        /// <param name="source">Non-null sequence enumerated once. Null entities are ignored. Beams require both endpoints;
        /// plates require A/B/C and may omit D for a triangle. Every participating coordinate must be finite.</param>
        /// <returns>An immutable AxisAlignedBounds containing XMin/XMax, YMin/YMax and ZMin/ZMax in the input units.</returns>
        /// <remarks><para>Uses all supplied nodes, both endpoints of every beam and the three/four corners of every plate.
        /// Computes coordinate extrema in one pass with constant auxiliary storage. Repeated entities do not change extrema.
        /// Supports a point or flat collection with equal minima/maxima. No width/volume calculation can overflow because
        /// this result retains coordinates, not subtracted extents.</para>
        /// <para>Unlike a spatial filter, this method returns a named bounds snapshot, not matching entities.
        /// It does not compute an oriented box, plate centroid, curved/offset envelope, native model extents or tolerance padding.
        /// Empty/all-null input throws rather than inventing zero bounds. Unsupported generic types are rejected before
        /// enumeration, even if their objects happen to be nodes. A triangle has no D; missing A/B/C is malformed.</para>
        /// <para>Reads the complete sequence and returns no partial result on invalid geometry or enumeration failure.
        /// The result remains unchanged after entity edits. Coordinate capture is not atomic across concurrent writers.
        /// No native acquisition, hydration, unit conversion, selection, analysis or mutation is performed.</para></remarks>
        /// <exception cref="ArgumentNullException">source is null.</exception>
        /// <exception cref="InvalidOperationException">T is unsupported or source contains no non-null supported entities.</exception>
        /// <exception cref="ArgumentException">A participating coordinate is nonfinite, a beam lacks an endpoint, or a plate lacks A/B/C.</exception>
        /// <example><code>
        /// var nodes = new[] { new StaadPro.Interop.Entities.Node(1, -2, 0, 4),
        ///     new StaadPro.Interop.Entities.Node(2, 3, 5, -1) };
        /// var bounds = StaadPro.Interop.Extensions.GeometryDiagnosticsExtensions.GetBoundingBox(nodes);
        /// System.Console.WriteLine(bounds.XMin + " to " + bounds.XMax);
        /// </code></example>
        public static AxisAlignedBounds GetBoundingBox<T>(this IEnumerable<T> source) where T : IEntity
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            Type type = typeof(T);
            if (type != typeof(Node) && type != typeof(Beam) && type != typeof(Plate))
                throw new InvalidOperationException("Bounds support Node, Beam or Plate as the generic entity type.");
            bool hasPoint = false;
            double xMin = 0, xMax = 0, yMin = 0, yMax = 0, zMin = 0, zMax = 0;
            void Include(Node node)
            {
                if (ReferenceEquals(node, null)) throw new ArgumentException("Required entity incidence is missing.", nameof(source));
                Point point = Capture(node, nameof(source));
                if (!hasPoint)
                {
                    xMin = xMax = point.X; yMin = yMax = point.Y; zMin = zMax = point.Z; hasPoint = true;
                }
                else
                {
                    xMin = Math.Min(xMin, point.X); xMax = Math.Max(xMax, point.X);
                    yMin = Math.Min(yMin, point.Y); yMax = Math.Max(yMax, point.Y);
                    zMin = Math.Min(zMin, point.Z); zMax = Math.Max(zMax, point.Z);
                }
            }
            foreach (T entity in source)
            {
                if (ReferenceEquals(entity, null)) continue;
                if (type == typeof(Node)) Include((Node)(object)entity);
                else if (type == typeof(Beam))
                {
                    var beam = (Beam)(object)entity;
                    Node start = beam.StartNode, end = beam.EndNode;
                    Include(start); Include(end);
                }
                else
                {
                    var plate = (Plate)(object)entity;
                    Node a = plate.A, b = plate.B, c = plate.C, d = plate.D;
                    Include(a); Include(b); Include(c);
                    if (!ReferenceEquals(d, null)) Include(d);
                }
            }
            if (!hasPoint) throw new InvalidOperationException("At least one non-null entity is required to compute bounds.");
            return new AxisAlignedBounds(xMin, xMax, yMin, yMax, zMin, zMax);
        }


        /// <summary>Filters supplied beams whose analytical ID occurs in none of the supplied groups.</summary>
        /// <param name="beamsCollection">Non-null sequence enumerated once after group membership is captured.
        /// Null beams are skipped. Order, repeated occurrences and original non-null beam references are retained.</param>
        /// <param name="groups">Non-null sequence enumerated once. Null groups and null entity entries are skipped;
        /// each non-null group's Entities set must be initialized. Names/families and geometry are not read.</param>
        /// <returns>An owned, read-only, eagerly materialized collection of original beams with no supplied
        /// group membership by captured Id. An empty or entirely grouped beam sequence returns an empty collection.</returns>
        /// <remarks><para>Checks only the supplied group data, not all groups in a live STAAD model. Unlike
        /// wrapper.Geometry.GetBeamsWithNoGroupAssignment, this detached overload performs no native call,
        /// model inventory, assignment hydration or selection. Missing groups in the input can make a grouped
        /// model beam appear unassigned; the result describes only these supplied collections.</para>
        /// <para>Membership uses analytical beam Id ONLY, matching the source helper's identifier identity.
        /// A distinct beam object with the same number is grouped even if endpoints differ or are absent.
        /// Does not use this assembly's geometry-sensitive Beam.Equals, node equality, endpoint order or reference
        /// identity. Draft zero/negative IDs are accepted and can match; IDs are not proof of native membership.
        /// Use data from one compatible model context because IDs from different models can collide.</para>
        /// <para>Captures all group IDs first, then each candidate ID once, ignoring group/entity hash-cache state.
        /// Duplicate grouped IDs collapse; duplicate ungrouped beam occurrences do not. Null Entities throws
        /// even for an empty beam sequence. Complete enumeration/qualification occurs before return; enumerator
        /// failures propagate without returning partial results. Later edits do not recompute membership.
        /// Original objects remain mutable, captures are not atomic and concurrent writers must be coordinated.
        /// Does not edit groups/beams, convert units, query geometry, analyze, save or acquire a session.</para></remarks>
        /// <exception cref="ArgumentNullException">beamsCollection or groups is null.</exception>
        /// <exception cref="ArgumentException">A non-null group has a null Entities collection.</exception>
        /// <example><code>
        /// var a = new StaadPro.Interop.Entities.Beam { Id = 1 };
        /// var b = new StaadPro.Interop.Entities.Beam { Id = 2 };
        /// var group = new StaadPro.Interop.Entities.EntityGroup&lt;StaadPro.Interop.Entities.Beam&gt;();
        /// group.Entities.Add(new StaadPro.Interop.Entities.Beam { Id = 1 });
        /// var remaining = StaadPro.Interop.Extensions.GeometryDiagnosticsExtensions.GetBeamsWithNoGroupAssignment(
        ///     new[] { a, b, b }, new[] { group });
        /// System.Console.WriteLine(remaining.Count()); // two occurrences of beam 2
        /// </code></example>
        public static IEnumerable<Beam> GetBeamsWithNoGroupAssignment(this IEnumerable<Beam> beamsCollection,
            IEnumerable<EntityGroup<Beam>> groups)
        {
            if (beamsCollection == null) throw new ArgumentNullException(nameof(beamsCollection));
            if (groups == null) throw new ArgumentNullException(nameof(groups));
            var assigned = new HashSet<int>();
            foreach (EntityGroup<Beam> group in groups)
            {
                if (ReferenceEquals(group, null)) continue;
                HashSet<Beam> entities = group.Entities;
                if (entities == null) throw new ArgumentException("A group has no initialized Entities collection.", nameof(groups));
                foreach (Beam beam in entities)
                    if (!ReferenceEquals(beam, null)) assigned.Add(beam.Id);
            }
            var result = new List<Beam>();
            foreach (Beam beam in beamsCollection)
            {
                if (ReferenceEquals(beam, null)) continue;
                int id = beam.Id;
                if (!assigned.Contains(id)) result.Add(beam);
            }
            return new ReadOnlyCollection<Beam>(result);
        }

        /// <summary>Extracts the first supplied node reference for each captured node ID from beam endpoints.</summary>
        /// <param name="beams">Non-null supplied beam sequence, enumerated once. Every entry must be non-null with
        /// initialized StartNode and EndNode references. Beam IDs and all coordinates are unused.</param>
        /// <returns>A new editable List of Node containing original references in first-encounter order: beam input
        /// order, then StartNode before EndNode. Duplicate captured node IDs retain only the first reference, even
        /// when later nodes with that ID have different coordinates. Empty input yields a new empty list.</returns>
        /// <remarks><para>Preserves the source node-ID dictionary semantics. Every Int32 ID is accepted, including
        /// zero/negative draft values. Different IDs remain distinct even at coincident coordinates. No Node equality,
        /// coordinate tolerance, geometry validation or node cloning participates. Nonfinite coordinates are retained
        /// as unused metadata rather than interpreted as valid geometry. This identifies supplied IDs only.</para>
        /// <para>Captures both endpoint references once per beam occurrence and each visited node ID once; first
        /// occurrences establish immutable deduplication keys for this call. Repeated beam occurrences are visited
        /// normally. The entire sequence is consumed before return, so later null endpoints or enumerator failures
        /// propagate at invocation and expose no partial list. Both endpoints are required even after earlier IDs match.</para>
        /// <para>The result owns its editable list storage, but node objects remain shared. Later input/list/ID edits
        /// do not rerun extraction; visible result IDs may cease to be unique. Concurrent or enumerator-driven ID
        /// changes can cause the same object to appear under different captured IDs; observations are not atomic.
        /// Callers must coordinate geometry and ID edits. Expected time is linear in endpoint
        /// occurrences with storage proportional to distinct captured IDs.</para>
        /// <para>Does not acquire STAAD, hydrate incidence, query connectivity, merge/renumber nodes, convert units,
        /// select, save, analyze or mutate supplied/native data. No native membership or engineering suitability is certified.</para></remarks>
        /// <exception cref="ArgumentNullException">beams is null.</exception>
        /// <exception cref="ArgumentException">A beam entry or either endpoint is null.</exception>
        /// <example><code>
        /// var first = new StaadPro.Interop.Entities.Node(1, 0, 0, 0);
        /// var duplicateId = new StaadPro.Interop.Entities.Node(1, 99, 0, 0);
        /// var other = new StaadPro.Interop.Entities.Node(2, 0, 0, 0);
        /// var beam = new StaadPro.Interop.Entities.Beam(first, other);
        /// var repeated = new StaadPro.Interop.Entities.Beam(duplicateId, first);
        /// var nodes = StaadPro.Interop.Extensions.GeometryDiagnosticsExtensions.GetNodesFromBeams(new[] { beam, repeated });
        /// System.Console.WriteLine(nodes.Count); // 2: node IDs 1, 2
        /// System.Console.WriteLine(System.Object.ReferenceEquals(nodes[0], first)); // True
        /// </code></example>
        public static List<Node> GetNodesFromBeams(this IEnumerable<Beam> beams)
        {
            if (beams == null) throw new ArgumentNullException(nameof(beams));
            var ids = new HashSet<int>();
            var result = new List<Node>();
            foreach (Beam beam in beams)
            {
                if (ReferenceEquals(beam, null)) throw new ArgumentException("A beam entry is missing.", nameof(beams));
                Node start = beam.StartNode, end = beam.EndNode;
                if (ReferenceEquals(start, null) || ReferenceEquals(end, null))
                    throw new ArgumentException("Both beam endpoints must be initialized.", nameof(beams));
                AddExtractedNode(start, ids, result);
                AddExtractedNode(end, ids, result);
            }
            return result;
        }

        /// <summary>Extracts the first supplied node reference for each captured ID from plate corner slots.</summary>
        /// <param name="plates">Non-null plate sequence, enumerated once. Every plate entry must be non-null.
        /// A/B/C/D slots may be null anywhere; only present corner references and their IDs are inspected.</param>
        /// <returns>A new editable List of Node containing original references in first-encounter order: plate input
        /// order, then A,B,C,D slot order. Null slots are skipped. The first node per captured Int32 ID is retained;
        /// repeated closing corners and later same-ID nodes are omitted. Empty/all-empty plates yield a new empty list.</returns>
        /// <remarks><para>Preserves source GetNodesFromPlates/GetNodes ID-only extraction, visiting raw A/B/C/D slots
        /// rather than a tolerance-deduplicated corner loop. All present IDs, including zero/negative draft IDs, are
        /// accepted. Same-ID corners with different coordinates keep the first reference, while different IDs at the
        /// same position remain distinct. Node/Plate equality, coordinate tolerances and plate geometry are unused;
        /// even nonfinite coordinates remain literal unvalidated metadata.</para>
        /// <para>Captures A/B/C/D references once per occurrence before visiting them and each present node ID once.
        /// Sparse/collapsed/triangular loops and completely empty plates are permitted; no area, winding, closure,
        /// planarity, incidence or connectivity check is inferred. Does not query the native model for missing slots.
        /// Enumeration or null-plate failures propagate at invocation with no partial list returned.</para>
        /// <para>Owns mutable list storage and retains shared Node objects. Later slot/collection edits do not change
        /// membership or order, but retained nodes' later edits remain visible without rededuplication. Visible IDs
        /// may no longer be unique; a changing object may appear twice under different captured IDs. Capture is not
        /// atomic; callers coordinate writers. Expected time is linear in visited corner slots, with storage proportional
        /// to distinct captured IDs.</para>
        /// <para>No STAAD acquisition, hydration, node merge/renumbering, unit conversion, selection, analysis, save or
        /// supplied/native mutation occurs. Extracted metadata does not certify native membership or engineering suitability.</para></remarks>
        /// <exception cref="ArgumentNullException">plates is null.</exception>
        /// <exception cref="ArgumentException">A plate entry is null. Missing corner slots are permitted and skipped.</exception>
        /// <example><code>
        /// var first = new StaadPro.Interop.Entities.Node(1, 0, 0, 0);
        /// var other = new StaadPro.Interop.Entities.Node(2, 0, 0, 0);
        /// var plate = new StaadPro.Interop.Entities.Plate(first, null, other, first);
        /// var nodes = StaadPro.Interop.Extensions.GeometryDiagnosticsExtensions.GetNodesFromPlates(new[] { plate, plate });
        /// System.Console.WriteLine(nodes.Count); // 2: sparse slots and duplicate IDs are handled
        /// System.Console.WriteLine(System.Object.ReferenceEquals(nodes[0], first)); // True
        /// </code></example>
        public static List<Node> GetNodesFromPlates(this IEnumerable<Plate> plates)
        {
            if (plates == null) throw new ArgumentNullException(nameof(plates));
            var ids = new HashSet<int>();
            var result = new List<Node>();
            foreach (Plate plate in plates)
            {
                if (ReferenceEquals(plate, null)) throw new ArgumentException("A plate entry is missing.", nameof(plates));
                Node a = plate.A, b = plate.B, c = plate.C, d = plate.D;
                if (!ReferenceEquals(a, null)) AddExtractedNode(a, ids, result);
                if (!ReferenceEquals(b, null)) AddExtractedNode(b, ids, result);
                if (!ReferenceEquals(c, null)) AddExtractedNode(c, ids, result);
                if (!ReferenceEquals(d, null)) AddExtractedNode(d, ids, result);
            }
            return result;
        }

        private const double CoordinateSearchTolerance = 0.0001;

        /// <summary>Finds the first supplied node within a fixed inclusive coordinate tolerance on all three axes.</summary>
        /// <param name="nodeCollection">Non-null candidate sequence, visited once in input order until the first match. Every visited entry must be non-null with finite X/Y/Z coordinates; later entries are not examined.</param>
        /// <param name="node">Non-null reference node with finite X/Y/Z values, captured before input enumeration. Its ID, equality and connectivity are unused; the reference itself is eligible to match.</param>
        /// <returns>The original first candidate reference for which each absolute X/Y/Z difference is at most 0.0001, or null when the valid sequence is empty or has no match. Does not return the nearest candidate, clone a node or allocate result collections.</returns>
        /// <remarks><para>Uses the fixed source tolerance 0.0001 in model length units, inclusively and independently on each axis. This is an axis-aligned box test, not a Euclidean radius; a diagonal candidate can qualify even when its Euclidean separation exceeds 0.0001. No optional tolerance, rounding, unit conversion, ID comparison, deduplication or equality override is used. Unlike GetClosestNode, input order decides the first match and the reference itself is not excluded. GetNodesFromBeams/GetNodesFromPlates instead deduplicate captured IDs; GetDuplicateNodes groups distinct IDs and accepts a caller-selected tolerance. Global Node equality uses both ID and a different distance tolerance.</para>
        /// <para>Validates the collection and reference coordinates at invocation, even for empty inputs. Reads each visited candidate's complete finite coordinate triple before comparing, and throws for null or nonfinite visited entries instead of silently treating malformed data as a non-match. Does not read any candidate ID, connected entities or native binding. Signed zero and ordinary finite extremes are accepted; subtraction follows Double arithmetic and an overflowed difference cannot match the small fixed tolerance.</para>
        /// <para>Streams once without sorting or materializing the sequence. Stops immediately at the first match and disposes the enumerator; later null/nonfinite entries, side effects or failures are deliberately unobserved. Enumerator/getter failures encountered before a match propagate unchanged. All work occurs before return, with no deferred requery or supplied/native mutation. Captured reference/candidate coordinates are not atomic across writers; callers coordinate edits. Returned nodes remain caller-owned and mutable, so later edits can invalidate the earlier match without recomputing it. No STAAD acquisition, geometry hydration, selection, merge, renumbering, analysis, save or native validation occurs.</para></remarks>
        /// <exception cref="ArgumentNullException">nodeCollection or node is null.</exception>
        /// <exception cref="ArgumentException">The reference node or a visited candidate contains nonfinite coordinates, or a visited candidate is null.</exception>
        /// <example><code>
        /// var candidate = new StaadPro.Interop.Entities.Node(99, 0.0001, -0.0001, 0);
        /// var nodes = new[] { candidate, new StaadPro.Interop.Entities.Node(1, 0, 0, 0) };
        /// var reference = new StaadPro.Interop.Entities.Node(7, 0, 0, 0);
        /// var match = StaadPro.Interop.Extensions.GeometryDiagnosticsExtensions.FirstNodeWithSameCoordinatesAs(
        ///     nodes, reference);
        /// System.Console.WriteLine(System.Object.ReferenceEquals(match, candidate)); // True: input order wins
        /// </code></example>
        public static Node FirstNodeWithSameCoordinatesAs(this IEnumerable<Node> nodeCollection, Node node)
        {
            if (nodeCollection == null) throw new ArgumentNullException(nameof(nodeCollection));
            if (ReferenceEquals(node, null)) throw new ArgumentNullException(nameof(node));
            double x = node.X, y = node.Y, z = node.Z;
            if (!Finite(x) || !Finite(y) || !Finite(z))
                throw new ArgumentException("Reference coordinates must be finite.", nameof(node));
            return FindFirstCoordinateMatch(nodeCollection, x, y, z);
        }

        /// <summary>Finds the first supplied node within a fixed inclusive coordinate tolerance on all three axes.</summary>
        /// <param name="nodeCollection">Non-null candidate sequence, visited once in input order until the first match. Every visited entry must be non-null with finite X/Y/Z coordinates; later entries are not examined.</param>
        /// <param name="x">Finite reference X coordinate in the same model length units as the candidates.</param>
        /// <param name="y">Finite reference Y coordinate in the same model length units as the candidates.</param>
        /// <param name="z">Finite reference Z coordinate in the same model length units as the candidates.</param>
        /// <returns>The original first candidate reference for which each absolute X/Y/Z difference is at most 0.0001, or null when the valid sequence is empty or has no match. Does not return the nearest candidate, clone a node or allocate result collections.</returns>
        /// <remarks><para>Uses the fixed source tolerance 0.0001 in model length units, inclusively and independently on each axis. This is an axis-aligned box test, not a Euclidean radius; a diagonal candidate can qualify even when its Euclidean separation exceeds 0.0001. No optional tolerance, rounding, unit conversion, ID comparison, deduplication or equality override is used. Unlike GetClosestNode, input order decides the first match and the reference itself is not excluded. GetNodesFromBeams/GetNodesFromPlates instead deduplicate captured IDs; GetDuplicateNodes groups distinct IDs and accepts a caller-selected tolerance. Global Node equality uses both ID and a different distance tolerance.</para>
        /// <para>Validates the collection and reference coordinates at invocation, even for empty inputs. Reads each visited candidate's complete finite coordinate triple before comparing, and throws for null or nonfinite visited entries instead of silently treating malformed data as a non-match. Does not read any candidate ID, connected entities or native binding. Signed zero and ordinary finite extremes are accepted; subtraction follows Double arithmetic and an overflowed difference cannot match the small fixed tolerance.</para>
        /// <para>Streams once without sorting or materializing the sequence. Stops immediately at the first match and disposes the enumerator; later null/nonfinite entries, side effects or failures are deliberately unobserved. Enumerator/getter failures encountered before a match propagate unchanged. All work occurs before return, with no deferred requery or supplied/native mutation. Captured reference/candidate coordinates are not atomic across writers; callers coordinate edits. Returned nodes remain caller-owned and mutable, so later edits can invalidate the earlier match without recomputing it. No STAAD acquisition, geometry hydration, selection, merge, renumbering, analysis, save or native validation occurs.</para></remarks>
        /// <exception cref="ArgumentNullException">nodeCollection is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">x, y or z is NaN or infinite.</exception>
        /// <exception cref="ArgumentException">A visited candidate is null or has nonfinite coordinates.</exception>
        /// <example><code>
        /// var candidate = new StaadPro.Interop.Entities.Node(99, 0.0001, -0.0001, 0);
        /// var nodes = new[] { candidate, new StaadPro.Interop.Entities.Node(1, 0, 0, 0) };
        /// var match = StaadPro.Interop.Extensions.GeometryDiagnosticsExtensions.FirstNodeWithSameCoordinatesAs(
        ///     nodes, 0, 0, 0);
        /// System.Console.WriteLine(System.Object.ReferenceEquals(match, candidate)); // True: input order wins
        /// </code></example>
        public static Node FirstNodeWithSameCoordinatesAs(this IEnumerable<Node> nodeCollection, double x, double y, double z)
        {
            if (nodeCollection == null) throw new ArgumentNullException(nameof(nodeCollection));
            if (!Finite(x)) throw new ArgumentOutOfRangeException(nameof(x), "The reference coordinate must be finite.");
            if (!Finite(y)) throw new ArgumentOutOfRangeException(nameof(y), "The reference coordinate must be finite.");
            if (!Finite(z)) throw new ArgumentOutOfRangeException(nameof(z), "The reference coordinate must be finite.");
            return FindFirstCoordinateMatch(nodeCollection, x, y, z);
        }

        private static Node FindFirstCoordinateMatch(IEnumerable<Node> nodeCollection, double x, double y, double z)
        {
            foreach (Node candidate in nodeCollection)
            {
                if (ReferenceEquals(candidate, null)) throw new ArgumentException("A visited node is null.", nameof(nodeCollection));
                double cx = candidate.X, cy = candidate.Y, cz = candidate.Z;
                if (!Finite(cx) || !Finite(cy) || !Finite(cz))
                    throw new ArgumentException("Visited node coordinates must be finite.", nameof(nodeCollection));
                if (Math.Abs(cx - x) <= CoordinateSearchTolerance && Math.Abs(cy - y) <= CoordinateSearchTolerance &&
                    Math.Abs(cz - z) <= CoordinateSearchTolerance) return candidate;
            }
            return null;
        }

        /// <summary>Tests coordinate coincidence using a fixed inclusive 0.0001 tolerance on each axis, without comparing IDs.</summary>
        /// <param name="left">First detached node; null returns false without reading the other node.</param>
        /// <param name="right">Second detached node; null returns false, including when both references are null.</param>
        /// <returns>True when both nodes have finite coordinates and each absolute X/Y/Z difference is at most 0.0001; otherwise false for valid nonmatching or null inputs.</returns>
        /// <remarks><para>Captures each non-null node's complete XYZ triple once. The fixed threshold is in the same model length units as the supplied data. This axis-aligned box test permits diagonal Euclidean separation greater than 0.0001. IDs, connectivity, hashing and Node.Equals are unused. The same finite node can match itself; a same-reference node with nonfinite data still throws. Null inputs take precedence over validating the other node.</para>
        /// <para>Uses the same coordinate criterion as FirstNodeWithSameCoordinatesAs, but compares one pair rather than searching a sequence. It does not replace entity identity, node-set equality, nearest-distance ranking or transitive clustering. General Node equality uses both ID and distance at 0.001; InXRange node-set equality instead uses ID and a 0.000001 distance threshold. No configurable tolerance, rounding, conversion or global equality change is introduced.</para>
        /// <para>NaN/infinite coordinates on non-null inputs produce explicit argument errors. Signed zero, subnormal values and finite extremes are accepted; an overflowing difference is too large to match. Captures are not atomic across writers. No mutation, cloning, STAAD acquisition/query, geometry hydration, selection, node merging/renumbering, analysis or save occurs; coordinate coincidence alone does not certify native identity or model suitability.</para></remarks>
        /// <exception cref="ArgumentException">A non-null pair contains nonfinite coordinates; left is validated before right.</exception>
        /// <example><code>
        /// var left = new StaadPro.Interop.Entities.Node(1, 0, 0, 0);
        /// var right = new StaadPro.Interop.Entities.Node(99, 0.0001, -0.0001, 0.0001);
        /// bool coincident = StaadPro.Interop.Extensions.GeometryDiagnosticsExtensions.HasSameCoordinatesAs(left, right);
        /// System.Console.WriteLine(coincident); // True: IDs and Euclidean radius are not used
        /// </code></example>
        public static bool HasSameCoordinatesAs(this Node left, Node right)
        {
            if (ReferenceEquals(left, null) || ReferenceEquals(right, null)) return false;
            double lx = left.X, ly = left.Y, lz = left.Z;
            if (!Finite(lx) || !Finite(ly) || !Finite(lz)) throw new ArgumentException("Coordinates must be finite.", nameof(left));
            double rx = right.X, ry = right.Y, rz = right.Z;
            if (!Finite(rx) || !Finite(ry) || !Finite(rz)) throw new ArgumentException("Coordinates must be finite.", nameof(right));
            return Math.Abs(lx - rx) <= CoordinateSearchTolerance && Math.Abs(ly - ry) <= CoordinateSearchTolerance &&
                Math.Abs(lz - rz) <= CoordinateSearchTolerance;
        }

        /// <summary>Selects nodes or whole beam/plate X extents inside an inclusive closed coordinate interval.</summary>
        /// <typeparam name="T">Exactly Node, Beam or Plate as the declared generic type. Interface-typed, mixed and other entity sequences are unsupported.</typeparam>
        /// <param name="source">Non-null finite entity sequence, enumerated once and fully validated before a set is returned. Null entries and missing required incidence are invalid.</param>
        /// <param name="xMin">Inclusive lower X bound in the same model length units; may be negative infinity for an open lower end. NaN is invalid.</param>
        /// <param name="xMax">Inclusive upper X bound, at least xMin; may be positive infinity for an open upper end. NaN is invalid.</param>
        /// <returns>A fresh editable HashSet retaining original qualifying entity references, with the first equal qualifying occurrence retained. Empty valid input/no matches return an empty set. Enumeration order of the returned set is unspecified.</returns>
        /// <remarks><para>Nodes qualify by X. A beam qualifies only when both endpoint X values lie in the interval; a plate requires all A/B/C and optional D X values to lie inside. This is complete extent containment, not midpoint selection, interval overlap or intersection. Bounds are inclusive with no expansion/tolerance or unit conversion. Beam endpoints and plate A/B/C must exist; D may be null or repeated. All visited participating X values must be finite, even on ultimately excluded entities. Node selections additionally require finite Y/Z for well-defined node-set equality; beam/plate Y/Z and incidence IDs are unused.</para>
        /// <para>The returned comparer preserves this selection's identity rules: beams and plates compare only integer IDs, including zero/negative/draft IDs; nodes require equal IDs and Euclidean separation at most 0.000001. These rules deliberately differ from general Beam/Plate geometry equality and Node's 0.001 equality tolerance. They also differ from HasSameCoordinatesAs, which ignores IDs and uses a 0.0001 per-axis box. Near-node equality is not transitive clustering; retained representatives can depend on input order. No entity ID, equality implementation or global tolerance is changed.</para>
        /// <para>Captures incidence/X bounds per occurrence and materializes qualifying metadata before building the set. Accepted IDs and accepted-node coordinates must remain stable while building; detectable changes throw rather than publishing corrupted hash membership. Captures/checks are not atomic, so callers coordinate concurrent writers. Full enumeration/validation occurs even after an early match; original enumeration failures propagate with no partial set returned. Working storage is proportional to qualifying occurrences.</para>
        /// <para>Owns editable set storage and retains mutable source objects. Later geometry edits do not reapply the range; Add/Remove do not run selection again. Keep stored IDs and node coordinates stable, or remove before editing/re-add afterwards. After bulk edits, rebuild through a materialized sequence with the returned comparer, for example new HashSet&lt;T&gt;(set.ToArray(), set.Comparer), to rehash and deduplicate. No cloning, native membership check, STAAD acquisition/query, geometry hydration, selection, analysis, save or supplied/native mutation is performed.</para></remarks>
        /// <exception cref="ArgumentNullException">source is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">A bound is NaN, or xMax is smaller than xMin.</exception>
        /// <exception cref="NotSupportedException">T is not exactly Node, Beam or Plate.</exception>
        /// <exception cref="ArgumentException">A visited entity/incidence is missing, participating coordinates are nonfinite, or accepted hash-relevant metadata changes during construction.</exception>
        /// <example><code>
        /// var start = new StaadPro.Interop.Entities.Node(1, 0, 0, 0);
        /// var end = new StaadPro.Interop.Entities.Node(2, 2, 0, 0);
        /// var beam = new StaadPro.Interop.Entities.Beam(7, start, end);
        /// var selected = StaadPro.Interop.Extensions.GeometryDiagnosticsExtensions.InXRange(new[] { beam }, 0, 2);
        /// System.Console.WriteLine(selected.Count); // 1: both endpoints are inside, including boundaries
        /// </code></example>
        public static HashSet<T> InXRange<T>(this IEnumerable<T> source, double xMin, double xMax) where T : IEntity =>
            StaadPro.Interop.Services.Geometry.Detached.XRangeSelectionService.Select(source, xMin, xMax);

        private static void AddExtractedNode(Node node, HashSet<int> ids, List<Node> result)
        {
            int id = node.Id;
            if (ids.Add(id)) result.Add(node);
        }

        private static double Midpoint(double a, double b)
        {
            double sum = a + b;
            return double.IsInfinity(sum) ? a / 2 + b / 2 : sum / 2;
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
