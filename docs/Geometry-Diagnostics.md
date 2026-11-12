# Detached geometry diagnostics

`GeometryDiagnosticsExtensions` provides four collection extensions in `StaadPro.Interop.Extensions`. They inspect supplied entities without acquiring STAAD, hydrating geometry, changing selections or repairing the model. Coordinate values and tolerance must use the same model length units.

| Method | Meaning |
| --- | --- |
| `GetDuplicateNodes(nodesCollection, tolerance = 0.0001)` | Seed-based groups of distinctly numbered nodes within the inclusive tolerance on each axis. |
| `GetZeroLengthBeams(beamsCollection, tolerance = 0.0001)` | Beams whose endpoint Euclidean distance is at most the inclusive tolerance. |
| `GetClosestNode(nodesCollection, referenceNode)` | Nearest eligible node, excluding the reference by ID-plus-distance equality. |
| `GetClosestBeam(beamsCollection, referenceNode)` | Beam with the nearer endpoint closest to the reference. |

The duplicate/zero-length methods validate non-null collections and finite nonnegative tolerances before enumeration. Null entries are ignored. Nonfinite coordinates throw `ArgumentException`; missing endpoints on a non-null beam also throw. Inputs are enumerated once and results are fully materialized before return. Enumerator exceptions propagate without returning partial results.

Duplicate-node detection captures each node's ID and coordinates. Repeated IDs with exactly equal coordinates keep the first instance. Conflicting coordinates for one ID throw rather than silently choosing a representation. This makes node-number identity explicit without depending on the entity equality tolerance. Callers supplying unnumbered point clouds should assign distinct IDs first. With zero tolerance, coordinate equality is exact.

Groups follow first-seen seed order. Each seed collects all still-unassigned nodes within tolerance of it on X, Y and Z; groups of one are omitted. This is a per-axis box and is not transitive clustering or a spherical radius. If A is near B and B near C, C joins A only if C is also near A. Worst-case grouping is quadratic in distinct IDs; choose appropriately sized detached batches.

Zero-length checks use Euclidean endpoint distance, preserve input order and repeated beams, and use a scaled comparison to avoid squared-distance overflow or underflow. Coincident endpoints match zero tolerance. This diagnostic reports geometry only; it does not determine solver acceptance or validate member properties, releases or units.

Returned collections own read-only storage, while their nodes and beams are the original mutable references. Editing those objects later does not recalculate membership. Coordinate reads are not atomic across concurrent writers; coordinate edits should be serialized with the diagnostic. No unit conversion, merge, deletion, renumbering or native action is performed.

`GetClosestNode` and `GetClosestBeam` require a non-null collection and reference node with finite coordinates. They enumerate once and ignore null entries. Every non-null candidate is validated, even after a zero-distance match; missing beam endpoints and nonfinite coordinates throw `ArgumentException`. Enumerator failures propagate without a partial choice. Empty/all-null inputs return null. A node input with no eligible candidate also returns null.

Nearest-node exclusion preserves the existing `Node` equality rule: the candidate has the reference ID **and** Euclidean distance at most `Constants.Tolerance` (0.001 model length units). A differently numbered coincident node is eligible. A same-ID node outside that radius is also eligible; duplicate IDs are not silently deduplicated. The reference ID/coordinates are captured before enumeration and candidate values once per occurrence. This snapshots equality inputs instead of repeatedly reading mutable entities.

Nearest-beam distance is the minimum of its two endpoint distances. It is **not** a projection onto the segment, infinite line or member surface. A long beam passing through the reference may therefore lose to a short beam with a nearer endpoint. IDs do not exclude beams, and coincident endpoints are allowed. Each beam's endpoint references and coordinates are read before comparison.

Nearest searches retain the first candidate with an equal computed distance and return the original mutable object. They perform an O(n) scan using constant auxiliary storage rather than sorting. Scaled Euclidean comparisons avoid squared-distance overflow/underflow; halved coordinate differences retain ordering even when subtraction would exceed `Double.MaxValue`. Distance ordering still has normal floating-point precision limits. No length-unit conversion or maximum-distance cutoff is applied. Later edits do not recompute the result; concurrent writers must be coordinated by the caller.

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
```

Offline tests cover seed/nontransitive grouping, inclusive axis/distance boundaries, exact/duplicate IDs, nulls, invalid coordinates/tolerances, missing incidence, collection ownership, one-pass enumeration, source failures, huge finite values and subnormal deltas. The installed NuGet consumer checks expanded XML, exact signatures/defaults, executes all four XML examples and compiles this guide. Nearest-search tests also cover equality exclusion, endpoint-versus-segment semantics, stable ties, reference capture, full-sequence validation and randomized moderate geometry. No live STAAD operation is involved.
