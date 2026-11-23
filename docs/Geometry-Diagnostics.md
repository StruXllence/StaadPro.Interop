# Detached geometry diagnostics

`GeometryDiagnosticsExtensions` provides detached collection extensions in `StaadPro.Interop.Extensions`. They inspect supplied entities without acquiring STAAD, hydrating geometry, changing selections or repairing the model. Coordinate values and tolerance must use the same model length units.

Related beam/node and physical-member connectivity extensions use selected node identity and coordinates. `PlateExtensions.GetCornerLoop` extracts ordered distinct supplied plate corners.

| Method | Meaning |
| --- | --- |
| `GetDuplicateNodes(nodesCollection, tolerance = 0.0001)` | Seed-based groups of distinctly numbered nodes within the inclusive tolerance on each axis. |
| `GetZeroLengthBeams(beamsCollection, tolerance = 0.0001)` | Beams whose endpoint Euclidean distance is at most the inclusive tolerance. |
| `GetClosestNode(nodesCollection, referenceNode)` | Nearest eligible node, excluding the reference by ID-plus-distance equality. |
| `GetClosestBeam(beamsCollection, referenceNode)` | Beam with the nearer endpoint closest to the reference. |
| `GetBeamsWithinRadius(beamsCollection, minRadius, maxRadius, centre = null)` | Materialized radial band filter over beam midpoints in the XZ plane. |
| `GetBoundingBox<T>(source)` | Immutable named axis-aligned bounds of Node, Beam or Plate collections. |
| `GetNodesFromBeams(beams)` | First supplied endpoint reference per captured node ID, in beam/start/end encounter order. |
| `GetNodesFromPlates(plates)` | First supplied corner reference per captured node ID, in plate/A-B-C-D order, skipping null slots. |
| `beam.IsConnectedToBeam(targetBeam)` | Endpoint pair with the same node ID and Euclidean separation at most 0.001. |
| `beam.IsConnectedToNode(node)` | Supplied node equivalent to either beam endpoint by ID and distance. |
| `member.IsConnectedToMember(targetMember, scope = EndpointsOnly)` | Equivalent node pair from both members' selected endpoints or supplied Nodes collections. |
| `member.IsConnectedToNode(node, scope = EndpointsOnly)` | Supplied node equivalent to an endpoint or a node in the selected supplied collection. |
| `plate.GetCornerLoop()` | Read-only list of first distinct supplied corners in A-B-C-D order; no repeated closing corner. |
| `plate.GetEdges()` | Named `PlateEdge` endpoint pairs between consecutive retained corners, wrapping last to first. |
| `plate.IsConnectedToPlate(targetPlate, scope = SharedEdge)` | A matching consecutive edge in both loops, or a common corner for `AnyCorner`. |
| `plate.GetPlatesSharingEdgeWith(candidatePlates)` | Ordered candidate occurrences sharing a consecutive edge in both loops, excluding the same plate ID. |
| `BeamGroupingService.GroupBeamsByPlanAngle(beams, origin, toleranceDegrees = 0.001)` | Radial XZ endpoint-to-origin bearing groups, sorted by seed bearing and midpoint radius. |
| `BeamGroupingService.GroupBeamsByRadius(beams, origin, tolerance = 0.001)` | Equal XZ endpoint-radius groups with start-radius seeds and ordered occurrences. |
| `BeamGroupingService.GroupBeamsByElevation(beams, tolerance = 0.001)` | Equal endpoint-Y elevation groups with signed start-elevation seeds and ordered occurrences. |

The duplicate/zero-length methods validate non-null collections and finite nonnegative tolerances before enumeration. Null entries are ignored. Nonfinite coordinates throw `ArgumentException`; missing endpoints on a non-null beam also throw. Inputs are enumerated once and results are fully materialized before return. Enumerator exceptions propagate without returning partial results.

Duplicate-node detection captures each node's ID and coordinates. Repeated IDs with exactly equal coordinates keep the first instance. Conflicting coordinates for one ID throw rather than silently choosing a representation. This makes node-number identity explicit without depending on the entity equality tolerance. Callers supplying unnumbered point clouds should assign distinct IDs first. With zero tolerance, coordinate equality is exact.

