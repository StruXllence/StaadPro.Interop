# Incremental View rotations and spins

Ported `RotateDown(double angle)`, `RotateLeft(double angle)`, `RotateRight(double angle)`, `SpinLeft(double angle)` and `SpinRight(double angle)` from the main source. Preserved the original `angle` parameter name, type and order on both interfaces and the concrete adapter. View now exposes **13 of 70 main-source overloads** and reaches **13 of 67 reviewed native names**; **57 source overloads** and **54 native names** remain pending.

All five accept finite signed degrees unchanged, including zero, negative zero and values beyond one revolution. Global X applies to RotateDown, global Y to RotateLeft/RotateRight and global Z to SpinLeft/SpinRight. STAAD defines the screen convention; no guessed clockwise sign or negative-angle equivalence is supplied. Every request uses the captured View receiver and existing adapter gate on the caller thread, preserves native exceptions and returns the established unacknowledged void-completion receipt.

Passed **7,137 offline tests**, including **201 View cases** and **116 additional cases** in this increment. Installed-package verification compiles and executes **1,306 selected XML examples**, including all **26 View interface/adapter examples**, through a fresh PackageReference consumer. Live native COM/UI behavior remains separate qualification work.

## Choose a request

| Method | Native axis | Source relationship |
| --- | --- | --- |
| `RotateDown(angle)` | Global X | Opposite-direction counterpart of `RotateUp`; retains its own native route. |
| `RotateLeft(angle)` | Global Y | Counterpart of `RotateRight`. |
| `RotateRight(angle)` | Global Y | Opposite-direction counterpart of `RotateLeft`. |
| `SpinLeft(angle)` | Global Z | Counterpart of `SpinRight`. |
| `SpinRight(angle)` | Global Z | Opposite-direction counterpart of `SpinLeft`. |

Use [fixed orientations](View-Orientations.md) for a standard direction. These five requests apply relative increments to the current graphical view. Calling an opposite route is not assumed equivalent to negating the other route's argument. A zero request is dispatched once; repeated calls are cumulative requests and are never deduplicated or retried automatically.

## Inputs, completion and lifetime

The source `double angle` parameter is preserved. Values are degrees, passed as the native VARIANT's boxed Double without normalization, rounding, clamping or radians conversion. NaN and positive/negative infinity throw ArgumentOutOfRangeException naming angle before any native access. Very large finite values are still subject to STAAD's numerical/UI behavior.

ViewOperationResult.NativeFunction records the exact invoked method. Normal void completion has Status InvokedWithoutAcknowledgement, NativeReturnCode null and IsSuccessful false. This false is an absence of success acknowledgement, not a reported native failure. A receipt does not verify camera state, repaint, visible effect or persistence.

All calls share the existing single non-partial OSViewAdapter and internal ViewService, with captured-binding checks before and after one dispatch. The per-adapter gate serializes its own requests; it does not marshal threads, lock external UI/raw COM callers or coordinate other adapters. Use the acquired session's established COM apartment. Disposal/replacement invalidates old adapters. Native COM/dynamic-binding errors propagate unchanged; UI changes may precede post-call errors. No model-coordinate transformation, member/plate-axis change, load activation, analysis, window selection, application launch, hidden save or rollback occurs.

The [contract index](contracts/ViewFoundationContractIndex.json) retains main-source mappings and three-year native declarations; the [native comparison](reports/data/OpenSTAAD-View-2024-2026-Comparison.json) retains each year's evidence independently. Reviewed 2024/2025/2026 signatures and inspected 2025/2026 metadata agree on void return and one by-value Object angle. Managed fixtures establish dispatch and validation, not live graphical readiness, licensing or native servicing-build compatibility.

## Examples and complete contracts

Examples assume IOSView view = wrapper.View for a caller-owned connected wrapper. They default to disabled because each operation changes the active graphical view.

### RotateDown

Requests an incremental graphical rotation about global X through the native RotateDown direction.

```csharp
ViewOperationResult RotateDown(double angle);
```

