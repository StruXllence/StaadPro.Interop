# View member visibility

`ShowMembers(IEnumerable<Beam> beams)`, `HideMember(Beam beam)` and `HideMembers(IEnumerable<Beam> beams)` are included in `1.0.0-preview.20261107`. Both `IOSView` and `OSViewAdapter` expose matching detailed XML IntelliSense. These ports preserve the source names and parameter names, types and order, while returning the established `ViewOperationResult` used by the other View operations.

The current preview exposes 24 typed View operations. For plate, solid and surface display requests, see [entity visibility contracts](View-Entity-Visibility.md).

## Choose a request

| Managed operation | Native call | Effect requested |
| --- | --- | --- |
| `ShowMembers(beams)` | `ShowMembers(ids.Length, ids)` | Display the supplied analytical members. |
| `HideMember(beam)` | `HideMember(beam.Id)` | Hide one analytical member. |
| `HideMembers(beams)` | `HideMembers(ids.Length, ids)` | Hide the supplied analytical members. |

These operations change display. They do not isolate or select members, delete geometry or loads, zoom, analyze or save. Showing some members does not hide others. No prior visibility state is captured for restoration. STAAD defines graphical conventions and repaint behavior.

## Inputs and snapshots

Each beam needs only a positive `Id` identifying an existing analytical member in the acquired model. `StartNode`, `EndNode` and other geometry are not read. Membership is not queried; the caller must supply IDs belonging to that model. Positive Int32 IDs, including `Int32.MaxValue`, are forwarded unchanged. The managed `Beam` objects are not passed to COM or modified.

Collection methods enumerate the sequence once and capture each ID once into an owned, one-dimensional, zero-based `int[]`. They preserve order and duplicates. The native count equals that same array's length. Empty sequences forward zero and an empty array, following the source contract; their graphical effect is not certified. There is no managed empty-input success shortcut.

| Invalid input | Exception and parameter |
| --- | --- |
| Null single beam | `ArgumentNullException`, `beam` |
| Null collection | `ArgumentNullException`, `beams` |
| Null collection element | `ArgumentException`, `beams` |
| Zero or negative ID | `ArgumentOutOfRangeException`, `beam` or `beams` |

Validation and full enumeration finish before native dispatch. Enumeration exceptions propagate unchanged without a partial native request. Coordinate concurrent edits to the source collection and beams while enumeration is in progress. Changes after capture do not change the dispatched IDs.

## Example

With an acquired `wrapper` on the caller's established COM apartment, use IDs belonging to its model:

```csharp
var beams = new[] { new Beam { Id = 17 }, new Beam { Id = 41 } };
var displayed = wrapper.View.ShowMembers(beams);
var hidden = wrapper.View.HideMember(beams[0]);
var batchHidden = wrapper.View.HideMembers(beams);
Console.WriteLine(batchHidden.NativeFunction + ": " + batchHidden.Status);
```
Use `StaadPro.Interop.Entities` for `Beam`. Every receipt names the exact native function, has `Status = InvokedWithoutAcknowledgement`, `NativeReturnCode = null` and `IsSuccessful = false`. That false reflects the native void contract, not a reported native failure. Normal return does not verify visible effects, repaint completion, graphical readiness or persistence.

## Lifetime, threading and errors

The existing service invokes the captured `RawView` once, on the caller's thread, under the per-adapter gate. It checks binding lifetime before and after dispatch. Reacquire `wrapper.View` after refresh. A replaced or disposed binding throws `InvalidOperationException`; a post-call error can follow an actual graphical change. Native `COMException` and missing-method binding errors propagate unchanged. No retry, rollback, alternate instance, background dispatch or implicit model launch occurs. The gate does not coordinate external UI changes, other adapters or raw automation calls.

## Evidence and qualification

The installed STAAD.Pro 2025 help documents one input VARIANT for `HideMember` and a count plus member-number array for the collection calls, all returning void. This agrees with the recorded 2024/2025/2026 contracts. The C++ const VARIANT references describe input automation parameters; the managed API has no public `ref` or `out` arguments. The source adapter materializes Int32 arrays; the port adds validation and explicit owned snapshots.

Managed regressions check single enumeration and disposal, ID boundaries, nulls, duplicates, order, empty arrays, enumeration errors, captured IDs, binding replacement during enumeration and dispatch, original native errors, caller-thread execution and the common gate. The installed-package verifier compiles all 48 View XML examples and exercises the new calls against a managed native-signature stand-in. These checks do not certify live SAFEARRAY marshaling, empty-input graphical effects or native compatibility across STAAD versions. Live COM/UI qualification remains separate.

Passed **7,240 offline tests**, including **299 View cases** and **48 additional regression cases** in this increment. A fresh installed NuGet consumer compiled and executed **1,322 XML examples**, including all **42 View examples**, and checked the new native-signature routes, input validation and owned snapshots.
