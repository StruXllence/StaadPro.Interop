using StaadPro.Interop.Models.View;
using StaadPro.Interop.Services.View;
using System;
using StaadPro.Interop.Adapters.Interfaces;
using StaadPro.Interop.Models;

namespace StaadPro.Interop.Adapters.Models
{
    /// <summary>Implements standard orientations and incremental rotation on the captured native View object.</summary>
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
    }
}
