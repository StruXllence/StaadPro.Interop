using System.Collections.Generic;
using StaadPro.Interop.Entities;
using StaadPro.Interop.Models.View;
using StaadPro.Interop.Services.View;
using System;
using StaadPro.Interop.Adapters.Interfaces;
using StaadPro.Interop.Models;

namespace StaadPro.Interop.Adapters.Models
{
    /// <summary>Implements orientations, rotations, zoom and analytical-entity visibility on the captured native View object.</summary>
    /// <remarks>Orientation requests record unacknowledged native void completion. Remaining View functions are separate ports.</remarks>
    public sealed class OSViewAdapter : OSBaseAdapter, IOSView
    {
        private readonly ViewService service;
        /// <summary>Captures the supplied wrapper’s View interface without calling its native functions.</summary>
        /// <param name="wrapper">The connected wrapper whose RawView must be non-null.</param>
        /// <exception cref="ArgumentNullException">The wrapper or its RawView binding is null.</exception>
        public OSViewAdapter(OpenStaadWrapper wrapper) : base(wrapper)
        {
            ComObject = wrapper.RawView ?? throw new ArgumentNullException(nameof(wrapper.RawView));
            service = new ViewService(wrapper, ComObject);
        }

        /// <summary>Gets the captured native View automation object.</summary>
        /// <value>The same object that RawView exposed at construction; refresh the wrapper after reopening a model.</value>
        public dynamic ComObject { get; }

        /// <summary>Requests the front orientation in the acquired instance's active graphical view.</summary>
        /// <returns>A ViewOperationResult with NativeFunction ShowFront and InvokedWithoutAcknowledgement. NativeReturnCode is null and IsSuccessful is false because native ShowFront returns void.</returns>
        /// <remarks><para>Ported from the main OSViewAdapter. Unlike Root application-window controls, this requests a graphical view orientation; it does not show a hidden application, create a window, run analysis, extract geometry or save. The native front convention belongs to STAAD. No orientation read-back or effect certification is provided.</para><para>Shared reviewed 2024/2025/2026 contract, corroborated by passive 2025/2026 metadata. Uses captured RawView on the caller's established COM apartment with a per-adapter gate and binding checks before/after dispatch. Reacquire after wrapper refresh. External UI/raw-object edits are not locked out. No automatic retry, fallback instance, rollback, model launch or implicit save occurs. Native errors propagate unchanged; post-call errors may follow a UI change. Native readiness/licensing and live compatibility remain separate acceptance work.</para></remarks>
        /// <exception cref="InvalidOperationException">The acquired View binding was disposed or replaced.</exception>
        /// <exception cref="System.Runtime.InteropServices.COMException">The native UI request failed; effects may precede an error.</exception>
        /// <exception cref="Microsoft.CSharp.RuntimeBinder.RuntimeBinderException">The acquired object lacks ShowFront.</exception>
        /// <example><code>bool changeView = false;
        /// if (changeView)
        /// {
        ///     var result = view.ShowFront();
        ///     System.Console.WriteLine(result.Status);
        /// }</code></example>
        public ViewOperationResult ShowFront() => service.ShowFront();

        /// <summary>Requests the back orientation in the acquired instance's active graphical view.</summary>
        /// <returns>A <see cref="ViewOperationResult"/> identifying native ShowBack and <see cref="StaadPro.Interop.Enums.View.ViewOperationStatus.InvokedWithoutAcknowledgement"/>. <see cref="ViewOperationResult.NativeReturnCode"/> is null and <see cref="ViewOperationResult.IsSuccessful"/> is false because native completion supplies no success acknowledgement; false does not report native failure.</returns>
        /// <remarks>
        /// <para>Use ShowBack to view the structure from its native back direction. Unlike <see cref="StaadPro.Interop.Adapters.Interfaces.IOSView.ShowFront()"/>, it requests back orientation; unlike Root application-window controls, it changes graphical orientation within the acquired STAAD instance. It does not show a hidden application, select another window, change model coordinates, run analysis or save. STAAD defines the viewing convention; this API does not infer an axis sign, read back the orientation or certify a visible change.</para>
        /// <para>The read-only main adapter and reviewed 2024, 2025 and 2026 help use the same parameterless void contract, also corroborated by passive 2025/2026 automation metadata. A licensed, ready native graphical view and the caller's correct COM apartment are required. The shared internal service invokes exactly once on the captured RawView, on the caller's thread, with a per-adapter gate and binding checks before and after dispatch. The gate does not coordinate external UI changes, raw-object calls or other adapters.</para>
        /// <para>Reacquire wrapper.View after wrapper refresh. No thread-pool dispatch, alternate instance, retry, rollback, analysis or implicit save is performed. Native errors propagate unchanged. A post-call lifetime error may follow an actual UI change, so retrying is a caller decision after checking the current native state. Matching signatures and managed tests do not establish live compatibility for every STAAD build.</para>
        /// </remarks>
        /// <exception cref="InvalidOperationException">The wrapper is disposed or its captured View binding was replaced, including during dispatch.</exception>
        /// <exception cref="System.Runtime.InteropServices.COMException">The native UI request failed; effects may precede the exception.</exception>
        /// <exception cref="Microsoft.CSharp.RuntimeBinder.RuntimeBinderException">The acquired automation object does not expose ShowBack with the reviewed contract.</exception>
        /// <example><code>bool changeView = false; // Set true only when a graphical orientation change is intended.
        /// if (changeView)
        /// {
        ///     var result = view.ShowBack();
        ///     System.Console.WriteLine(result.NativeFunction + ": " + result.Status);
        /// }</code></example>
        public ViewOperationResult ShowBack() => service.ShowBack();

        /// <summary>Requests the left orientation in the acquired instance's active graphical view.</summary>
        /// <returns>A <see cref="ViewOperationResult"/> identifying native ShowLeft and <see cref="StaadPro.Interop.Enums.View.ViewOperationStatus.InvokedWithoutAcknowledgement"/>. <see cref="ViewOperationResult.NativeReturnCode"/> is null and <see cref="ViewOperationResult.IsSuccessful"/> is false because native completion supplies no success acknowledgement; false does not report native failure.</returns>
        /// <remarks>
        /// <para>Use ShowLeft to view the structure from its native left direction. Unlike <see cref="StaadPro.Interop.Adapters.Interfaces.IOSView.ShowBack()"/>, it requests left orientation; unlike Root application-window controls, it changes graphical orientation within the acquired STAAD instance. It does not show a hidden application, select another window, change model coordinates, run analysis or save. STAAD defines the viewing convention; this API does not infer an axis sign, read back the orientation or certify a visible change.</para>
        /// <para>The read-only main adapter and reviewed 2024, 2025 and 2026 help use the same parameterless void contract, also corroborated by passive 2025/2026 automation metadata. A licensed, ready native graphical view and the caller's correct COM apartment are required. The shared internal service invokes exactly once on the captured RawView, on the caller's thread, with a per-adapter gate and binding checks before and after dispatch. The gate does not coordinate external UI changes, raw-object calls or other adapters.</para>
        /// <para>Reacquire wrapper.View after wrapper refresh. No thread-pool dispatch, alternate instance, retry, rollback, analysis or implicit save is performed. Native errors propagate unchanged. A post-call lifetime error may follow an actual UI change, so retrying is a caller decision after checking the current native state. Matching signatures and managed tests do not establish live compatibility for every STAAD build.</para>
        /// </remarks>
        /// <exception cref="InvalidOperationException">The wrapper is disposed or its captured View binding was replaced, including during dispatch.</exception>
        /// <exception cref="System.Runtime.InteropServices.COMException">The native UI request failed; effects may precede the exception.</exception>
        /// <exception cref="Microsoft.CSharp.RuntimeBinder.RuntimeBinderException">The acquired automation object does not expose ShowLeft with the reviewed contract.</exception>
        /// <example><code>bool changeView = false; // Set true only when a graphical orientation change is intended.
        /// if (changeView)
        /// {
        ///     var result = view.ShowLeft();
        ///     System.Console.WriteLine(result.NativeFunction + ": " + result.Status);
        /// }</code></example>
        public ViewOperationResult ShowLeft() => service.ShowLeft();

