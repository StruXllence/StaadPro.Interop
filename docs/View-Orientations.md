# View orientations and incremental rotation

The View adapter exposes seven standard orientation requests and one incremental rotation adapted from the read-only main source: ShowFront, ShowBack, ShowLeft, ShowRight, ShowPlan, ShowBottom, ShowIsometric and RotateUp. Each targets the captured native View interface of the acquired OpenStaadWrapper. The common contract is the same in the reviewed STAAD.Pro 2024, 2025 and 2026 manuals; passive 2025/2026 automation metadata also declares void methods, with a by-value Object input for RotateUp. No separate per-year service is needed for these matching contracts.

## Choose the graphical direction

| Managed call | Native call | Use |
| --- | --- | --- |
| ShowFront() | ShowFront() | Request the structure's front orientation. |
| ShowBack() | ShowBack() | Request the structure's back orientation, rather than its front. |
| ShowLeft() | ShowLeft() | Request the structure's left orientation, rather than its front or back. |
| ShowRight() | ShowRight() | Request the native right view, rather than the left view. |
| ShowPlan() | ShowPlan() | Request the top (plan) view, rather than the bottom view. |
| ShowBottom() | ShowBottom() | Request the bottom view, rather than the top (plan) view. |
| ShowIsometric() | ShowIsometric() | Request the native isometric view, rather than a plan view. |
| RotateUp(angleDegrees) | RotateUp(angleDegrees) | Incrementally rotate the current graphical view about global X. |

All eight return ViewOperationResult. STAAD defines these graphical directions; the wrapper does not assign a guessed global-axis sign. These functions do not rotate model coordinates, alter member or plate local axes, extract geometry, run analysis, save a model, open another model, select another window or show a hidden STAAD application. Use Root's application-window functions for the application window itself.

## Understand the completion receipt

| Property | Value and meaning |
| --- | --- |
| NativeFunction | The exact invoked native function, such as ShowPlan or RotateUp. |
| Status | InvokedWithoutAcknowledgement, after the native void call returned normally and the binding remained valid. |
| NativeReturnCode | null; the reviewed native methods do not supply an integer status. |
| IsSuccessful | false because there is no native success acknowledgement. This is not a failure flag. |

A normal receipt records dispatch completion. It does not verify the orientation on screen, native graphical readiness, repaint completion or persisted view settings. A native exception is propagated rather than converted into a successful result.

## Rotation input and behavior

~~~csharp
ViewOperationResult RotateUp(double angleDegrees);
~~~

The descriptive managed parameter replaces the source parameter named angle. It is a finite signed Double in degrees. The native metadata accepts an Object input, so the value is marshaled as the corresponding boxed Double. Zero, negative values and values beyond one revolution are passed unchanged; NaN and either infinity throw ArgumentOutOfRangeException for angleDegrees before dispatch. No undocumented angle limit, radians conversion or normalization is introduced. Very large finite values remain subject to native numerical behavior.

Choose a Show method for a standard direction. Choose RotateUp to incrementally rotate the current view about the documented global X-axis. STAAD controls the positive-angle screen convention; the managed API does not certify a clockwise direction or infer an absolute camera angle. Zero is still dispatched once. Repeated requests remain separate native calls and can accumulate, so an ambiguous completion must not be retried automatically.

## Example

This example requires an already connected wrapper, an available graphical view and the correct caller COM apartment. The explicit guard avoids an accidental UI change when adapting the example.

~~~csharp
using StaadPro.Interop.Adapters.Interfaces;
using StaadPro.Interop.Models.View;

IOSView view = wrapper.View;
if (view == null)
    throw new System.InvalidOperationException("The acquired STAAD instance has no View interface.");

bool changeView = false;
if (changeView)
{
    ViewOperationResult back = view.ShowBack();
    System.Console.WriteLine(back.NativeFunction + ": " + back.Status);

    ViewOperationResult plan = view.ShowPlan();
    System.Console.WriteLine(plan.NativeFunction + ": " + plan.Status);

    ViewOperationResult isometric = view.ShowIsometric();
    System.Console.WriteLine(isometric.NativeFunction + ": " + isometric.Status);

    ViewOperationResult rotation = view.RotateUp(15.0);
    System.Console.WriteLine(rotation.NativeFunction + ": " + rotation.Status);
}
~~~

## Binding, threading and errors

The single non-partial OSViewAdapter captures RawView at construction and delegates to the common internal ViewService. Invocation stays on the calling thread. A per-adapter gate serializes these requests and checks binding identity before and after each native call; it does not lock other adapters, raw automation clients or the STAAD UI. A lock does not select or marshal a COM apartment. Call from the application's established COM context.

Reacquire wrapper.View after wrapper refresh. Calls through a stale or disposed binding throw InvalidOperationException before dispatch. Replacement or disposal during dispatch also throws; the native UI may already have changed. Native COM and dynamic-binding errors remain visible to the caller. No alternate STAAD instance, automatic retry, rollback, background dispatch or implicit save is attempted.

## Coverage and verification

View provides **8 of 70 main-source overloads**, reaching **8 of 67 reviewed native names**. There are **62 main-source overloads** and **59 native names** still pending. Main-source ports precede native-only expansion. This increment does not authorize the parked Load or Property work.

Regression coverage checks exact native names and captured targets, 2024/2025/2026 fixture profiles, caller-thread identity, unacknowledged receipts, stale/disposed/replaced bindings, post-call lifetime failures, unchanged native exceptions, missing methods and absence of retries. Rotation cases additionally verify finite signed Double transport, zero/negative-zero preservation, nonfinite rejection and repeated-request behavior. Interface and adapter XML entries match and all sixteen View XML examples are checked through an isolated installed-package consumer. No real native view or model is changed by these managed tests.

The [source porting status](reports/OpenSTAAD-Main-Source-Porting-Status.md) records 160 active source overloads pending before this increment and 155 afterwards, across all reviewed domains. Native-only expansion and excluded obsolete signatures are counted separately.

Source fingerprints and exact declarations are retained in the [View contract index](contracts/ViewFoundationContractIndex.json). See [the View roadmap](View-Porting.md), [API reference](API-Reference.md), [cross-version plan](reports/OpenSTAAD-View-Multi-Version-Architecture-and-Plan.md), and [installation guide](Installation.md) for remaining scope and prerequisites. Live COM/UI validation remains separate from managed signature coverage.