Groups follow first-seen seed order. Each seed collects all still-unassigned nodes within tolerance of it on X, Y and Z; groups of one are omitted. This is a per-axis box and is not transitive clustering or a spherical radius. If A is near B and B near C, C joins A only if C is also near A. Worst-case grouping is quadratic in distinct IDs; choose appropriately sized detached batches.

Zero-length checks use Euclidean endpoint distance, preserve input order and repeated beams, and use a scaled comparison to avoid squared-distance overflow or underflow. Coincident endpoints match zero tolerance. This diagnostic reports geometry only; it does not determine solver acceptance or validate member properties, releases or units.

Returned collections own read-only storage, while their nodes and beams are the original mutable references. Editing those objects later does not recalculate membership. Coordinate reads are not atomic across concurrent writers; coordinate edits should be serialized with the diagnostic. No unit conversion, merge, deletion, renumbering or native action is performed.

`GetClosestNode` and `GetClosestBeam` require a non-null collection and reference node with finite coordinates. They enumerate once and ignore null entries. Every non-null candidate is validated, even after a zero-distance match; missing beam endpoints and nonfinite coordinates throw `ArgumentException`. Enumerator failures propagate without a partial choice. Empty/all-null inputs return null. A node input with no eligible candidate also returns null.

Nearest-node exclusion preserves the existing `Node` equality rule: the candidate has the reference ID **and** Euclidean distance at most `Constants.Tolerance` (0.001 model length units). A differently numbered coincident node is eligible. A same-ID node outside that radius is also eligible; duplicate IDs are not silently deduplicated. The reference ID/coordinates are captured before enumeration and candidate values once per occurrence. This snapshots equality inputs instead of repeatedly reading mutable entities.

Nearest-beam distance is the minimum of its two endpoint distances. It is **not** a projection onto the segment, infinite line or member surface. A long beam passing through the reference may therefore lose to a short beam with a nearer endpoint. IDs do not exclude beams, and coincident endpoints are allowed. Each beam's endpoint references and coordinates are read before comparison.

Nearest searches retain the first candidate with an equal computed distance and return the original mutable object. They perform an O(n) scan using constant auxiliary storage rather than sorting. Scaled Euclidean comparisons avoid squared-distance overflow/underflow; halved coordinate differences retain ordering even when subtraction would exceed `Double.MaxValue`. Distance ordering still has normal floating-point precision limits. No length-unit conversion or maximum-distance cutoff is applied. Later edits do not recompute the result; concurrent writers must be coordinated by the caller.


## Plan-view radial filtering

`GetBeamsWithinRadius` measures the **arithmetic midpoint in the XZ plane**, relative to the supplied centre or the origin when null. It includes both radial boundaries without an additional tolerance. Y does not affect membership, although all supplied coordinates must be finite. The coordinate system is not inferred from a model's vertical-axis setting and units are not converted.

This differs from `GetClosestBeam`, which measures a three-dimensional distance to the nearer endpoint. It also differs from clipping a beam segment or requiring both endpoints to lie inside a band. A beam with endpoints on opposite sides of the origin can match a zero-radius band even if both endpoints are far outside it.

Radii must be finite, `minRadius >= 0` and `maxRadius >= minRadius`. The centre is captured before enumeration; null beams are skipped. Every other beam requires two finite endpoints. The method enumerates once, validates all items and returns owned read-only storage with original references, input order and repetitions retained. Membership does not change after return, even though the referenced entities remain mutable.

Midpoint arithmetic avoids overflowing a finite endpoint sum; scaled distance comparisons avoid squared-distance overflow/underflow and distinguish distances larger than Double.MaxValue. Normal floating-point rounding still applies. Invalid geometry or a failing enumerator throws without returning a partial result.

## Named bounds snapshots

`GetBoundingBox<T>` returns `StaadPro.Interop.Models.Geometry.AxisAlignedBounds`, an immutable class with `XMin`, `XMax`, `YMin`, `YMax`, `ZMin` and `ZMax`. It has no public constructor, setters, tuple fields or native ownership.

