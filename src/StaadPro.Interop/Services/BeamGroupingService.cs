using System;
using System.Collections.Generic;
using System.Linq;
using StaadPro.Interop.Entities;
using StaadPro.Interop.Models;
using StaadPro.Interop.Models.Geometry;
using StaadPro.Interop.Services.Geometry.Detached;

namespace StaadPro.Interop.Services
{
    /// <summary>Provides detached grouping of supplied analytical beams using captured geometry.</summary>
    public static class BeamGroupingService
    {
        /// <summary>Groups beams whose endpoint-to-origin XZ bearings agree within a circular angular tolerance.</summary>
        /// <param name="beams">Non-null sequence of non-null beams with two finite initialized endpoints. Enumerated
        /// once; input order and duplicate occurrences participate. IDs are unused; draft IDs are allowed.</param>
        /// <param name="origin">Non-null finite reference point in compatible model coordinates and length units.
        /// X/Z define the bearing and midpoint radius; Y is validated but otherwise unused.</param>
        /// <param name="toleranceDegrees">Finite nonnegative inclusive circular difference in degrees; default 0.001.
        /// Zero requires equal computed bearings; values at least 180 accept every defined angular difference.</param>
        /// <returns>An immediately materialized editable list of independently owned BeamGroup of Double records,
        /// sorted by ascending seed bearing in [0,360). Each group's Beams list is sorted by captured XZ midpoint
        /// distance from origin, with original input order retained for distance ties. Empty/no qualifying input returns
        /// an empty list. Original Beam references and repeated occurrences are retained.</returns>
        /// <remarks><para>Preserves the source bearing convention: atan2(origin.Z - endpoint.Z, origin.X - endpoint.X),
        /// converted to degrees and normalized, measured from +X toward +Z for the endpoint-to-origin vector. These
        /// are inward bearings, not outward origin-to-endpoint bearings or member local-axis angles. Endpoints on
        /// opposite rays do not qualify at small tolerance; tangential/other beams whose endpoint bearings differ
        /// are skipped. An endpoint exactly coincident with origin in XZ has no defined bearing, so that beam is skipped.</para>
        /// <para>A qualifying beam uses its captured start bearing as key. Joins the first existing group whose seed
        /// has shortest circular separation at most toleranceDegrees; otherwise creates a new group. Seeds are never
        /// averaged or moved. Grouping is first-seed, order-dependent and nontransitive; it is not clustering or a
        /// physical-member identification operation. Circular wraparound at zero participates in comparisons.</para>
        /// <para>Captures origin once and every participating endpoint once, validating all entries before grouping.
        /// Invalid later geometry cannot disappear behind an earlier match or an ineligible beam. Uses scaled
        /// differences and midpoint distances to avoid overflow/underflow in finite extreme coordinates; ordinary
        /// floating-point rounding still applies. Midpoint ordering ignores Y. Worst-case grouping is quadratic.</para>
        /// <para>Owns result list/group list storage, with editable Key/Beams properties. Later source edits do not
        /// recompute keys, membership or order, though edits to retained beams remain visible. No native acquisition,
        /// selection, geometry hydration, conversion, merge, save, analysis or mutation occurs. Does not certify
        /// collinearity in 3D, model membership or engineering suitability. Caller coordinates concurrent writers;
        /// captures are not atomic and enumeration failures propagate.</para></remarks>
        /// <exception cref="ArgumentNullException">beams or origin is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">toleranceDegrees is negative, NaN or infinite.</exception>
        /// <exception cref="ArgumentException">A beam/endpoint is missing or any supplied participating coordinate is nonfinite.</exception>
        /// <example><code>
        /// var origin = new StaadPro.Interop.Entities.Node(0, 0, 0, 0);
        /// var far = new StaadPro.Interop.Entities.Beam(new StaadPro.Interop.Entities.Node(1, 3, 0, 0),
        ///     new StaadPro.Interop.Entities.Node(2, 4, 0, 0));
        /// var near = new StaadPro.Interop.Entities.Beam(new StaadPro.Interop.Entities.Node(3, 1, 0, 0),
        ///     new StaadPro.Interop.Entities.Node(4, 2, 0, 0));
        /// var groups = StaadPro.Interop.Services.BeamGroupingService.GroupBeamsByPlanAngle(new[] { far, near }, origin);
        /// System.Console.WriteLine(groups[0].Key); // 180: +X endpoints point inward toward origin
        /// System.Console.WriteLine(System.Object.ReferenceEquals(groups[0].Beams[0], near)); // True
        /// </code></example>
        public static List<BeamGroup<double>> GroupBeamsByPlanAngle(IEnumerable<Beam> beams, Node origin, double toleranceDegrees = 1e-3)
        {
            if (beams == null) throw new ArgumentNullException(nameof(beams));
            if (ReferenceEquals(origin, null)) throw new ArgumentNullException(nameof(origin));
            if (double.IsNaN(toleranceDegrees) || double.IsInfinity(toleranceDegrees) || toleranceDegrees < 0)
                throw new ArgumentOutOfRangeException(nameof(toleranceDegrees));
            var reference = new GeometryConnectivityService.NodeSnapshot(origin, nameof(origin));
            var captured = new List<Observation>();
            foreach (var beam in beams)
            {
                if (ReferenceEquals(beam, null)) throw new ArgumentException("A beam is missing.", nameof(beams));
                var points = GeometryConnectivityService.BeamNodes(beam, nameof(beams));
                captured.Add(new Observation(beam, points[0], points[1], reference));
            }
            var groups = new List<Group>();
            foreach (var item in captured)
            {
                if (!item.Defined || !EqualAngle(item.StartAngle, item.EndAngle, toleranceDegrees)) continue;
                var group = groups.FirstOrDefault(g => EqualAngle(g.Key, item.StartAngle, toleranceDegrees));
                if (group == null) { group = new Group(item.StartAngle); groups.Add(group); }
                group.Items.Add(item);
            }
            return groups.OrderBy(g => g.Key).Select(g => new BeamGroup<double>
            { Key = g.Key, Beams = g.Items.OrderBy(item => item.Distance).Select(item => item.Beam).ToList() }).ToList();
        }