        /// <summary>Requests the right orientation in the acquired instance's active graphical view.</summary>
        /// <returns>A <see cref="ViewOperationResult"/> identifying native ShowRight and <see cref="StaadPro.Interop.Enums.View.ViewOperationStatus.InvokedWithoutAcknowledgement"/> after normal native void completion and the final binding check. <see cref="ViewOperationResult.NativeReturnCode"/> is null and <see cref="ViewOperationResult.IsSuccessful"/> is false because no native success acknowledgement exists; false does not indicate native failure.</returns>
        /// <remarks>
        /// <para>Use this method when the intended graphical direction is right. Requests the native right view, opposite to the left view. Unlike <see cref="StaadPro.Interop.Adapters.Interfaces.IOSView.ShowLeft()"/>, it requests right orientation. STAAD defines the view convention; this API does not guess axis signs, viewing extents, orthographic projection settings or exact camera vectors. Unlike Root application-window controls, this operates within the acquired graphical view. It does not change structural coordinates, member or plate local axes, analysis results, the selected window or application visibility. A normal return does not certify the visible result, repaint completion or persistence.</para>
        /// <para>The main-source adapter and reviewed STAAD.Pro 2024, 2025 and 2026 help share this contract; inspected 2025/2026 automation metadata agrees. The common internal service invokes the captured RawView exactly once, on the caller's established COM apartment, under a per-adapter gate, with binding checks before and after dispatch. The gate does not coordinate other adapters, raw automation access or the STAAD UI. Reacquire wrapper.View after wrapper refresh. Native errors propagate unchanged; a post-call binding error can follow an actual graphical change. Coordinate model and UI access before calling. There is no automatic retry, rollback, thread-pool dispatch, alternate instance, model launch, analysis, implicit save or orientation read-back. Signature agreement and managed fixtures do not establish live native graphical readiness or certify every servicing build.</para>
        /// </remarks>
        /// <exception cref="InvalidOperationException">The wrapper is disposed or its captured View binding was replaced, including during native dispatch.</exception>
        /// <exception cref="System.Runtime.InteropServices.COMException">The native graphical request failed; effects may precede the exception.</exception>
        /// <exception cref="Microsoft.CSharp.RuntimeBinder.RuntimeBinderException">The captured automation object lacks ShowRight with the reviewed contract.</exception>
        /// <example><code>bool changeView = false; // Set true only when a graphical change is intended.
        /// if (changeView)
        /// {
        ///     var result = view.ShowRight();
        ///     System.Console.WriteLine(result.NativeFunction + ": " + result.Status);
        /// }</code></example>
        public ViewOperationResult ShowRight() => service.ShowRight();

        /// <summary>Requests the top (plan) orientation in the acquired instance's active graphical view.</summary>
        /// <returns>A <see cref="ViewOperationResult"/> identifying native ShowPlan and <see cref="StaadPro.Interop.Enums.View.ViewOperationStatus.InvokedWithoutAcknowledgement"/> after normal native void completion and the final binding check. <see cref="ViewOperationResult.NativeReturnCode"/> is null and <see cref="ViewOperationResult.IsSuccessful"/> is false because no native success acknowledgement exists; false does not indicate native failure.</returns>
        /// <remarks>
        /// <para>Use this method when the intended graphical direction is top (plan). Requests the native top (plan) view rather than the bottom view. Unlike <see cref="StaadPro.Interop.Adapters.Interfaces.IOSView.ShowBottom()"/>, it requests top (plan) orientation. STAAD defines the view convention; this API does not guess axis signs, viewing extents, orthographic projection settings or exact camera vectors. Unlike Root application-window controls, this operates within the acquired graphical view. It does not change structural coordinates, member or plate local axes, analysis results, the selected window or application visibility. A normal return does not certify the visible result, repaint completion or persistence.</para>
        /// <para>The main-source adapter and reviewed STAAD.Pro 2024, 2025 and 2026 help share this contract; inspected 2025/2026 automation metadata agrees. The common internal service invokes the captured RawView exactly once, on the caller's established COM apartment, under a per-adapter gate, with binding checks before and after dispatch. The gate does not coordinate other adapters, raw automation access or the STAAD UI. Reacquire wrapper.View after wrapper refresh. Native errors propagate unchanged; a post-call binding error can follow an actual graphical change. Coordinate model and UI access before calling. There is no automatic retry, rollback, thread-pool dispatch, alternate instance, model launch, analysis, implicit save or orientation read-back. Signature agreement and managed fixtures do not establish live native graphical readiness or certify every servicing build.</para>
        /// </remarks>
        /// <exception cref="InvalidOperationException">The wrapper is disposed or its captured View binding was replaced, including during native dispatch.</exception>
        /// <exception cref="System.Runtime.InteropServices.COMException">The native graphical request failed; effects may precede the exception.</exception>
        /// <exception cref="Microsoft.CSharp.RuntimeBinder.RuntimeBinderException">The captured automation object lacks ShowPlan with the reviewed contract.</exception>
        /// <example><code>bool changeView = false; // Set true only when a graphical change is intended.
        /// if (changeView)
        /// {
        ///     var result = view.ShowPlan();
        ///     System.Console.WriteLine(result.NativeFunction + ": " + result.Status);
        /// }</code></example>
        public ViewOperationResult ShowPlan() => service.ShowPlan();

        /// <summary>Requests the bottom orientation in the acquired instance's active graphical view.</summary>
        /// <returns>A <see cref="ViewOperationResult"/> identifying native ShowBottom and <see cref="StaadPro.Interop.Enums.View.ViewOperationStatus.InvokedWithoutAcknowledgement"/> after normal native void completion and the final binding check. <see cref="ViewOperationResult.NativeReturnCode"/> is null and <see cref="ViewOperationResult.IsSuccessful"/> is false because no native success acknowledgement exists; false does not indicate native failure.</returns>
        /// <remarks>
        /// <para>Use this method when the intended graphical direction is bottom. Requests the native bottom view rather than the top (plan) view. Unlike <see cref="StaadPro.Interop.Adapters.Interfaces.IOSView.ShowPlan()"/>, it requests bottom orientation. STAAD defines the view convention; this API does not guess axis signs, viewing extents, orthographic projection settings or exact camera vectors. Unlike Root application-window controls, this operates within the acquired graphical view. It does not change structural coordinates, member or plate local axes, analysis results, the selected window or application visibility. A normal return does not certify the visible result, repaint completion or persistence.</para>
        /// <para>The main-source adapter and reviewed STAAD.Pro 2024, 2025 and 2026 help share this contract; inspected 2025/2026 automation metadata agrees. The common internal service invokes the captured RawView exactly once, on the caller's established COM apartment, under a per-adapter gate, with binding checks before and after dispatch. The gate does not coordinate other adapters, raw automation access or the STAAD UI. Reacquire wrapper.View after wrapper refresh. Native errors propagate unchanged; a post-call binding error can follow an actual graphical change. Coordinate model and UI access before calling. There is no automatic retry, rollback, thread-pool dispatch, alternate instance, model launch, analysis, implicit save or orientation read-back. Signature agreement and managed fixtures do not establish live native graphical readiness or certify every servicing build.</para>
        /// </remarks>
        /// <exception cref="InvalidOperationException">The wrapper is disposed or its captured View binding was replaced, including during native dispatch.</exception>
        /// <exception cref="System.Runtime.InteropServices.COMException">The native graphical request failed; effects may precede the exception.</exception>
        /// <exception cref="Microsoft.CSharp.RuntimeBinder.RuntimeBinderException">The captured automation object lacks ShowBottom with the reviewed contract.</exception>
        /// <example><code>bool changeView = false; // Set true only when a graphical change is intended.
        /// if (changeView)
        /// {
        ///     var result = view.ShowBottom();
        ///     System.Console.WriteLine(result.NativeFunction + ": " + result.Status);
        /// }</code></example>
        public ViewOperationResult ShowBottom() => service.ShowBottom();

