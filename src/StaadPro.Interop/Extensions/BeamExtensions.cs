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

    }
}