**angle:** The finite signed rotation angle in degrees, preserving the main-source parameter name. Zero, negative values, negative zero and values beyond one revolution are passed unchanged. No radians conversion, clamping, normalization or rounding is performed.

**Returns:** A ViewOperationResult identifying native RotateDown with InvokedWithoutAcknowledgement after normal native void completion and final binding checks. NativeReturnCode is null and IsSuccessful is false because the native method supplies no success flag; this does not report native failure.

The main-source description identifies this as the opposite-direction counterpart of RotateUp; the native help identifies global X. STAAD determines the screen convention and effect. No guessed clockwise/counterclockwise signs, camera vectors or equivalence to negating RotateUp are asserted. Unlike ShowFront/ShowPlan and other fixed orientations, this is relative to the current graphical view; successive calls accumulate and zero is still dispatched. This is a graphical request, not a transformation of analytical coordinates, structural local axes or analysis results. It does not select a different graphical window or show the application. Reuses the single View adapter and its common captured-binding service. Shared reviewed 2024/2025/2026 declarations and passive 2025/2026 automation metadata agree on void return and one VARIANT angle input. Invokes captured RawView exactly once on the caller's established COM apartment under the adapter's serialization gate, with disposal/replacement checks before and after dispatch. This gate does not coordinate other adapters, raw COM callers or external UI edits. Reacquire wrapper.View after refresh. Nonfinite inputs fail before any native call. Native exceptions propagate unchanged; a post-call exception may follow an actual graphical change. No automatic retry, fallback instance, rollback, background-thread dispatch, analysis, model launch or implicit save occurs. A receipt proves invocation completion only; repaint, visible effect, readiness, licensing, persistence and live native servicing-build compatibility require separate qualification.

**Exceptions:** ArgumentOutOfRangeException for nonfinite input; InvalidOperationException for disposed/replaced binding; original COMException or RuntimeBinderException for native/dynamic transport. No retry or fallback follows an error.

```csharp
bool changeView = false; // Enable only when an incremental graphical change is intended.
if (changeView)
{
    double angle = 15.0; // Degrees; negative values are passed unchanged.
    var result = view.RotateDown(angle);
    System.Console.WriteLine(result.NativeFunction + ": " + result.Status);
}
```

### RotateLeft

Requests an incremental graphical rotation about global Y through the native RotateLeft direction.

```csharp
ViewOperationResult RotateLeft(double angle);
```

**angle:** The finite signed rotation angle in degrees, preserving the main-source parameter name. Zero, negative values, negative zero and values beyond one revolution are passed unchanged. No radians conversion, clamping, normalization or rounding is performed.

**Returns:** A ViewOperationResult identifying native RotateLeft with InvokedWithoutAcknowledgement after normal native void completion and final binding checks. NativeReturnCode is null and IsSuccessful is false because the native method supplies no success flag; this does not report native failure.

The main-source and native help identify global Y; use RotateRight for its opposite-direction counterpart. STAAD determines the screen convention and effect. No guessed clockwise/counterclockwise signs, camera vectors or equivalence to negating RotateRight are asserted. Unlike ShowFront/ShowPlan and other fixed orientations, this is relative to the current graphical view; successive calls accumulate and zero is still dispatched. This is a graphical request, not a transformation of analytical coordinates, structural local axes or analysis results. It does not select a different graphical window or show the application. Reuses the single View adapter and its common captured-binding service. Shared reviewed 2024/2025/2026 declarations and passive 2025/2026 automation metadata agree on void return and one VARIANT angle input. Invokes captured RawView exactly once on the caller's established COM apartment under the adapter's serialization gate, with disposal/replacement checks before and after dispatch. This gate does not coordinate other adapters, raw COM callers or external UI edits. Reacquire wrapper.View after refresh. Nonfinite inputs fail before any native call. Native exceptions propagate unchanged; a post-call exception may follow an actual graphical change. No automatic retry, fallback instance, rollback, background-thread dispatch, analysis, model launch or implicit save occurs. A receipt proves invocation completion only; repaint, visible effect, readiness, licensing, persistence and live native servicing-build compatibility require separate qualification.