        /// <summary>Groups beams whose two captured XZ endpoint radii agree within an inclusive length tolerance.</summary>
        /// <param name="beams">Non-null sequence of non-null beams with two initialized finite endpoints. Enumerated
        /// once with duplicate occurrences and input order retained. Node/beam IDs are unused.</param>
        /// <param name="origin">Non-null finite reference point in the same coordinates and length units as the
        /// endpoints. Only X/Z define radius; Y is validated but ignored.</param>
        /// <param name="tolerance">Finite nonnegative absolute length tolerance, default 0.001. Applied inclusively
        /// both to endpoint-radius agreement and to matching a group's fixed start-radius seed. Zero uses exact
        /// computed Double equality; no relative or angular tolerance is applied.</param>
        /// <returns>A new editable list of independent BeamGroup of Double records sorted by ascending seed radius.
        /// Each Beams list retains original references in input order, including duplicates. Empty/no qualifying input
        /// returns a new empty list. Group keys are finite nonnegative radii in supplied length units.</returns>
        /// <remarks><para>Computes XZ distance from origin to each endpoint. A beam qualifies when the two radii
        /// differ by at most tolerance; its start radius becomes the grouping value. Joins the first previously
        /// created seed within tolerance, otherwise creates a group. Seeds are not averaged or moved. This is
        /// input-order-dependent, nontransitive first-seed grouping, not circle fitting, curved-member detection or
        /// physical-member identification. Straight chords, coincident endpoints and origin-projected endpoints can
        /// qualify. Y does not affect eligibility or ordering. Reversing endpoints can change the seed within tolerance.</para>
        /// <para>Captures origin once and each endpoint once per occurrence before grouping; validates every entry,
        /// including otherwise ineligible beams. Scaled distance calculation avoids squaring overflow/underflow.
        /// Each computed radius must fit a finite Double; an unrepresentable radius throws rather than producing
        /// an infinite or NaN seed. Ordinary floating-point rounding applies to distances and tolerance boundaries.
        /// Worst-case grouping is quadratic; input and result storage grow with supplied occurrences.</para>
        /// <para>Owns outer/group list storage while retaining original Beam references. Later edits do not recompute
        /// keys, membership or order; mutable Key/Beams setters affect detached data only. Enumeration/validation
        /// failures propagate at invocation with no partial result. Captures are not atomic; coordinate concurrent
        /// writers. No native acquisition, geometry hydration, selection, conversion, analysis, save or mutation occurs.
        /// Compatible coordinates/units are the caller's responsibility; no engineering suitability is certified.</para></remarks>
        /// <exception cref="ArgumentNullException">beams or origin is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">tolerance is negative, NaN or infinite.</exception>
        /// <exception cref="ArgumentException">A beam/endpoint is missing, a participating coordinate is nonfinite,
        /// or a computed endpoint radius cannot be represented as a finite Double.</exception>
        /// <example><code>
        /// var origin = new StaadPro.Interop.Entities.Node(0, 0, 0, 0);
        /// var chord = new StaadPro.Interop.Entities.Beam(new StaadPro.Interop.Entities.Node(1, 3, 10, 4),
        ///     new StaadPro.Interop.Entities.Node(2, -3, -10, 4));
        /// var groups = StaadPro.Interop.Services.BeamGroupingService.GroupBeamsByRadius(new[] { chord, chord }, origin);
        /// System.Console.WriteLine(groups[0].Key); // 5 model length units; Y is ignored
        /// System.Console.WriteLine(groups[0].Beams.Count); // 2: repeated occurrences are retained
        /// </code></example>
        public static List<BeamGroup<double>> GroupBeamsByRadius(IEnumerable<Beam> beams, Node origin, double tolerance = 1e-3)
        {
            ValidateScalarArguments(beams, tolerance);
            if (ReferenceEquals(origin, null)) throw new ArgumentNullException(nameof(origin));
            var reference = new GeometryConnectivityService.NodeSnapshot(origin, nameof(origin));
            return GroupScalars(CaptureScalarBeams(beams, reference), tolerance);
        }

