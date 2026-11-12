# StaadPro.Interop release notes

## 1.0.0-preview.20261112 - 12 November 2026

Ported `GetDuplicateNodes(this IEnumerable<Node> nodesCollection, double tolerance = 0.0001)` and `GetZeroLengthBeams(this IEnumerable<Beam> beamsCollection, double tolerance = 0.0001)` with complete XML IntelliSense and [detached diagnostic examples](Geometry-Diagnostics.md).

Duplicate-node groups use inclusive per-axis comparisons with first-seen seeds, preserving nontransitive grouping. Repeated IDs with identical coordinates retain their first instance; conflicting snapshots are rejected explicitly. Zero-length beams use inclusive Euclidean distance with scaled arithmetic for extreme and subnormal deltas. Both helpers validate finite geometry/tolerances, enumerate once, preserve original references and return owned read-only collections without native changes. Null entries are ignored; missing incidence and enumerator failures remain explicit errors.

Passed a zero-warning Release build and 7,309 tests, including 17 new diagnostics cases. The exact installed NuGet consumer verifies both signatures/defaults and expanded XML, executes both XML examples, compiles the guide and checks result ownership/numeric boundaries. No native model operation or new runtime qualification is involved.

## 1.0.0-preview.20261109 - 9 November 2026

Ported `HidePlate(Plate plate)`, `HideSolid(int solidId)` and `HideSurface(int surfaceId)` with preserved function names, parameter names/types/order and complete matching XML IntelliSense on `IOSView` and `OSViewAdapter`. The package now exposes 24 typed View operations.

Validated positive analytical IDs before one matching native call. `HidePlate` captures only `Plate.Id` once; node fields and geometry are not required or read. Reused the captured View receiver, caller-thread dispatch, per-adapter gate and binding checks before/after native access. Each returns the existing `ViewOperationResult` with explicit unacknowledged void completion, without fabricating a success flag or certifying visible effects. Native errors and post-call lifetime failures propagate without retry or rollback. See [entity visibility contracts](View-Entity-Visibility.md).

Passed **7,292 offline tests**, including **356 View cases** and **52 new regression cases**. An isolated installed NuGet consumer compiled and executed **1,328 XML examples**, including all **48 View examples**, checked the new native-signature routes and validation, and verified expanded XML beside the DLL. No live STAAD graphical mutation or new cross-version COM/UI qualification was performed.

## 1.0.0-preview.20261107 - 7 November 2026

Ported `ShowMembers(IEnumerable<Beam> beams)`, `HideMember(Beam beam)` and `HideMembers(IEnumerable<Beam> beams)`. Preserved source names and parameters while reusing the captured View service and `ViewOperationResult`. Collection methods enumerate once, capture positive IDs in an owned array, preserve order and duplicates, and forward a matching count/array pair. Null inputs, null elements and nonpositive IDs fail before native dispatch. Native void completion remains explicitly unacknowledged.

Added comprehensive matching interface/adapter XML IntelliSense, guarded examples and regression coverage for input snapshots, enumeration failures, binding lifetime, dispatch and native errors. View reaches **21/70 source overloads** and **21/67 reviewed native names**; **142 active source overloads remain** across the reviewed domains. Live native COM/UI qualification remains separate. See [member visibility contracts](View-Member-Visibility.md).

Passed **7,240 offline tests**, including **299 View cases** and **48 additional regression cases** in this increment. A fresh installed NuGet consumer compiled and executed **1,322 XML examples**, including all **42 View examples**, and checked the new native-signature routes, input validation and owned snapshots.

## 1.0.0-preview.20261105 - 5 November 2026 (prepared preview)

Added complete matching XML IntelliSense with comparative zoom/visibility usage, identity requirements, lifetime and error behavior, plus guarded examples. View reaches **18/70 source overloads** and **18/67 native names**. **145 active source overloads remain** across the reviewed domains. Updated the website badge and navigation wording to reflect its live status.

## 1.0.0-preview.20261104 - 4 November 2026 (prepared preview)

All five accept finite signed degrees unchanged, including zero, negative zero and values beyond one revolution. Global X applies to RotateDown, global Y to RotateLeft/RotateRight and global Z to SpinLeft/SpinRight. STAAD defines the screen convention; no guessed clockwise sign or negative-angle equivalence is supplied. Every request uses the captured View receiver and existing adapter gate on the caller thread, preserves native exceptions and returns the established unacknowledged void-completion receipt.

## 1.0.0-preview.20261103 - 3 November 2026 (prepared preview)

RotateUp accepts finite signed degrees, including zero and negative values, without normalization or conversion. NaN and infinity are rejected before native dispatch. All five requests execute once on the caller's established COM apartment and preserve lifetime checks and native errors. Native void completion remains explicitly unacknowledged; a receipt does not certify the visible orientation.

Passed **7,021 offline tests**, including **85 View cases**. Installed-package verification selects **1,296 XML examples**, including sixteen View examples, and checks metadata, packaged documentation and a fresh PackageReference consumer. Native COM/UI and engineering qualification remain separate. Prepared for manual upload; public availability of this new version is not confirmed.

## 1.0.0-preview.20261102.1 - 2 November 2026 (prepared preview)

Updated the README and project documentation. This distribution uses Apache License 2.0, with NOTICE attribution and the retained MIT-origin notice included. Previously published packages retain their original license terms. The package identity, Windows/net481 target and runtime API behavior are unchanged.

The current implementation passed 6,964 offline tests. Installed-package verification covers 1,286 selected XML examples. Published on NuGet.org; the public package and archive metadata were verified on 2 November 2026.

## 1.0.0-preview.20261102 - 2 November 2026 (prepared preview)

