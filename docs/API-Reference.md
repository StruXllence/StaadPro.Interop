# StaadPro.Interop API Reference

Passed **7,137 offline tests**, including **201 View cases** and **116 additional cases** in this increment. Installed-package verification compiles and executes **1,306 selected XML examples**, including all **26 View interface/adapter examples**, through a fresh PackageReference consumer. Live native COM/UI behavior remains separate qualification work.

**Design checkpoint, 27 October 2026:** ten common methods reach five native names. See [the complete Design contract guide](Design.md). Current source validation passed 6,448 offline tests and all 70 new Commands/Support/Design XML examples. These additions are included in preview 1.0.0-preview.20261027.1.

**Commands/Support checkpoint:** IOSCommands now exposes 17 analysis-command methods and IOSSupport covers 27/35 native routes. See [the 25 new contracts](Commands-and-Support.md). These configure model commands or explicit support mutations; Commands calls do not execute the solver.

## Current implementation - plate-corner workflows

## Managed Output workflows - current source

Added governing-case resolution, per-item batch outcomes, staged caller utilization caches, native global offset-transfer audit packets, geometry bases, plate-center composites, ordered static checks and opposite support actions. All new APIs have matching XML documentation and examples; no public ref/out, tuples, partial adapters or thread-pool COM calls were introduced.

## `OpenStaadWrapperProvider` Members

- `Get(string fileFullPath = null)`: With a path, reuses that model if already open; otherwise opens it once (or creates a blank model when the file is missing). Polls every 250 ms for up to two minutes for the matching filename and available Geometry/Load interfaces and count calls. Returns null on failure/timeout with Trace diagnostics. Without a path, attaches to a running session or starts STAAD. Call on an STA thread.
- `GetRunning()`: Attaches to an existing active STAAD session without launching or modifying model state.

## `OpenStaadWrapper` Members

Construction/refresh read native metadata on the session’s COM thread. Five typed Root metadata queries now reuse wrapper snapshots. Common live analysis observation is also implemented. Root profile selection, other pending operations and optional capabilities remain separate work.

## `IOSGeometry` Members

- `CreateMultipleNodesExtended(IOSRoot openStaad, IEnumerable<Node> nodeCollection)`: Creates explicit node IDs using an XYZ coordinate matrix in model input units.
- `CreateMultipleBeamsExtended(IOSRoot openStaad, IEnumerable<Beam> beamCollection)`: Creates explicit beam IDs in order through synchronous native `CreateBeam` calls; endpoints must exist.
- `MergeNodesExtended(IOSRoot openStaad, Node retain, IEnumerable<Node> nodesToMerge)`: Merges existing IDs, requiring the retained ID in the collection. See [Geometry.md](Geometry.md) for validation and partial-failure behavior.
- `IsZUp()`: Returns whether the model uses global Z upwards.
- `GetGlobalVerticalAxis()`: Returns `GlobalVerticalAxis.Y` or `GlobalVerticalAxis.Z`.
- `CreateNode(Node)`: Creates a single node.
- `CreateBeam(Beam)`: Creates a single beam.
- `CreatePlate(Plate)`: Creates a 3-node or 4-node plate.
- `CreateAnnularParametricSurface(...)`: Creates an annular parametric surface with boundary generation.
- `CreateSolidCircularParametricSurface(...)`: Creates a circular parametric surface.
- `AddDensityPointToSurface(...)`: Adds mesh refinement density point.
- `AddDensityLineToSurface(...)`: Adds mesh density control line.
- `AddPolygonalRegionToSurface(...)`: Defines polygonal openings or refinements.
- `GetPlatesConnectedAtNode(int nodeId)`: Returns all incident plates for a node.
- `DistributeTorque(...)`: Distributes torsional load as equivalent nodal forces.

## Load expansion contracts for review

## Load expansion

## `IOSLoad` Members

### Completed main-interface reference coverage

