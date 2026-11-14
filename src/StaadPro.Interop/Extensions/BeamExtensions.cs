using System;
using System.Collections.Generic;
using StaadPro.Interop.Services.Geometry.Detached;
using System.Linq;
using StaadPro.Interop.Entities;

namespace StaadPro.Interop.Extensions
{
    /// <summary>Provides collection utilities and detached connectivity checks for analytical beams.</summary>
    public static class BeamExtensions
    {
        /// <include file="../Documentation/ApiMembers.xml" path="doc/members/member[@name='BeamExtensions.LastId`0(thisIEnumerable&lt;Beam&gt;)']/*" />
        public static int LastId(this IEnumerable<Beam> beams) => beams.Any() ? beams.Max(b => b.Id) : 0;

        /// <summary>Determines whether two supplied analytical beams share an equivalent endpoint node.</summary>
        /// <param name="beam">Non-null beam with both endpoint nodes initialized and finite coordinates.</param>
        /// <param name="targetBeam">Non-null comparison beam with both endpoint nodes initialized and finite coordinates.</param>
        /// <returns>True when any endpoint pair has equal node IDs and Euclidean separation at most
        /// Constants.Tolerance (0.001 model length units); otherwise false.</returns>
        /// <remarks><para>Uses captured node identity and coordinates, preserving the existing Node equality rule.
        /// Different IDs at identical coordinates do not connect. Equal IDs with separation exceeding 0.001 do not connect.
        /// Zero/nonpositive draft IDs are not rejected; this is detached equality, not validation of native model membership.
        /// Beam IDs, endpoint direction, cached connected-beam collections and geometric segment intersections are irrelevant.</para>
        /// <para>Checks all four endpoints before comparing, so an early matching pair does not hide missing/nonfinite incidence.
        /// Uses a scaled three-dimensional distance check without squared-distance overflow/underflow.
        /// Calling on the same valid beam returns true. No coordinate merging, topology hydration or native call occurs.
        /// Captures are not atomic across simultaneous writers; callers coordinate edits and use a common coordinate system/units.
        /// This is a supplied-data connectivity test, not solver connectivity or engineering acceptance.</para></remarks>
        /// <exception cref="ArgumentNullException">beam or targetBeam is null.</exception>
        /// <exception cref="ArgumentException">Either beam has missing endpoints or nonfinite coordinates.</exception>
        /// <example><code>
        /// var a = new StaadPro.Interop.Entities.Node(1, 0, 0, 0);
        /// var joint = new StaadPro.Interop.Entities.Node(2, 1, 0, 0);
        /// var c = new StaadPro.Interop.Entities.Node(3, 2, 0, 0);
        /// var first = new StaadPro.Interop.Entities.Beam(1, a, joint);
        /// var second = new StaadPro.Interop.Entities.Beam(2, joint, c);
        /// System.Console.WriteLine(StaadPro.Interop.Extensions.BeamExtensions.IsConnectedToBeam(first, second));
        /// </code></example>
        public static bool IsConnectedToBeam(this Beam beam, Beam targetBeam)
        {
            if (ReferenceEquals(beam, null)) throw new ArgumentNullException(nameof(beam));
            if (ReferenceEquals(targetBeam, null)) throw new ArgumentNullException(nameof(targetBeam));
            var left = GeometryConnectivityService.BeamNodes(beam, nameof(beam));
            var right = GeometryConnectivityService.BeamNodes(targetBeam, nameof(targetBeam));
            return GeometryConnectivityService.ShareNode(left, right);
        }

        /// <summary>Determines whether a supplied node is equivalent to either analytical beam endpoint.</summary>
        /// <param name="beam">Non-null beam with two initialized endpoint nodes and finite coordinates.</param>
        /// <param name="node">Non-null comparison node with finite coordinates in the same model coordinate system and length units.</param>
        /// <returns>True when the supplied node's ID matches an endpoint ID and their three-dimensional Euclidean
        /// separation is at most Constants.Tolerance (0.001 model length units); otherwise false.</returns>
        /// <remarks><para>Captures both endpoint references and all participating IDs/coordinates before matching.
        /// A match at one endpoint does not bypass validation of the other. Preserves Node equality identity and its
        /// inclusive distance threshold, using scaled arithmetic for very large or subnormal coordinate differences.
        /// IDs may be draft values; this helper cannot establish native model ownership or node existence.</para>
        /// <para>Only beam endpoints participate. A point on the beam interior qualifies only when it also equals an
        /// endpoint by the documented ID/distance rule. Coincident coordinates with a different ID remain distinct.
        /// Reads supplied managed entities without hydrating geometry, using cached connectivity, acquiring STAAD,
        /// selecting entities, converting units or modifying a model. Captures are not atomic across concurrent edits;
        /// callers must coordinate changes. Subsequent edits do not recompute this returned Boolean.</para></remarks>
        /// <exception cref="ArgumentNullException">beam or node is null.</exception>
        /// <exception cref="ArgumentException">A participating endpoint is missing or any participating coordinate is nonfinite.</exception>
        /// <example><code>
        /// var joint = new StaadPro.Interop.Entities.Node(2, 1, 0, 0);
        /// var beam = new StaadPro.Interop.Entities.Beam(10, new StaadPro.Interop.Entities.Node(1, 0, 0, 0), joint);
        /// var copy = new StaadPro.Interop.Entities.Node(2, 1, 0, 0);
        /// System.Console.WriteLine(StaadPro.Interop.Extensions.BeamExtensions.IsConnectedToNode(beam, copy));
        /// </code></example>
        public static bool IsConnectedToNode(this Beam beam, Node node)
        {
            if (ReferenceEquals(beam, null)) throw new ArgumentNullException(nameof(beam));
            if (ReferenceEquals(node, null)) throw new ArgumentNullException(nameof(node));
            var endpoints = GeometryConnectivityService.BeamNodes(beam, nameof(beam));
            var target = new GeometryConnectivityService.NodeSnapshot(node, nameof(node));
            return GeometryConnectivityService.ShareNode(endpoints, new[] { target });
        }

    }
}
