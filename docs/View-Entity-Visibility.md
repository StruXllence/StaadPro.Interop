# Hiding plates, solids and surfaces

Preview `1.0.0-preview.20261109` adds `HidePlate(Plate plate)`, `HideSolid(int solidId)` and `HideSurface(int surfaceId)` to `IOSView` and `OSViewAdapter`, with matching detailed XML IntelliSense. Each requests a graphical visibility change for one analytical entity and returns the existing `ViewOperationResult` receipt.

| Managed call | Native input | Display request |
| --- | --- | --- |
| `HidePlate(plate)` | One Int32 snapshot of `plate.Id` | Hide the identified plate |
| `HideSolid(solidId)` | The supplied Int32 `solidId` | Hide the identified solid |
| `HideSurface(surfaceId)` | The supplied Int32 `surfaceId` | Hide the identified surface |

## Entity identity and validation

`HidePlate` needs a non-null `Plate` with a positive analytical plate number. Only `Id` is read, once before dispatch. `A`, `B`, `C`, `D`, connectivity and other geometry fields are not required or queried. The managed object is not modified or sent to COM. A later change to the caller's object cannot change the captured ID, but the caller must coordinate concurrent edits while it is being captured.

`HideSolid` and `HideSurface` accept positive Int32 numbers belonging to the corresponding entity type in the acquired model. IDs are passed unchanged, including `Int32.MaxValue`. They are not collection positions or IDs from another model. No geometry lookup, existence check, type inference or cross-model mapping occurs. A positive number alone does not prove membership or native readiness.

| Invalid input | Exception | Parameter |
| --- | --- | --- |
| Null plate | `ArgumentNullException` | `plate` |
| Plate ID zero or negative | `ArgumentOutOfRangeException` | `plate` |
| Solid ID zero or negative | `ArgumentOutOfRangeException` | `solidId` |
| Surface ID zero or negative | `ArgumentOutOfRangeException` | `surfaceId` |

Validation finishes before native dispatch. An invalid request makes no native call. Each valid request invokes exactly its matching native function once. Repeated calls remain separate requests; no visibility cache or deduplication is added.

## Completion and effects

All three native functions return void. The receipt identifies `HidePlate`, `HideSolid` or `HideSurface`, has `Status = InvokedWithoutAcknowledgement`, `NativeReturnCode = null` and `IsSuccessful = false`. False means no success acknowledgement was provided; it is not a reported native failure. Normal return does not verify hidden state, repaint completion, visible effect, graphical readiness or persistence.

These methods request display changes. They do not delete or renumber entities, change geometry, connectivity, loads or results, select or isolate geometry, zoom, orient the camera, run analysis or save. Native STAAD defines display scope and interactions between graphical entity types. The library does not capture prior visibility or supply automatic restoration or undo.

## Example

With an acquired `wrapper` on its established COM apartment, supply IDs belonging to its current model:

```csharp
bool changeVisibility = false;
if (changeVisibility)
{
    var plate = new StaadPro.Interop.Entities.Plate { Id = 17 };
    int solidId = 41;
    int surfaceId = 63;
    var hiddenPlate = wrapper.View.HidePlate(plate);
    var hiddenSolid = wrapper.View.HideSolid(solidId);
    var hiddenSurface = wrapper.View.HideSurface(surfaceId);
    System.Console.WriteLine(hiddenSurface.NativeFunction + ": " + hiddenSurface.Status);
}
```

Choose IDs for entities that actually exist; the numbers above are illustrative. The three requests are independent and do not form a transaction. An error in a later call does not undo earlier graphical changes.

## Lifetime, apartment and errors

The shared View service invokes its captured `RawView` on the caller's thread under the existing per-adapter gate. It checks binding identity before and after dispatch. Reacquire `wrapper.View` after refresh. A disposed or replaced binding throws `InvalidOperationException`; a post-call check can throw after an actual UI effect. Native `COMException` and missing-member binding errors propagate unchanged, without retry, alternate-instance acquisition or rollback.

The gate serializes calls made through that adapter. It does not marshal the caller to a COM thread, enforce every apartment rule, or coordinate external UI, raw objects, other adapters or wrappers. Call from the established session apartment and coordinate shared native access. No background dispatch, implicit launch, save or effect read-back is supplied.

## Contract evidence and verification

The installed STAAD.Pro 2025 help documents void `HidePlate`, `HideSolid` and `HideSurface` functions, each with one input VARIANT containing its entity number. These agree with the previously recorded 2024/2025/2026 signatures. Native const VARIANT references describe automation inputs; the managed signatures expose no public reference outputs. The source function names, parameter names, types and order are preserved. Returning a named void-completion receipt follows the other View operations in this package.

Managed tests cover positive ID boundaries, nulls, nonpositive IDs, correct native routes and boxed Int32 inputs, plate identity snapshots, repeated calls, instance isolation, caller-thread dispatch, the shared gate, original errors and binding replacement/disposal before or during calls. Installed-package verification checks matching expanded XML on both public surfaces, compiles the guide and all 48 View XML examples, and exercises the new native signatures with a managed stand-in. No live graphical change was performed; COM marshaling, native licensing and UI effects across STAAD builds require separate qualification.