        /// <summary>Requests the isometric orientation in the acquired instance's active graphical view.</summary>
        /// <returns>A <see cref="ViewOperationResult"/> identifying native ShowIsometric and <see cref="StaadPro.Interop.Enums.View.ViewOperationStatus.InvokedWithoutAcknowledgement"/> after normal native void completion and the final binding check. <see cref="ViewOperationResult.NativeReturnCode"/> is null and <see cref="ViewOperationResult.IsSuccessful"/> is false because no native success acknowledgement exists; false does not indicate native failure.</returns>
        /// <remarks>
        /// <para>Use this method when the intended graphical direction is isometric. Requests the native isometric view rather than an orthographic plan view. Unlike <see cref="StaadPro.Interop.Adapters.Interfaces.IOSView.ShowPlan()"/>, it requests isometric orientation. STAAD defines the view convention; this API does not guess axis signs, viewing extents, orthographic projection settings or exact camera vectors. Unlike Root application-window controls, this operates within the acquired graphical view. It does not change structural coordinates, member or plate local axes, analysis results, the selected window or application visibility. A normal return does not certify the visible result, repaint completion or persistence.</para>
        /// <para>The main-source adapter and reviewed STAAD.Pro 2024, 2025 and 2026 help share this contract; inspected 2025/2026 automation metadata agrees. The common internal service invokes the captured RawView exactly once, on the caller's established COM apartment, under a per-adapter gate, with binding checks before and after dispatch. The gate does not coordinate other adapters, raw automation access or the STAAD UI. Reacquire wrapper.View after wrapper refresh. Native errors propagate unchanged; a post-call binding error can follow an actual graphical change. Coordinate model and UI access before calling. There is no automatic retry, rollback, thread-pool dispatch, alternate instance, model launch, analysis, implicit save or orientation read-back. Signature agreement and managed fixtures do not establish live native graphical readiness or certify every servicing build.</para>
        /// </remarks>
        /// <exception cref="InvalidOperationException">The wrapper is disposed or its captured View binding was replaced, including during native dispatch.</exception>
        /// <exception cref="System.Runtime.InteropServices.COMException">The native graphical request failed; effects may precede the exception.</exception>
        /// <exception cref="Microsoft.CSharp.RuntimeBinder.RuntimeBinderException">The captured automation object lacks ShowIsometric with the reviewed contract.</exception>
        /// <example><code>bool changeView = false; // Set true only when a graphical change is intended.
        /// if (changeView)
        /// {
        ///     var result = view.ShowIsometric();
        ///     System.Console.WriteLine(result.NativeFunction + ": " + result.Status);
        /// }</code></example>
        public ViewOperationResult ShowIsometric() => service.ShowIsometric();

        /// <summary>Rotates the acquired instance's active graphical view about the global X-axis by the specified angle in degrees.</summary>
        /// <param name="angleDegrees">The finite signed rotation angle, in degrees. Zero and negative angles are accepted and passed unchanged. No normalization, rounding, radians conversion or undocumented angular range is imposed.</param>
        /// <returns>A <see cref="ViewOperationResult"/> identifying native RotateUp and <see cref="StaadPro.Interop.Enums.View.ViewOperationStatus.InvokedWithoutAcknowledgement"/> after normal native void completion and the final binding check. <see cref="ViewOperationResult.NativeReturnCode"/> is null and <see cref="ViewOperationResult.IsSuccessful"/> is false because no native success acknowledgement exists; false does not indicate native failure.</returns>
        /// <remarks>
        /// <para>Unlike the fixed-direction <see cref="StaadPro.Interop.Adapters.Interfaces.IOSView.ShowIsometric()"/> request, this applies an incremental graphical rotation to the current view. The reviewed native contract describes rotation about global X; STAAD controls the positive-angle screen convention. The requested signed Double is passed to the native VARIANT input without interpreting an absolute orientation. A zero angle is still dispatched once; repeated calls are cumulative requests, so callers should not retry an ambiguous completion without inspecting the current view. Unlike Root application-window controls, this operates within the acquired graphical view. It does not change structural coordinates, member or plate local axes, analysis results, the selected window or application visibility. A normal return does not certify the visible result, repaint completion or persistence.</para>
        /// <para>The main-source adapter and reviewed STAAD.Pro 2024, 2025 and 2026 help share this contract; inspected 2025/2026 automation metadata agrees. The common internal service invokes the captured RawView exactly once, on the caller's established COM apartment, under a per-adapter gate, with binding checks before and after dispatch. The gate does not coordinate other adapters, raw automation access or the STAAD UI. Reacquire wrapper.View after wrapper refresh. Native errors propagate unchanged; a post-call binding error can follow an actual graphical change. Coordinate model and UI access before calling. There is no automatic retry, rollback, thread-pool dispatch, alternate instance, model launch, analysis, implicit save or orientation read-back. Signature agreement and managed fixtures do not establish live native graphical readiness or certify every servicing build.</para>
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">angleDegrees is NaN or positive or negative infinity. Validation occurs before native dispatch.</exception>
        /// <exception cref="InvalidOperationException">The wrapper is disposed or its captured View binding was replaced, including during native dispatch.</exception>
        /// <exception cref="System.Runtime.InteropServices.COMException">The native graphical request failed; effects may precede the exception.</exception>
        /// <exception cref="Microsoft.CSharp.RuntimeBinder.RuntimeBinderException">The captured automation object lacks RotateUp with the reviewed contract.</exception>
        /// <example><code>bool changeView = false; // Set true only when a graphical change is intended.
        /// if (changeView)
        /// {
        ///     double angleDegrees = 15.0;
        ///     var result = view.RotateUp(angleDegrees);
        ///     System.Console.WriteLine(result.NativeFunction + ": " + result.Status);
        /// }</code></example>
        public ViewOperationResult RotateUp(double angleDegrees) => service.RotateUp(angleDegrees);

        /// <summary>Requests an incremental graphical rotation about global X through the native RotateDown direction.</summary>
        /// <param name="angle">The finite signed rotation angle in degrees, preserving the main-source parameter name. Zero, negative values, negative zero and values beyond one revolution are passed unchanged. No radians conversion, clamping, normalization or rounding is performed.</param>
        /// <returns>A ViewOperationResult identifying native RotateDown with InvokedWithoutAcknowledgement after normal native void completion and final binding checks. NativeReturnCode is null and IsSuccessful is false because the native method supplies no success flag; this does not report native failure.</returns>
        /// <remarks>The main-source description identifies this as the opposite-direction counterpart of RotateUp; the native help identifies global X. STAAD determines the screen convention and effect. No guessed clockwise/counterclockwise signs, camera vectors or equivalence to negating RotateUp are asserted. Unlike ShowFront/ShowPlan and other fixed orientations, this is relative to the current graphical view; successive calls accumulate and zero is still dispatched. This is a graphical request, not a transformation of analytical coordinates, structural local axes or analysis results. It does not select a different graphical window or show the application. Reuses the single View adapter and its common captured-binding service. Shared reviewed 2024/2025/2026 declarations and passive 2025/2026 automation metadata agree on void return and one VARIANT angle input. Invokes captured RawView exactly once on the caller&#x27;s established COM apartment under the adapter&#x27;s serialization gate, with disposal/replacement checks before and after dispatch. This gate does not coordinate other adapters, raw COM callers or external UI edits. Reacquire wrapper.View after refresh. Nonfinite inputs fail before any native call. Native exceptions propagate unchanged; a post-call exception may follow an actual graphical change. No automatic retry, fallback instance, rollback, background-thread dispatch, analysis, model launch or implicit save occurs. A receipt proves invocation completion only; repaint, visible effect, readiness, licensing, persistence and live native servicing-build compatibility require separate qualification.</remarks>
        /// <exception cref="ArgumentOutOfRangeException">angle is NaN or positive/negative infinity; rejected before native dispatch.</exception>
        /// <exception cref="InvalidOperationException">The wrapper is disposed or its captured View binding is replaced before or during dispatch; reacquire the adapter.</exception>
        /// <exception cref="System.Runtime.InteropServices.COMException">The native graphical request failed; UI effects may precede the error. The original exception propagates without retry.</exception>
        /// <exception cref="Microsoft.CSharp.RuntimeBinder.RuntimeBinderException">The captured automation object lacks RotateDown with a compatible one-angle contract.</exception>
        /// <example><code>
        /// bool changeView = false; // Enable only when an incremental graphical change is intended.
        /// if (changeView)
        /// {
        ///     double angle = 15.0; // Degrees; negative values are passed unchanged.
        ///     var result = view.RotateDown(angle);
        ///     System.Console.WriteLine(result.NativeFunction + ": " + result.Status);
        /// }
        /// </code></example>
        public ViewOperationResult RotateDown(double angle) => service.RotateDown(angle);