The generic type must be exactly `Node`, `Beam` or `Plate`. Node collections use their coordinates; beams use both endpoints; plates use A/B/C plus D when present. A missing D represents a triangle; a missing A/B/C is an error. Null entities are skipped. An empty/all-null sequence or unsupported generic type throws `InvalidOperationException`; a null source throws `ArgumentNullException`. A sequence declared as `IEnumerable<IEntity>` is unsupported even if its current elements are nodes.

The implementation performs one scan with constant auxiliary storage, captures finite coordinate extrema and permits point/flat bounds with equal minima and maxima. Repeated entities do not change the bounds. Extreme finite extrema remain representable because no width, area or volume is subtracted/multiplied. Missing incidence, nonfinite coordinates and enumerator errors throw. The snapshot does not track later mutations and is not an oriented box, centroid, solver envelope or live model boundary.

## Detached beam connectivity

`BeamExtensions.IsConnectedToBeam(beam, targetBeam)` checks all four endpoint pairs. A pair connects only if node IDs match **and** three-dimensional Euclidean distance is at most the existing `Constants.Tolerance` of 0.001 model length units. A different ID at identical coordinates is distinct; equal IDs far apart are distinct. Beam IDs and endpoint orientation do not matter. A valid beam connects to itself.

Both beams must be non-null, with two finite endpoints. All participating geometry is validated before comparison, so an early match cannot hide malformed incidence. Draft node IDs are accepted; this does not certify native model membership. Captured ID/coordinate data avoids mutable hash-set membership assumptions.

This method does not detect geometric segment crossings or a node in a segment's interior. It does not merge nodes, populate cached connected-beam lists, select geometry, query STAAD or determine solver/engineering connectivity. The two supplied objects must belong to a compatible coordinate system, model context and length unit, which the helper cannot infer.

`BeamExtensions.IsConnectedToNode(beam, node)` uses the same ID-and-distance test against either beam endpoint. Both endpoints and the comparison node require finite initialized geometry. A matching start node does not hide a missing or invalid end node. A point in the beam interior qualifies only when it also equals an endpoint by ID and distance. Positive native IDs are not required for these supplied-data checks; draft IDs retain equality semantics.

`MemberExtensions.IsConnectedToMember(member, targetMember, scope)` defaults to the existing `MemberConnectivityScope.EndpointsOnly` value (0). Both members' endpoints participate, while their `Nodes` and `Beams` collections are unused. `AnyNode` (1) instead captures exactly each supplied `Nodes` collection, so an interior T-junction is recognized when the joint is present in both selected sets. It does not add endpoints, scan constituent beams or populate missing collections. Endpoint geometry outside the supplied sets is unused in this scope. Undefined scope values throw `ArgumentOutOfRangeException` before geometry reads.

Each selected ID and coordinate is captured once. Both selections are fully qualified before matching. Missing selected collections/incidence, null selected nodes and nonfinite coordinates throw `ArgumentException`, even after an apparent match or opposite an empty set. Valid empty AnyNode selections return false. Capturing values and using an ID index avoids dependence on mutable HashSet keys or cached connected-member lists. Repeated IDs at distinct coordinates are compared individually. Collection modifications during enumeration propagate their exception; callers must coordinate edits. Large repeated-ID buckets may require pairwise comparisons.

The result is an immediate Boolean observation. Changing the supplied objects later does not recompute it. These checks use the same 3D Euclidean tolerance and require compatible model coordinates and length units. Native model membership and engineering connectivity cannot be inferred from the supplied data.

`MemberExtensions.IsConnectedToNode(member, node, scope)` uses the same default endpoint scope and exact supplied `Nodes` scope as member-to-member connectivity. `AnyNode` recognizes a supplied interior junction without adding endpoints or scanning beams. The comparison node is validated even when the selected collection is empty. All selected geometry is qualified before matching, including invalid entries after an apparent match. Null top-level arguments throw `ArgumentNullException`; malformed participating geometry throws `ArgumentException`. Undefined scopes throw before geometry reads. A Boolean result does not update after later edits.