**Exceptions:** ArgumentOutOfRangeException for nonfinite input; InvalidOperationException for disposed/replaced binding; original COMException or RuntimeBinderException for native/dynamic transport. No retry or fallback follows an error.

```csharp
bool changeView = false; // Enable only when an incremental graphical change is intended.
if (changeView)
{
    double angle = 15.0; // Degrees; negative values are passed unchanged.
    var result = view.RotateLeft(angle);
    System.Console.WriteLine(result.NativeFunction + ": " + result.Status);
}
```

### RotateRight

Requests an incremental graphical rotation about global Y through the native RotateRight direction.

```csharp
ViewOperationResult RotateRight(double angle);
```

**angle:** The finite signed rotation angle in degrees, preserving the main-source parameter name. Zero, negative values, negative zero and values beyond one revolution are passed unchanged. No radians conversion, clamping, normalization or rounding is performed.

**Returns:** A ViewOperationResult identifying native RotateRight with InvokedWithoutAcknowledgement after normal native void completion and final binding checks. NativeReturnCode is null and IsSuccessful is false because the native method supplies no success flag; this does not report native failure.

The main-source description identifies this as the opposite-direction counterpart of RotateLeft; the native help identifies global Y. STAAD determines the screen convention and effect. No guessed clockwise/counterclockwise signs, camera vectors or equivalence to negating RotateLeft are asserted. Unlike ShowFront/ShowPlan and other fixed orientations, this is relative to the current graphical view; successive calls accumulate and zero is still dispatched. This is a graphical request, not a transformation of analytical coordinates, structural local axes or analysis results. It does not select a different graphical window or show the application. Reuses the single View adapter and its common captured-binding service. Shared reviewed 2024/2025/2026 declarations and passive 2025/2026 automation metadata agree on void return and one VARIANT angle input. Invokes captured RawView exactly once on the caller's established COM apartment under the adapter's serialization gate, with disposal/replacement checks before and after dispatch. This gate does not coordinate other adapters, raw COM callers or external UI edits. Reacquire wrapper.View after refresh. Nonfinite inputs fail before any native call. Native exceptions propagate unchanged; a post-call exception may follow an actual graphical change. No automatic retry, fallback instance, rollback, background-thread dispatch, analysis, model launch or implicit save occurs. A receipt proves invocation completion only; repaint, visible effect, readiness, licensing, persistence and live native servicing-build compatibility require separate qualification.

**Exceptions:** ArgumentOutOfRangeException for nonfinite input; InvalidOperationException for disposed/replaced binding; original COMException or RuntimeBinderException for native/dynamic transport. No retry or fallback follows an error.

```csharp
bool changeView = false; // Enable only when an incremental graphical change is intended.
if (changeView)
{
    double angle = 15.0; // Degrees; negative values are passed unchanged.
    var result = view.RotateRight(angle);
    System.Console.WriteLine(result.NativeFunction + ": " + result.Status);
}
```

### SpinLeft

Requests an incremental graphical rotation about global Z through the native SpinLeft direction.

```csharp
ViewOperationResult SpinLeft(double angle);
```

**angle:** The finite signed rotation angle in degrees, preserving the main-source parameter name. Zero, negative values, negative zero and values beyond one revolution are passed unchanged. No radians conversion, clamping, normalization or rounding is performed.

**Returns:** A ViewOperationResult identifying native SpinLeft with InvokedWithoutAcknowledgement after normal native void completion and final binding checks. NativeReturnCode is null and IsSuccessful is false because the native method supplies no success flag; this does not report native failure.

