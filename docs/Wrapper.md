# OpenStaadWrapper implementation and source parity

## 2. Metadata semantics and deliberate adaptations

### Native versus host application version

Native version querying uses the documented four reference outputs and version-text return, improving on the source's by-value call. Written numeric parts are checked against the returned text. Missing optional member yields null; malformed/non-string/contradictory supplied metadata throws instead of inventing a version. A return-text-only automation shim can be accepted when all numeric buffers remain their initial sentinels. A returned StaadVersion value is a defensive copy, so editing the mutable model cannot change the cached observation used for future routing.

The source-compatible ApplicationVersion model contains Major, Update, Revision, Build, Display and both Parse overloads. CLR Major/Minor/Build/Revision map to those fields in that order. Text parsing is deliberately stricter than the source helper: exactly four nonnegative integer parts are required. The source helper silently ignored extra components and allowed negative values. Display uses invariant formatting. Edition values remain None=0, V8i=1, Connect=2; obsolete machine-specific installation paths are not embedded in enum descriptions.

### Unit parsing, fallback and caching

The wrapper now uses reference-output unit calls. It prefers a parsed native output over a usable return, then falls back to Kip/Feet for an Imperial base system and kN/Meter otherwise. This fallback, including on transition exceptions, is retained from the main wrapper and explicitly documented; it does not prove the actual current input unit.

The port handles all 0–7 numeric code values, the length code-1 feet alias, standard source names/abbreviations, and the documented lb/MTon/DN forms missing from the source parser. Boolean values and floating/decimal coercions cannot turn into unit codes through Convert.ToInt32 rounding. The 2024 help's documented success return is conservatively not treated as a returned unit when reference output is unusable. That guard is not a native-verified contract registry. A missing native version retains the source's compatible numeric-return heuristic; it must not be advertised as strict unit provenance.

### Window identity and lifecycle

The source default window helper scans process names and chooses the first match. The port deliberately reads GetMainWindowHandle on the exact captured root, preventing accidental selection of another STAAD instance. The injected IStaadModelServices hook is retained; its implementation must target the same instance. The source edition rule is retained: Connect requests the handle; other/unknown editions expose zero.

The handle is non-owning and cached. Zero is allowed; invalid sentinel/coercible noninteger/overflow shapes are rejected. The wrapper does not call IsWindow or promise that a previously observed handle remains live. Existing Root window-control functions continue their independent fresh HWND/PID ownership checks.

Both refresh methods reject a disposed wrapper. Disposal clears managed bindings as well as raw bindings, improving on the source's retained managed adapters. Cached metadata remains readable. An external reference to an old adapter is not revoked; callers must reacquire adapters after RefreshComObjects. Refresh itself is synchronous and in place, so a propagated error may leave partial refreshed state. Call on the correct COM thread, serialize access, and recover deliberately.

For compatibility, missing Geometry/Load properties retain the repository's existing direct-root dispatch fallback. Other missing native domains remain null. Unexpected native interface/metadata failures propagate rather than masquerading as missing optional members. Missing metadata on legacy/test roots is tolerated; it is not evidence of version-specific support.

## 3. Root property reuse (implemented)

| Implemented Root function | Implemented wrapper value | Important meaning |
| --- | --- | --- |
| GetApplicationVersion | `StaadVersion` | Native STAAD version, defensive copy, not host ApplicationVersion. |
| GetBaseUnit | `BaseUnitSystem` | Cached normalized base-system value. |
| GetInputUnitForForce | `InputForceUnit` | Cached enum, possibly source-compatible fallback. |
| GetInputUnitForLength | `InputLengthUnit` | Cached enum with normalized feet aliases. |
| GetMainWindowHandle | `StaadApplicationWindowHandle` | Cached, non-owning, Connect-family lookup. |
| Version/family metadata | `StaadVersion`, `StaadEdition` | Select native policy from the observed version; edition alone is insufficient. |

Wrapper-bound projections now reuse the internal metadata accessor without native version/unit/window reads. Native text, raw-code aliases and output provenance are not retained by these public cache properties; do not fabricate them. GetProcessId has no wrapper property and remains a separate native query. Standalone raw-root adapters use the shared WrapperMetadataReader. The implemented metadata-access seam is described in the architecture plan.

## 4. Usage

```csharp
using System;
using StaadPro.Interop.Models;
using StaadPro.Interop.Services;

// Run on the session's appropriate COM thread.
using (OpenStaadWrapper wrapper = OpenStaadWrapperProvider.GetRunning())
{
    if (wrapper == null) return;
    Console.WriteLine(wrapper.StaadVersion?.Display);       // STAAD.Pro
    Console.WriteLine(wrapper.ApplicationVersion.Display); // consuming application
    Console.WriteLine(wrapper.InputForceUnit);

    // After an explicit native input-unit change:
    wrapper.RefreshUnits();

    // After explicit model reopening AND native database initialization:
    wrapper.RefreshComObjects();
    var geometry = wrapper.Geometry; // acquire the replacement adapter

    // Raw binding remains available; prefer the typed Output queries where implemented.
    object nativeOutput = wrapper.RawOutput;
}
```
Construction and refresh do not show host UI or launch/close STAAD. The default host interaction service cancels selections and ignores notifications; consumers can inject their own service. An already-created wrapper retains the service captured at construction.

## 5. Validation and release boundary

Source wrapper SHA-256: `83737733D57A672313385F99EA0CA8FFA10F1917FB6A33B89E43CE27E3F37B14`.