        /// <summary>Requests an incremental graphical rotation about global Y through the native RotateLeft direction.</summary>
        /// <param name="angle">The finite signed rotation angle in degrees, preserving the main-source parameter name. Zero, negative values, negative zero and values beyond one revolution are passed unchanged. No radians conversion, clamping, normalization or rounding is performed.</param>
        /// <returns>A ViewOperationResult identifying native RotateLeft with InvokedWithoutAcknowledgement after normal native void completion and final binding checks. NativeReturnCode is null and IsSuccessful is false because the native method supplies no success flag; this does not report native failure.</returns>
        /// <remarks>The main-source and native help identify global Y; use RotateRight for its opposite-direction counterpart. STAAD determines the screen convention and effect. No guessed clockwise/counterclockwise signs, camera vectors or equivalence to negating RotateRight are asserted. Unlike ShowFront/ShowPlan and other fixed orientations, this is relative to the current graphical view; successive calls accumulate and zero is still dispatched. This is a graphical request, not a transformation of analytical coordinates, structural local axes or analysis results. It does not select a different graphical window or show the application. Reuses the single View adapter and its common captured-binding service. Shared reviewed 2024/2025/2026 declarations and passive 2025/2026 automation metadata agree on void return and one VARIANT angle input. Invokes captured RawView exactly once on the caller&#x27;s established COM apartment under the adapter&#x27;s serialization gate, with disposal/replacement checks before and after dispatch. This gate does not coordinate other adapters, raw COM callers or external UI edits. Reacquire wrapper.View after refresh. Nonfinite inputs fail before any native call. Native exceptions propagate unchanged; a post-call exception may follow an actual graphical change. No automatic retry, fallback instance, rollback, background-thread dispatch, analysis, model launch or implicit save occurs. A receipt proves invocation completion only; repaint, visible effect, readiness, licensing, persistence and live native servicing-build compatibility require separate qualification.</remarks>
        /// <exception cref="ArgumentOutOfRangeException">angle is NaN or positive/negative infinity; rejected before native dispatch.</exception>
        /// <exception cref="InvalidOperationException">The wrapper is disposed or its captured View binding is replaced before or during dispatch; reacquire the adapter.</exception>
        /// <exception cref="System.Runtime.InteropServices.COMException">The native graphical request failed; UI effects may precede the error. The original exception propagates without retry.</exception>
        /// <exception cref="Microsoft.CSharp.RuntimeBinder.RuntimeBinderException">The captured automation object lacks RotateLeft with a compatible one-angle contract.</exception>
        /// <example><code>
        /// bool changeView = false; // Enable only when an incremental graphical change is intended.
        /// if (changeView)
        /// {
        ///     double angle = 15.0; // Degrees; negative values are passed unchanged.
        ///     var result = view.RotateLeft(angle);
        ///     System.Console.WriteLine(result.NativeFunction + ": " + result.Status);
        /// }
        /// </code></example>
        public ViewOperationResult RotateLeft(double angle) => service.RotateLeft(angle);

        /// <summary>Requests an incremental graphical rotation about global Y through the native RotateRight direction.</summary>
        /// <param name="angle">The finite signed rotation angle in degrees, preserving the main-source parameter name. Zero, negative values, negative zero and values beyond one revolution are passed unchanged. No radians conversion, clamping, normalization or rounding is performed.</param>
        /// <returns>A ViewOperationResult identifying native RotateRight with InvokedWithoutAcknowledgement after normal native void completion and final binding checks. NativeReturnCode is null and IsSuccessful is false because the native method supplies no success flag; this does not report native failure.</returns>
        /// <remarks>The main-source description identifies this as the opposite-direction counterpart of RotateLeft; the native help identifies global Y. STAAD determines the screen convention and effect. No guessed clockwise/counterclockwise signs, camera vectors or equivalence to negating RotateLeft are asserted. Unlike ShowFront/ShowPlan and other fixed orientations, this is relative to the current graphical view; successive calls accumulate and zero is still dispatched. This is a graphical request, not a transformation of analytical coordinates, structural local axes or analysis results. It does not select a different graphical window or show the application. Reuses the single View adapter and its common captured-binding service. Shared reviewed 2024/2025/2026 declarations and passive 2025/2026 automation metadata agree on void return and one VARIANT angle input. Invokes captured RawView exactly once on the caller&#x27;s established COM apartment under the adapter&#x27;s serialization gate, with disposal/replacement checks before and after dispatch. This gate does not coordinate other adapters, raw COM callers or external UI edits. Reacquire wrapper.View after refresh. Nonfinite inputs fail before any native call. Native exceptions propagate unchanged; a post-call exception may follow an actual graphical change. No automatic retry, fallback instance, rollback, background-thread dispatch, analysis, model launch or implicit save occurs. A receipt proves invocation completion only; repaint, visible effect, readiness, licensing, persistence and live native servicing-build compatibility require separate qualification.</remarks>
        /// <exception cref="ArgumentOutOfRangeException">angle is NaN or positive/negative infinity; rejected before native dispatch.</exception>
        /// <exception cref="InvalidOperationException">The wrapper is disposed or its captured View binding is replaced before or during dispatch; reacquire the adapter.</exception>
        /// <exception cref="System.Runtime.InteropServices.COMException">The native graphical request failed; UI effects may precede the error. The original exception propagates without retry.</exception>
        /// <exception cref="Microsoft.CSharp.RuntimeBinder.RuntimeBinderException">The captured automation object lacks RotateRight with a compatible one-angle contract.</exception>
        /// <example><code>
        /// bool changeView = false; // Enable only when an incremental graphical change is intended.
        /// if (changeView)
        /// {
        ///     double angle = 15.0; // Degrees; negative values are passed unchanged.
        ///     var result = view.RotateRight(angle);
        ///     System.Console.WriteLine(result.NativeFunction + ": " + result.Status);
        /// }
        /// </code></example>
        public ViewOperationResult RotateRight(double angle) => service.RotateRight(angle);

        /// <summary>Requests an incremental graphical rotation about global Z through the native SpinLeft direction.</summary>
        /// <param name="angle">The finite signed rotation angle in degrees, preserving the main-source parameter name. Zero, negative values, negative zero and values beyond one revolution are passed unchanged. No radians conversion, clamping, normalization or rounding is performed.</param>
        /// <returns>A ViewOperationResult identifying native SpinLeft with InvokedWithoutAcknowledgement after normal native void completion and final binding checks. NativeReturnCode is null and IsSuccessful is false because the native method supplies no success flag; this does not report native failure.</returns>
        /// <remarks>The main-source and native help identify global Z; use SpinRight for its opposite-direction counterpart. STAAD determines the screen convention and effect. No guessed clockwise/counterclockwise signs, camera vectors or equivalence to negating SpinRight are asserted. Unlike ShowFront/ShowPlan and other fixed orientations, this is relative to the current graphical view; successive calls accumulate and zero is still dispatched. This is a graphical request, not a transformation of analytical coordinates, structural local axes or analysis results. It does not select a different graphical window or show the application. Reuses the single View adapter and its common captured-binding service. Shared reviewed 2024/2025/2026 declarations and passive 2025/2026 automation metadata agree on void return and one VARIANT angle input. Invokes captured RawView exactly once on the caller&#x27;s established COM apartment under the adapter&#x27;s serialization gate, with disposal/replacement checks before and after dispatch. This gate does not coordinate other adapters, raw COM callers or external UI edits. Reacquire wrapper.View after refresh. Nonfinite inputs fail before any native call. Native exceptions propagate unchanged; a post-call exception may follow an actual graphical change. No automatic retry, fallback instance, rollback, background-thread dispatch, analysis, model launch or implicit save occurs. A receipt proves invocation completion only; repaint, visible effect, readiness, licensing, persistence and live native servicing-build compatibility require separate qualification.</remarks>
        /// <exception cref="ArgumentOutOfRangeException">angle is NaN or positive/negative infinity; rejected before native dispatch.</exception>
        /// <exception cref="InvalidOperationException">The wrapper is disposed or its captured View binding is replaced before or during dispatch; reacquire the adapter.</exception>
        /// <exception cref="System.Runtime.InteropServices.COMException">The native graphical request failed; UI effects may precede the error. The original exception propagates without retry.</exception>
        /// <exception cref="Microsoft.CSharp.RuntimeBinder.RuntimeBinderException">The captured automation object lacks SpinLeft with a compatible one-angle contract.</exception>
        /// <example><code>
        /// bool changeView = false; // Enable only when an incremental graphical change is intended.
        /// if (changeView)
        /// {
        ///     double angle = 15.0; // Degrees; negative values are passed unchanged.
        ///     var result = view.SpinLeft(angle);
        ///     System.Console.WriteLine(result.NativeFunction + ": " + result.Status);
        /// }
        /// </code></example>
        public ViewOperationResult SpinLeft(double angle) => service.SpinLeft(angle);