        /// <summary>Groups beams whose captured endpoint Y elevations agree within an inclusive length tolerance.</summary>
        /// <param name="beams">Non-null sequence of non-null beams with two initialized finite endpoints. Enumerated
        /// once with original order/duplicate occurrences retained. Node/beam IDs are unused.</param>
        /// <param name="tolerance">Finite nonnegative absolute Y difference in supplied length units, default 0.001.
        /// Applied inclusively both to endpoint agreement and to the first fixed group seed. Zero requires exact
        /// computed Y equality; no relative tolerance or model-unit conversion is applied.</param>
        /// <returns>A new editable list of independent BeamGroup of Double records sorted by ascending start-elevation
        /// seed, including negative elevations. Each Beams list retains input order, duplicates and original Beam
        /// references. Empty/no qualifying input returns a new empty list.</returns>
        /// <remarks><para>Requires the absolute difference of captured StartNode.Y and EndNode.Y to be at most
        /// tolerance; qualifying beams use start Y as their grouping value. Joins the first created fixed seed within
        /// tolerance, otherwise creates a new group. Seeds are never averaged or moved. This first-seed grouping is
        /// input-order-dependent and nontransitive. Endpoint reversal may change a seed within tolerance. Coincident
        /// endpoints qualify; this does not certify horizontal alignment beyond the supplied Y criterion.</para>
        /// <para>Validates/captures both endpoints of every occurrence before grouping. All X/Y/Z coordinates must be
        /// finite, though only Y participates. Finite opposite-sign differences exceeding Double range are outside
        /// every finite tolerance and do not match. Ordinary floating-point rounding applies at tolerance boundaries.
        /// Worst-case grouping is quadratic and storage grows with input occurrences.</para>
        /// <para>Owns outer/group list storage while retaining original Beam references. Later source changes do not
        /// recompute membership/keys/order. Mutable result setters affect detached data only. Enumeration/validation
        /// failures propagate at invocation with no partial result; callers coordinate concurrent writers because
        /// captures are not atomic. No native acquisition, hydration, selection, conversion, analysis, save or mutation
        /// occurs. Compatible coordinates/units remain the caller's responsibility; no engineering acceptance is inferred.</para></remarks>
        /// <exception cref="ArgumentNullException">beams is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">tolerance is negative, NaN or infinite.</exception>
        /// <exception cref="ArgumentException">A beam/endpoint is missing or any participating X/Y/Z coordinate is nonfinite.</exception>
        /// <example><code>
        /// var low = new StaadPro.Interop.Entities.Beam(new StaadPro.Interop.Entities.Node(1, 0, -2, 0),
        ///     new StaadPro.Interop.Entities.Node(2, 10, -2, 20));
        /// var high = new StaadPro.Interop.Entities.Beam(new StaadPro.Interop.Entities.Node(3, 0, 3, 0),
        ///     new StaadPro.Interop.Entities.Node(4, 10, 3, 20));
        /// var groups = StaadPro.Interop.Services.BeamGroupingService.GroupBeamsByElevation(new[] { high, low, low });
        /// System.Console.WriteLine(groups[0].Key); // -2
        /// System.Console.WriteLine(groups[0].Beams.Count); // 2
        /// </code></example>
        public static List<BeamGroup<double>> GroupBeamsByElevation(IEnumerable<Beam> beams, double tolerance = 1e-3)
        {
            ValidateScalarArguments(beams, tolerance);
            return GroupScalars(CaptureScalarBeams(beams, null), tolerance);
        }