`PlateExtensions.GetCornerLoop(plate)` captures A, B, C and D and skips missing slots anywhere in that order. A corner is omitted when an already retained representative has the same ID and Euclidean separation at most 0.001. The first representative survives; tolerance equality is not transitive, so omitted corners do not bridge a chain of later corners. Coincident corners with different IDs and matching IDs farther apart remain distinct. A triangle with D missing or equivalent to A normally returns three corners; a four-distinct-corner plate returns four. The first corner is not repeated at the end.

The corner list is immediately materialized in owned read-only storage. It retains original node references: changing plate slots later does not change the list, but edits to retained nodes remain visible without rerunning deduplication. All present corners must have finite coordinates, including apparently repeated corners. Null plates throw `ArgumentNullException`; nonfinite present coordinates throw `ArgumentException`. Sparse and collapsed inputs can return zero, one or two corners. This is extraction rather than validation of area, planarity, orientation, self-intersection, native membership or engineering suitability. Draft IDs are allowed. Inputs must use common model coordinates and length units.

`PlateExtensions.GetEdges(plate)` returns an immediately materialized read-only collection through `IEnumerable<PlateEdge>`. The named result replaces a public tuple and exposes immutable `Start`/`End` endpoint slots retaining original Node references. It uses the same captured first-representative corner loop as `GetCornerLoop`, then pairs each retained corner with its successor, closing last to first. Zero corners gives no edges, one gives a self-edge, two gives both directed edges, and ordinary triangles/quadrilaterals give three/four. Validation occurs at the call rather than deferred enumeration. Later plate edits do not change membership or endpoint slots; node edits remain visible without recomputation.

`PlateExtensions.IsConnectedToPlate(plate, targetPlate, scope)` retains `PlateConnectivityScope.SharedEdge` (0) as its default. A qualifying edge must be consecutive in both retained loops, matching either direction under the existing node ID-plus-Euclidean-distance rule (0.001 model length units). The closing last-to-first pair participates. Two shared corners that form a diagonal in either plate do not qualify. Both endpoints cannot qualify solely by matching the same target corner. Both loops need at least two retained corners; a one-corner self-edge does not turn corner-only contact into edge contact. Two-corner collapsed loops can qualify as extracted topology, without certifying a valid analytical plate.

`AnyCorner` (1) instead requires one equivalent retained corner. Empty loops return false under either scope. Same-object input follows the same rule: at least one retained corner for `AnyCorner`, at least two for `SharedEdge`. Plate IDs and cached adjacency are unused. Coordinate coincidence with different node IDs does not connect geometry. Draft IDs remain allowed. Undefined scopes throw `ArgumentOutOfRangeException` before geometry reads; null plate arguments throw `ArgumentNullException`.

Both supplied loops are fully captured and all present coordinates qualified before matching, even opposite an empty loop or after an apparent early match. Any nonfinite present corner throws `ArgumentException` naming its plate parameter. Missing corner slots are skipped using the same nontransitive first-retained-representative rule as `GetCornerLoop`; no mutable HashSet membership is used. No area, planarity, self-intersection, native model membership, solver connectivity or engineering suitability is inferred. Geometric segment crossings and interior points do not become corner/edge matches. Connectivity is an immediate Boolean observation and is not recomputed after edits.

`GetPlatesSharingEdgeWith(plate, candidatePlates)` enumerates candidates once and returns immediately materialized read-only membership retaining original Plate references, order and duplicates. Null candidates and candidates whose captured plate ID equals the reference ID are skipped, including draft IDs. Their geometry is unused. This explicitly preserves source ID-based self-exclusion despite differences in Plate equality across implementations. Every other candidate's corners are captured and qualified, even after prior matches or when the reference has no usable edge. Later validation/enumeration failure returns no partial result. Matching reuses actual consecutive-edge comparison on both retained loops, including closure/either direction and the existing 0.001 node-ID/distance rule. Later edits do not recompute membership.

`BeamGroupingService.GroupBeamsByPlanAngle(beams, origin, toleranceDegrees = 0.001)` groups beams with agreeing endpoint-to-origin bearings in the XZ plane. It deliberately retains the source convention `atan2(origin.Z - endpoint.Z, origin.X - endpoint.X)` in degrees normalized to [0,360). For example, endpoints on the +X ray have an inward key of 180 degrees. These are not outward origin-to-node bearings or beam-local-axis angles. Y coordinates are qualified as finite but ignored in bearing and midpoint-radius calculations. IDs are unused.