All 115 main-interface reference signatures are implemented; source now exposes 171 overloads: 115 reference signatures, five conveniences, 15 additional native inspection queries and 36 Load expansion methods. See [Load inspection](Load-Inspection-Queries.md) for their status/data contracts and activation effects. The final 15 include AddSeismicDefMemberWeight, AddSeismicDefMemberUniformWeight, AddSeismicDefMemberConcentratedWeight, AddSeismicDefWallArea, AddResponseSpectrumLoadEx, AddDirectAnalysisDefinitionParameter, AddDirectAnalysisFlexParameter, AddDirectAnalysisAxialParameter, DeleteDirectAnalysisDefinitionParameter, DeleteDirectAnalysisDefinition, AddRSLoad. See [packet contracts, overload details, model reuse, side effects and native validation limits](Load.md#load-reference-completion). The latest additions are outside the published preview.

### ASCE wind and seismic definitions

### Reference, Repeat and Temperature Assignments

- `AddReferenceLoadsTo(int loadCaseId, Dictionary<int, double> loadCaseNumberFactorPairs)`: Adds factored reference contributions to an existing primary case.
- `AddRepeatLoadsTo(int loadCaseId, IDictionary<int, double> loadCaseNumberFactorPairs)`: Adds factored repeat contributions to an existing primary case.
- `AddAxialThermalGradientTo(int loadCaseId, IEnumerable<int> entities, double axialThermalGradient)`: Synchronously passes the raw thermal input as the first native temperature argument, with transverse arguments zero.
- `AddAxialThermalGradientTo(int loadCaseId, IEnumerable<int> entities, double axialThermalGradient, int nThreads)`: Preserves the reference four-parameter signature, but executes one scalar assignment per entity on the caller's COM thread. `nThreads` must be positive and does not create workers.

Inputs are validated before activation. Empty collections make no native calls; nonempty assignments leave the primary target active, including on native assignment failure. Native assignment return values are not interpreted. See [Load.md](Load.md) for complete contracts, examples and the remaining load inventory.

### Grouped Assignments by Load Case

- `AddMemberLoadsFromGroupsByLoadCase(Dictionary<ILoadCase, List<(MemberLoad Load, List<Beam> Beams)>> grouped)`: Delegates member tuples to `AddMemberLoad`, skipping null load definitions and null/empty beam lists.
- `AddNodalLoadsFromGroupsByLoadCase(Dictionary<ILoadCase, List<(NodalLoad Load, List<Node> Nodes)>> grouped)`: Delegates every nodal tuple to `AddNodalLoad`, which rejects null loads/node lists and filters individual null nodes.

Dictionary keys select the destination case. These functions preserve the existing assignment helpers' native mapping and activation-status handling; they do not convert units, change load metadata, refresh managed collections or supply rollback. Use valid existing primary/reference cases. See [Load.md](Load.md) for empty-entry behavior and partial failures.

### Load Assignment Queries

- `GetNodalLoads(ILoadCase lc, int nId)`: Reads six-component nodal assignments into `List<NodalLoad>`.
- `GetMemberUniformlyDistributedLoads(ILoadCase lc, int mId)`: Reads UDL direction, magnitude, start/end distances, and eccentricity into `List<MemberUniformlyDistributedLoad>`.
- `GetMemberConcentratedLoads(ILoadCase lc, int mId)`: Reads point-force direction, magnitude, position, and eccentricity into `List<MemberConcentratedLoad>`.
- `GetMemberConcentratedMoments(ILoadCase lc, int mId)`: Reads assigned moment axis, magnitude, position, and eccentricity into `List<MemberConcentratedMoment>`.
- `GetMemberLinearVaryingLoads(ILoadCase lc, int mId)`: Reads local-only full-member start/end/middle intensities into `List<MemberLinearVaryingLoad>`, preserving the named fields despite the constructor's different argument order.
- `GetMemberTrapezoidalLoads(ILoadCase lc, int mId)`: Reads local/global/projected directions, endpoint intensities, and loaded-span positions into `List<MemberTrapezoidalLoad>`.
- `GetMemberUniformMoments(ILoadCase lc, int mId)`: Reads uniform moment intensity, span and eccentricity into `List<MemberUniformMoment>`.

These queries activate an existing primary/reference case and leave it active. They preserve record order and raw values without unit conversion, return an empty list for a valid zero count, and throw on failure or malformed data. See [Load-Queries.md](Load-Queries.md) for all field mappings, error contracts, COM/session requirements, examples, and verification steps.

### Load Case Discovery

- `GetAllPrimaryLoadCases()`: Returns a fresh `HashSet<ILoadCase>` with primary IDs, titles and engineering types.
- `GetAllReferenceLoadCases()`: Returns reference metadata using the reference-specific type API.

Discovery does not activate cases. Counts, ID arrays, titles and types are validated; errors throw rather than returning partial sets. HashSet ordering is unspecified. See [Load-Queries.md](Load-Queries.md#discovering-primary-and-reference-load-cases).

### Load Case Lookup

- `GetPrimaryLoadCaseFromId(int lcId)`: Reads a fresh primary `LoadCase` metadata object, returned as `ILoadCase`.
- `GetReferenceLoadCaseFromId(int lcId)`: Reads reference metadata with the reference-specific title/type APIs.
- `GetPrimaryLoadCasesFromIds(IEnumerable<int> loadCasesIds)`: Returns an eager `List<ILoadCase>` in input order, preserving duplicates as independent objects.
- `GetReferenceLoadCasesFromIds(IEnumerable<int> loadCasesIds)`: Uses the same ordered, eager batch contract with reference-specific title/type APIs.

All IDs must be positive. Batch input is enumerated once and completely validated before COM access; empty input returns a fresh empty list. Lookups leave the active case unchanged, do not enumerate cases, and throw on malformed/native-error metadata. See [Load-Queries.md](Load-Queries.md#looking-up-cases-by-id) for ownership, exceptions, examples and verification.

### Load Combination Metadata

- `GetAllLoadCombinationCases()`: Returns a fresh `HashSet<ILoadCase>` with combination IDs/titles, `CaseType=LoadCombination` and `Type=None`.
- `GetLoadCombinationCaseFromId(int lcId)`: Verifies membership in the combination ID list before reading the requested title; absent IDs throw.
- `GetLoadCombinationCasesFromIds(IEnumerable<int> lcIds)`: Returns an eager distinct set exposed as `IEnumerable<ILoadCase>`, validating all input and memberships before titles; duplicates collapse and order is unspecified.

These methods leave the active case unchanged and validate native count/number/title results. Combinations have no engineering type category, so no type API is called. Factors and constituent cases are not loaded. See [Load-Queries.md](Load-Queries.md#discovering-and-looking-up-load-combinations).

### Family Selection and Generic Lookup

- `GetAllLoadCasesOfType(LoadCaseType loadCaseType = LoadCaseType.PrimaryLoad)`: Eagerly returns the selected primary/reference/combination metadata set through `IEnumerable<ILoadCase>`.
- `GetLoadCase(int lcId, LoadCaseType loadCaseType)`: Reads the complete selected family, then returns its matching metadata object or null if the positive ID is absent.

Repeat and undefined families throw `NotSupportedException` before COM; nonpositive generic lookup IDs throw `ArgumentOutOfRangeException`. Native failures throw even when the requested ID is absent or another case fails. Both leave the active case unchanged. See [Load-Queries.md](Load-Queries.md#selecting-a-family-and-finding-a-case).

### Range, ID Filters and Lists

- `GetLoadCases(int fromId, int toId, LoadCaseType loadCaseType)`: Eagerly reads a family and returns cases in the inclusive positive ID range. Equal bounds are allowed; reversed/nonpositive bounds throw before COM.
- `GetLoadCasesFromIds(IEnumerable<int> loadCasesIds, LoadCaseType loadCaseType)`: Snapshots and validates input once, filters a complete family read, collapses duplicate requests and omits missing IDs. Empty input returns a fresh empty list without COM, after family validation.
- `GetLoadCasesOfType(LoadCaseType loadCaseType)`: Returns a concrete, fresh `List<ILoadCase>` for a supported family.

All three preserve active-case state and return eager metadata lists in unspecified order. Primary/reference/combination families are supported; repeat/undefined families throw. Unmatched metadata errors still throw during full-family reads. See [Load-Queries.md](Load-Queries.md#filtering-and-materializing-case-lists).

### Load Case Creation & Titles

- `CreateNewPrimaryLoad(string lcTitle, LoadType loadType)`: Creates a new primary load case with automatic ID assignment.
- `CreateNewPrimaryLoad(int lcId, string lcTitle, LoadType loadType)`: Creates a new primary load case with explicit ID.
- `CreateNewPrimaryLoadEx(LoadCase lc)`: Creates a primary load case from a `LoadCase` entity.
- `CreateNewPrimaryLoadEx2(LoadCase lc)`: Creates a primary load case with explicit ID from a `LoadCase` entity.
- `CreateNewReferenceLoad(int lcId, string lcTitle, LoadType loadType)`: Creates a new reference load case.
- `CreateNewReferenceLoad(LoadCase lc)`: Creates a reference load case from a `LoadCase` entity.
- `GetLoadCaseTitle(int id)`: Retrieves the string title of any load case or combination by ID.
- `SetLoadCaseActive(ILoadCase lc)`: Sets a primary or reference load case as active in the STAAD model.
- `CreateNewLoadCase(ILoadCase lc)`: Creates a primary or reference load case based on `ILoadCase.CaseType`.

### Support Settlement

- `AddSupportSettlement(ILoadCase, int nodeId, SettlementDirection, double mmSettlement)`: Applies support settlement/displacement to a single node by ID.
- `AddSupportSettlement(ILoadCase, Node, SettlementDirection, double mmSettlement)`: Applies support settlement/displacement to a single Node entity.
- `AddSupportSettlement(ILoadCase, IEnumerable<Node>, SettlementDirection, double mmSettlement)`: Applies support settlement/displacement to multiple Node entities.
- `AddSupportSettlement(ILoadCase, IEnumerable<int>, SettlementDirection, double mmSettlement)`: Applies support settlement/displacement to multiple node IDs.

### Nodal Loading

- `AddNodalLoad(ILoadCase lc, Node node, NodalLoad nl)`: Applies a 6-DOF concentrated nodal load to a single node.
- `AddNodalLoad(ILoadCase lc, IEnumerable<Node> nodes, NodalLoad nl)`: Applies a 6-DOF concentrated nodal load across multiple nodes.

### Member Loading

- `AddMemberLoad(ILoadCase lc, Beam beam, MemberLoad ml)`: Applies a member load (uniformly distributed load or concentrated point load) to a single beam element.
- `AddMemberLoad(ILoadCase lc, IEnumerable<Beam> beams, MemberLoad ml)`: Applies a member load to multiple beam elements.

### Plate Pressure Loading

- `AddPlateUniformPressure(ILoadCase lc, int plateId, double pressure, LoadDirection direction = LoadDirection.LocalZ)`: Applies uniform surface pressure to a single plate by plate ID.
- `AddPlateUniformPressure(ILoadCase lc, IEnumerable<int> plateIds, double pressure, LoadDirection direction = LoadDirection.LocalZ)`: Applies uniform surface pressure across multiple plate IDs.
- `AddPlateUniformPressure(ILoadCase lc, IEnumerable<Plate> plates, double pressure, LoadDirection direction = LoadDirection.LocalZ)`: Applies uniform surface pressure across multiple Plate entities.

### Load Envelope Metadata

| Method | Behavior |
| --- | --- |
| `HashSet<int> GetEnvelopeIds()` | Reads distinct positive native envelope IDs; a successful zero count returns a fresh empty set. |
| `ILoadEnvelopeDetail GetLoadEnvelopeDetail(int envId)` | Reads the classification, reported count and independent membership set for a positive envelope ID. |
| `HashSet<ILoadEnvelopeDetail> GetAllLoadEnvelopeDetails()` | Eagerly reads all envelopes after one ID discovery; throws on any failure without returning partial results. |

Each method preserves active-case/load-list state and checks native fetch success **1**, counts, array dimensions, positive IDs and duplicate IDs. Valid zero-member envelopes are retained with an empty set. `LoadEnvelopType` preserves native values 0–6 and the reference spelling. See the [full envelope guide](Load-Queries.md#load-envelope-queries) for examples, errors, ownership and native validation.

### Batch Load Case Creation

- `CreatePrimaryLoadCases(HashSet<ILoadCase> / IEnumerable<ILoadCase>)`: Batch creates primary load cases in the active model.
- `CreateReferenceLoadCases(HashSet<ILoadCase> / IEnumerable<ILoadCase>)`: Batch creates reference load cases in the active model.

### Load Case Clearing

- `ClearPrimaryLoadCase(ILoadCase)`: Clears a single primary load case.
- `ClearPrimaryLoadCase(ILoadCase, bool isReferenceLoad)`: Clears a primary load case with reference load flag.
- `ClearPrimaryLoadCases(IEnumerable<ILoadCase>)`: Clears multiple primary load cases.
- `ClearPrimaryLoadCases(IEnumerable<ILoadCase>, bool isReferenceLoad)`: Clears multiple primary load cases with reference flag.
- `ClearPrimaryLoadCases(IEnumerable<int>)`: Clears primary load cases by IDs.
- `ClearPrimaryLoadCases(IEnumerable<int>, bool isReferenceLoad)`: Clears primary load cases by IDs with reference flag.
- `ClearReferenceLoadCase(ILoadCase)`: Clears a single reference load case.
- `ClearReferenceLoadCases(IEnumerable<ILoadCase>)`: Clears multiple reference load cases.
- `ClearReferenceLoadCases(IEnumerable<int>)`: Clears reference load cases by IDs.

### Self-Weight, Envelope Creation and Load Lists

| Method | Native behavior |
| --- | --- |
| `AddSelfweightInXYZTo(int loadCaseId, LoadCaseType lcType, IEnumerable<int> entities, SelfWeightDirection loadDirection = GlobalY, double factor = -1)` | Checked primary/reference activation followed by one geometry self-weight assignment; empty input skips native access. |
| `CreateLoadEnvelopExt(int envelopId, LoadEnvelopType envelopType, IEnumerable<int> loadCaseList)` | One checked native envelope creation; no activation or read-back. |
| `CreateLoadList(LoadListType loadListType, IEnumerable<int> loadCaseIds)` | Native load-case/envelope-list selection, returning its original integer-zero success status. |

Inputs are snapshotted and validated before native access. The first two mutation APIs use Boolean success; load-list selection uses integer zero. Native existence/suitability and empty-envelope/list behavior remain native decisions. See [Load.md](Load.md) for input, empty-result, failure and state-change contracts.

## Root Adapter

`OpenStaadWrapper.Root` exposes `IOSRoot`, implemented by the existing non-partial `OSRootAdapter`.

The metadata queries below reuse cached wrapper values. RefreshUnits updates unit snapshots; RefreshComObjects updates native version and handle snapshots. Standalone OSRootAdapter bindings read their supplied native root on each call through shared internal readers. Native unit failures can use the documented compatibility fallback; these enums do not certify measured units. Metadata access through an adapter captured from a disposed wrapper throws ObjectDisposedException.

| Metadata query | Return / meaning |
| --- | --- |
| `GetApplicationVersion()` | `ApplicationVersion`: defensive copy of native StaadVersion; null when absent, distinct from the host ApplicationVersion. |
| `GetBaseUnit()` | `BaseUnitSystem`: Unknown for unavailable values; unrecognized integral codes remain inspectable. |
| `GetInputUnitForForce()` | `ForceInputUnit`: cached normalized value; fallback KiloPound for Imperial, KiloNewton otherwise. |
| `GetInputUnitForLength()` | `LengthInputUnit`: cached normalized value; fallback Feet for Imperial, Meter otherwise. |
| `GetMainWindowHandle()` | `IntPtr`: borrowed handle, possibly zero, with the source-family rule and configured wrapper service; no live-window guarantee. |

| Method | Result |
| --- | --- |
| `string GetStaadFileFolder()` | One native directory read; exact text and explicitly written empty output preserved. |
| `bool IsPhysicalModel()` | One live native classification predicate; Boolean or integral 0/1 only. |
| `string GetErrorMessage()` | One explicit exact-text native message read; no promise that reading preserves the server error state. |
| `void SetFullJobInfo(...)` | Thirteen non-null strings in native order; literal date text, no automatic save or date serialization. |
| `void Analyze()` | One native UI-equivalent analysis request; normal return does not establish completion or success. |
| `void AnalyzeModel(AnalysisEngine engine = AnalysisEngine.StaadPro)` | One validated native engine request (-1); verified void contract. |
| `ConnectedProjectResult GetConnectedProjectInfo()` | Native query flag and immutable project data only after success and written string outputs. |
| `bool SetConnectedProjectInfo(string projectId, string projectName)` | Validated nonblank text; exact native assignment flag, without cloud or persistence verification. |
| `bool SetConnectedProjectInfo(ConnectedProjectInfo projectInfo)` | Reuse immutable project data, validate and delegate without another query. |
| `NativeProcessHandleValue GetProcessHandle()` | Opaque immutable UInt64 bit pattern; no ownership, native width or caller-process HANDLE validity implied. |
| `void NewSTAADFile(string filePath, LengthInputUnit lengthUnit, ForceInputUnit forceUnit)` | Validated new .std destination and unnamed session; one native request, with no atomic no-overwrite/completion guarantee. |
| `void CreateNamedView(string viewName, int creationFlag)` | Name validation, required caller-established Int32 flag and inspected two-input void native call. |
| `void SaveNamedView(string viewName)` | Name validation and inspected one-input void native request; no fabricated error output. |
| `void Quit()` | Explicit bound-application shutdown; no automatic save, wrapper disposal or process kill. |
| `void RemoveNamedView(string viewName)` | Inspected two-input native request; zero initializes the by-value error slot without fabricating a status. |
| `void ModifyNamedView(string viewName, IEnumerable<int> entityIds, NamedViewEntityType entityType, int modificationFlag)` | Validated private positive-ID Int32 array; inspected four-input call with a caller-established mode. |
| `MemberForcesResult RetrieveMemberForces(IEnumerable<int> beamIds, IEnumerable<int> loadCaseIds)` | Bound native availability and ordered per-end success/data; source-compatible force records without unit conversion. |
| `uint GetProcessId()` | Same-root positive UInt32 PID query; shared strict validation, no process enumeration or liveness guarantee. |
| `void SetInputUnitForLength(LengthInputUnit lengthUnit)` | Independent length request; codes 0–7 including unnamed feet alias 1; explicit wrapper refresh required for fresh caches. |
| `void SetInputUnitForForce(ForceInputUnit forceUnit)` | Independent validated force request; no automatic length request, save or cache refresh. |
| `void SetShortJobInfo(string jobName, string jobClient, string engineerName)` | Submit three non-null strings in native order; preserve text, no automatic save. |
| `void SetShortJobInfo(ShortJobInfo jobInfo)` | Snapshot Name/Client/Originator from the existing model, validate, then delegate. |
| `void UpdateStructure()` | One explicit native update request; no added save, analysis, refresh or completion verification. |
| `bool IsAnalyzing()` | One live native running-state read; Boolean or integral 0/1 only. False does not indicate analysis success. |
| `AnalysisStatusResult GetAnalysisStatus(string modelFilePath)` | Normalized explicit-path query; immutable native/typed status, success, completion and nullable diagnostic counts/CPU seconds. |
| `AnalysisStatusResult GetAnalysisStatus()` | One current filename read, then delegates; unnamed models throw before status access. |
| `string GetStaadFileFullPath()` | Exact current model path from one void `GetSTAADFile` output read. |
| `string GetStaadFileNameOnly()` | Windows filename including its extension, derived from one path read. |
| `string GetAnlFileFullPath()` | Same directory and model stem with uppercase `.ANL`, derived from one path read. |
| `string GetOpenedStaadFilename()` | Normalized absolute opened-model path from one native read; empty text remains empty. |
| `bool IsTarget(string staadFileFullPath)` | Checks complete normalized path identity with ordinal case-insensitive comparison. |
| `bool ModelIsOpen(string fileFullPath)` | The same full-path identity check; equal basenames in different folders do not match. |
| `ShortJobInfo GetShortJobInfo()` | Fresh name/client/originator metadata; all native BSTR outputs must be written strings. |
| `FullJobInfo GetFullJobInfo()` | Fresh thirteen-field metadata with culture-aware dates and DateTime.MinValue for unspecified dates. |
| `IOSRoot SetInputUnitsEx(LengthInputUnit lengthUnit, ForceInputUnit forceUnit)` | Validates enum values, sends length/force integer codes to native SetInputUnits and returns this adapter. |
| `bool SetSilentMode(SilentMode silentMode)` | Sends validated native code 0/1; true means call completion, false means dispatch threw. The native previous setting is ignored. |
| `IOSRoot SaveModel(SilentMode silentMode)` | Sends validated code 0/1 to the native void save API and returns this adapter; no file-persistence verification. |
| `IOSRoot CloseSTAADFile()` | Requests native model close once and returns this adapter; no explicit save, silent-mode change, application termination or close verification. |

| `IOSRoot OpenSTAADFile(string filePath)` | Validated absolute path to one native open request; returns this adapter; Native dialogs apply; no readiness verification |
| `IOSRoot ShowApplication()` | One native visibility request; returns this adapter; No model or visibility verification |

| `bool AwaitModelHandle(string fileFullPath, double timeoutDurationInSeconds = 60, double sleepIntervalInSeconds = 0.1)` | Poll exact normalized path with monotonic timeout; Blank is a nonmatch; no model opening |
| `bool OpenStaadModel(string staadFileFullPath, int sleepInterval, int maxWaitTime = 600)` | One open request then full-path polling; milliseconds interval, seconds limit; Timeout does not undo native opening |

| `void CloseCurrentStaadModel()` | Read filename; if non-whitespace, enable silent mode and request close; Blank/whitespace skips mutation; no close verification |
| `bool CloseWriteReopenStaadFile(string filePath, List<string> fileContents, int sleepTimeMilliseconds = 200, int maxTimeMilliseconds = 5000)` | Close matching target, await blank/unlocked state, atomically replace then request reopen; False for mismatch, wait expiry or intervening switch; partial effects possible |

| `void MinimizeStaadMainWindow()` | Validate native window/PID then post ShowWindowAsync command 6; No model mutation or final-state verification |
| `void MaximizeStaadMainWindow()` | Validate native window/PID then post ShowWindowAsync command 3; No model mutation or final-state verification |

| `void RestoreStaadMainWindow()` | Validate native window/PID and post ShowWindowAsync command 9; Accepted request does not verify final state |
| `AnalysisResult AnalyzeEx(SilentMode silentMode, HiddenMode hiddenMode, WaitAnalysisMode waitMode)` | Validate/send native silent, hidden, wait codes; preserve integer analysis status; Starts native analysis; wait mode can block |

AnalysisEngine is in `StaadPro.Interop.Enums.Root`. ConnectedProjectResult and ConnectedProjectInfo are in `StaadPro.Interop.Models.Root`; their properties are immutable. A false project query returns null Project, while native success requires validated string outputs. The assignment overload rejects null/blank fields before native access.

NativeProcessHandleValue is in `StaadPro.Interop.Models.Root`. It exposes immutable numeric data, not an owning or verified Windows handle. CreateNamedView/SaveNamedView follow inspected 2025/2026 managed interop declarations; their help pages contain inconsistent error-output arguments. Their void return does not establish success.

Optional access uses `root.Capabilities.GetAdinaExport()`, `GetAnalysisErrorMessages()` and `GetAnalysisWarningMessages()`. Successful acquisitions return IOSRootAdinaExport, IOSRootAnalysisErrorMessages or IOSRootAnalysisWarningMessages respectively. Each exposes its typed operation and status/data result. Unsupported results contain no adapter; exact candidate profiles and metadata checks are not runtime certification. See the [completion and capability guide](Root-Completion-and-Capabilities.md).

## Output result queries

IOSOutput and OSOutputAdapter expose 96 managed overloads covering analysis/buckling availability, nodal displacement, support reactions, local member-end forces, static checks, seven plate-center result families and member design section/ratio observations. Single native observations return OutputQueryResult&lt;T&gt;; managed workflows additionally return named batch, population and coordinate-system results with documented status/ownership rules; inspect IsSuccessful before Data. No public reference outputs or tuples are used.

## Property observations

Existing `MemberEnd` and `Vector3D` types are reused. New named results distinguish native success, missing entities, absent specifications, unavailable members, collection limits and changed counts. Native units, signs and array ordering are preserved. These methods do not assign properties, convert units, run analysis or certify result freshness. The documented-but-metadata-absent plate property reference query reports `Unsupported` when its exact member is unavailable.

## Earlier extended Output and final documented Property cleanup

Implemented **25 additional documented Output routes**, bringing IOSOutput and the single non-partial OSOutputAdapter to **40 typed queries covering 40/60 native names**, with **20 documented routes pending**. Added member/physical-member results, solid stresses, mat/base-pressure snapshots, modal/buckling observations, nonlinear load steps, steel-design observations and time-history queries. See [the extended contracts and comparative usage guide](Output-Extended-Queries.md).

The selected contracts are shared or cosmetic across the installed 2024–2026 references. One internal common reader performs native dispatch and validation; direction-code policies, uncertain member-end layouts, cut/point buffers and incompatible scalar-output transports remain separate work. No new public ref/out, tuples or raw VARIANT packets are exposed. Existing wrapper/provider behavior is unchanged.

Validation passed **5,353 offline tests**, including 165 new Output cases and 32 Property completion cases. The isolated installed-package consumer verifies **1,002 selected XML examples**, including all 80 Output examples and 510 Property/capability examples. Live native marshaling and engineering qualification remain pending.

The subsequent domain increment implements **25 Output routes**, bringing typed Output coverage to **40/60**. Validation passed **5,353 offline tests** with zero failures; package verification covers **1,002 selected XML examples**. These checks use managed native-signature fixtures; live 2024–2026 COM and engineering qualification remain pending.

## Output steel-design blocks and Support foundations

Implemented six further common documented Output routes for named design-parameter-block observations and independent failure-ratio limits. Typed Output coverage is **46/60**, with **14 documented routes pending**. Matching interface/adapter XML, named subset results and exact block identity distinguish the new overloads from the existing member-only APIs. See [the steel-block guide](Output-Steel-Design-Blocks.md).

Release build passed with zero warnings/errors and **5,523 offline tests** (170 new cases). Isolated documentation/package verification covers **1,052 selected XML examples**, including all 92 Output and 38 Support examples. Managed fixtures do not certify live 2024–2026 COM or engineering behavior.