        private static void ValidateScalarArguments(IEnumerable<Beam> beams, double tolerance)
        {
            if (beams == null) throw new ArgumentNullException(nameof(beams));
            if (double.IsNaN(tolerance) || double.IsInfinity(tolerance) || tolerance < 0)
                throw new ArgumentOutOfRangeException(nameof(tolerance));
        }

        private sealed class ScalarObservation
        {
            internal readonly Beam Beam;
            internal readonly double Start, End;
            internal ScalarObservation(Beam beam, double start, double end) { Beam = beam; Start = start; End = end; }
        }

        private static List<ScalarObservation> CaptureScalarBeams(IEnumerable<Beam> beams,
            GeometryConnectivityService.NodeSnapshot origin)
        {
            var captured = new List<ScalarObservation>();
            foreach (Beam beam in beams)
            {
                if (ReferenceEquals(beam, null)) throw new ArgumentException("A beam is missing.", nameof(beams));
                var points = GeometryConnectivityService.BeamNodes(beam, nameof(beams));
                double start = origin == null ? points[0].Y
                    : DistanceKey.Between(points[0].X, points[0].Z, origin.X, origin.Z).FiniteRadius(nameof(beams));
                double end = origin == null ? points[1].Y
                    : DistanceKey.Between(points[1].X, points[1].Z, origin.X, origin.Z).FiniteRadius(nameof(beams));
                captured.Add(new ScalarObservation(beam, start, end));
            }
            return captured;
        }

        private static List<BeamGroup<double>> GroupScalars(List<ScalarObservation> captured, double tolerance)
        {
            var groups = new List<BeamGroup<double>>();
            foreach (ScalarObservation item in captured)
            {
                if (Math.Abs(item.Start - item.End) > tolerance) continue;
                var group = groups.FirstOrDefault(g => Math.Abs(g.Key - item.Start) <= tolerance);
                if (group == null) { group = new BeamGroup<double> { Key = item.Start }; groups.Add(group); }
                group.Beams.Add(item.Beam);
            }
            return groups.OrderBy(g => g.Key).ToList();
        }