The main-source and native help identify global Z; use SpinRight for its opposite-direction counterpart. STAAD determines the screen convention and effect. No guessed clockwise/counterclockwise signs, camera vectors or equivalence to negating SpinRight are asserted. Unlike ShowFront/ShowPlan and other fixed orientations, this is relative to the current graphical view; successive calls accumulate and zero is still dispatched. This is a graphical request, not a transformation of analytical coordinates, structural local axes or analysis results. It does not select a different graphical window or show the application. Reuses the single View adapter and its common captured-binding service. Shared reviewed 2024/2025/2026 declarations and passive 2025/2026 automation metadata agree on void return and one VARIANT angle input. Invokes captured RawView exactly once on the caller's established COM apartment under the adapter's serialization gate, with disposal/replacement checks before and after dispatch. This gate does not coordinate other adapters, raw COM callers or external UI edits. Reacquire wrapper.View after refresh. Nonfinite inputs fail before any native call. Native exceptions propagate unchanged; a post-call exception may follow an actual graphical change. No automatic retry, fallback instance, rollback, background-thread dispatch, analysis, model launch or implicit save occurs. A receipt proves invocation completion only; repaint, visible effect, readiness, licensing, persistence and live native servicing-build compatibility require separate qualification.

**Exceptions:** ArgumentOutOfRangeException for nonfinite input; InvalidOperationException for disposed/replaced binding; original COMException or RuntimeBinderException for native/dynamic transport. No retry or fallback follows an error.

```csharp
bool changeView = false; // Enable only when an incremental graphical change is intended.
if (changeView)
{
    double angle = 15.0; // Degrees; negative values are passed unchanged.
    var result = view.SpinLeft(angle);
    System.Console.WriteLine(result.NativeFunction + ": " + result.Status);
}
```

### SpinRight

Requests an incremental graphical rotation about global Z through the native SpinRight direction.

```csharp
ViewOperationResult SpinRight(double angle);
```

**angle:** The finite signed rotation angle in degrees, preserving the main-source parameter name. Zero, negative values, negative zero and values beyond one revolution are passed unchanged. No radians conversion, clamping, normalization or rounding is performed.

**Returns:** A ViewOperationResult identifying native SpinRight with InvokedWithoutAcknowledgement after normal native void completion and final binding checks. NativeReturnCode is null and IsSuccessful is false because the native method supplies no success flag; this does not report native failure.

The main-source description identifies this as the opposite-direction counterpart of SpinLeft; the native help identifies global Z. STAAD determines the screen convention and effect. No guessed clockwise/counterclockwise signs, camera vectors or equivalence to negating SpinLeft are asserted. Unlike ShowFront/ShowPlan and other fixed orientations, this is relative to the current graphical view; successive calls accumulate and zero is still dispatched. This is a graphical request, not a transformation of analytical coordinates, structural local axes or analysis results. It does not select a different graphical window or show the application. Reuses the single View adapter and its common captured-binding service. Shared reviewed 2024/2025/2026 declarations and passive 2025/2026 automation metadata agree on void return and one VARIANT angle input. Invokes captured RawView exactly once on the caller's established COM apartment under the adapter's serialization gate, with disposal/replacement checks before and after dispatch. This gate does not coordinate other adapters, raw COM callers or external UI edits. Reacquire wrapper.View after refresh. Nonfinite inputs fail before any native call. Native exceptions propagate unchanged; a post-call exception may follow an actual graphical change. No automatic retry, fallback instance, rollback, background-thread dispatch, analysis, model launch or implicit save occurs. A receipt proves invocation completion only; repaint, visible effect, readiness, licensing, persistence and live native servicing-build compatibility require separate qualification.

**Exceptions:** ArgumentOutOfRangeException for nonfinite input; InvalidOperationException for disposed/replaced binding; original COMException or RuntimeBinderException for native/dynamic transport. No retry or fallback follows an error.

```csharp
bool changeView = false; // Enable only when an incremental graphical change is intended.
if (changeView)
{
    double angle = 15.0; // Degrees; negative values are passed unchanged.
    var result = view.SpinRight(angle);
    System.Console.WriteLine(result.NativeFunction + ": " + result.Status);
}
```