Added ShowBack and ShowLeft to IOSView and OSViewAdapter. Both reuse the captured native View binding and the existing named completion receipt, invoke once on the caller's established COM thread, and preserve stale-binding and native-error behavior. Matching XML IntelliSense explains the difference from ShowFront and Root application-window controls, the native viewing convention and unacknowledged void completion.

Author: Srinivasa Rao Masanam. Publisher: StruXllence. Copyright (c) 2026 StruXllence. Licensed under MIT.

Passed **6,964 offline tests**, including **28 View orientation cases**, and verified **1,286 installed-package XML examples**, including six View examples. The package checks cover metadata, notices and a fresh consumer restore/build/runtime workflow; live native COM/UI qualification remains separate. Prepared for manual upload; public availability of this version is not confirmed.

## 1.0.0-preview.20261028.4 - 28 October 2026 (prepared preview)

Updated the README file.

## 1.0.0-preview.20261028.3 - 28 October 2026 (prepared preview)

Reused existing color types, Table result/status types and validation. Ported source alignment code maps as Table-prefixed enums, isolated pure color conversion and View dispatch internally, and added matching comprehensive XML documentation. Preserved ignored-alpha and finite HSL clamping semantics; restricted malformed-color black fallback to parsing so native exceptions never cause another write. Documented void completion, one-based coordinates, native context and qualification limits.

Retained the compact Markdown banner, both original logos, independence/trademark notices, Srinivasa Rao Masanam author and StruXllence publisher/copyright. The package targets .NET Framework 4.8.1 on Windows. No live COM, UI effect, storage or engineering certification is implied by managed tests.

Passed **6,944 offline tests**, including **496 Table/View cases** (164 additional cases in this increment), and **1,282 installed-package XML examples**, including all 58 Table examples and two View examples. Managed fixture workflows cover the current Table routes and ShowFront; live native COM/UI/storage qualification remains separate.

Prepared for manual upload; public NuGet.org availability is not confirmed.

## 1.0.0-preview.20261028.2 - 28 October 2026 (prepared preview)

Reused the existing Table result/status types and common internal service. Added matching comparative XML documentation, positive identity/dimension checks, one-based cell coordinates, finite positive native font sizes, exact rename text, scoped save receipts and explicit unacknowledged void completion. Kept native exception and captured-binding behavior, with no implicit save, rollback or success claim. The documented resize contract supports growth; shrink behavior and storage persistence are not inferred.

Retained the verified 400-pixel Markdown banner, original logos, independence/trademark notices, Srinivasa Rao Masanam author and StruXllence publisher/copyright. The package targets .NET Framework 4.8.1 on Windows. Live COM, native report UI/storage and engineering qualification remain separate acceptance work.

Passed **6,780 offline tests**, including **332 Table cases** (145 additional cases in this increment). Compiled and executed **1,262 installed-package XML examples**, including **forty Table examples**, and exercised the lifecycle/formatting workflow against a managed native-signature fixture.

Prepared for manual upload; public NuGet.org availability is not confirmed.

## 1.0.0-preview.20261028.1 - 28 October 2026 (prepared preview)

Corrected the NuGet README banner to use supported Markdown image syntax and a 400 × 133 PNG rendition. Preserved both original logo assets and the symbol-only package icon. Rebuilt this prepared preview under the same version at the package owner's request; API behavior is unchanged.

Added independence and trademark notices to the repository, NuGet README and packaged NOTICE.md. Preserved the StruXllence publisher identity, Srinivasa Rao Masanam author, original branding assets and compact README banner. The common Table service reuses strict native readers and retains caller-thread, captured-binding and original-exception behavior. Live native COM and report-storage qualification remain pending.

Passed **6,635 offline tests**, including **187 Table cases**. Compiled and executed **1,242 installed-package XML examples**, including all twenty Table examples, plus an isolated Table write/read/save workflow against the exact archive. These checks do not certify live STAAD.Pro behavior or report storage.

Prepared the package for manual upload; NuGet.org publication is not confirmed. Earlier archives are preserved.

## 1.0.0-preview.20261027.2 - 27 October 2026 (prepared preview)

Reduced the logo-with-text banner in the repository and NuGet READMEs to **400 pixels wide**, retaining its original aspect ratio and automatic height (approximately 133 pixels). Preserved both supplied PNG assets and the embedded symbol-only package icon. Updated branding guidance and package verification to retain the compact banner sizing.

Retained the managed APIs and runtime contracts from 1.0.0-preview.20261027.1. Verified the new archive metadata, assets and **1,222 selected XML examples** through an isolated installed-package consumer. Reran and passed all **6,448 offline tests** for this build; live COM and engineering qualification remain pending. Author: Srinivasa Rao Masanam. Company/publisher: StruXllence. Copyright (c) 2026 StruXllence.

Prepared the archive for manual NuGet.org upload; public availability has not been confirmed. Earlier archives are preserved.

## 1.0.0-preview.20261027.1 - 27 October 2026 (prepared preview)

Added complete comparative XML documentation, source-compatible analysis options/design-code vocabulary, immutable Design requests and explicit native acknowledgements. Preserved stable assignment ordering, identified partial completion, caller-thread dispatch, binding checks and original native exceptions. Copied Design parameter rows from privately owned native holders using explicit indexed-property reads.

Adopted the supplied symbol-only PNG as the embedded NuGet icon and the logo with text as the repository/NuGet README banner. Included both original assets and branding guidance. Author: Srinivasa Rao Masanam. Publisher/company: StruXllence. Copyright (c) 2026 StruXllence.

The Release build passed with zero warnings/errors and 6,448 offline tests, including 412 new Commands/Support/Design cases. Exact-archive verification compiled and executed 1,222 selected XML examples through an isolated PackageReference consumer, including all 34 Commands, 54 Support and twenty Design examples. These checks validate managed behavior and packaging; live native COM and engineering qualification remain pending.

