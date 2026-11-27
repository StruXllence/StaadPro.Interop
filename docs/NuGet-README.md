![StaadPro.Interop](https://raw.githubusercontent.com/StruXllence/StaadPro.Interop/main/docs/images/logo-with-text-400.png)

# StaadPro.Interop

A strongly-typed .NET Framework wrapper for Bentley STAAD.Pro OpenSTAAD

By **Srinivasa Rao Masanam**.

[![YouTube @StruXllence](https://img.shields.io/badge/YouTube-%40StruXllence-FF0000?style=flat-square&logo=youtube&logoColor=white)](https://www.youtube.com/@StruXllence) [![Website — live](https://img.shields.io/badge/Website-Live-16A34A?style=flat-square)](https://struxllence.com) [![Community forum](https://img.shields.io/badge/Community-Forum-0866C6?style=flat-square)](https://forum.struxllence.com) [![Revamp forum — preview](https://img.shields.io/badge/Revamp%20forum-Preview-7657B5?style=flat-square)](https://new-forum.struxllence.com)

The [StruXllence website](https://struxllence.com) is live. Join the community forum for structural-engineering discussions, explore the forum preview, or follow @StruXllence on YouTube. Report package defects through [GitHub Issues](https://github.com/StruXllence/StaadPro.Interop/issues).

## StruXllence themes for Visual Studio Code

[![Install StruXllence Themes from Visual Studio Marketplace](https://img.shields.io/badge/StruXllence%20Themes-Install-007ACC?style=flat-square)](https://marketplace.visualstudio.com/items?itemName=StruXllence.struxllence-themes)

Code your C# and OpenSTAAD tools with **StruXllence Themes**, a dark and light Visual Studio Code theme suite with matching file and product icons by Srinivasa Rao Masanam.

[Install StruXllence Themes from the Visual Studio Marketplace](https://marketplace.visualstudio.com/items?itemName=StruXllence.struxllence-themes) or use the command line:

~~~powershell
code --install-extension StruXllence.struxllence-themes
# For VS Code Insiders:
code-insiders --install-extension StruXllence.struxllence-themes
~~~

After installation, run **Preferences: Color Theme** and select **StruXllence Dark** or **StruXllence Light**. See the [theme previews and icon setup](https://github.com/StruXllence/struxllence-themes). The editor theme is optional and is installed separately from the NuGet library.

## Version 1.0.0-preview.20261128

Two new `GetPlateCornerForcesAsync` overloads read every corner of one plate across selected load cases, or every corner of selected plates for one load case. Both complete native reads on the calling COM apartment and return indexed results that retain per-corner status. Output provides 110 managed overloads across 48 documented native names. [All-corner batch guide](https://github.com/StruXllence/StaadPro.Interop/blob/main/docs/Output-Plate-Corners.md#task-shaped-all-corner-batches).

## 1.0.0-preview.20261127.2

Two new `GetPlateCornerForcesAsync` overloads read one corner across a set of load cases or all unique corners for one load case. Both return an indexed result set that retains per-observation status and completes native work on the calling COM apartment. Output provides 108 managed overloads across 48 documented native names. [Plate-corner selection contracts and examples](https://github.com/StruXllence/StaadPro.Interop/blob/main/docs/Output-Plate-Corners.md#task-shaped-corner-selections).

## 1.0.0-preview.20261127.1

Two new `GetPlateCornerForcesAsync` overloads accept numeric IDs and the current Geometry and Load adapters. Choose automatic case metadata discovery or supply the load-case family when it is known. Both preserve qualified results and status and complete on the calling COM apartment before returning a task. Output now provides 106 managed overloads across 48 documented native names. [Plate-corner task contracts and examples](https://github.com/StruXllence/StaadPro.Interop/blob/main/docs/Output-Plate-Corners.md#task-shaped-corner-observations).

## 1.0.0-preview.20261126.1

Added `GetMemberSteelUtilizationRatioAsync(beamId)` for a single analytical member ID and `GetMemberSteelUtilizationRatiosAsync(beams)` for a set of beams. Both reuse the existing status-bearing results and finish native reads on the calling COM apartment before returning a completed or faulted task. Batch items retain captured member IDs and individual query statuses. Output now provides 102 overloads reaching 48 documented native names. [Member task contracts and examples](https://github.com/StruXllence/StaadPro.Interop/blob/main/docs/Output-Managed-Workflows.md#task-shaped-member-observations).

Added `GetMemberEndForcesAsync(...)` and the Beam-based `GetMemberSteelUtilizationRatioAsync(...)` with complete matching XML IntelliSense. Both reuse existing named/status results and run native work on the calling COM apartment before returning settled tasks. Native query failure/absence is distinct from a faulted task; no worker COM, managed cache update or native mutation is introduced. That preview provided 100 overloads reaching 48 documented native names. See [task-shaped member observations](https://github.com/StruXllence/StaadPro.Interop/blob/main/docs/Output-Managed-Workflows.md#task-shaped-member-observations).

Added `GetBucklingFactor(bucklingMode)` and the Node-based `GetBucklingModeDisplacements(nativeBucklingModeNumber, node)` overload with complete matching XML IntelliSense. Factor retrieval qualifies native status and written by-reference finite Double output. Mode queries retain all six translation/rotation components, captured node identity and native normalization without hydration, scaling or model changes. Output now provides 98 managed overloads reaching 48 documented native names. See [buckling contracts and examples](https://github.com/StruXllence/StaadPro.Interop/blob/main/docs/Output-Extended-Queries.md#buckling-factor-and-node-overload).

Added `InYRange<T>` and `InZRange<T>` in `GeometryDiagnosticsExtensions` with complete XML IntelliSense. Both retain whole Node/Beam/Plate Y and Z extent containment, ordered open-ended bounds and dedicated source set equality: beam/plate IDs and node ID plus 0.000001 distance, without changing global entities or calling STAAD. See [range selection contracts](https://github.com/StruXllence/StaadPro.Interop/blob/main/docs/Geometry-Diagnostics.md).

Added `HasSameCoordinatesAs` and `InXRange<T>` with complete XML IntelliSense. Coordinate comparison ignores IDs, while range selection uses whole extents and dedicated set equality: beam/plate IDs and node ID plus 0.000001 distance. No global equality or native state changes occur. See [comparison and range contracts](https://github.com/StruXllence/StaadPro.Interop/blob/main/docs/Geometry-Diagnostics.md).

Ported `GetUnwrappedAngles(nodes, origin)` and detached `GetBeamsWithNoGroupAssignment(beamsCollection, groups)` with complete XML IntelliSense. Angle results use immutable named `UnwrappedNodeAngle` records in outward XZ radians; group filtering captures analytical IDs and returns owned, read-only results without native queries. See [geometry diagnostics](https://github.com/StruXllence/StaadPro.Interop/blob/main/docs/Geometry-Diagnostics.md) for seam/order, equality and snapshot behavior.

Added `GroupBeamsByRadius(beams, origin, tolerance = 0.001)` and `GroupBeamsByElevation(beams, tolerance = 0.001)` with full XML IntelliSense. Grouping uses captured XZ endpoint radii or signed Y elevations, fixed first seeds, inclusive length tolerances and ordered duplicate beam occurrences. Every participating endpoint is validated before grouping; scaled radii avoid squaring overflow/underflow and unrepresentable keys are rejected. See [geometry contracts and examples](https://github.com/StruXllence/StaadPro.Interop/blob/main/docs/Geometry-Diagnostics.md).

Added `GetPlatesSharingEdgeWith(plate, candidatePlates)` and `GroupBeamsByPlanAngle(beams, origin, toleranceDegrees = 0.001)` with full XML IntelliSense. Neighbours retain ordered matching occurrences, excluding null/same-ID candidates. Plan-angle grouping preserves endpoint-to-origin XZ bearings and first-seed circular grouping, with midpoint-radius ordering and independently owned editable `BeamGroup<double>` lists. See [contracts and examples](https://github.com/StruXllence/StaadPro.Interop/blob/main/docs/Geometry-Diagnostics.md).

Added `GetEdges(plate)` returning named `PlateEdge` endpoint pairs, and `IsConnectedToPlate(plate, targetPlate, scope = SharedEdge)` with full XML IntelliSense. Edge extraction preserves retained winding and closure in owned read-only storage. Shared-edge connectivity checks consecutive pairs on both loops in either direction; two shared corners forming a diagonal do not qualify. `AnyCorner` supports corner-only neighbours. See [plate edge/connectivity examples](https://github.com/StruXllence/StaadPro.Interop/blob/main/docs/Geometry-Diagnostics.md).

Added `MemberExtensions.IsConnectedToNode(member, node, scope = EndpointsOnly)` and `PlateExtensions.GetCornerLoop(plate)` with complete XML IntelliSense. Member checks use either endpoints or exactly the supplied `Nodes` collection. Plate loops preserve first distinct supplied corners in A-B-C-D order without a repeated closing corner, with read-only membership and original node references. See [geometry contracts and examples](https://github.com/StruXllence/StaadPro.Interop/blob/main/docs/Geometry-Diagnostics.md).

Added `BeamExtensions.IsConnectedToNode(beam, node)` and `MemberExtensions.IsConnectedToMember(member, targetMember, scope = EndpointsOnly)` with expanded XML IntelliSense. Scope selects either both members' endpoints or exactly their supplied `Nodes` collections. Selected incidence is fully qualified before comparing node IDs and the existing Euclidean tolerance. See [connectivity contracts](https://github.com/StruXllence/StaadPro.Interop/blob/main/docs/Geometry-Diagnostics.md).

Ported `GetBeamsWithinRadius(...)`, `GetBoundingBox<T>(...)` and `IsConnectedToBeam(...)` with comprehensive XML IntelliSense. Radial filtering uses XZ beam midpoints, bounds return an immutable named class, and beam connectivity requires a matching endpoint ID plus the existing distance tolerance. Inputs are validated before a detached result is returned; no native changes occur. See [geometry filtering, bounds and connectivity](https://github.com/StruXllence/StaadPro.Interop/blob/main/docs/Geometry-Diagnostics.md).

Added `GetClosestNode(nodesCollection, referenceNode)` and `GetClosestBeam(beamsCollection, referenceNode)` with complete XML IntelliSense. Searches consume the input once, retain first-seen ties and compare scaled 3D distances. Node exclusion preserves ID-plus-distance equality; beam distance uses the nearer endpoint. See [nearest geometry examples](https://github.com/StruXllence/StaadPro.Interop/blob/main/docs/Geometry-Diagnostics.md).

Added `GetDuplicateNodes(...)` and `GetZeroLengthBeams(...)` for detached geometry diagnostics, with finite input validation, owned materialized collections and explicit identity/tolerance behavior. See [geometry diagnostics](https://github.com/StruXllence/StaadPro.Interop/blob/main/docs/Geometry-Diagnostics.md) for examples and contracts. These helpers perform no native model changes.

Ported `HidePlate(Plate plate)`, `HideSolid(int solidId)` and `HideSurface(int surfaceId)` with complete matching interface/adapter XML IntelliSense. Each validates a positive analytical entity ID and dispatches once to the captured native View. Plate identity is captured without reading geometry. The package now exposes 24 typed View operations. See [entity visibility contracts](https://github.com/StruXllence/StaadPro.Interop/blob/main/docs/View-Entity-Visibility.md) for completion receipts, inputs and native limits.

Published by **StruXllence**. **Copyright (c) 2026 StruXllence**. The package targets Windows and .NET Framework 4.8.1 (net481) and requires a licensed STAAD.Pro installation for live use. This preview is prepared for manual upload; public availability of this exact version is not confirmed. Native COM/UI and engineering qualification remain separate from managed tests.

## Earlier incremental rotations — 1.0.0-preview.20261104

The earlier increment passed **7,137 offline tests**, including **201 View cases**. Its installed-package verification covered **1,306 XML examples**, including twenty-six View examples. The package checks cover metadata, notices and a fresh consumer restore/build/runtime workflow; live native COM/UI qualification remains separate.

StaadPro.Interop provides typed adapters and managed models for working with an existing Bentley STAAD.Pro application through OpenSTAAD. It includes geometry and load operations, model information and lifecycle functions, application-window controls, and native analysis requests.

## Completed Table ports and View foundation

Color APIs support explicit RGB, WPF Color, text, packed/byte ARGB and HSL with documented fallback/clamping/alpha behavior. Typed Table alignment preserves native 0/1/2 and 0/4/8 maps; table-wide sizing complements per-cell sizing. ShowFront requests graphical orientation and returns unacknowledged void completion. See [exact contracts and examples](https://github.com/StruXllence/StaadPro.Interop/blob/main/docs/Table-Completion-and-View-Foundations.md).

## Earlier Table lifecycle and formatting

New operations cover report/table deletion, renaming, growing existing table dimensions, saving all reports or one table, and cell bold/italic/underline/font sizing. All reuse the existing named receipt; normal native void completion does not certify success or persistence. See [exact contracts and examples](https://github.com/StruXllence/StaadPro.Interop/blob/main/docs/Table-Lifecycle-and-Formatting.md).

## Earlier Table report foundations

## View orientations

Use fixed `ShowFront()`, `ShowBack()`, `ShowLeft()`, `ShowRight()`, `ShowPlan()`, `ShowBottom()` or `ShowIsometric()` directions, or incremental `RotateUp(angleDegrees)`, `RotateDown(angle)`, `RotateLeft(angle)`, `RotateRight(angle)`, `SpinLeft(angle)` and `SpinRight(angle)` requests. All use the captured View and existing named completion receipt. See [orientation usage](https://github.com/StruXllence/StaadPro.Interop/blob/main/docs/View-Orientations.md) and [incremental rotations](https://github.com/StruXllence/StaadPro.Interop/blob/main/docs/View-Incremental-Rotations.md).

## Commands, Support and Design foundations

The Release build passed with zero warnings/errors and 6,448 offline tests, including 412 new Commands/Support/Design cases. Exact-archive verification compiled and executed 1,222 selected XML examples through an isolated PackageReference consumer, including all 34 Commands, 54 Support and twenty Design examples. These checks validate managed behavior and packaging; live native COM and engineering qualification remain pending.

The symbol-only icon and logo-with-text banner are included as the adopted package branding. See [analysis commands and Support](https://github.com/StruXllence/StaadPro.Interop/blob/main/docs/Commands-and-Support.md) and [Design contracts](https://github.com/StruXllence/StaadPro.Interop/blob/main/docs/Design.md) for exact signatures, differences between related methods, status handling, units and examples.

## Earlier preview - plate-corner workflows

## Output steel-design blocks and Support foundations

Added **six common documented Output routes and 19 Support functions**, with full matching interface/adapter IntelliSense and named managed results. Output now covers **46/60** documented native names; Support covers **19/35**. The new Output overloads distinguish named parameter blocks from member-only observations, while Support preserves node/definition identity, six-DOF states and operation-specific native acknowledgements.

See [the Output steel-block guide](https://github.com/StruXllence/StaadPro.Interop/blob/main/docs/Output-Steel-Design-Blocks.md) and [the Support foundation guide](https://github.com/StruXllence/StaadPro.Interop/blob/main/docs/Support-Foundations.md).

## Earlier Property completion and extended Output queries

Output provides **40 typed queries**, including 25 new member, solid, support-node, modal, buckling, nonlinear, steel-design and time-history operations. Twenty documented native routes remain. See [the extended Output guide](https://github.com/StruXllence/StaadPro.Interop/blob/main/docs/Output-Extended-Queries.md) for exact signatures and comparisons between similar methods.

The Release build and **5,353 offline tests** passed. The isolated package consumer verifies **1,002 selected XML examples**, including all 80 Output and 510 Property/capability examples. Native COM and engineering qualification remain pending. The library targets Windows/.NET Framework 4.8.1.

Public UPT methods use PascalCase, including `AddUptPropertyChannel`. See [the API rename table](https://github.com/StruXllence/StaadPro.Interop/blob/main/docs/Property-API-Naming.md) when upgrading from an earlier preview.

## Earlier Property UPT profiles and catalogs

Passed a Release build with zero warnings/errors and **4,959 offline tests**, including **213 new cases**. The isolated PackageReference-only consumer verifies **892 selected XML examples**, including all **450 Property interface/adapter examples**. All sixteen new mutation examples are guarded; separate managed checks execute all 25 routes through both public surfaces. Live native mutation and engineering qualification remain pending.

Added immutable explicit UPT profile inputs, owned table inventories and named section observations. Kept operation-specific creation, insertion and removal outcomes separate. Qualified ten UPT read-back layouts and the database-folder setter against exact acquired native profiles 24.00.02.354, 25.00.01.424 and 26.00.00.340, retaining their different folder return conventions. Unreviewed builds cannot select those policies. Table numbers, native indices and property references retain separate identities.

## Earlier Property fireproofing, attributes and UPT

Passed a Release build with zero warnings/errors and **4,746 offline tests**, including **195 new cases**. The fresh PackageReference-only consumer verifies **842 selected XML examples**, including all **400 Property interface/adapter examples**. All ten new mutation examples are guarded; dedicated managed checks execute every new route through both public surfaces. Verification does not change a live STAAD model.

Added immutable exact attribute pairs, pair/domain/ID snapshots, fireproofing correspondence and explicit UPT value packets. Bounded count/list/count reads validate written outputs and preserve order/duplicates. By-reference native attribute-value rewriting returns StateChanged rather than data for the wrong pair. UPT table IDs, insertion acknowledgements and duplicate outcomes remain separate; scalar and unequal wide-flange routes retain their different return conventions.

## Earlier Property section and specification creation

Passed a Release build with zero warnings/errors and **4,551 offline tests**, including **205 new cases**. The isolated PackageReference-only consumer verifies **792 selected XML examples**, including all **350 Property interface/adapter examples**. All 24 new mutation examples are guarded; dedicated managed checks exercise every new route through both the interface and concrete adapter. Verification does not change a live STAAD model.

Added explicit immutable wide-flange, general-prismatic, six-DOF and partial-factor inputs. Retained separate created section IDs, created specification IDs, zero-based fireproofing indices and zero-success status-only results. The void unique-ID setter returns InvokedWithoutAcknowledgement with no fabricated native code and IsSuccessful false; its exact requested property/text identity is retained. Existing signatures, current-material context and native units remain explicit.

## Earlier Property material and member-section completion

Passed a Release build with zero warnings/errors and **4,346 offline tests**, including **190 new cases**. The isolated PackageReference-only consumer verifies **742 selected XML examples**, including all **300 Property interface/adapter examples**. New mutation examples are guarded; separate installed-consumer checks invoke all eleven mutations against managed stand-ins. No live STAAD model is changed by verification.

Added immutable explicit material definitions/strengths, orthotropic constants, member-section snapshots and named mutation acknowledgements retaining actual native evidence and requested identity. Kept Steel Fu/Fy/Rt/Ry order distinct from extended Fy/Fu/Ry/Rt/Fcu order. Added bounded owned solid-ID snapshots, strict written-output checks, operation-specific absence/error handling and native-version section layouts. A successful mutation acknowledgement does not prove design-strength persistence, save the model or provide rollback.

## Primary and combination case enumeration fix

Fixed primary and combination discovery rejecting the Boolean `true` returned by `GetPrimaryLoadCaseNumbers` and `GetLoadCombinationCaseNumbers` on the observed STAAD.Pro 25.00.00.539 build. The shared readers now handle Boolean success separately from integral case counts, validate the complete ID array, and recheck the separate count before reading metadata. Boolean false, malformed arrays and inconsistent counts still fail. Reference discovery retains its integer-count contract; matching documented integral primary/combination results remain supported.

## Earlier Property section completion

Incorporated the remaining **19 active, nonconflicting main-interface Property signatures** plus **six documented native observations**, bringing Property to **125 overloads**. Added section creation/assignment/read-back, explicit material conversion and captured-receiver uniform-thickness creation with complete matching XML IntelliSense. Native dispatch now covers **66 of 204 active candidates**, with **138 remaining**. See [all 25 section-completion contracts](https://github.com/StruXllence/StaadPro.Interop/blob/main/docs/Property-Sections.md).

Passed a Release build with zero warnings/errors and **4,746 offline tests**, including **225 new section cases**. Isolated installed-package verification compiles and runs **692 selected XML examples**, including all **250 Property interface/adapter examples**, through a fresh PackageReference-only consumer and cache. Managed stand-ins perform every example; live native mutation and engineering qualification remain pending.

Selected 47 reviewed section layouts per acquired STAAD 24/25/26 profile, reading native version metadata after wrapper initialization and rechecking it before returning packets. Corrected the source assignment query filtering an unfilled array. Preserved native identity/field labels, explicit missing-area results, operation-specific statuses and partial-write behavior. Added small section selection/data models without importing private catalog/UI services; reused the repository UnitsConverter for explicit modulus/weight-density conversion. Existing typed-return collisions (eight) and obsolete source overloads (five) remain documented exceptions.

## Root and wrapper APIs included in this preview

This preview includes the completed wrapper bindings and five additional Root metadata queries: `GetApplicationVersion`, `GetBaseUnit`, `GetInputUnitForForce`, `GetInputUnitForLength` and `GetMainWindowHandle`. Wrapper-owned queries reuse cached properties; directly constructed Root adapters read their supplied native root. Unit compatibility fallbacks are documented in the [Root guide](https://github.com/StruXllence/StaadPro.Interop/blob/main/docs/Root.md). Analysis observation also adds `IsAnalyzing()`, `GetAnalysisStatus()` and `GetAnalysisStatus(string modelFilePath)`, with immutable status/data results and no public reference outputs. See the [analysis guide](https://github.com/StruXllence/StaadPro.Interop/blob/main/docs/Root-Analysis-Queries.md). Common reads also add `GetStaadFileFolder()`, `IsPhysicalModel()` and `GetErrorMessage()`; see the [common query guide](https://github.com/StruXllence/StaadPro.Interop/blob/main/docs/Root-Common-Queries.md). This preview also includes `GetProcessId()`, `SetInputUnitForLength(LengthInputUnit)`, `SetInputUnitForForce(ForceInputUnit)`, `SetShortJobInfo(...)` (including a ShortJobInfo overload) and `UpdateStructure()`. See the [model setup guide](https://github.com/StruXllence/StaadPro.Interop/blob/main/docs/Root-Model-Setup.md). These APIs are included in this preview archive.

Additional operations are `SetFullJobInfo(...)`, `Analyze()`, `AnalyzeModel(AnalysisEngine)`, `GetConnectedProjectInfo()` and `SetConnectedProjectInfo(...)`, including a project-data overload. They provide literal native date text, a typed analysis-engine selector and immutable project query results with no public reference outputs. Complete contracts, examples and native-validation limits are in the [job/analysis/project guide](https://github.com/StruXllence/StaadPro.Interop/blob/main/docs/Root-Job-Analysis-Projects.md). These APIs are included in this preview archive.

This preview also adds `NewSTAADFile(...)`, `CreateNamedView(string, int)`, `SaveNamedView(string)`, `GetProcessHandle()` and explicit `Quit()`. These include guarded native creation, inspected view signatures and opaque handle data. See the [creation/view/lifecycle guide](https://github.com/StruXllence/StaadPro.Interop/blob/main/docs/Root-Creation-Views-and-Lifecycle.md) for concurrency limits, native signature discrepancies, raw flag policy and shutdown behavior. These APIs are included in this preview archive.

## Geometry, Load and Output included in this preview

Geometry exposes all **143 reference overloads**, including IdentifyPhysicalMembers, GroupPhysicalMembersByPlanAngle and GeneratePolygonMesh. See the [Geometry guide](https://github.com/StruXllence/StaadPro.Interop/blob/main/docs/Geometry.md) for connectivity, managed grouping, mesh quality and native mutation contracts.

## Requirements

- Windows and **.NET Framework 4.8.1** (`net481`).
- A separately installed and licensed STAAD.Pro application exposing the OpenSTAAD functions used by your application.
- OpenSTAAD access from an appropriate STA thread, with shared model and application operations coordinated by the caller.

The package does not include Bentley executables, COM binaries or a STAAD.Pro licence. This distribution targets .NET Framework; it does not contain a .NET 8 library asset.

## Installation

After this version is available on NuGet.org:

```powershell
dotnet add package StaadPro.Interop --version 1.0.0-preview.20261128
```
For a .NET Framework project, the package reference is:

```xml
<PropertyGroup>
  <TargetFramework>net481</TargetFramework>
</PropertyGroup>
<ItemGroup>
  <PackageReference Include="StaadPro.Interop" Version="1.0.0-preview.20261128" />
</ItemGroup>
```
Enable prerelease packages when searching in Visual Studio. The [installation guide](https://github.com/StruXllence/StaadPro.Interop/blob/main/docs/Installation.md) covers runtime requirements, public and local feeds, and consumer setup.

## Connect to an open model

Use a synchronous STA entry point for a console application. This example attaches to an available running session and reads the model path:

```csharp
using System;
using StaadPro.Interop.Services;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        using (var wrapper = OpenStaadWrapperProvider.GetRunning())
        {
            if (wrapper == null)
            {
                Console.WriteLine("No usable running OpenSTAAD session was found.");
                return;
            }

            Console.WriteLine(wrapper.Root.GetStaadFileFullPath());
            // Use wrapper.Geometry, wrapper.Load and wrapper.Root on this same thread.
        }
    }
}
```
Keep acquisition and subsequent COM operations on the appropriate session thread. The application is responsible for coordinating concurrent access, native dialogs and model changes.

## Geometry and connectivity

`IOSGeometry` and `OSGeometryAdapter` expose typed geometry operations. Managed connectivity grouping is available through `SegregateBeamsByConnectivity`, `SegregateMembersByConnectivity` and `SegregatePlatesByConnectivity`, including the standalone `ConnectivityService`.

Extended root-bound operations include `CreateMultipleNodesExtended`, `CreateMultipleBeamsExtended` and `MergeNodesExtended`. These operations use the supplied root session; mutations can have partial native effects and do not automatically refresh managed geometry or roll back earlier changes.

See the [geometry guide](https://github.com/StruXllence/StaadPro.Interop/blob/main/docs/Geometry.md) for signatures, connectivity rules and mutation contracts.

## Load assignments and metadata

Assignment readers include:

- `GetNodalLoads`
- `GetMemberUniformlyDistributedLoads`
- `GetMemberConcentratedLoads`
- `GetMemberConcentratedMoments`
- `GetMemberLinearVaryingLoads`
- `GetMemberTrapezoidalLoads`
- `GetMemberUniformMoments`

Returned values preserve native signs and units. Assignment reads can activate the selected case; they are distinct from analysis-result retrieval.

Case discovery and filtering include `GetAllPrimaryLoadCases`, `GetAllReferenceLoadCases`, `GetAllLoadCombinationCases`, `GetRepeatLoadCases`, `GetAllLoadCasesOfType`, `GetLoadCase`, `GetLoadCases`, `GetLoadCasesFromIds` and `GetLoadCasesOfType`. Direct and collection lookups include `GetPrimaryLoadCaseFromId`, `GetReferenceLoadCaseFromId`, `GetLoadCombinationCaseFromId`, `GetPrimaryLoadCasesFromIds`, `GetReferenceLoadCasesFromIds` and `GetLoadCombinationCasesFromIds`.

Envelope queries include `GetEnvelopeIds`, `GetLoadEnvelopeDetail` and `GetAllLoadEnvelopeDetails`. Combination definition read-back is available through `GetLoadCombination`.

Assignment and creation operations include `AddReferenceLoadsTo`, `AddRepeatLoadsTo`, `AddAxialThermalGradientTo`, `AddSelfweightInXYZTo`, `CreateLoadCombinations`, `CreateLoadEnvelopExt` and `CreateLoadList`. Grouped operations include `GetMemberLoadsGroupedByLoadCase`, `GetNodalLoadsGroupedByLoadCase`, `AddMemberLoadsFromGroupsByLoadCase` and `AddNodalLoadsFromGroupsByLoadCase`.

Managed member-load population is available through `PopulateMemberConcentratedLoads`, `PopulateMemberConcentratedMoments`, `PopulateMemberLinearVaryingLoads`, `PopulateMemberTrapezoidalLoads` and `PopulateMemberLoads`. Individual-family population and full refresh have different append/replacement behavior; see their XML documentation before combining calls.

Additional included operations are:

| Area | Managed operations |
| --- | --- |
| Population | `PopulateMemberUniformlyDistributedLoads`, `PopulateMemberUniformMoments`, `PopulateNodalLoads` |
| Wind definitions and profiles | `AddWindDefinition`, `AddWindExposure`, `AddWindIntensity`, `DeleteWindDefinition`, `DeleteAllWindDefinitions`, `AddWindDefinitionASCE7Parameters`, `ComputeWallWindPressureProfile`, `ComputeWallWindPressureProfileASCE72016` |
| Seismic definitions and weights | `AddSeismicDefinition`, `ModifySeismicDefinitionParam`, `ModifySeismicDefinitionParams`, `AddSeismicDefSelfWeight`, `AddSeismicDefJointWeight`, `AddSeismicDefMemberWeight`, `AddSeismicDefMemberUniformWeight`, `AddSeismicDefMemberConcentratedWeight`, `AddSeismicDefWallArea` |
| Response spectra | `AddRSLoad`, `AddResponseSpectrumLoadEx` |
| Direct analysis | `AddDirectAnalysisDefinitionParameter`, `AddDirectAnalysisFlexParameter`, `AddDirectAnalysisAxialParameter`, `DeleteDirectAnalysisDefinitionParameter`, `DeleteDirectAnalysisDefinition` |

The [load-query guide](https://github.com/StruXllence/StaadPro.Interop/blob/main/docs/Load-Queries.md) and [load-operation guide](https://github.com/StruXllence/StaadPro.Interop/blob/main/docs/Load.md) explain native mappings, activation, validation, units and partial-failure behavior.

## Model and application operations

`OpenStaadWrapper.Root` exposes `IOSRoot`, implemented by `OSRootAdapter`.

| Area | Functions |
| --- | --- |
| Filenames and model identity | `GetStaadFileFullPath`, `GetStaadFileNameOnly`, `GetAnlFileFullPath`, `GetOpenedStaadFilename`, `IsTarget`, `ModelIsOpen` |
| Job information and units | `GetShortJobInfo`, `GetFullJobInfo`, `SetInputUnitsEx` |
| Saving and closing | `SetSilentMode`, `SaveModel`, `CloseSTAADFile`, `CloseCurrentStaadModel`, `CloseWriteReopenStaadFile` |
| Opening and model waiting | `OpenSTAADFile`, `OpenStaadModel`, `AwaitModelHandle` |
| Application presentation | `ShowApplication`, `MinimizeStaadMainWindow`, `MaximizeStaadMainWindow`, `RestoreStaadMainWindow` |
| Analysis | `AnalyzeEx`, with `SilentMode`, `HiddenMode`, `WaitAnalysisMode` and `AnalysisResult` |

Filename identity does not establish geometry/load readiness. Saving, closing, opening, rewriting and analysis are explicit native mutations that can affect unsaved work, files or captured interfaces. Window requests do not independently verify final state or focus. Inspect analysis status explicitly; asynchronous return mode does not mean analysis has completed.

See the [root guide](https://github.com/StruXllence/StaadPro.Interop/blob/main/docs/Root.md) and [API reference](https://github.com/StruXllence/StaadPro.Interop/blob/main/docs/API-Reference.md) for complete contracts.

## IntelliSense and included documentation

Expanded XML IntelliSense is included for **143 Geometry overloads, 171 Load overloads, 249 common Property overloads plus five optional catalog queries, 110 Output overloads, 27 Support operations, seventeen Commands methods, ten Design methods, twenty-nine Table methods, twenty-one View methods, 53 common Root operations and three optional Root operation interfaces**, on both interfaces and concrete adapters. Keep `StaadPro.Interop.xml` beside the library when distributing assemblies outside normal NuGet restore.

The archive includes Property, Geometry, Load, Output and Root guides, installation and publishing instructions, and the release history.

## Preview validation and operating limits

The current source passed **7,192 offline tests**. Installed-package verification checks **1,316 selected XML example blocks**, including thirty-six View and fifty-eight Table examples, compiles a fresh consumer and exercises managed native stand-ins; mutation and window-changing documentation examples are guarded off. These checks do not certify every native COM response, STAAD release or engineering-unit convention.

Silent and hidden options do not guarantee unattended operation. Synchronous native calls can block beyond logical polling budgets, and mutations can leave partial effects when a later step fails. Validate native behavior against controlled models and the STAAD versions used by your application before production deployment.

## Licence

Apache License 2.0. Copyright (c) 2026 StruXllence. Original author: Srinivasa Rao Masanam. The archive includes LICENSE, NOTICE.md and the retained MIT-origin notice. Bentley STAAD.Pro is a separate product.

For package submission and availability checks, see the [publishing guide](https://github.com/StruXllence/StaadPro.Interop/blob/main/docs/Publishing.md).

## Independence and trademarks

StaadPro.Interop is an independent open-source project published by StruXllence. Neither this project, its repository, nor its NuGet packages are affiliated with, sponsored by, endorsed by, or officially supported by Bentley Systems, Incorporated or its subsidiaries or affiliates.

Bentley, STAAD, STAAD.Pro and OpenSTAAD are trademarks or registered trademarks of Bentley Systems, Incorporated and/or its subsidiaries or affiliates. All other product names and trademarks belong to their respective owners. These names are used solely to identify the products with which this library interoperates; no trademark rights or endorsement are claimed.

Apache License 2.0 applies to the current project distribution; the retained notice for MIT-origin code is in LICENSES/MIT-legacy.txt. It does not grant a license to Bentley software or trademarks. A separately licensed STAAD.Pro installation is required for live use; no Bentley proprietary binaries are included.
