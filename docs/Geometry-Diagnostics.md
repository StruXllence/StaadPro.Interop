# Detached geometry diagnostics

`GeometryDiagnosticsExtensions` provides six collection extensions in `StaadPro.Interop.Extensions`. They inspect supplied entities without acquiring STAAD, hydrating geometry, changing selections or repairing the model. Coordinate values and tolerance must use the same model length units.

The related `BeamExtensions.IsConnectedToBeam` helper checks two supplied analytical beams using endpoint identity and coordinates.

| Method | Meaning |
| --- | --- |
| `GetDuplicateNodes(nodesCollection, tolerance = 0.0001)` | Seed-based groups of distinctly numbered nodes within the inclusive tolerance on each axis. |
| `GetZeroLengthBeams(beamsCollection, tolerance = 0.0001)` | Beams whose endpoint Euclidean distance is at most the inclusive tolerance. |
| `GetClosestNode(nodesCollection, referenceNode)` | Nearest eligible node, excluding the reference by ID-plus-distance equality. |
| `GetClosestBeam(beamsCollection, referenceNode)` | Beam with the nearer endpoint closest to the reference. |
| `GetBeamsWithinRadius(beamsCollection, minRadius, maxRadius, centre = null)` | Materialized radial band filter over beam midpoints in the XZ plane. |
| `GetBoundingBox<T>(source)` | Immutable named axis-aligned bounds of Node, Beam or Plate collections. |
| `beam.IsConnectedToBeam(targetBeam)` | Endpoint pair with the same node ID and Euclidean separation at most 0.001. |

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

All these operations are detached and non-mutating. Captures are not atomic across simultaneous writers; coordinate/collection edits must be coordinated by the caller.

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
```

Offline tests cover seed/nontransitive grouping, inclusive axis/distance boundaries, exact/duplicate IDs, nulls, invalid coordinates/tolerances, missing incidence, collection ownership, one-pass enumeration, source failures, huge finite values and subnormal deltas. The installed NuGet consumer checks expanded XML, exact signatures/defaults, executes all seven XML examples and compiles this guide. Nearest-search tests also cover equality exclusion, endpoint-versus-segment semantics, stable ties, reference capture, full-sequence validation and randomized moderate geometry. No live STAAD operation is involved.