Each beam must have two finite endpoints. Beams with either endpoint coincident with the origin in XZ have undefined bearing and are skipped; a fabricated zero bearing is not assigned. Other beams qualify when the shortest circular endpoint-bearing separation is at most the finite nonnegative `toleranceDegrees`. Wraparound at zero is supported. Tolerance zero means exact computed equality; tolerances at least 180 accept every defined difference. Undefined/missing/nonfinite data throws. Even later invalid geometry on an otherwise ineligible beam is validated before grouping.

A qualifying beam joins the first created group whose fixed seed matches its captured start bearing. Otherwise its start bearing becomes a new seed. Seeds are not averaged/moved: this is input-order-dependent, nontransitive first-seed grouping, not connected clustering or physical-member identification. Groups are returned by ascending key. Original beam occurrences, including duplicates, are sorted within each group by captured XZ midpoint distance to origin, preserving input order for equal distances. Finite-extreme scaled distances/midpoints avoid overflow and squaring underflow with normal floating-point rounding. Worst-case grouping is quadratic.

Results are editable `List<BeamGroup<double>>` records with `Key` and independently owned editable `Beams` lists. Original beam references remain shared; later geometry edits are visible but do not recompute keys, membership or order. Manual setters store values/references without validation. Inputs are enumerated once, origin is captured once and endpoints once per occurrence. These observations do not certify 3D collinearity, native membership or engineering suitability. No grouping function mutates native or supplied geometry.

`GroupBeamsByRadius(beams, origin, tolerance = 0.001)` computes XZ endpoint distances from one captured origin. Y is finite-validated but ignored. A beam qualifies when its two computed radii agree within the inclusive absolute length tolerance. Its start radius is matched to the first created fixed seed within that tolerance, otherwise a new group is created. Chords, coincident endpoints and endpoints projected at origin may qualify; no circle fit or curved-member inference is made. Radii use scaled hypotenuse arithmetic to avoid squaring overflow/underflow. Every computed radius must fit a finite Double; unrepresentable endpoint radii throw `ArgumentException` rather than create Infinity/NaN keys, including otherwise ineligible beams. Normal floating-point rounding applies.

`GroupBeamsByElevation(beams, tolerance = 0.001)` uses signed endpoint Y values. A beam qualifies when their absolute difference is within tolerance, then joins the first fixed start-elevation seed within tolerance. X/Z are finite-validated but ignored. Negative elevations and coincident endpoints are valid. An opposite-sign Y difference beyond Double range cannot match a finite tolerance. This criterion alone is not a 3D horizontal-member or engineering certification.

Both functions preserve public names, parameter order and defaults. Tolerance is finite nonnegative, in supplied length units, and applied inclusively to both endpoint eligibility and group membership. Zero means exact computed equality; no relative/angular tolerance or unit conversion is introduced. First seeds are never averaged or moved: grouping is order-dependent and nontransitive. Reversing endpoints can change a key within tolerance. Groups sort by ascending seed, but their beam occurrences preserve original input order and duplicates, without the plan-angle method's midpoint-distance sorting.

Inputs are enumerated once, both finite initialized endpoints captured once per occurrence and every entry validated before grouping. IDs are unused; draft IDs are allowed. Missing/nonfinite geometry or enumeration errors propagate at invocation with no partial result. Origin is required and validated even for empty radius input. Result outer lists, `BeamGroup<double>` records and each beam list are independently owned and editable; original Beam references remain shared, with later edits visible but without regrouping. Empty/no eligible inputs return new empty lists. Worst-case grouping is quadratic; memory grows with input. Captures are not atomic, so callers coordinate writers and supply compatible coordinates/units.

## Node extraction by supplied ID

`GeometryDiagnosticsExtensions.GetNodesFromBeams(beams)` returns a `List<Node>` visiting beams in input order and each beam's StartNode then EndNode. Both endpoints must be initialized and every beam entry non-null; later missing entries fail even after their apparent IDs were seen earlier. The first visited node for each captured ID is retained.