Prepared this archive for manual NuGet.org upload; public availability has not been confirmed. Earlier distribution archives are preserved.

## 1.0.0-preview.20261026.4 - 26 October 2026 (prepared preview)

Added owned family-aware keys, request snapshots and indexed outcomes, with explicit failure data access and managed member-coordinate results. Ported the source Boolean/out-vector convenience as a named result. Kept global corner forces distinct from plate-center stresses and member-local projections.

## 1.0.0-preview.20261026.3 - 26 October 2026 (prepared preview)

Prepared the package for Windows/.NET Framework 4.8.1 with the managed Output workflows below, the preceding six steel-design-block queries and nineteen Support operations. The package exposes 71 managed Output overloads covering 46 documented native functions, alongside the existing Geometry, Load, Root, Property and wrapper APIs.

Added governing-case resolution, per-item batch outcomes, staged caller utilization caches, native global offset-transfer audit packets, geometry bases, plate-center composites, ordered static checks and opposite support actions. All new APIs have matching XML documentation and examples; no public ref/out, tuples, partial adapters or thread-pool COM calls were introduced.

Validation passed **5,788 offline tests** and an isolated package consumer compiling/executing **1,102 selected XML examples**, including all 142 Output examples. Live native COM and engineering qualification remain pending. Publisher metadata remains StruXllence, with Srinivasa Rao Masanam as author. The earlier prepared distribution archive remains unchanged.

## 1.0.0-preview.20261026.1 - 26 October 2026 (prepared preview)

Added four documented Property operations for model-wide element-node release cleanup, member fireproofing cleanup and country-qualified profile-name translation. Property now provides **249 common overloads and five optional catalog queries**, covering **195 of 204** active documented native routes. Nine documented contract gates and fifteen supplemental discovery candidates remain.

Added **25 typed Output queries**, bringing coverage to **40 of 60** documented native names. The new APIs cover member and physical-member results, solid stresses, mat influence areas and base pressures, modal and buckling observations, nonlinear load steps, steel-design observations and time-history results. Named immutable payloads preserve native status evidence, units and component meanings, with bounded collections and result-state checks.

All new APIs include matching interface/adapter XML documentation and usage examples explaining their differences from similar operations. Validation passed **5,353 offline tests** and **1,002 selected packaged XML examples** using managed native-signature fixtures. Live COM and engineering qualification remain pending. The target remains Windows/.NET Framework 4.8.1.

See the [Property completion guide](Property-Profile-Names-and-Final-Cleanup.md) and [extended Output guide](Output-Extended-Queries.md) for contracts, examples and limitations.

## 1.0.0-preview.20261024.12 - 24 October 2026 (prepared preview)

Includes the Property catalog and cleanup APIs from the preceding preview: 245 common overloads and five separately acquired catalog queries. See [the Property guide](Property-Catalog-Capabilities-and-Cleanup.md) for operation-specific results and native-version requirements.

Public UPT methods use PascalCase, including `AddUptPropertyChannel`. Consumers upgrading from earlier previews should use [the API rename table](Property-API-Naming.md).

## 1.0.0-preview.20261024.11 - 24 October 2026 (prepared preview)

Passed a Release build with zero warnings/errors and **5,156 offline tests**, including **197 new cases**. The isolated PackageReference-only consumer verifies **944 selected XML examples**: 490 common Property examples plus twelve optional catalog/acquisition examples, alongside the existing groups. Separate managed checks exercise all twenty new mutations and five catalog queries through both public surfaces. Live native mutation and engineering qualification remain pending.

Added a passive version/signature acquisition boundary for the newer catalog group on exact native profiles **25.00.01.424** and **26.00.00.340**. Retained operation-specific zero-success, Boolean-success and explicit no-specification removal outcomes, owned bounded bulk targets, and separate member-end, cable-definition and analytical-node selectors. No native model is modified by verification.

Renamed **17 public UPT methods** to PascalCase, including **AddUptPropertyChannel**, **AddUptPropertyWideFlange** and **CreateUptTable**. This preview requires callers to update those method names. Parameter names, types, ordering, defaults and return types are preserved. Bentley native dispatch names and recorded contract evidence retain their original spelling.

See [all 17 API renames](Property-API-Naming.md).

- Incorporated `RemovePropertyFromPlate`: Remove Thickness Property From the specific Plate.
- Incorporated `RemoveMaterialFromPlate`: Remove Material From the specific Plates.
- Incorporated `RemoveMaterialFromSolid`: Remove Material From the specific Solids.
- Incorporated `DeleteMemberReleaseSpec`: Delete MEMBER RELEASE specification.
- Incorporated `DeleteAllControlDependentRelations`: Delete all control/dependent joint specifications from model.
- Incorporated `RemoveAllElementOffsetSpec`: Remove all element node offset specification from the model.
- Incorporated `RemovePropertyFromBeam`: Remove property from beam.
- Incorporated `DeleteProperty`: Delete property based the on the property ID passed.
- Incorporated `DeleteMemberSpec`: Delete specification based on the specification number passed.
- Incorporated `RemoveMemberReleaseSpecFromBeam`: Removes the member specification from a particular member at the provided location (Start or End)..
- Incorporated `RemoveMemberOffsetSpecFromBeam`: Removes the member offset specification from a particular member at the provided location (Start or End).
- Incorporated `RemoveMemberTrussSpecFromBeam`: Remove member truss specification from beam.
- Incorporated `RemoveMemberInactiveSpecFromBeam`: Remove member inactive specification from beam.
- Incorporated `RemoveMemberTensionSpecFromBeam`: Remove member tension specification from beam.
- Incorporated `RemoveMemberCompressionSpecFromBeam`: Remove member compression specification from beam.
- Incorporated `RemoveMemberIgnoreStiffSpecFromBeam`: Remove member ignore stiff specification from beam.
- Incorporated `RemoveMemberCableSpecFromBeam`: Removes the member cable specification from a particular member at the provided location type (Tension or Length).
- Incorporated `RemoveElementPlaneStressSpecFromPlate`: Remove element plane stress specification from plate.
- Incorporated `RemoveElementIgnoreInplaneRotnSpecFromPlate`: Remove element ignore in plane rotation specification from plate.
- Incorporated `RemoveElementNodeReleaseSpecFromPlate`: Remove element node release specification from plate.
- Incorporated `GetDefaultStandardProfileDBFolder`: Gets standard profile default database folder path.
- Incorporated `IsStandardDatabaseSection`: Checks if the specified section property reference number is from standard section database source or not.
- Incorporated `GetStandardSectionDatabaseName`: Gets standard section database name for the specified section property reference number.
- Incorporated `GetStandardSectionTableName`: Get the table name from the standard section database for the specified standard section property reference number.
- Incorporated `GetStandardSectionName`: Get the section name from the standard section database and table for the specified standard section property reference number.

