using StaadPro.Interop.Models.View;
using StaadPro.Interop.Services.View;
using System;
using StaadPro.Interop.Adapters.Interfaces;
using StaadPro.Interop.Models;

namespace StaadPro.Interop.Adapters.Models
{
    /// <summary>Implements front, back and left orientations on the wrapper's captured native View object.</summary>
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
    }
}