`GeometryDiagnosticsExtensions.GetNodesFromPlates(plates)` visits each non-null plate's raw A-B-C-D slots in input order, skipping null corners anywhere. Sparse/collapsed/triangular and completely empty plates are allowed. It uses raw slots, not `GetCornerLoop`'s tolerance-based representative selection. A repeated closing corner or any later same-ID node keeps the first supplied node reference.

Both helpers use **node ID only**. Same-ID nodes with conflicting coordinates deliberately keep the first; different IDs at the same coordinates remain distinct. All Int32 IDs, including zero/negative draft IDs, are accepted. Node equality, mutable Node hashing, distance tolerances and coordinates are unused. Nonfinite coordinate values are retained as unvalidated metadata, so extraction does not imply valid geometry. Beam/plate IDs are unused, and no native incidence is queried.

Each input is enumerated once and the result fully materialized before return. Endpoint/corner references are captured once per entity occurrence and each visited ID once. An owned editable result list preserves first-encounter order without relying on dictionary enumeration order. Enumerator failures propagate unchanged, and null entities/missing beam endpoints produce explicit argument errors without a partial result.

Original Node objects remain shared. Later collection/slot edits do not change result membership, while later node/ID edits remain visible without rededuplication. Visible IDs may no longer be unique. If a shared node changes ID between occurrences, the same object can be retained under different captured keys. Captures are not atomic; callers coordinate writers. Expected time is linear in endpoint/corner occurrences and storage proportional to distinct captured IDs. No model access, hydration, merge, renumbering, conversion, selection, analysis or mutation occurs.

All these operations are detached and non-mutating. Captures are not atomic across simultaneous writers; coordinate/collection edits must be coordinated by the caller.


## Unwrapped node angles

`BeamGroupingService.GetUnwrappedAngles(nodes, origin)` returns a `List<UnwrappedNodeAngle>`. Each immutable named record contains the original `Node` reference and `AngleInRadians`; no tuple or positional Item fields are required. The outer list remains editable and owns its storage. Later node edits do not recompute captured angles.

The angle is **outward** from the origin toward the node in the **XZ plane**, using atan2(node.Z-origin.Z, node.X-origin.X) in **radians**. This differs from `GroupBeamsByPlanAngle`, which uses inward endpoint-to-origin bearings in degrees. Y must be finite but does not change the angle. IDs are unused; repeated nodes and equal bearings remain separate, with stable input ordering for ties.

Negative bearings first receive a full turn (2π), then the normalized values are sorted. Consecutive gaps strictly greater than π shift the entire remaining suffix by a full turn, following the source convention. There is no tolerance, closing last-to-first comparison, optimal-seam choice, original-path traversal or final re-sort.

For example, nodes initially at 10° and 350° are returned in that initial normalized sort order, with captured radians equivalent to +10° and -10°. The returned values need not be ascending and are not guaranteed to lie in [-π,π]. An exact π gap is left unchanged. Tiny negative angles may round to exactly 2π during normalization.

Coincident XZ points are retained with `Math.Atan2` zero/signed-zero behavior; this does not supply a physically meaningful direction. Origin coordinates are captured before enumeration. Every node must be non-null and finite on all axes, even unused Y. Empty input returns a fresh empty list. Input is enumerated once; malformed later nodes or a failing enumerator throw without a partial list.

When finite coordinate subtraction overflows, both X/Z differences are halved before atan2 to preserve direction. Ordinary floating-point angle/rounding limits remain. Data must share compatible coordinates/units and concurrent writers must be coordinated; captures are not atomic. No native call, unit conversion, selection, geometry hydration or mutation occurs.

## Detached ungrouped-beam filtering

`GeometryDiagnosticsExtensions.GetBeamsWithNoGroupAssignment(beamsCollection, groups)` checks only the supplied managed groups. It differs from the live Geometry adapter's same-named query: it does not retrieve all model groups or verify native assignments. Omitted group data can make a beam appear unassigned relative to the supplied collections.