        /// <summary>Requests an incremental graphical rotation about global Z through the native SpinRight direction.</summary>
        /// <param name="angle">The finite signed rotation angle in degrees, preserving the main-source parameter name. Zero, negative values, negative zero and values beyond one revolution are passed unchanged. No radians conversion, clamping, normalization or rounding is performed.</param>
        /// <returns>A ViewOperationResult identifying native SpinRight with InvokedWithoutAcknowledgement after normal native void completion and final binding checks. NativeReturnCode is null and IsSuccessful is false because the native method supplies no success flag; this does not report native failure.</returns>
        /// <remarks>The main-source description identifies this as the opposite-direction counterpart of SpinLeft; the native help identifies global Z. STAAD determines the screen convention and effect. No guessed clockwise/counterclockwise signs, camera vectors or equivalence to negating SpinLeft are asserted. Unlike ShowFront/ShowPlan and other fixed orientations, this is relative to the current graphical view; successive calls accumulate and zero is still dispatched. This is a graphical request, not a transformation of analytical coordinates, structural local axes or analysis results. It does not select a different graphical window or show the application. Reuses the single View adapter and its common captured-binding service. Shared reviewed 2024/2025/2026 declarations and passive 2025/2026 automation metadata agree on void return and one VARIANT angle input. Invokes captured RawView exactly once on the caller&#x27;s established COM apartment under the adapter&#x27;s serialization gate, with disposal/replacement checks before and after dispatch. This gate does not coordinate other adapters, raw COM callers or external UI edits. Reacquire wrapper.View after refresh. Nonfinite inputs fail before any native call. Native exceptions propagate unchanged; a post-call exception may follow an actual graphical change. No automatic retry, fallback instance, rollback, background-thread dispatch, analysis, model launch or implicit save occurs. A receipt proves invocation completion only; repaint, visible effect, readiness, licensing, persistence and live native servicing-build compatibility require separate qualification.</remarks>
        /// <exception cref="ArgumentOutOfRangeException">angle is NaN or positive/negative infinity; rejected before native dispatch.</exception>
        /// <exception cref="InvalidOperationException">The wrapper is disposed or its captured View binding is replaced before or during dispatch; reacquire the adapter.</exception>
        /// <exception cref="System.Runtime.InteropServices.COMException">The native graphical request failed; UI effects may precede the error. The original exception propagates without retry.</exception>
        /// <exception cref="Microsoft.CSharp.RuntimeBinder.RuntimeBinderException">The captured automation object lacks SpinRight with a compatible one-angle contract.</exception>
        /// <example><code>
        /// bool changeView = false; // Enable only when an incremental graphical change is intended.
        /// if (changeView)
        /// {
        ///     double angle = 15.0; // Degrees; negative values are passed unchanged.
        ///     var result = view.SpinRight(angle);
        ///     System.Console.WriteLine(result.NativeFunction + ": " + result.Status);
        /// }
        /// </code></example>
        public ViewOperationResult SpinRight(double angle) => service.SpinRight(angle);

        /// <summary>Requests a zoom of the current graphical view to display the whole structure.</summary>
        /// <returns>A <see cref="ViewOperationResult"/> whose <see cref="ViewOperationResult.NativeFunction"/> is ZoomAll and whose status is <see cref="StaadPro.Interop.Enums.View.ViewOperationStatus.InvokedWithoutAcknowledgement"/> after native void completion and the final binding check. <see cref="ViewOperationResult.NativeReturnCode"/> is null and <see cref="ViewOperationResult.IsSuccessful"/> is false because native success was not acknowledged; false is not a native failure flag.</returns>
        /// <remarks>
        /// <para>Choose ZoomAll to fit the whole structure in the current view. Unlike ZoomExtentsMainView, it retains its distinct native current-view route rather than substituting a main-view extents request. It does not call ShowAllMembers or infer that hidden members become visible, and the result does not certify final extents, scale or repaint completion.</para>
        /// <para>Shared reviewed STAAD.Pro 2024/2025/2026 help and inspected 2025/2026 automation metadata declare native void completion. Reuses the captured RawView and common internal View service on the caller's established COM apartment, under the same per-adapter gate as orientation and rotation requests. Binding identity is checked before and after exactly one dispatch. The gate does not coordinate other wrappers/adapters, raw clients or external UI changes. Reacquire wrapper.View after wrapper refresh. Native and binding errors propagate unchanged; a post-call error can follow an actual UI effect. These functions do not transform structural coordinates, execute analysis, open another model, show a hidden application or implicitly save. No automatic retry, fallback instance, rollback, background dispatch or effect read-back is attempted. Normal completion does not certify native readiness, the visible result or persistence; live native UI/COM qualification remains separate.</para>
        /// </remarks>
        /// <exception cref="InvalidOperationException">The captured View binding was disposed or replaced before or during dispatch.</exception>
        /// <exception cref="System.Runtime.InteropServices.COMException">The native graphical request failed; an effect may precede the error.</exception>
        /// <exception cref="Microsoft.CSharp.RuntimeBinder.RuntimeBinderException">The captured automation object does not expose ZoomAll with the reviewed contract.</exception>
        /// <example><code>bool changeView = false; // Enable only when the graphical change is intended.
        /// if (changeView)
        /// {
        ///     var result = view.ZoomAll();
        ///     System.Console.WriteLine(result.NativeFunction + ": " + result.Status);
        /// }</code></example>
        public ViewOperationResult ZoomAll() => service.ZoomAll();

        /// <summary>Requests the native main-view zoom-to-extents operation.</summary>
        /// <returns>A <see cref="ViewOperationResult"/> whose <see cref="ViewOperationResult.NativeFunction"/> is ZoomExtentsMainView and whose status is <see cref="StaadPro.Interop.Enums.View.ViewOperationStatus.InvokedWithoutAcknowledgement"/> after native void completion and the final binding check. <see cref="ViewOperationResult.NativeReturnCode"/> is null and <see cref="ViewOperationResult.IsSuccessful"/> is false because native success was not acknowledged; false is not a native failure flag.</returns>
        /// <remarks>
        /// <para>Choose ZoomExtentsMainView for the native main-view extents operation. Unlike ZoomAll, it invokes the dedicated main-view route; the reviewed help describes adjusting the main viewing scale to extents in a new view. This wrapper does not synthesize a window, activate a window, substitute ZoomAll or guarantee the resulting window arrangement. It does not call ShowAllMembers or certify final extents, scale or repaint completion.</para>
        /// <para>Shared reviewed STAAD.Pro 2024/2025/2026 help and inspected 2025/2026 automation metadata declare native void completion. Reuses the captured RawView and common internal View service on the caller's established COM apartment, under the same per-adapter gate as orientation and rotation requests. Binding identity is checked before and after exactly one dispatch. The gate does not coordinate other wrappers/adapters, raw clients or external UI changes. Reacquire wrapper.View after wrapper refresh. Native and binding errors propagate unchanged; a post-call error can follow an actual UI effect. These functions do not transform structural coordinates, execute analysis, open another model, show a hidden application or implicitly save. No automatic retry, fallback instance, rollback, background dispatch or effect read-back is attempted. Normal completion does not certify native readiness, the visible result or persistence; live native UI/COM qualification remains separate.</para>
        /// </remarks>
        /// <exception cref="InvalidOperationException">The captured View binding was disposed or replaced before or during dispatch.</exception>
        /// <exception cref="System.Runtime.InteropServices.COMException">The native graphical request failed; an effect may precede the error.</exception>
        /// <exception cref="Microsoft.CSharp.RuntimeBinder.RuntimeBinderException">The captured automation object does not expose ZoomExtentsMainView with the reviewed contract.</exception>
        /// <example><code>bool changeView = false; // Enable only when the graphical change is intended.
        /// if (changeView)
        /// {
        ///     var result = view.ZoomExtentsMainView();
        ///     System.Console.WriteLine(result.NativeFunction + ": " + result.Status);
        /// }</code></example>
        public ViewOperationResult ZoomExtentsMainView() => service.ZoomExtentsMainView();

        /// <summary>Requests display of all members in the current structure.</summary>
        /// <returns>A <see cref="ViewOperationResult"/> whose <see cref="ViewOperationResult.NativeFunction"/> is ShowAllMembers and whose status is <see cref="StaadPro.Interop.Enums.View.ViewOperationStatus.InvokedWithoutAcknowledgement"/> after native void completion and the final binding check. <see cref="ViewOperationResult.NativeReturnCode"/> is null and <see cref="ViewOperationResult.IsSuccessful"/> is false because native success was not acknowledged; false is not a native failure flag.</returns>
        /// <remarks>
        /// <para>Choose ShowAllMembers to request member visibility for the whole current structure. Unlike ShowMember, this makes no per-member selection and takes no Beam. Unlike ZoomAll and ZoomExtentsMainView, it requests visibility rather than fitting the camera. It does not enumerate, create or select analytical members, restore a previous visibility snapshot, or certify plate, solid or surface visibility.</para>
        /// <para>Shared reviewed STAAD.Pro 2024/2025/2026 help and inspected 2025/2026 automation metadata declare native void completion. Reuses the captured RawView and common internal View service on the caller's established COM apartment, under the same per-adapter gate as orientation and rotation requests. Binding identity is checked before and after exactly one dispatch. The gate does not coordinate other wrappers/adapters, raw clients or external UI changes. Reacquire wrapper.View after wrapper refresh. Native and binding errors propagate unchanged; a post-call error can follow an actual UI effect. These functions do not transform structural coordinates, execute analysis, open another model, show a hidden application or implicitly save. No automatic retry, fallback instance, rollback, background dispatch or effect read-back is attempted. Normal completion does not certify native readiness, the visible result or persistence; live native UI/COM qualification remains separate.</para>
        /// </remarks>
        /// <exception cref="InvalidOperationException">The captured View binding was disposed or replaced before or during dispatch.</exception>
        /// <exception cref="System.Runtime.InteropServices.COMException">The native graphical request failed; an effect may precede the error.</exception>
        /// <exception cref="Microsoft.CSharp.RuntimeBinder.RuntimeBinderException">The captured automation object does not expose ShowAllMembers with the reviewed contract.</exception>
        /// <example><code>bool changeView = false; // Enable only when the graphical change is intended.
        /// if (changeView)
        /// {
        ///     var result = view.ShowAllMembers();
        ///     System.Console.WriteLine(result.NativeFunction + ": " + result.Status);
        /// }</code></example>
        public ViewOperationResult ShowAllMembers() => service.ShowAllMembers();