        /// <summary>Captures outward XZ node angles, sorts their normalized bearings and unwraps large consecutive gaps.</summary>
        /// <param name="nodes">Non-null sequence of non-null nodes with finite X/Y/Z coordinates, enumerated once.
        /// Repeated nodes/IDs remain separate occurrences; IDs are not used for sorting or filtering.</param>
        /// <param name="origin">Non-null finite reference point, captured before enumeration. X/Z define outward
        /// angles; Y is validated but otherwise unused. Supply compatible coordinates and length units.</param>
        /// <returns>A fresh editable List of immutable UnwrappedNodeAngle records holding original node references
        /// and captured angles in radians. Empty input returns a fresh empty list; equal normalized bearings retain
        /// input order. No tuple is exposed. Later entity/collection edits do not recompute angles or ordering.</returns>
        /// <remarks><para>Computes atan2(node.Z-origin.Z, node.X-origin.X), measured from +X toward +Z in the XZ plane.
        /// Adds 2*pi to negative values, then stably sorts these normalized angles. Unlike GroupBeamsByPlanAngle,
        /// this is an outward origin-to-node bearing in RADIANS, not an inward endpoint-to-origin key in degrees.
        /// Coincident XZ points are retained using Math.Atan2's zero/signed-zero convention; this does not infer
        /// a geometrically defined direction. Tiny negative angles can round to 2*pi during normalization.</para>
        /// <para>Scans consecutive sorted entries. If current minus previous exceeds pi, subtracts 2*pi from that
        /// entry and every following angle; if previous minus current exceeds pi, adds 2*pi to that suffix.
        /// The pi comparison is strict, with no tolerance. Does not inspect the closing last-to-first gap,
        /// choose an optimal seam, unwrap in original traversal order or sort again after adjustment.
        /// For bearings 10 and 350 degrees, result order remains the 10-degree node followed by the
        /// 350-degree node, with angles equivalent to +10 and -10 degrees expressed in radians.</para>
        /// <para>All coordinates are captured/validated and no partial result escapes on invalid geometry or
        /// enumeration failure. Uses halved finite coordinate differences when direct subtraction overflows;
        /// ordinary Double angle/rounding limits remain. Captures are not atomic across concurrent writers.
        /// Returned list owns its storage; immutable records retain mutable original nodes. No unit conversion,
        /// geometry hydration, grouping, native acquisition/selection, analysis or model mutation occurs.</para></remarks>
        /// <exception cref="ArgumentNullException">nodes or origin is null.</exception>
        /// <exception cref="ArgumentException">A node is null or any supplied coordinate is NaN or infinite.</exception>
        /// <example><code>
        /// var origin = new StaadPro.Interop.Entities.Node(0, 0, 0, 0);
        /// var nodes = new[] { new StaadPro.Interop.Entities.Node(1, 1, 0, 1),
        ///     new StaadPro.Interop.Entities.Node(2, 1, 0, -1) };
        /// var angles = StaadPro.Interop.Services.BeamGroupingService.GetUnwrappedAngles(nodes, origin);
        /// foreach (var angle in angles) System.Console.WriteLine(angle.Node.Id + ": " + angle.AngleInRadians);
        /// </code></example>
        public static List<UnwrappedNodeAngle> GetUnwrappedAngles(IEnumerable<Node> nodes, Node origin)
        {
            if (nodes == null) throw new ArgumentNullException(nameof(nodes));
            if (ReferenceEquals(origin, null)) throw new ArgumentNullException(nameof(origin));
            var centre = new GeometryConnectivityService.NodeSnapshot(origin, nameof(origin));
            var captured = new List<UnwrappedNodeAngle>();
            foreach (Node node in nodes)
            {
                var point = new GeometryConnectivityService.NodeSnapshot(node, nameof(nodes));
                double x = point.X - centre.X, z = point.Z - centre.Z;
                if (double.IsInfinity(x) || double.IsInfinity(z))
                { x = point.X / 2 - centre.X / 2; z = point.Z / 2 - centre.Z / 2; }
                double theta = Math.Atan2(z, x);
                if (theta < 0) theta += 2 * Math.PI;
                captured.Add(new UnwrappedNodeAngle(node, theta));
            }
            var result = captured.OrderBy(a => a.AngleInRadians).ToList();
            for (int i = 1; i < result.Count; i++)
            {
                double previous = result[i - 1].AngleInRadians, current = result[i].AngleInRadians;
                double turn = current - previous > Math.PI ? -2 * Math.PI :
                    previous - current > Math.PI ? 2 * Math.PI : 0;
                if (turn == 0) continue;
                for (int j = i; j < result.Count; j++)
                    result[j] = new UnwrappedNodeAngle(result[j].Node, result[j].AngleInRadians + turn);
            }
            return result;
        }