Prepared `artifacts/release/StaadPro.Interop.1.0.0-preview.20261024.11.nupkg` for manual upload.

## 1.0.0-preview.20261024.10 - 24 October 2026 (prepared preview)

Passed a Release build with zero warnings/errors and **4,959 offline tests**, including **213 new cases**. The isolated PackageReference-only consumer verifies **892 selected XML examples**, including all **450 Property interface/adapter examples**. All sixteen new mutation examples are guarded; separate managed checks execute all 25 routes through both public surfaces. Live native mutation and engineering qualification remain pending.

Added immutable explicit UPT profile inputs, owned table inventories and named section observations. Kept operation-specific creation, insertion and removal outcomes separate. Qualified ten UPT read-back layouts and the database-folder setter against exact acquired native profiles 24.00.02.354, 25.00.01.424 and 26.00.00.340, retaining their different folder return conventions. Unreviewed builds cannot select those policies. Table numbers, native indices and property references retain separate identities.

- Incorporated `AddUPTPropertyCHANNEL`: Adds explicit channel data to an existing native UPT table.
- Incorporated `AddUPTPropertyANGLE`: Adds explicit angle data to an existing native UPT table.
- Incorporated `AddUPTPropertyDOUBLEANGLE`: Adds explicit double angle data to an existing native UPT table.
- Incorporated `AddUPTPropertyTEE`: Adds explicit tee data to an existing native UPT table.
- Incorporated `AddUPTPropertyPIPE`: Adds explicit pipe data to an existing native UPT table.
- Incorporated `AddUPTPropertyTUBE`: Adds explicit tube data to an existing native UPT table.
- Incorporated `AddUPTPropertyGENERAL`: Adds explicit general profile data to an existing native UPT table.
- Incorporated `AddUPTPropertyISECTION`: Adds explicit I-section data to an existing native UPT table.
- Incorporated `AddUPTPropertyPRISMATIC`: Adds explicit prismatic profile data to an existing native UPT table.
- Incorporated `AddUPTPropertyWIDEFLANGECOMPOSITE`: Adds an explicit 12-, 16- or 19-slot composite wide-flange UPT section.
- Incorporated `CreatePropertyFromUPTTable`: Creates an assigned section-property reference from an existing named UPT entry.
- Incorporated `CreatePropertyFromUserTable`: Obtains a positive section-property reference through the native section-name-first user-table route.
- Incorporated `CreateUPTTableEx`: Creates a UPT table using an explicit requested table number and type.
- Incorporated `RemoveUPTTable`: Requests native removal of an explicitly identified UPT table.
- Incorporated `RemovePropertyFromUPTTable`: Requests deletion of an exact named section from a UPT table.
- Incorporated `GetUserProvidedTableCount`: Reads the native user-provided table count.
- Incorporated `GetUserProvidedTableList`: Reads a bounded owned snapshot of native UPT table IDs.
- Incorporated `GetUserProvidedTableSectionCount`: Reads a native table's section count with explicit table identity.
- Incorporated `GetUserProvidedTableSectionType`: Reads the written scalar native UPT section type with count consistency checks.
- Incorporated `GetUserProvidedTableSectionPropertyCount`: Retains the native named-section count with exact requested UPT identity.
- Incorporated `GetUserProvidedTableSectionProperties`: Reads an owned version-qualified native UPT section-value layout.
- Incorporated `GetUserProvidedTableNo`: Reads a table number for an explicitly supplied native UPT inventory index.
- Incorporated `GetShapeCode`: Reads a raw country-specific section shape code with exact catalog identity.
- Incorporated `SetStandardProfileDBFolder`: Requests a standard-profile database folder change under an exact native return-policy profile.
- Incorporated `GetStandardProfileDBFolder`: Reads the exact native standard-profile database folder path.

Prepared `artifacts/release/StaadPro.Interop.1.0.0-preview.20261024.10.nupkg` for manual upload.

## 1.0.0-preview.20261024.8 - 24 October 2026 (prepared preview)

Passed a Release build with zero warnings/errors and **4,746 offline tests**, including **195 new cases**. The fresh PackageReference-only consumer verifies **842 selected XML examples**, including all **400 Property interface/adapter examples**. All ten new mutation examples are guarded; dedicated managed checks execute every new route through both public surfaces. Verification does not change a live STAAD model.