        /// <summary>Requests hiding all members in the current structure.</summary>
        /// <returns>A <see cref="ViewOperationResult"/> whose <see cref="ViewOperationResult.NativeFunction"/> is HideAllMembers and whose status is <see cref="StaadPro.Interop.Enums.View.ViewOperationStatus.InvokedWithoutAcknowledgement"/> after native void completion and the final binding check. <see cref="ViewOperationResult.NativeReturnCode"/> is null and <see cref="ViewOperationResult.IsSuccessful"/> is false because native success was not acknowledged; false is not a native failure flag.</returns>
        /// <remarks>
        /// <para>Choose HideAllMembers to request whole-structure member hiding. Unlike ShowAllMembers, this hides rather than displays members; unlike either zoom operation, it requests visibility rather than a camera fit. The analytical members remain in the model. It does not delete geometry, clear loads or analysis data, record a reversible visibility snapshot, or certify plate, solid or surface visibility.</para>
        /// <para>Shared reviewed STAAD.Pro 2024/2025/2026 help and inspected 2025/2026 automation metadata declare native void completion. Reuses the captured RawView and common internal View service on the caller's established COM apartment, under the same per-adapter gate as orientation and rotation requests. Binding identity is checked before and after exactly one dispatch. The gate does not coordinate other wrappers/adapters, raw clients or external UI changes. Reacquire wrapper.View after wrapper refresh. Native and binding errors propagate unchanged; a post-call error can follow an actual UI effect. These functions do not transform structural coordinates, execute analysis, open another model, show a hidden application or implicitly save. No automatic retry, fallback instance, rollback, background dispatch or effect read-back is attempted. Normal completion does not certify native readiness, the visible result or persistence; live native UI/COM qualification remains separate.</para>
        /// </remarks>
        /// <exception cref="InvalidOperationException">The captured View binding was disposed or replaced before or during dispatch.</exception>
        /// <exception cref="System.Runtime.InteropServices.COMException">The native graphical request failed; an effect may precede the error.</exception>
        /// <exception cref="Microsoft.CSharp.RuntimeBinder.RuntimeBinderException">The captured automation object does not expose HideAllMembers with the reviewed contract.</exception>
        /// <example><code>bool changeView = false; // Enable only when the graphical change is intended.
        /// if (changeView)
        /// {
        ///     var result = view.HideAllMembers();
        ///     System.Console.WriteLine(result.NativeFunction + ": " + result.Status);
        /// }</code></example>
        public ViewOperationResult HideAllMembers() => service.HideAllMembers();

        /// <summary>Requests display of the analytical member identified by the supplied Beam.</summary>
        /// <param name="beam">An existing Beam for an analytical member in the acquired model. Must be non-null and have a positive Id. The Id is captured once and passed unchanged as native Int32; no node coordinates are required. The caller must prevent concurrent identity changes and ensure model membership.</param>
        /// <returns>A <see cref="ViewOperationResult"/> whose <see cref="ViewOperationResult.NativeFunction"/> is ShowMember and whose status is <see cref="StaadPro.Interop.Enums.View.ViewOperationStatus.InvokedWithoutAcknowledgement"/> after native void completion and the final binding check. <see cref="ViewOperationResult.NativeReturnCode"/> is null and <see cref="ViewOperationResult.IsSuccessful"/> is false because native success was not acknowledged; false is not a native failure flag.</returns>
        /// <remarks>
        /// <para>Choose ShowMember for one analytical beam already belonging to the acquired model. Unlike ShowAllMembers, it sends only this member number; unlike zoom operations, it requests visibility rather than changing the viewing scale. Only a snapshot of beam.Id is used. The original Beam reference is not sent to COM and is not modified, and node coordinates, connectivity and physical-member membership are not read. The method does not discover, create, select or isolate a member, hide other members or verify that the number exists. A positive ID cannot establish that a Beam belongs to this native model; the caller supplies that identity.</para>
        /// <para>Shared reviewed STAAD.Pro 2024/2025/2026 help and inspected 2025/2026 automation metadata declare native void completion. Reuses the captured RawView and common internal View service on the caller's established COM apartment, under the same per-adapter gate as orientation and rotation requests. Binding identity is checked before and after exactly one dispatch. The gate does not coordinate other wrappers/adapters, raw clients or external UI changes. Reacquire wrapper.View after wrapper refresh. Native and binding errors propagate unchanged; a post-call error can follow an actual UI effect. These functions do not transform structural coordinates, execute analysis, open another model, show a hidden application or implicitly save. No automatic retry, fallback instance, rollback, background dispatch or effect read-back is attempted. Normal completion does not certify native readiness, the visible result or persistence; live native UI/COM qualification remains separate.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">beam is null; no native call is made.</exception>
        /// <exception cref="ArgumentOutOfRangeException">beam.Id is zero or negative; the exception names beam and no native call is made.</exception>
        /// <exception cref="InvalidOperationException">The captured View binding was disposed or replaced before or during dispatch.</exception>
        /// <exception cref="System.Runtime.InteropServices.COMException">The native graphical request failed; an effect may precede the error.</exception>
        /// <exception cref="Microsoft.CSharp.RuntimeBinder.RuntimeBinderException">The captured automation object does not expose ShowMember with the reviewed contract.</exception>
        /// <example><code>bool changeView = false; // Enable only when the graphical change is intended.
        /// if (changeView)
        /// {
        ///     var beam = new StaadPro.Interop.Entities.Beam { Id = 17 }; // Must identify a member in this model.
        ///     var result = view.ShowMember(beam);
        ///     System.Console.WriteLine(result.NativeFunction + ": " + result.Status);
        /// }</code></example>
        public ViewOperationResult ShowMember(Beam beam) => service.ShowMember(beam);

        /// <summary>Displays the specified analytical members in the acquired STAAD graphical view.</summary>
        /// <param name="beams">A non-null sequence of non-null beams with positive analytical member IDs in the acquired model. Enumerated once before dispatch; each ID is captured once into an owned Int32 array. Order and duplicates are retained. An empty sequence forwards count zero and an empty array; its graphical effect is native-defined. Geometry fields are not required.</param>
        /// <returns>A <see cref="ViewOperationResult"/> naming ShowMembers with InvokedWithoutAcknowledgement, a null NativeReturnCode and IsSuccessful false. Native void completion provides no success acknowledgement; false does not indicate native failure.</returns>
        /// <remarks>
        /// <para>Changes member display only. Unlike HideAllMembers or ShowAllMembers, this request targets supplied member IDs. It does not isolate, select, delete or renumber members, change geometry or loads, zoom, analyze or save. The caller must ensure IDs belong to the acquired model; membership is not queried. A normal return does not certify graphical readiness, repaint completion, visible effects or persistence.</para>
        /// <para>The main-source name and IEnumerable&lt;Beam&gt; parameter are preserved. A single enumeration snapshots IDs before native dispatch. No sorting, deduplication, geometry lookup or caller collection mutation occurs. Enumeration exceptions propagate without calling native code. The native call receives the exact array length and that same owned Int32 array, including zero length; empty-input effects require native qualification. Concurrent mutation while enumerating is the caller's responsibility.</para>
        /// <para>Uses the captured RawView on the caller's established COM apartment under the existing per-adapter gate, with binding checks before and after dispatch. Reacquire wrapper.View after refresh. The gate does not coordinate raw-object access, other adapters or external UI changes. Native errors propagate unchanged; a post-call lifetime error can follow a graphical change. No retry, rollback, alternate instance, background dispatch or implicit model launch occurs. The installed 2025 help agrees with the recorded 2024/2025/2026 input-VARIANT contracts. Managed fixtures do not establish live COM marshaling or graphical compatibility for every STAAD build.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">beams is null.</exception>
        /// <exception cref="ArgumentException">beams contains a null element.</exception>
        /// <exception cref="ArgumentOutOfRangeException">A supplied beam ID is zero or negative; the parameter name is beams.</exception>
        /// <exception cref="InvalidOperationException">The captured View binding is disposed or replaced, including during dispatch; sequence enumeration can also propagate its own InvalidOperationException.</exception>
        /// <exception cref="System.Runtime.InteropServices.COMException">The native request failed; graphical effects may precede the exception.</exception>
        /// <exception cref="Microsoft.CSharp.RuntimeBinder.RuntimeBinderException">The captured automation object lacks the reviewed ShowMembers contract.</exception>
        /// <example><code>bool changeVisibility = false; // Enable only when a graphical change is intended.
        /// if (changeVisibility)
        /// {
        ///     var beams = new[] { new StaadPro.Interop.Entities.Beam { Id = 17 }, new StaadPro.Interop.Entities.Beam { Id = 41 } };
        ///     var receipt = view.ShowMembers(beams);
        ///     System.Console.WriteLine(receipt.NativeFunction + ": " + receipt.Status);
        /// }</code></example>
        public ViewOperationResult ShowMembers(IEnumerable<Beam> beams) => service.ShowMembers(beams);