        private sealed class Group
        {
            internal readonly double Key;
            internal readonly List<Observation> Items = new List<Observation>();
            internal Group(double key) { Key = key; }
        }
        private sealed class Observation
        {
            internal readonly Beam Beam;
            internal readonly double StartAngle, EndAngle;
            internal readonly bool Defined;
            internal readonly DistanceKey Distance;
            internal Observation(Beam beam, GeometryConnectivityService.NodeSnapshot a,
                GeometryConnectivityService.NodeSnapshot b, GeometryConnectivityService.NodeSnapshot origin)
            {
                Beam = beam;
                Defined = (a.X != origin.X || a.Z != origin.Z) && (b.X != origin.X || b.Z != origin.Z);
                StartAngle = Angle(a, origin); EndAngle = Angle(b, origin);
                Distance = DistanceKey.Between(Midpoint(a.X, b.X), Midpoint(a.Z, b.Z), origin.X, origin.Z);
            }
        }
        private static bool EqualAngle(double a, double b, double tolerance)
        { double delta = Math.Abs(a - b); return Math.Min(delta, 360 - delta) <= tolerance; }
        private static double Angle(GeometryConnectivityService.NodeSnapshot point, GeometryConnectivityService.NodeSnapshot origin)
        {
            double x = origin.X - point.X, z = origin.Z - point.Z;
            if (double.IsInfinity(x) || double.IsInfinity(z))
            { x = origin.X / 2 - point.X / 2; z = origin.Z / 2 - point.Z / 2; }
            double degrees = Math.Atan2(z, x) * (180 / Math.PI);
            if (degrees < 0) degrees += 360;
            return degrees == 360 || degrees == 0 ? 0 : degrees;
        }
        private static double Midpoint(double a, double b)
        { return Math.Sign(a) == Math.Sign(b) ? a + (b - a) / 2 : (a + b) / 2; }
        private readonly struct DistanceKey : IComparable<DistanceKey>
        {
            private readonly double scale, factor;
            private DistanceKey(double scale, double factor) { this.scale = scale; this.factor = factor; }
            internal static DistanceKey Between(double ax, double az, double bx, double bz)
            {
                double x = Math.Abs(ax - bx), z = Math.Abs(az - bz), multiplier = 1;
                if (double.IsInfinity(x) || double.IsInfinity(z))
                { x = Math.Abs(ax / 2 - bx / 2); z = Math.Abs(az / 2 - bz / 2); multiplier = 2; }
                double scale = Math.Max(x, z);
                if (scale == 0) return default;
                double nx = x / scale, nz = z / scale;
                return new DistanceKey(scale, multiplier * Math.Sqrt(nx * nx + nz * nz));
            }
            public int CompareTo(DistanceKey other)
            {
                if (scale == 0 || other.scale == 0) return scale.CompareTo(other.scale);
                return scale <= other.scale ? ((scale / other.scale) * factor).CompareTo(other.factor)
                    : factor.CompareTo((other.scale / scale) * other.factor);
            }
            internal double FiniteRadius(string parameter)
            {
                double value = scale * factor;
                if (double.IsNaN(value) || double.IsInfinity(value))
                    throw new ArgumentException("A computed endpoint radius must fit a finite Double.", parameter);
                return value;
            }
        }
    }
}
