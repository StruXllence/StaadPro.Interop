using System;
using StaadPro.Interop.Models;
using StaadPro.Interop.Models.View;

namespace StaadPro.Interop.Services.View
{
    internal sealed class ViewService
    {
        private readonly OpenStaadWrapper wrapper;
        private readonly object native;
        private readonly object gate = new object();

        internal ViewService(OpenStaadWrapper wrapper, object native)
        {
            this.wrapper = wrapper;
            this.native = native;
        }

        internal ViewOperationResult ShowFront() => RequestOrientation("ShowFront", view => view.ShowFront());
        internal ViewOperationResult ShowBack() => RequestOrientation("ShowBack", view => view.ShowBack());
        internal ViewOperationResult ShowLeft() => RequestOrientation("ShowLeft", view => view.ShowLeft());
        internal ViewOperationResult ShowRight() => RequestOrientation("ShowRight", view => view.ShowRight());
        internal ViewOperationResult ShowPlan() => RequestOrientation("ShowPlan", view => view.ShowPlan());
        internal ViewOperationResult ShowBottom() => RequestOrientation("ShowBottom", view => view.ShowBottom());
        internal ViewOperationResult ShowIsometric() => RequestOrientation("ShowIsometric", view => view.ShowIsometric());

        internal ViewOperationResult RotateUp(double angleDegrees)
        {
            if (double.IsNaN(angleDegrees) || double.IsInfinity(angleDegrees))
                throw new ArgumentOutOfRangeException(nameof(angleDegrees), "The rotation angle must be finite.");

            return RequestOrientation("RotateUp", view => view.RotateUp(angleDegrees));
        }

        internal ViewOperationResult RotateDown(double angle) => RequestRotation("RotateDown", angle, view => view.RotateDown(angle));
        internal ViewOperationResult RotateLeft(double angle) => RequestRotation("RotateLeft", angle, view => view.RotateLeft(angle));
        internal ViewOperationResult RotateRight(double angle) => RequestRotation("RotateRight", angle, view => view.RotateRight(angle));
        internal ViewOperationResult SpinLeft(double angle) => RequestRotation("SpinLeft", angle, view => view.SpinLeft(angle));
        internal ViewOperationResult SpinRight(double angle) => RequestRotation("SpinRight", angle, view => view.SpinRight(angle));

        private ViewOperationResult RequestRotation(string nativeFunction, double angle, Action<dynamic> request)
        {
            if (double.IsNaN(angle) || double.IsInfinity(angle))
                throw new ArgumentOutOfRangeException(nameof(angle), "The rotation angle must be finite degrees.");
            return RequestOrientation(nativeFunction, request);
        }

        private void CheckBinding()
        {
            if (wrapper.RawOpenStaad == null || !ReferenceEquals((object)wrapper.RawView, native))
                throw new InvalidOperationException("The View binding is disposed or replaced. Reacquire wrapper.View.");
        }

        private ViewOperationResult RequestOrientation(string nativeFunction, Action<dynamic> request)
        {
            lock (gate)
            {
                CheckBinding();
                request(native);
                CheckBinding();
                return new ViewOperationResult(nativeFunction);
            }
        }
    }
}