        /// <summary>Hides one specified analytical member in the acquired STAAD graphical view.</summary>
        /// <param name="beam">A non-null beam whose positive Id identifies an existing analytical member in the acquired model. Only Id is captured, once; StartNode, EndNode and other geometry are not required.</param>
        /// <returns>A <see cref="ViewOperationResult"/> naming HideMember with InvokedWithoutAcknowledgement, a null NativeReturnCode and IsSuccessful false. Native void completion provides no success acknowledgement; false does not indicate native failure.</returns>
        /// <remarks>
        /// <para>Changes member display only. Unlike HideAllMembers or ShowAllMembers, this request targets supplied member IDs. It does not isolate, select, delete or renumber members, change geometry or loads, zoom, analyze or save. The caller must ensure IDs belong to the acquired model; membership is not queried. A normal return does not certify graphical readiness, repaint completion, visible effects or persistence.</para>
        /// <para>Uses the captured RawView on the caller's established COM apartment under the existing per-adapter gate, with binding checks before and after dispatch. Reacquire wrapper.View after refresh. The gate does not coordinate raw-object access, other adapters or external UI changes. Native errors propagate unchanged; a post-call lifetime error can follow a graphical change. No retry, rollback, alternate instance, background dispatch or implicit model launch occurs. The installed 2025 help agrees with the recorded 2024/2025/2026 input-VARIANT contracts. Managed fixtures do not establish live COM marshaling or graphical compatibility for every STAAD build.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">beam is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">A supplied beam.Id is zero or negative; the parameter name is beam.</exception>
        /// <exception cref="InvalidOperationException">The captured View binding is disposed or replaced, including during dispatch; sequence enumeration can also propagate its own InvalidOperationException.</exception>
        /// <exception cref="System.Runtime.InteropServices.COMException">The native request failed; graphical effects may precede the exception.</exception>
        /// <exception cref="Microsoft.CSharp.RuntimeBinder.RuntimeBinderException">The captured automation object lacks the reviewed HideMember contract.</exception>
        /// <example><code>bool changeVisibility = false; // Enable only when a graphical change is intended.
        /// if (changeVisibility)
        /// {
        ///     var beam = new StaadPro.Interop.Entities.Beam { Id = 17 };
        ///     var receipt = view.HideMember(beam);
        ///     System.Console.WriteLine(receipt.NativeFunction + ": " + receipt.Status);
        /// }</code></example>
        public ViewOperationResult HideMember(Beam beam) => service.HideMember(beam);

        /// <summary>Hides the specified analytical members in the acquired STAAD graphical view.</summary>
        /// <param name="beams">A non-null sequence of non-null beams with positive analytical member IDs in the acquired model. Enumerated once before dispatch; each ID is captured once into an owned Int32 array. Order and duplicates are retained. An empty sequence forwards count zero and an empty array; its graphical effect is native-defined. Geometry fields are not required.</param>
        /// <returns>A <see cref="ViewOperationResult"/> naming HideMembers with InvokedWithoutAcknowledgement, a null NativeReturnCode and IsSuccessful false. Native void completion provides no success acknowledgement; false does not indicate native failure.</returns>
        /// <remarks>
        /// <para>Changes member display only. Unlike HideAllMembers or ShowAllMembers, this request targets supplied member IDs. It does not isolate, select, delete or renumber members, change geometry or loads, zoom, analyze or save. The caller must ensure IDs belong to the acquired model; membership is not queried. A normal return does not certify graphical readiness, repaint completion, visible effects or persistence.</para>
        /// <para>The main-source name and IEnumerable&lt;Beam&gt; parameter are preserved. A single enumeration snapshots IDs before native dispatch. No sorting, deduplication, geometry lookup or caller collection mutation occurs. Enumeration exceptions propagate without calling native code. The native call receives the exact array length and that same owned Int32 array, including zero length; empty-input effects require native qualification. Concurrent mutation while enumerating is the caller's responsibility.</para>
        /// <para>Uses the captured RawView on the caller's established COM apartment under the existing per-adapter gate, with binding checks before and after dispatch. Reacquire wrapper.View after refresh. The gate does not coordinate raw-object access, other adapters or external UI changes. Native errors propagate unchanged; a post-call lifetime error can follow a graphical change. No retry, rollback, alternate instance, background dispatch or implicit model launch occurs. The installed 2025 help agrees with the recorded 2024/2025/2026 input-VARIANT contracts. Managed fixtures do not establish live COM marshaling or graphical compatibility for every STAAD build.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">beams is null.</exception>
        /// <exception cref="ArgumentException">beams contains a null element.</exception>
        /// <exception cref="ArgumentOutOfRangeException">A supplied beam ID is zero or negative; the parameter name is beams.</exception>
        /// <exception cref="InvalidOperationException">The captured View binding is disposed or replaced, including during dispatch; sequence enumeration can also propagate its own InvalidOperationException.</exception>
        /// <exception cref="System.Runtime.InteropServices.COMException">The native request failed; graphical effects may precede the exception.</exception>
        /// <exception cref="Microsoft.CSharp.RuntimeBinder.RuntimeBinderException">The captured automation object lacks the reviewed HideMembers contract.</exception>
        /// <example><code>bool changeVisibility = false; // Enable only when a graphical change is intended.
        /// if (changeVisibility)
        /// {
        ///     var beams = new[] { new StaadPro.Interop.Entities.Beam { Id = 17 }, new StaadPro.Interop.Entities.Beam { Id = 41 } };
        ///     var receipt = view.HideMembers(beams);
        ///     System.Console.WriteLine(receipt.NativeFunction + ": " + receipt.Status);
        /// }</code></example>
        public ViewOperationResult HideMembers(IEnumerable<Beam> beams) => service.HideMembers(beams);