Added immutable exact attribute pairs, pair/domain/ID snapshots, fireproofing correspondence and explicit UPT value packets. Bounded count/list/count reads validate written outputs and preserve order/duplicates. By-reference native attribute-value rewriting returns StateChanged rather than data for the wrong pair. UPT table IDs, insertion acknowledgements and duplicate outcomes remain separate; scalar and unequal wide-flange routes retain their different return conventions.

Prepared `artifacts/release/StaadPro.Interop.1.0.0-preview.20261024.8.nupkg` for manual upload; no NuGet.org upload or registry ownership change is performed.

## 1.0.0-preview.20261024.7 - 24 October 2026 (prepared preview)

Passed a Release build with zero warnings/errors and **4,551 offline tests**, including **205 new cases**. The isolated PackageReference-only consumer verifies **792 selected XML examples**, including all **350 Property interface/adapter examples**. All 24 new mutation examples are guarded; dedicated managed checks exercise every new route through both the interface and concrete adapter. Verification does not change a live STAAD model.

Added explicit immutable wide-flange, general-prismatic, six-DOF and partial-factor inputs. Retained separate created section IDs, created specification IDs, zero-based fireproofing indices and zero-success status-only results. The void unique-ID setter returns InvokedWithoutAcknowledgement with no fabricated native code and IsSuccessful false; its exact requested property/text identity is retained. Existing signatures, current-material context and native units remain explicit.

Prepared `artifacts/release/StaadPro.Interop.1.0.0-preview.20261024.7.nupkg` for manual upload; no NuGet.org upload or registry owner change is performed.

## 1.0.0-preview.20261024.6 - 24 October 2026 (prepared preview)

Passed a Release build with zero warnings/errors and **4,346 offline tests**, including **190 new cases**. The isolated PackageReference-only consumer verifies **742 selected XML examples**, including all **300 Property interface/adapter examples**. New mutation examples are guarded; separate installed-consumer checks invoke all eleven mutations against managed stand-ins. No live STAAD model is changed by verification.

Added immutable explicit material definitions/strengths, orthotropic constants, member-section snapshots and named mutation acknowledgements retaining actual native evidence and requested identity. Kept Steel Fu/Fy/Rt/Ry order distinct from extended Fy/Fu/Ry/Rt/Fcu order. Added bounded owned solid-ID snapshots, strict written-output checks, operation-specific absence/error handling and native-version section layouts. A successful mutation acknowledgement does not prove design-strength persistence, save the model or provide rollback.

Prepared `artifacts/release/StaadPro.Interop.1.0.0-preview.20261024.6.nupkg` for manual upload.

## 1.0.0-preview.20261024.5 - 24 October 2026 (prepared preview)

Incorporated the remaining **19 active, nonconflicting main-interface Property signatures** plus **six documented native observations**, bringing Property to **125 overloads**. Added section creation/assignment/read-back, explicit material conversion and captured-receiver uniform-thickness creation with complete matching XML IntelliSense. Native dispatch now covers **66 of 204 active candidates**, with **138 remaining**. See [all 25 section-completion contracts](Property-Sections.md).

Passed a Release build with zero warnings/errors and **4,156 offline tests**, including **225 new section cases**. Isolated installed-package verification compiles and runs **692 selected XML examples**, including all **250 Property interface/adapter examples**, through a fresh PackageReference-only consumer and cache. Managed stand-ins perform every example; live native mutation and engineering qualification remain pending.

Selected 47 reviewed section layouts per acquired STAAD 24/25/26 profile, reading native version metadata after wrapper initialization and rechecking it before returning packets. Corrected the source assignment query filtering an unfilled array. Preserved native identity/field labels, explicit missing-area results, operation-specific statuses and partial-write behavior. Added small section selection/data models without importing private catalog/UI services; reused the repository UnitsConverter for explicit modulus/weight-density conversion. Existing typed-return collisions (eight) and obsolete source overloads (five) remain documented exceptions.

- Incorporated five section assignment, two create-and-assign, two explicit material conversion, one captured-receiver thickness creation, two section creation and seven section read-back/area/assignment overloads.
- Added the six standalone native observations listed in the section guide; retained raw country/type codes and database record zero/-1 semantics.
- Added reviewed section layout profiles, small table-definition carriers, owned section packets, Beam.SectionProperty and dimensional converter aliases. Kept unsupported layouts/families and missing areas explicit.
- Strengthened inventory dispatch lifetime checks and preserved caller-apartment writes, prevalidated input snapshots and nontransactional failure behavior.
- Updated shipped guides, coverage, package metadata and all interface/adapter XML examples.

Prepared `artifacts/release/StaadPro.Interop.1.0.0-preview.20261024.5.nupkg` for manual upload; no NuGet.org upload or registry ownership change is performed.

## 1.0.0-preview.20261024.4 - 24 October 2026 (prepared preview)

Passed a Release build with zero warnings/errors and **3,931 offline tests**, including **168 new workflow cases**. Isolated installed-package verification compiles and runs **642 selected XML examples**, including all **200 Property interface/adapter examples**, through a fresh PackageReference-only consumer and package cache. Examples run against managed native stand-ins; live mutation and engineering qualification remain pending.

Material creation records native Created/Updated outcomes separately from list IDs; legacy creation retains its explicitly documented E/G x1000 scale. Indexed material collections retain native names and consistent name equality/hash behavior. Release read-back preserves mixed native modes and partial-mode provenance; basic creation refuses unsupported partial-moment packets. Populations stage native reads before changing caller packets, degree overloads retain caller-apartment dispatch, and multi-step writes recheck binding lifetime around each native write.