Membership compares **captured analytical beam IDs only**, matching the main source helper's identity. A different beam object with the same ID counts as assigned even if endpoints differ or are absent. This deliberately differs from the package's geometry-sensitive `Beam.Equals`. No coordinates, group names or entity-family properties are read. Draft zero/negative IDs participate; supply groups and beams from a compatible model context to avoid cross-model ID collisions.

The non-null group sequence is enumerated once first. Null groups and null entity entries are skipped. Every non-null group must have an initialized `Entities` set; a null set throws even when the beam sequence is empty. Grouped IDs are snapshotted without relying on potentially stale hash buckets of mutable beam objects.

The non-null candidate beam sequence is then enumerated once. Null beams are skipped; unassigned input order, repetitions and original references are retained in owned **read-only, eagerly materialized** storage. Unlike a deferred filter, later collection or ID edits do not change result membership. Objects remain mutable, and concurrent edits must be coordinated. Enumerator/validation failures propagate without returning a partial result.

This is a detached collection filter, not validation of group completeness, model membership or engineering suitability. It performs no acquisition, native query, selection, conversion, analysis, save or group/entity edits.

```csharp
var nodes = new[]
{
    new StaadPro.Interop.Entities.Node(1, 0, 0, 0),
    new StaadPro.Interop.Entities.Node(2, 0.00001, 0, 0)
};
var groups = StaadPro.Interop.Extensions.GeometryDiagnosticsExtensions.GetDuplicateNodes(nodes);
System.Console.WriteLine(groups.Count());

var beams = new[] { new StaadPro.Interop.Entities.Beam(1, nodes[0], nodes[1]) };
var shortBeams = StaadPro.Interop.Extensions.GeometryDiagnosticsExtensions.GetZeroLengthBeams(beams);
System.Console.WriteLine(shortBeams.Count());

var target = nodes[0];
var closestNode = StaadPro.Interop.Extensions.GeometryDiagnosticsExtensions.GetClosestNode(nodes, target);
var closestBeam = StaadPro.Interop.Extensions.GeometryDiagnosticsExtensions.GetClosestBeam(beams, target);
System.Console.WriteLine(closestNode == null ? "No eligible node" : closestNode.Id.ToString());
System.Console.WriteLine(closestBeam == null ? "No beam" : closestBeam.Id.ToString());

var radial = StaadPro.Interop.Extensions.GeometryDiagnosticsExtensions.GetBeamsWithinRadius(beams, 0, 1);
var bounds = StaadPro.Interop.Extensions.GeometryDiagnosticsExtensions.GetBoundingBox(nodes);
bool connected = StaadPro.Interop.Extensions.BeamExtensions.IsConnectedToBeam(beams[0], beams[0]);
System.Console.WriteLine(radial.Count() + ": " + bounds.XMin + " to " + bounds.XMax + "; " + connected);

bool atNode = StaadPro.Interop.Extensions.BeamExtensions.IsConnectedToNode(beams[0], nodes[0]);
var firstMember = new StaadPro.Interop.Entities.Member(nodes[0], nodes[1]);
var secondMember = new StaadPro.Interop.Entities.Member(nodes[1], new StaadPro.Interop.Entities.Node(3, 1, 0, 0));
bool endpoints = StaadPro.Interop.Extensions.MemberExtensions.IsConnectedToMember(firstMember, secondMember);
bool suppliedNodes = StaadPro.Interop.Extensions.MemberExtensions.IsConnectedToMember(firstMember, secondMember,
    StaadPro.Interop.Enums.MemberConnectivityScope.AnyNode);
System.Console.WriteLine(atNode + ": " + endpoints + ": " + suppliedNodes);

bool memberAtNode = StaadPro.Interop.Extensions.MemberExtensions.IsConnectedToNode(firstMember, nodes[0]);
var plate = new StaadPro.Interop.Entities.Plate(nodes[0], nodes[1], new StaadPro.Interop.Entities.Node(4, 0, 1, 0), nodes[0]);
var corners = StaadPro.Interop.Extensions.PlateExtensions.GetCornerLoop(plate);
System.Console.WriteLine(memberAtNode + ": " + corners.Count);

foreach (var edge in StaadPro.Interop.Extensions.PlateExtensions.GetEdges(plate))
    System.Console.WriteLine(edge.Start.Id + " to " + edge.End.Id);
var neighbour = new StaadPro.Interop.Entities.Plate(plate.B, plate.A,
    new StaadPro.Interop.Entities.Node(5, 0, -1, 0));
bool plateConnected = StaadPro.Interop.Extensions.PlateExtensions.IsConnectedToPlate(plate, neighbour);
bool cornerContact = StaadPro.Interop.Extensions.PlateExtensions.IsConnectedToPlate(plate, neighbour,
    StaadPro.Interop.Enums.PlateConnectivityScope.AnyCorner);
System.Console.WriteLine(plateConnected + ": " + cornerContact);

plate.Id = 1; neighbour.Id = 2;
foreach (var match in StaadPro.Interop.Extensions.PlateExtensions.GetPlatesSharingEdgeWith(plate,
    new[] { plate, neighbour, neighbour })) System.Console.WriteLine(match.Id);
var radialBeam = new StaadPro.Interop.Entities.Beam(new StaadPro.Interop.Entities.Node(6, 1, 0, 0),
    new StaadPro.Interop.Entities.Node(7, 2, 0, 0));
var planGroups = StaadPro.Interop.Services.BeamGroupingService.GroupBeamsByPlanAngle(
    new[] { radialBeam }, new StaadPro.Interop.Entities.Node(0, 0, 0, 0));
System.Console.WriteLine(planGroups[0].Key); // 180: inward bearing
var chord = new StaadPro.Interop.Entities.Beam(new StaadPro.Interop.Entities.Node(8, 3, -2, 4),
    new StaadPro.Interop.Entities.Node(9, -3, -2, 4));
var radiusGroups = StaadPro.Interop.Services.BeamGroupingService.GroupBeamsByRadius(
    new[] { chord, chord }, new StaadPro.Interop.Entities.Node(0, 0, 0, 0));
System.Console.WriteLine(radiusGroups[0].Key); // 5; XZ only
var elevationGroups = StaadPro.Interop.Services.BeamGroupingService.GroupBeamsByElevation(new[] { chord });
System.Console.WriteLine(elevationGroups[0].Key); // -2

var outward = StaadPro.Interop.Services.BeamGroupingService.GetUnwrappedAngles(
    new[] { new StaadPro.Interop.Entities.Node(10, 1, 0, 1),
        new StaadPro.Interop.Entities.Node(11, 1, 0, -1) },
    new StaadPro.Interop.Entities.Node(0, 0, 0, 0));
foreach (var observation in outward)
    System.Console.WriteLine(observation.Node.Id + ": " + observation.AngleInRadians);

var assigned = new StaadPro.Interop.Entities.EntityGroup<StaadPro.Interop.Entities.Beam>();
assigned.Entities.Add(new StaadPro.Interop.Entities.Beam { Id = 101 });
var candidates = new[] { new StaadPro.Interop.Entities.Beam { Id = 101 },
    new StaadPro.Interop.Entities.Beam { Id = 102 } };
var notInSuppliedGroups = StaadPro.Interop.Extensions.GeometryDiagnosticsExtensions.GetBeamsWithNoGroupAssignment(
    candidates, new[] { assigned });
System.Console.WriteLine(notInSuppliedGroups.Count());

var extractedBeamNodes = StaadPro.Interop.Extensions.GeometryDiagnosticsExtensions.GetNodesFromBeams(beams);
var extractedPlateNodes = StaadPro.Interop.Extensions.GeometryDiagnosticsExtensions.GetNodesFromPlates(new[] { plate, neighbour });
System.Console.WriteLine(extractedBeamNodes.Count + ": " + extractedPlateNodes.Count);
```

Offline tests cover seed/nontransitive grouping, inclusive axis/distance boundaries, exact/duplicate IDs, nulls, invalid coordinates/tolerances, missing incidence, collection ownership, one-pass enumeration, source failures, huge finite values and subnormal deltas. The installed NuGet consumer checks expanded XML, exact signatures/defaults, executes all twenty-one XML examples and compiles this guide. Nearest-search tests also cover equality exclusion, endpoint-versus-segment semantics, stable ties, reference capture, full-sequence validation and randomized moderate geometry. No live STAAD operation is involved.
