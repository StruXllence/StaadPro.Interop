using System;
using System.Collections.Generic;
using System.Linq;
using StaadPro.Interop.Entities;
using StaadPro.Interop.Enums;
using StaadPro.Interop.Models.Geometry;
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
            return PlateConnectivityService.CaptureLoop(plate, nameof(plate)).Select(corner => corner.Node).ToList().AsReadOnly();
        }

        /// <summary>Gets directed edges between consecutive retained plate corners, including the closing edge.</summary>
        /// <param name="plate">Non-null plate whose supplied A-B-C-D slots are captured. Null slots are skipped;
        /// every present corner must have finite coordinates in a common coordinate system and length unit.</param>
        /// <returns>An immediately materialized read-only collection of named PlateEdge results in winding order.
        /// Each edge goes from one retained corner to the next, with the last wrapping to the first. Zero retained
        /// corners gives zero edges, one gives a self-edge, and two gives both directed edges. A usual triangle gives
        /// three edges and a four-distinct-corner plate gives four. Endpoints retain original Node references.</returns>
        /// <remarks><para>Uses the same first-representative A-B-C-D corner extraction as GetCornerLoop. Equivalent
        /// corners require matching IDs and 3D Euclidean separation at most Constants.Tolerance (0.001 model length
        /// units); missing slots are skipped and a repeated closing corner is omitted. Equality is not transitive:
        /// omitted corners do not bridge later deduplication. Draft IDs are allowed. All present coordinates are
        /// validated before edge construction, including later apparently repeated corners.</para>
        /// <para>Returns named endpoint slots instead of a public tuple. Validation and materialization happen when
        /// called, not on later enumeration. Owned read-only collection storage and immutable edge slots preserve
        /// membership/order after plate edits. Nodes are not cloned: their later ID/coordinate edits remain visible
        /// without recalculating membership or order. Repeated enumeration reuses the same observations.</para>
        /// <para>Sparse/collapsed loops are permitted extraction inputs. A returned edge does not certify positive
        /// length, plate area, planarity, orientation, self-intersection, native membership or engineering connectivity.
        /// Does not query STAAD, hydrate incidence, merge, convert units, select, save, analyze or mutate geometry.
        /// Captures are not atomic; callers must coordinate simultaneous changes.</para></remarks>
        /// <exception cref="ArgumentNullException">plate is null.</exception>
        /// <exception cref="ArgumentException">A present corner coordinate is NaN or infinite.</exception>
        /// <example><code>
        /// var a = new StaadPro.Interop.Entities.Node(1, 0, 0, 0);
        /// var b = new StaadPro.Interop.Entities.Node(2, 1, 0, 0);
        /// var c = new StaadPro.Interop.Entities.Node(3, 0, 1, 0);
        /// var plate = new StaadPro.Interop.Entities.Plate(a, b, c);
        /// foreach (var edge in StaadPro.Interop.Extensions.PlateExtensions.GetEdges(plate))
        ///     System.Console.WriteLine(edge.Start.Id + " to " + edge.End.Id); // 1 to 2, 2 to 3, 3 to 1
        /// </code></example>
        public static IEnumerable<PlateEdge> GetEdges(this Plate plate)
        {
            if (ReferenceEquals(plate, null)) throw new ArgumentNullException(nameof(plate));
            var loop = PlateConnectivityService.CaptureLoop(plate, nameof(plate));
            var edges = new List<PlateEdge>(loop.Length);
            for (int i = 0; i < loop.Length; i++)
                edges.Add(new PlateEdge(loop[i].Node, loop[(i + 1) % loop.Length].Node));
            return edges.AsReadOnly();
        }

        /// <summary>Determines whether two supplied plates share a retained corner or a consecutive edge.</summary>
        /// <param name="plate">Non-null first plate. Every supplied non-null corner must have finite coordinates.</param>
        /// <param name="targetPlate">Non-null comparison plate in compatible model context, coordinates and length units.</param>
        /// <param name="scope">SharedEdge (0, the default) requires a matching consecutive edge in both retained loops,
        /// in either direction. AnyCorner (1) requires one equivalent retained corner. Other values throw before geometry reads.</param>
        /// <returns>True when the selected condition holds using matching node IDs and 3D Euclidean distance at most
        /// Constants.Tolerance (0.001 model length units). Empty loops return false. SharedEdge requires at least two
        /// retained corners on each side; a one-corner self-edge never upgrades corner contact to edge contact.</returns>
        /// <remarks><para>Captures each plate's A-B-C-D slots/IDs/coordinates and qualifies all present coordinates on
        /// both sides before matching, even after an apparent early match or opposite an empty loop. Null corner slots
        /// are skipped, with first-retained-representative deduplication as in GetCornerLoop. Draft IDs are accepted;
        /// coordinate coincidence with another ID does not connect plates. Captured value comparisons avoid mutable
        /// HashSet membership assumptions. Plate IDs and cached adjacency data are unused.</para>
        /// <para>SharedEdge compares both endpoint pairs of actual consecutive edges, including last-to-first closure,
        /// and accepts either winding direction. Two shared corners forming a diagonal in either loop do not qualify.
        /// This corrects checking whether both endpoints merely occur somewhere in the target's corner set. AnyCorner
        /// allows corner-only/diagonal neighbours. Two-corner collapsed loops may qualify as extracted edges; this is
        /// topology over supplied retained data, not validation of a complete analytical plate.</para>
        /// <para>Same-object inputs use the same rule: a nonempty loop connects to itself in AnyCorner, while SharedEdge
        /// requires at least two retained corners. Sparse or collapsed loops are allowed and no area, planarity,
        /// self-intersection, solver membership or engineering suitability is certified. The Boolean is an immediate
        /// observation; later edits do not recompute it. No native acquisition, hydration, selection, conversion, merge,
        /// save, analysis or mutation occurs. Callers coordinate concurrent writers; captures are not atomic.</para></remarks>
        /// <exception cref="ArgumentNullException">plate or targetPlate is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">scope is not SharedEdge or AnyCorner.</exception>
        /// <exception cref="ArgumentException">A present corner in either plate has a NaN or infinite coordinate.</exception>
        /// <example><code>
        /// var a = new StaadPro.Interop.Entities.Node(1, 0, 0, 0);
        /// var b = new StaadPro.Interop.Entities.Node(2, 1, 0, 0);
        /// var first = new StaadPro.Interop.Entities.Plate(a, b, new StaadPro.Interop.Entities.Node(3, 0, 1, 0));
        /// var second = new StaadPro.Interop.Entities.Plate(b, a, new StaadPro.Interop.Entities.Node(4, 0, -1, 0));
        /// System.Console.WriteLine(StaadPro.Interop.Extensions.PlateExtensions.IsConnectedToPlate(first, second)); // True
        /// System.Console.WriteLine(StaadPro.Interop.Extensions.PlateExtensions.IsConnectedToPlate(first, second,
        ///     StaadPro.Interop.Enums.PlateConnectivityScope.AnyCorner)); // True
        /// </code></example>
        public static bool IsConnectedToPlate(this Plate plate, Plate targetPlate,
            PlateConnectivityScope scope = PlateConnectivityScope.SharedEdge)
        {
            if (ReferenceEquals(plate, null)) throw new ArgumentNullException(nameof(plate));
            if (ReferenceEquals(targetPlate, null)) throw new ArgumentNullException(nameof(targetPlate));
            if (scope != PlateConnectivityScope.SharedEdge && scope != PlateConnectivityScope.AnyCorner)
                throw new ArgumentOutOfRangeException(nameof(scope));
            var left = PlateConnectivityService.CaptureLoop(plate, nameof(plate));
            var right = PlateConnectivityService.CaptureLoop(targetPlate, nameof(targetPlate));
            return scope == PlateConnectivityScope.SharedEdge
                ? PlateConnectivityService.ShareEdge(left, right)
                : PlateConnectivityService.ShareCorner(left, right);
        }

        /// <summary>Gets candidate plate occurrences sharing an actual consecutive edge with a supplied plate.</summary>
        /// <param name="plate">Non-null reference plate. Present corner coordinates must be finite. Its captured ID
        /// excludes candidates with the same ID, including draft zero/negative IDs.</param>
        /// <param name="candidatePlates">Non-null sequence enumerated once. Null entries and candidates with the same
        /// captured plate ID are skipped. Every other candidate's present corners must have finite coordinates.</param>
        /// <returns>An immediately materialized owned read-only collection through IEnumerable, retaining matching
        /// original Plate references, input order and duplicate occurrences. Empty/no-match input returns an empty
        /// collection. No partial result escapes on a later validation or enumeration failure.</returns>
        /// <remarks><para>Captures the reference plate ID/retained corner values once and each eligible candidate's
        /// retained corner values once, before filtering. All eligible geometry is qualified even after a prior match,
        /// or when the reference has no usable edge. Null/same-ID candidates are unused and their geometry is not read.
        /// Preserves source self-exclusion by plate ID rather than relying on differing plate equality implementations.</para>
        /// <para>Uses IsConnectedToPlate's corrected SharedEdge semantics: consecutive endpoint pairs in both retained
        /// A-B-C-D loops, including closure and either direction. Each loop needs two retained corners; corner-only,
        /// target-diagonal or one-corner self-edge matches do not qualify. Node identity requires matching IDs plus
        /// inclusive scaled 3D Euclidean separation at most 0.001 model length units. Missing slots are skipped and
        /// first-retained-representative deduplication is nontransitive. Draft node IDs are allowed.</para>
        /// <para>Membership/order is fixed at invocation, with original editable Plate/Node references retained. Later
        /// edits remain visible through those references but do not rerun filtering. No STAAD acquisition, geometry
        /// hydration, selection, unit conversion, merge, save, analysis or mutation occurs. Does not certify plate
        /// validity, native membership, solver connectivity or engineering suitability. Inputs must share compatible
        /// model coordinates/length units; callers coordinate concurrent writers because captures are not atomic.</para></remarks>
        /// <exception cref="ArgumentNullException">plate or candidatePlates is null.</exception>
        /// <exception cref="ArgumentException">A present corner of the reference or an eligible candidate is nonfinite.</exception>
        /// <example><code>
        /// var a = new StaadPro.Interop.Entities.Node(1, 0, 0, 0);
        /// var b = new StaadPro.Interop.Entities.Node(2, 1, 0, 0);
        /// var plate = new StaadPro.Interop.Entities.Plate(1, a, b, new StaadPro.Interop.Entities.Node(3, 0, 1, 0));
        /// var neighbour = new StaadPro.Interop.Entities.Plate(2, b, a, new StaadPro.Interop.Entities.Node(4, 0, -1, 0));
        /// foreach (var match in StaadPro.Interop.Extensions.PlateExtensions.GetPlatesSharingEdgeWith(plate,
        ///     new[] { plate, neighbour })) System.Console.WriteLine(match.Id); // 2
        /// </code></example>
        public static IEnumerable<Plate> GetPlatesSharingEdgeWith(this Plate plate, IEnumerable<Plate> candidatePlates)
        {
            if (ReferenceEquals(plate, null)) throw new ArgumentNullException(nameof(plate));
            if (candidatePlates == null) throw new ArgumentNullException(nameof(candidatePlates));
            int id = plate.Id;
            var reference = PlateConnectivityService.CaptureLoop(plate, nameof(plate));
            var candidates = new List<KeyValuePair<Plate, PlateConnectivityService.Corner[]>>();
            foreach (var candidate in candidatePlates)
            {
                if (ReferenceEquals(candidate, null) || candidate.Id == id) continue;
                candidates.Add(new KeyValuePair<Plate, PlateConnectivityService.Corner[]>(candidate,
                    PlateConnectivityService.CaptureLoop(candidate, nameof(candidatePlates))));
            }
            return candidates.Where(candidate => PlateConnectivityService.ShareEdge(reference, candidate.Value))
                .Select(candidate => candidate.Key).ToList().AsReadOnly();
        }
    }
}
