using System;
using StaadPro.Interop.Core.Enums;
using StaadPro.Interop.Core.Models;

namespace StaadPro.Interop.Core.Services.Root.Metadata
{
    internal interface IRootMetadataAccess
    {
        ApplicationVersion GetApplicationVersion();
        BaseUnitSystem GetBaseUnit();
        ForceInputUnit GetInputUnitForForce();
        LengthInputUnit GetInputUnitForLength();
        IntPtr GetMainWindowHandle();
    }
}
