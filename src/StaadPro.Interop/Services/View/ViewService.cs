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