- Incorporated seven material read/population overloads, four material creation overloads and three material assignment overloads.
- Incorporated four uniform thickness overloads, four offset assignment/pipeline overloads and three member-release population/creation overloads.
- Added MaterialProperty, PlateThickness, MemberReleases, Releases, Springs, MpFactors, ReleaseType and MaterialCreationStatus with XML documentation. Preserved WPF change notifications and documented private catalog/unit-converter omissions.
- Updated package metadata/version, shipped workflow/installation/publishing guides, coverage and installed XML verification. Retained prior Boolean load enumeration fixes and existing qualified observation signatures.

Prepared `artifacts/release/StaadPro.Interop.1.0.0-preview.20261024.4.nupkg` for manual upload. No public upload, registry owner change or live model mutation is performed by this task.

## 1.0.0-preview.20261024.3 - 24 October 2026 (prepared preview)

Passed a Release solution build with zero warnings/errors and **3,763 offline tests**, including **189 new Property management cases**. The isolated installed-package verifier compiles and runs **592 selected XML examples**, including all **150 Property interface/adapter examples**, through a fresh PackageReference-only consumer and cache. Managed stand-ins exercise the examples without changing a native STAAD model. Live creation/assignment and engineering qualification for this increment remain pending.

Prepared `artifacts/release/StaadPro.Interop.1.0.0-preview.20261024.3.nupkg` for manual upload; this task does not publish it to NuGet.org or change registry owners.

## 1.0.0-preview.20261024 - 24 October 2026 (prepared preview)

Fixed primary and combination discovery rejecting the Boolean `true` returned by `GetPrimaryLoadCaseNumbers` and `GetLoadCombinationCaseNumbers` on the observed STAAD.Pro 25.00.00.539 build. The shared readers now handle Boolean success separately from integral case counts, validate the complete ID array, and recheck the separate count before reading metadata. Boolean false, malformed arrays and inconsistent counts still fail. Reference discovery retains its integer-count contract; matching documented integral primary/combination results remain supported.

- Corrected the readers used by `GetLoadCasesFromIds`, `GetAllPrimaryLoadCases`, family/range filters, primary population queries and combination discovery/lookups. Kept the general integer decoder strict and public signatures unchanged.
- Added Boolean-success/failure, 74-primary/230-combination, in-place buffer, malformed-array, changed-count, strict reference-return and integral compatibility regressions. Removed the test expectation that Boolean true is invalid for primary/combination number retrieval.
- Updated matching interface/adapter XML IntelliSense and usage guides. Retained all 50 Property overloads, 171 Load overloads, 143 reference Geometry overloads, 15 typed Output queries and reviewed Root/wrapper APIs.

The net481 archive is prepared for manual upload and has not been published to NuGet.org by this task. No model-opening, case-activation, geometry-editing or save operation is added by the correction.

Post-package validation on STAAD.Pro **25.00.00.539** passed using the DLL extracted from this exact archive: the reported ID filter returned cases 1, 2 and 3, and primary/reference/combination discovery matched the native inventories. The model path, active case, node count and case counts remained unchanged. This additional source documentation records the live result without replacing the prepared archive. See the validation record for scope and limits.

## 1.0.0-preview.20261023.1 - 23 October 2026 (prepared preview)

The net481 archive includes the earlier source-only Property foundation, Load inspection and Load expansion increments recorded below. Verification uses managed stand-ins and an isolated PackageReference consumer restoring the exact local archive; it performs no customer-model mutations. Native COM marshaling, section-layout interpretation and engineering qualification remain pending. This archive is prepared for manual upload and has not been published to NuGet.org.

## 1.0.0-preview.20261023 - 23 October 2026 (prepared preview)

Release verification covers a Release solution build, **2,259 offline tests**, and isolated PackageReference installation with compilation/execution of **340 selected XML examples** against managed stand-ins, including all 30 new Output examples. Package verification checks metadata, target assets, XML documentation and shipped guides. These checks do not invoke a native STAAD model or certify live marshaling, result units or engineering accuracy.

Targets Windows and .NET Framework 4.8.1 (net481). The archive contains no .NET Core/.NET 8 library asset. Live native compatibility and engineering qualification remain pending; 45 documented Output mappings and additional Load scopes remain separate work. This entry describes a locally prepared preview, not a new public NuGet.org publication.

## 1.0.0-preview.20261015 - 15 October 2026 (prepared preview)

Release validation covers 1,547 offline tests, a Release solution build, and isolated installation of the archive with compilation and execution of 217 selected XML examples against managed stand-ins. Mutating examples remain guarded; these checks do not run native STAAD operations. See the [Root completion guide](Root-Completion-and-Capabilities.md) for contracts and remaining validation gates.

Targets Windows and .NET Framework 4.8.1 (net481). This package does not contain a .NET Core library asset. New interface members require updates to custom interface implementations. Existing geometry and load APIs remain included. Live STAAD compatibility, native flag meanings, result-unit conventions and file side effects require validation before a stable release. This entry describes a prepared archive; public availability requires a separate NuGet.org upload and verification.

## 1.0.0-preview.20261014.7 - 14 October 2026 (preview)

This maintenance release improves the package metadata and documentation. The runtime API is unchanged from `1.0.0-preview.20261014.6`.

- Set the package description to **A strongly-typed .NET Framework wrapper for Bentley STAAD.Pro OpenSTAAD**.
- Reorganized the package README into a current product overview, installation instructions, API reference links and operating guidance. Removed internal implementation-history headings and corrected the documented preview version.
- Synchronized the package release notes, README and publishing instructions. Added version checks during packaging so distribution archives require a matching, reviewed documentation entry; automated CI previews receive their actual version in the packaged documents.

The package includes geometry and load operations, model identity and lifecycle functions, application-window controls, and typed native analysis requests. Expanded XML IntelliSense accompanies the library. The existing implementation passed **914 offline tests**; installed-package verification checks **149 XML example blocks**, including managed runtime checks and guarded window examples.

