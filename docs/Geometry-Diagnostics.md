# Detached geometry diagnostics

`GeometryDiagnosticsExtensions` provides two collection extensions in `StaadPro.Interop.Extensions`. They inspect supplied entities without acquiring STAAD, hydrating geometry, changing selections or repairing the model. Coordinate values and tolerance must use the same model length units.

| Method | Meaning |
| --- | --- |
| `GetDuplicateNodes(nodesCollection, tolerance = 0.0001)` | Seed-based groups of distinctly numbered nodes within the inclusive tolerance on each axis. |
| `GetZeroLengthBeams(beamsCollection, tolerance = 0.0001)` | Beams whose endpoint Euclidean distance is at most the inclusive tolerance. |

Both methods validate non-null collections and finite nonnegative tolerances before enumeration. Null entries are ignored. Nonfinite coordinates throw `ArgumentException`; missing endpoints on a non-null beam also throw. Inputs are enumerated once and results are fully materialized before return. Enumerator exceptions propagate without returning partial results.

Duplicate-node detection captures each node's ID and coordinates. Repeated IDs with exactly equal coordinates keep the first instance. Conflicting coordinates for one ID throw rather than silently choosing a representation. This makes node-number identity explicit without depending on the entity equality tolerance. Callers supplying unnumbered point clouds should assign distinct IDs first. With zero tolerance, coordinate equality is exact.

Groups follow first-seen seed order. Each seed collects all still-unassigned nodes within tolerance of it on X, Y and Z; groups of one are omitted. This is a per-axis box and is not transitive clustering or a spherical radius. If A is near B and B near C, C joins A only if C is also near A. Worst-case grouping is quadratic in distinct IDs; choose appropriately sized detached batches.

Zero-length checks use Euclidean endpoint distance, preserve input order and repeated beams, and use a scaled comparison to avoid squared-distance overflow or underflow. Coincident endpoints match zero tolerance. This diagnostic reports geometry only; it does not determine solver acceptance or validate member properties, releases or units.

Returned collections own read-only storage, while their nodes and beams are the original mutable references. Editing those objects later does not recalculate membership. Coordinate reads are not atomic across concurrent writers; coordinate edits should be serialized with the diagnostic. No unit conversion, merge, deletion, renumbering or native action is performed.

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
```

Offline tests cover seed/nontransitive grouping, inclusive axis/distance boundaries, exact/duplicate IDs, nulls, invalid coordinates/tolerances, missing incidence, collection ownership, one-pass enumeration, source failures, huge finite values and subnormal deltas. The installed NuGet consumer checks expanded XML, exact signatures/defaults, executes both XML examples and compiles this guide. No live STAAD operation is involved.