        /// <summary>Requests hiding one specified analytical plate in the acquired STAAD graphical view.</summary>
        /// <param name="plate">A non-null Plate whose positive Id identifies an existing analytical plate in the acquired model. Only Id is captured once; node coordinates/connectivity and other fields are not read. The caller must ensure model membership and coordinate concurrent identity changes.</param>
        /// <returns>A <see cref="ViewOperationResult"/> naming HidePlate with
        /// <see cref="StaadPro.Interop.Enums.View.ViewOperationStatus.InvokedWithoutAcknowledgement"/>,
        /// a null NativeReturnCode and IsSuccessful false after normal native void completion and the final binding check.
        /// False reflects the absence of native success acknowledgement; it is not a native failure flag.</returns>
        /// <remarks>
        /// <para>Targets one plate's display. Unlike HideMember, this uses the native plate visibility operation;
        /// unlike zoom/orientation requests, it asks to hide the identified entity rather than change the camera.
        /// Does not delete, select, isolate or renumber geometry, change connectivity, loads or results, analyze or save.
        /// STAAD defines display scope, interactions with other entity types and repaint behavior. A return receipt
        /// does not certify hidden state, visible effect, UI readiness or persistence. Prior visibility is not captured
        /// and no automatic restoration or undo is supplied.</para>
        /// <para>Snapshots plate.Id once before native dispatch and sends that Int32 unchanged. The Plate object is neither sent to COM nor modified; A, B, C and D can be null. No geometry, topology, selection or membership lookup is performed.</para>
        /// <para>Preserves the source name and parameter contract while returning the same named receipt as other
        /// View methods. The installed STAAD.Pro 2025 help and recorded 2024/2025/2026 contracts specify a single
        /// input VARIANT (nPlate) and void completion. The input const VARIANT reference is internal to native
        /// dispatch; no public ref/out parameter or guessed status code is introduced. Signature agreement and
        /// managed stand-ins do not establish live COM marshaling, native licensing or cross-version UI effects.</para>
        /// <para>Invokes the captured RawView exactly once, on the caller's established COM apartment, under the
        /// existing per-adapter gate with binding checks before and after dispatch. Reacquire wrapper.View after
        /// refresh. The gate does not marshal callers to another thread, reject every wrong-apartment caller, or
        /// coordinate raw-object calls, other adapters/wrappers or external UI. Native errors propagate unchanged;
        /// a post-call lifetime error can follow a real graphical change. No retry, fallback instance, rollback,
        /// background dispatch, model launch or effect read-back occurs.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">plate is null; no native call occurs.</exception>
        /// <exception cref="ArgumentOutOfRangeException">plate.Id is zero or negative; the exception names plate and no native call occurs.</exception>
        /// <exception cref="InvalidOperationException">The captured View binding is disposed or replaced, including during dispatch.</exception>
        /// <exception cref="System.Runtime.InteropServices.COMException">The native visibility request fails; effects may precede the exception.</exception>
        /// <exception cref="Microsoft.CSharp.RuntimeBinder.RuntimeBinderException">The captured native View lacks the reviewed HidePlate signature.</exception>
        /// <example><code>bool changeVisibility = false; // Enable only when the graphical change is intended.
        /// if (changeVisibility)
        /// {
        ///     var plate = new StaadPro.Interop.Entities.Plate { Id = 17 }; // An existing plate in this model.
        ///     var receipt = view.HidePlate(plate);
        ///     System.Console.WriteLine(receipt.NativeFunction + ": " + receipt.Status);
        /// }</code></example>
        public ViewOperationResult HidePlate(Plate plate) => service.HidePlate(plate);

        /// <summary>Requests hiding one specified analytical solid in the acquired STAAD graphical view.</summary>
        /// <param name="solidId">A positive Int32 number identifying an existing solid in the acquired model. Passed unchanged; it is not a member/plate number or a collection index. Model membership and graphical readiness are the caller's responsibility.</param>
        /// <returns>A <see cref="ViewOperationResult"/> naming HideSolid with
        /// <see cref="StaadPro.Interop.Enums.View.ViewOperationStatus.InvokedWithoutAcknowledgement"/>,
        /// a null NativeReturnCode and IsSuccessful false after normal native void completion and the final binding check.
        /// False reflects the absence of native success acknowledgement; it is not a native failure flag.</returns>
        /// <remarks>
        /// <para>Targets one solid's display. Unlike HideMember, this uses the native solid visibility operation;
        /// unlike zoom/orientation requests, it asks to hide the identified entity rather than change the camera.
        /// Does not delete, select, isolate or renumber geometry, change connectivity, loads or results, analyze or save.
        /// STAAD defines display scope, interactions with other entity types and repaint behavior. A return receipt
        /// does not certify hidden state, visible effect, UI readiness or persistence. Prior visibility is not captured
        /// and no automatic restoration or undo is supplied.</para>
        /// <para>Sends the supplied solidId as one Int32 input unchanged, including Int32.MaxValue. The library does not resolve geometry, infer an entity type, query existence or map an ID from another model. A positive value alone does not establish membership.</para>
        /// <para>Preserves the source name and parameter contract while returning the same named receipt as other
        /// View methods. The installed STAAD.Pro 2025 help and recorded 2024/2025/2026 contracts specify a single
        /// input VARIANT (nSolid) and void completion. The input const VARIANT reference is internal to native
        /// dispatch; no public ref/out parameter or guessed status code is introduced. Signature agreement and
        /// managed stand-ins do not establish live COM marshaling, native licensing or cross-version UI effects.</para>
        /// <para>Invokes the captured RawView exactly once, on the caller's established COM apartment, under the
        /// existing per-adapter gate with binding checks before and after dispatch. Reacquire wrapper.View after
        /// refresh. The gate does not marshal callers to another thread, reject every wrong-apartment caller, or
        /// coordinate raw-object calls, other adapters/wrappers or external UI. Native errors propagate unchanged;
        /// a post-call lifetime error can follow a real graphical change. No retry, fallback instance, rollback,
        /// background dispatch, model launch or effect read-back occurs.</para>
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">solidId is zero or negative; no native call occurs.</exception>
        /// <exception cref="InvalidOperationException">The captured View binding is disposed or replaced, including during dispatch.</exception>
        /// <exception cref="System.Runtime.InteropServices.COMException">The native visibility request fails; effects may precede the exception.</exception>
        /// <exception cref="Microsoft.CSharp.RuntimeBinder.RuntimeBinderException">The captured native View lacks the reviewed HideSolid signature.</exception>
        /// <example><code>bool changeVisibility = false; // Enable only when the graphical change is intended.
        /// if (changeVisibility)
        /// {
        ///     int solidId = 17; // An existing solid in this model.
        ///     var receipt = view.HideSolid(solidId);
        ///     System.Console.WriteLine(receipt.NativeFunction + ": " + receipt.Status);
        /// }</code></example>
        public ViewOperationResult HideSolid(int solidId) => service.HideSolid(solidId);

        /// <summary>Requests hiding one specified analytical surface in the acquired STAAD graphical view.</summary>
        /// <param name="surfaceId">A positive Int32 number identifying an existing surface in the acquired model. Passed unchanged; it is not a member/plate number or a collection index. Model membership and graphical readiness are the caller's responsibility.</param>
        /// <returns>A <see cref="ViewOperationResult"/> naming HideSurface with
        /// <see cref="StaadPro.Interop.Enums.View.ViewOperationStatus.InvokedWithoutAcknowledgement"/>,
        /// a null NativeReturnCode and IsSuccessful false after normal native void completion and the final binding check.
        /// False reflects the absence of native success acknowledgement; it is not a native failure flag.</returns>
        /// <remarks>
        /// <para>Targets one surface's display. Unlike HideMember, this uses the native surface visibility operation;
        /// unlike zoom/orientation requests, it asks to hide the identified entity rather than change the camera.
        /// Does not delete, select, isolate or renumber geometry, change connectivity, loads or results, analyze or save.
        /// STAAD defines display scope, interactions with other entity types and repaint behavior. A return receipt
        /// does not certify hidden state, visible effect, UI readiness or persistence. Prior visibility is not captured
        /// and no automatic restoration or undo is supplied.</para>
        /// <para>Sends the supplied surfaceId as one Int32 input unchanged, including Int32.MaxValue. The library does not resolve geometry, infer an entity type, query existence or map an ID from another model. A positive value alone does not establish membership.</para>
        /// <para>Preserves the source name and parameter contract while returning the same named receipt as other
        /// View methods. The installed STAAD.Pro 2025 help and recorded 2024/2025/2026 contracts specify a single
        /// input VARIANT (nSurface) and void completion. The input const VARIANT reference is internal to native
        /// dispatch; no public ref/out parameter or guessed status code is introduced. Signature agreement and
        /// managed stand-ins do not establish live COM marshaling, native licensing or cross-version UI effects.</para>
        /// <para>Invokes the captured RawView exactly once, on the caller's established COM apartment, under the
        /// existing per-adapter gate with binding checks before and after dispatch. Reacquire wrapper.View after
        /// refresh. The gate does not marshal callers to another thread, reject every wrong-apartment caller, or
        /// coordinate raw-object calls, other adapters/wrappers or external UI. Native errors propagate unchanged;
        /// a post-call lifetime error can follow a real graphical change. No retry, fallback instance, rollback,
        /// background dispatch, model launch or effect read-back occurs.</para>
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">surfaceId is zero or negative; no native call occurs.</exception>
        /// <exception cref="InvalidOperationException">The captured View binding is disposed or replaced, including during dispatch.</exception>
        /// <exception cref="System.Runtime.InteropServices.COMException">The native visibility request fails; effects may precede the exception.</exception>
        /// <exception cref="Microsoft.CSharp.RuntimeBinder.RuntimeBinderException">The captured native View lacks the reviewed HideSurface signature.</exception>
        /// <example><code>bool changeVisibility = false; // Enable only when the graphical change is intended.
        /// if (changeVisibility)
        /// {
        ///     int surfaceId = 17; // An existing surface in this model.
        ///     var receipt = view.HideSurface(surfaceId);
        ///     System.Console.WriteLine(receipt.NativeFunction + ": " + receipt.Status);
        /// }</code></example>
        public ViewOperationResult HideSurface(int surfaceId) => service.HideSurface(surfaceId);
    }
}