Targets Windows and .NET Framework 4.8.1 (`net481`). Native save/open/close, analysis-engine behavior and compatibility across STAAD releases require further validation before a stable release. This entry describes the prepared archive; public availability depends on NuGet.org acceptance and validation.

## 1.0.0-preview.20261014.6 - 14 October 2026 (prepared)

Added RestoreStaadMainWindow and AnalyzeEx with matching signatures and complete IntelliSense. Restore validates the reported window/PID before posting command 9. Analysis validates three native flags before one call, preserves documented error/in-progress/completion codes and unknown Int32 codes, and rejects unsupported status types or ranges. Added HiddenMode, WaitAnalysisMode and AnalysisResult; no pre-save, extra silent-mode mutation, result-file retrieval, retry, rollback or managed analysis timeout occurs.

Root coverage is **22 of 23 methods**, with **1 pending**. Custom `IOSRoot` implementations must implement the new members. Validated **914 offline tests** and **149 installed-package XML examples**. Live native mutation/version validation remains pending; no public upload is claimed.

## 1.0.0-preview.20261014.5 - 14 October 2026 (prepared)

Added MinimizeStaadMainWindow and MaximizeStaadMainWindow with matching void signatures and complete IntelliSense. Read supported integral native handle/process values, reject malformed or foreign windows and post the correct Windows state request. Accepted posting does not prove the final state or focus; identity checks are snapshots and handle-recycling races remain possible. No model mutation, process launch, retry or rollback occurs.

Root coverage is **20 of 23 methods**, with **3 pending**. Custom `IOSRoot` implementations must implement the new members. Validated **868 offline tests** and **145 installed-package XML examples**. Live native mutation/version validation remains pending; no public upload is claimed.

## 1.0.0-preview.20261014.4 - 14 October 2026 (prepared)

Added CloseCurrentStaadModel and CloseWriteReopenStaadFile with matching signatures and detailed IntelliSense. Conditional close skips explicit blank filenames and propagates errors. Rewrite validates/snapshots inputs, requires the current target, waits for blank identity and released locks, replaces complete UTF-8 contents atomically and avoids reopening over an intervening model. No save, backup, silent-state restoration, rollback or hard native deadline is supplied; false can follow native/file side effects.

Root coverage is **18 of 23 methods**, with **5 pending**. Custom `IOSRoot` implementations must implement the new members. Validated **841 offline tests** and **141 installed-package XML examples**. Live native mutation/version validation remains pending; no public upload is claimed.

## 1.0.0-preview.20261014.3 - 14 October 2026 (prepared)

Added AwaitModelHandle and OpenStaadModel with matching signatures, explicit duration validation and complete IntelliSense. Poll full normalized paths using a monotonic budget, distinguish matching models from similarly named files, check immediately and cap waits to remaining time. Open requests occur once and count toward the documented seconds limit. Native errors propagate; logical timeouts cannot interrupt synchronous COM calls, and a path match does not certify Geometry/Load readiness.

Root coverage is **16 of 23 methods**, with **7 pending**. Custom `IOSRoot` implementations must implement the new members. Validated **808 offline tests** and **137 installed-package XML examples**. Live native mutation/version validation remains pending; no public upload is claimed.

## 1.0.0-preview.20261014.2 - 14 October 2026 (prepared)

Root coverage is **14 of 23 methods**, with **9 pending**. Custom `IOSRoot` implementations must implement the new members. Validated **774 offline tests** and **133 installed-package XML examples**. Live native mutation/version validation remains pending; no public upload is claimed.

## 1.0.0-preview.20261014.1 - 14 October 2026 (root save/close batch, prepared)

## 1.0.0-preview.20261013.10 - 13 October 2026 (load batch 4, prepared)

Added PopulateMemberLinearVaryingLoads, PopulateMemberLoads and PopulateMemberTrapezoidalLoads with matching signatures and detailed IntelliSense. Individual-family calls append; full population replaces managed lists with all six supported families in reference order and returns an eager snapshot of original beam objects. Preserved raw fields and supplied case identity with input validation before updates and documented partial failure behavior.

## 1.0.0-preview.20261013.9 - 13 October 2026 (load batch 3, prepared)

Added GetRepeatLoadCases by scanning checked primary activation and native repeat-command counts, replacing the reference stub. Returned metadata classifies underlying primary IDs as RepeatLoad; native zero-count ambiguity is documented. Added append-only PopulateMemberConcentratedLoads and PopulateMemberConcentratedMoments with original case identity, single input snapshots and validated native readers.

## 1.0.0-preview.20261013.8 - 13 October 2026 (load batch 2, prepared)

Added GetLoadCombination, GetMemberLoadsGroupedByLoadCase and GetNodalLoadsGroupedByLoadCase with detailed matching IntelliSense. Validated combination Boolean status, source arrays and primary membership; rejected unrepresentable SRSS overall factors. Preserved managed refresh, reference case identity, exact definition comparison and duplicate-assignment buckets.

## 1.0.0-preview.20261013.7 - 13 October 2026 (load batch 1, prepared)

Added the thread-count AddSelfweightInXYZTo overload, CreateLoadCombinations and editor-definition CreateLoadEnvelopExt with matching signatures. Preserved the existing typed self-weight and integer-envelope implementations. Added invariant-culture editor combinations, eager filtering/validation and caller-thread entity-specific self-weight. Shared XML documentation is now an explicit compiler input for reliable incremental IntelliSense updates.

## 1.0.0-preview.20261013.4 - 13 October 2026 (prepared)

## 1.0.0-preview.20261013.3 - 13 October 2026 (prepared)

## 1.0.0-preview.20261013.2 - 13 October 2026 (prepared)

## 1.0.0-preview.20261013.1 - 13 October 2026 (prepared)

