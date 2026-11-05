using StaadPro.Interop.Core.Enums;
using StaadPro.Interop.Core.Services.Interop;

namespace StaadPro.Interop.Core.Services
{
    internal static class RootAnalysisStatus
    {
        internal static AnalysisResult Read(object value) => (AnalysisResult)NativeValues.ReadInt32(value, "analysis status");
    }
}
