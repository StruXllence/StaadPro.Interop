using System;
using System.Collections.Generic;
using System.Linq;
using StaadPro.Interop.Entities;
using StaadPro.Interop.Models;
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
        }
    }
}