## 1.0.0-preview.20261013 - 13 October 2026 (prepared)

## 1.0.0-preview.20261012.3 - 12 October 2026 (prepared)

## 1.0.0-preview.20261012.2 - 12 October 2026 (prepared)

- Added `SegregateBeamsByConnectivity(IEnumerable<Beam> beams)` to group connected analytical beams and order simple chains.
- Added `SegregateMembersByConnectivity(IEnumerable<Member> members, MemberConnectivityScope scope = MemberConnectivityScope.EndpointsOnly)` for endpoint or all-node member grouping.
- Added `SegregatePlatesByConnectivity(IEnumerable<Plate> plates, PlateConnectivityScope scope = PlateConnectivityScope.SharedEdge)` for candidate-edge or any-corner plate grouping, preserving the reference predicate.
- Added the standalone `ConnectivityService` and matching connectivity-scope enums. The algorithms operate on managed entities without native STAAD calls. Beam input is explicitly deduplicated by ID to accommodate the target entity's endpoint-aware equality.
- Restored and expanded XML documentation for every geometry/load overload on both interfaces and concrete adapters, plus previously undocumented exported managed functions. Shared XML source comments expand into the NuGet XML file; documentation warnings now fail builds.
- Added a complete [geometry comparison](Geometry.md): nine methods were pending before this increment, three are now ported, and six remain.
- Extended package checks to validate complete adapter documentation and compile/run 51 XML examples, including the three new APIs. Added offline connectivity and compiled-XML regression coverage. This version has not been published to NuGet.org.

## 1.0.0-preview.20261012.1 - 12 October 2026

This corrected version supersedes `1.0.0-preview.20261012`. Already-published `1.0.0-preview.20261012` remains unchanged. Public availability is recorded in the release assessment; see [Publishing.md](Publishing.md) for public-feed verification.

## 1.0.0-preview.20261012 — 12 October 2026

### Latest additions

- `GetLoadCases(int fromId, int toId, LoadCaseType loadCaseType)` returns fresh metadata for an inclusive positive ID range. Invalid bounds are rejected before COM access; sparse ranges do not require generating every integer.
- `GetLoadCasesFromIds(IEnumerable<int> loadCasesIds, LoadCaseType loadCaseType)` snapshots input once, validates IDs before COM access, collapses duplicate requests and omits missing IDs. Empty input returns a fresh empty list without native reads after validating the selected family.
- `GetLoadCasesOfType(LoadCaseType loadCaseType)` returns a fresh concrete `List<ILoadCase>` for the selected primary, reference or combination family.

These methods read the complete selected family eagerly and preserve the active load case. Metadata failures propagate even for cases excluded by a filter. Output ordering is unspecified. Repeat and undefined families are unsupported. Results contain metadata; they do not load assignments, combination factors or analysis results.

### Other load queries included in this preview

Seven assignment readers return nodal forces/moments and member distributed forces, point forces, point moments, linear varying loads, trapezoidal loads and uniform moments:

- `GetNodalLoads`
- `GetMemberUniformlyDistributedLoads`
- `GetMemberConcentratedLoads`
- `GetMemberConcentratedMoments`
- `GetMemberLinearVaryingLoads`
- `GetMemberTrapezoidalLoads`
- `GetMemberUniformMoments`

Eleven other metadata queries provide family discovery, direct lookup, batch lookup and generic family selection:

- `GetAllPrimaryLoadCases`, `GetAllReferenceLoadCases`, `GetAllLoadCombinationCases`
- `GetPrimaryLoadCaseFromId`, `GetReferenceLoadCaseFromId`, `GetLoadCombinationCaseFromId`
- `GetPrimaryLoadCasesFromIds`, `GetReferenceLoadCasesFromIds`, `GetLoadCombinationCasesFromIds`
- `GetAllLoadCasesOfType`, `GetLoadCase`

Primary/reference batches preserve requested order and duplicates as separate objects; combination batches collapse duplicates and validate all memberships before reading titles. Generic lookup returns null for a missing positive ID after successfully reading the selected family. Assignment readers activate the supplied case and preserve native signs, order and raw units. Family metadata readers leave active-case state unchanged. See [Load-Queries.md](Load-Queries.md) and the [API reference](https://github.com/structuraldeveloper/StaadPro.Interop/blob/main/docs/API-Reference.md) for complete signatures, contracts and examples.

### Validation and documentation

- Inputs, native statuses, counts, arrays, directions and metadata are checked; malformed responses throw rather than returning partial results. SAFEARRAYs with nonzero lower bounds are supported by the assignment readers.
- Uniform moment model conversion uses moment divided by length for intensity and treats eccentricity as a length. This corrects the main model's dimensional conversion and omitted eccentricity conversion.
- All twenty-one ported functions have expanded XML IntelliSense on both `IOSLoad` and `OSLoadAdapter`, including parameters, return values, exceptions, side effects and executable examples. Adapter implementations remain in a single non-partial class.
- The package ships XML beside the DLL, a NuGet README, load-query documentation, release notes, and [installation](Installation.md)/[publishing](Publishing.md) guides.
- Release verification covers **342 passing offline tests**, **42 compiled and executed XML examples**, and a compiled installation example in an isolated PackageReference consumer using a fresh cache. The managed stand-in exercises mapping and error contracts; it does not certify native STAAD behavior.

### Installation

After this exact version becomes publicly available:

```powershell
dotnet add ./YourApp.csproj package StaadPro.Interop --version 1.0.0-preview.20261012 --source https://api.nuget.org/v3/index.json
```
Enable prerelease packages in Visual Studio's NuGet UI. The verified archive can also be installed through a local feed before publication; see [Installation.md](Installation.md).
