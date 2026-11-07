# View zoom and member visibility

For current coverage and the subsequent `ShowMembers`, `HideMember` and `HideMembers` ports, see [member visibility contracts](View-Member-Visibility.md). The checkpoint below records the preceding batch.

**Implemented in source: 5 November 2026.** Ported five methods from the read-only main OSViewAdapter into IOSView and the existing, non-partial OSViewAdapter. The common internal service and ViewOperationResult are reused; no new public models, enums or tuples were introduced.

View now implements **18 of 70 source overloads** and **18 of 67 reviewed native names**. There are **52 View source overloads**, **49 native names**, and **145 active source overloads across the reviewed domains** still pending. The new 1.0.0-preview.20261105 prepared preview includes these additions; earlier archives retain their original scope.

## Choose the operation

| Managed method | Native operation | Purpose and distinction |
| --- | --- | --- |
| ZoomAll() | ZoomAll() | Fit the whole structure in the current view; does not invoke ShowAllMembers. |
| ZoomExtentsMainView() | ZoomExtentsMainView() | Invoke the dedicated main-view extents operation; does not substitute ZoomAll or explicitly create/activate a window. |
| ShowAllMembers() | ShowAllMembers() | Request display of all members; does not zoom or select members. |
| HideAllMembers() | HideAllMembers() | Request hiding all members; does not delete analytical members or their loads. |
| ShowMember(Beam beam) | ShowMember(beam.Id) | Request display of one analytical member; does not hide the others or verify model membership. |

Zoom and visibility remain separate native requests. Zooming does not implicitly unhide members. Showing one member does not isolate it. Hiding all members does not capture a previous visibility state for later restoration. Native graphical conventions and repaint behavior belong to STAAD.

## Public signatures

~~~csharp
ViewOperationResult ZoomAll();
ViewOperationResult ZoomExtentsMainView();
ViewOperationResult ShowAllMembers();
ViewOperationResult HideAllMembers();
ViewOperationResult ShowMember(Beam beam);
~~~

Every method returns the existing named receipt. NativeFunction identifies the exact call, Status is InvokedWithoutAcknowledgement, NativeReturnCode is null, and IsSuccessful is false because native void completion supplies no acknowledgement. That false is not a reported native failure. Normal return does not verify the visible result, graphical readiness, repaint completion or persistence.

The reviewed 2024, 2025 and 2026 native help contracts agree. Inspected 2025/2026 metadata also agrees: four parameterless void calls and ShowMember(Object) with an input member number. One common service handles these contracts; no speculative year-specific branch is added.

## ShowMember input

The existing Beam class is reused, and the source name/type/order Beam beam is preserved. A null Beam throws ArgumentNullException for beam; zero or negative Id throws ArgumentOutOfRangeException for beam. Both errors occur before native dispatch. Positive IDs, including Int32.MaxValue, are forwarded unchanged as a boxed Int32.

The method captures Id once. It does not read StartNode, EndNode, axes, offsets or other geometry, so a Beam containing only an existing analytical member number is sufficient. The managed object is not sent to COM or modified. The caller must ensure that this member belongs to the acquired model and coordinate concurrent changes to the Beam identity. A positive number alone cannot establish native model membership. There is no additional integer overload in this source-only increment.

## Example

Use an already connected wrapper on its established COM apartment. The guard prevents examples from accidentally changing a real graphical view. Run only the requests needed by your application; this sequence illustrates distinctions and does not restore an earlier view state.

~~~csharp
using StaadPro.Interop.Adapters.Interfaces;
using StaadPro.Interop.Entities;

IOSView view = wrapper.View;
if (view == null)
    throw new System.InvalidOperationException("The acquired instance has no View interface.");

bool changeView = false;
if (changeView)
{
    var fitCurrent = view.ZoomAll();
    var fitMain = view.ZoomExtentsMainView();
    var showAll = view.ShowAllMembers();
    var hideAll = view.HideAllMembers();

    // The ID must belong to an existing analytical member in this native model.
    var beam = new Beam { Id = 17 };
    var showOne = view.ShowMember(beam);
    System.Console.WriteLine(showOne.NativeFunction + ": " + showOne.Status);
}
~~~

## Lifetime, errors and side effects

All five functions share the View adapter gate with existing orientations and rotations. They invoke the captured native object exactly once on the calling thread. The gate serializes this adapter's requests; it does not marshal COM apartments, lock other wrappers/adapters, exclude raw callers or prevent external UI edits.

Binding identity is checked before and after dispatch. Reacquire wrapper.View after refresh. A stale, replaced or disposed binding throws InvalidOperationException; a post-call failure may follow an actual graphical change. Native COMException and missing-method RuntimeBinderException propagate without translation, retries or alternate-instance attachment.

These operations do not save, analyze, open a different model, change structural coordinates, change analysis results or show a hidden application. There is no automatic rollback or read-back. The methods preserve their individual native targets, including the distinction between current-view ZoomAll and the dedicated main-view extents route.

## Verification and evidence

All 256 View cases passed, including 55 additional cases for this increment. Coverage checks all five routes against 2024/2025/2026 fixture profiles, original native exceptions, missing methods, binding replacement/disposal before and during calls, caller-thread dispatch, matching compiled interface/adapter XML and existing receipt semantics. Focused cases cover boxed Int32 IDs, null/nonpositive IDs, missing Beam geometry, captured-ID changes and distinct visibility/zoom sequences. Managed concurrency fixtures check the common adapter gate; they do not authorize background-thread COM access.

The installed-package verifier selects all 36 current View XML examples and invokes each route against a managed native stand-in, including invalid-member checks. Real STAAD UI/COM, native effects and engineering qualification remain separate.

[Contract index and source fingerprints](contracts/ViewFoundationContractIndex.json) · [Native comparison](reports/OpenSTAAD-View-API-Coverage-and-Contract-Inventory.md) · [Source-status totals](reports/OpenSTAAD-Main-Source-Porting-Status.md) · [Orientations](View-Orientations.md) · [Rotations](View-Incremental-Rotations.md)
