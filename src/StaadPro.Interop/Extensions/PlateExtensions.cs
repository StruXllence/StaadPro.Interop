using System;
using System.Collections.Generic;
using System.Linq;
using StaadPro.Interop.Entities;
using StaadPro.Interop.Services.Geometry.Detached;

namespace StaadPro.Interop.Extensions
{
    /// <summary>Provides collection and detached corner helpers for supplied analytical plates.</summary>
    public static class PlateExtensions
    {
        /// <include file="../Documentation/ApiMembers.xml" path="doc/members/member[@name='PlateExtensions.LastId`0(thisIEnumerable&lt;Plate&gt;)']/*" />
        public static int LastId(this IEnumerable<Plate> plates) => plates.Any() ? plates.Max(p => p.Id) : 0;

        /// <summary>Gets the first distinct supplied corners of a plate in its original A-B-C-D winding order.</summary>
        /// <param name="plate">Non-null plate to inspect. Missing corner slots are skipped; every supplied corner
        /// must have finite coordinates in a common coordinate system and model length unit.</param>
        /// <returns>An immediately materialized, read-only list containing zero to four original Node references.
        /// The first representative of each equivalent corner is retained. A triangle with D missing or equivalent
        /// to A normally yields three corners; a quadrilateral with four distinct corners yields four.
        /// The first corner is not appended again to close the loop. All missing slots yield an empty list.</returns>
        /// <remarks><para>Visits the captured A, B, C and D slots in that order, ignoring null slots anywhere in the plate.
        /// A corner is omitted when it equals an already retained representative: IDs must match and three-dimensional
        /// Euclidean separation must be at most Constants.Tolerance (0.001 model length units). Coincident corners with
        /// different IDs remain distinct; matching IDs beyond the tolerance remain distinct. Draft IDs are accepted.</para>
        /// <para>Distance equality is not transitive. Deduplication compares against retained representatives, not
        /// every previously visited or omitted corner, preserving first-seen order rather than clustering chains.
        /// All present corners are validated before deduplication, so a later invalid or apparently repeated corner
        /// cannot disappear behind an earlier representative. Uses captured IDs/coordinates and scaled distances.</para>
        /// <para>The list owns its read-only storage but does not clone nodes. Later plate-slot edits do not change
        /// membership or order; edits to retained nodes remain visible and do not rerun deduplication. Captures are not
        /// atomic across concurrent writers, so the caller must coordinate edits.</para>
        /// <para>This extracts supplied data; sparse or collapsed loops are allowed. It does not validate area, planarity,
        /// orientation, self-intersection, native plate membership or engineering suitability. It does not query STAAD,
        /// hydrate incidence, merge nodes, convert units, select, save or mutate the model.</para></remarks>
        /// <exception cref="ArgumentNullException">plate is null.</exception>
        /// <exception cref="ArgumentException">A present corner coordinate is NaN or infinite.</exception>
        /// <example><code>
        /// var a = new StaadPro.Interop.Entities.Node(1, 0, 0, 0);
        /// var b = new StaadPro.Interop.Entities.Node(2, 1, 0, 0);
        /// var c = new StaadPro.Interop.Entities.Node(3, 0, 1, 0);
        /// var plate = new StaadPro.Interop.Entities.Plate(a, b, c,
        ///     new StaadPro.Interop.Entities.Node(1, 0, 0, 0));
        /// var corners = StaadPro.Interop.Extensions.PlateExtensions.GetCornerLoop(plate);
        /// System.Console.WriteLine(corners.Count); // 3; A is not repeated at the end
        /// System.Console.WriteLine(System.Object.ReferenceEquals(corners[0], a)); // True
        /// </code></example>
        public static IReadOnlyList<Node> GetCornerLoop(this Plate plate)
        {
            if (ReferenceEquals(plate, null)) throw new ArgumentNullException(nameof(plate));
            var corners = new[] { plate.A, plate.B, plate.C, plate.D };
            var captured = new GeometryConnectivityService.NodeSnapshot[corners.Length];
            for (int i = 0; i < corners.Length; i++)
                if (!ReferenceEquals(corners[i], null))
                    captured[i] = new GeometryConnectivityService.NodeSnapshot(corners[i], nameof(plate));

            var retained = new List<GeometryConnectivityService.NodeSnapshot>();
            var loop = new List<Node>();
            for (int i = 0; i < corners.Length; i++)
            {
                if (captured[i] == null) continue;
                bool duplicate = false;
                foreach (var representative in retained)
                    if (GeometryConnectivityService.Equal(representative, captured[i])) { duplicate = true; break; }
                if (duplicate) continue;
                retained.Add(captured[i]);
                loop.Add(corners[i]);
            }
            return loop.AsReadOnly();
        }
    }
}
