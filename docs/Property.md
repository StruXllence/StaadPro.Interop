# Property observations and API operations

**Updated:** 26 October 2026

## Current documented P10 completion

The subsequent domain increment implements **25 Output routes**, bringing typed Output coverage to **40/60**. Validation passed **5,353 offline tests** with zero failures; package verification covers **1,002 selected XML examples**. These checks use managed native-signature fixtures; live 2024–2026 COM and engineering qualification remain pending.

## Previous catalog/cleanup increment and API naming

Passed a Release build with zero warnings/errors and **5,156 offline tests**, including **197 new cases**. The isolated PackageReference-only consumer verifies **944 selected XML examples**: 490 common Property examples plus twelve optional catalog/acquisition examples, alongside the existing groups. Separate managed checks exercise all twenty new mutations and five catalog queries through both public surfaces. Live native mutation and engineering qualification remain pending.

Added a passive version/signature acquisition boundary for the newer catalog group on exact native profiles **25.00.01.424** and **26.00.00.340**. Retained operation-specific zero-success, Boolean-success and explicit no-specification removal outcomes, owned bounded bulk targets, and separate member-end, cable-definition and analytical-node selectors. No native model is modified by verification.

Renamed **17 public UPT methods** to PascalCase, including **AddUptPropertyChannel**, **AddUptPropertyWideFlange** and **CreateUptTable**. This preview requires callers to update those method names. Parameter names, types, ordering, defaults and return types are preserved. Bentley native dispatch names and recorded contract evidence retain their original spelling.

See [the API rename table](Property-API-Naming.md).

## Earlier UPT/catalog increment

Passed a Release build with zero warnings/errors and **4,959 offline tests**, including **213 new cases**. The isolated PackageReference-only consumer verifies **892 selected XML examples**, including all **450 Property interface/adapter examples**. All sixteen new mutation examples are guarded; separate managed checks execute all 25 routes through both public surfaces. Live native mutation and engineering qualification remain pending.

Added immutable explicit UPT profile inputs, owned table inventories and named section observations. Kept operation-specific creation, insertion and removal outcomes separate. Qualified ten UPT read-back layouts and the database-folder setter against exact acquired native profiles 24.00.02.354, 25.00.01.424 and 26.00.00.340, retaining their different folder return conventions. Unreviewed builds cannot select those policies. Table numbers, native indices and property references retain separate identities.

## Earlier increment: fireproofing, attributes and UPT

Passed a Release build with zero warnings/errors and **4,746 offline tests**, including **195 new cases**. The fresh PackageReference-only consumer verifies **842 selected XML examples**, including all **400 Property interface/adapter examples**. All ten new mutation examples are guarded; dedicated managed checks execute every new route through both public surfaces. Verification does not change a live STAAD model.

Added immutable exact attribute pairs, pair/domain/ID snapshots, fireproofing correspondence and explicit UPT value packets. Bounded count/list/count reads validate written outputs and preserve order/duplicates. By-reference native attribute-value rewriting returns StateChanged rather than data for the wrong pair. UPT table IDs, insertion acknowledgements and duplicate outcomes remain separate; scalar and unequal wide-flange routes retain their different return conventions.

## Previous increment: section and specification creation

Passed a Release build with zero warnings/errors and **4,551 offline tests**, including **205 new cases**. The isolated PackageReference-only consumer verifies **792 selected XML examples**, including all **350 Property interface/adapter examples**. All 24 new mutation examples are guarded; dedicated managed checks exercise every new route through both the interface and concrete adapter. Verification does not change a live STAAD model.

Added explicit immutable wide-flange, general-prismatic, six-DOF and partial-factor inputs. Retained separate created section IDs, created specification IDs, zero-based fireproofing indices and zero-success status-only results. The void unique-ID setter returns InvokedWithoutAcknowledgement with no fabricated native code and IsSuccessful false; its exact requested property/text identity is retained. Existing signatures, current-material context and native units remain explicit.

## Previous increment: P3/P4/P5 completion

Passed a Release build with zero warnings/errors and **4,346 offline tests**, including **190 new cases**. The isolated PackageReference-only consumer verifies **742 selected XML examples**, including all **300 Property interface/adapter examples**. New mutation examples are guarded; separate installed-consumer checks invoke all eleven mutations against managed stand-ins. No live STAAD model is changed by verification.

Added immutable explicit material definitions/strengths, orthotropic constants, member-section snapshots and named mutation acknowledgements retaining actual native evidence and requested identity. Kept Steel Fu/Fy/Rt/Ry order distinct from extended Fy/Fu/Ry/Rt/Fcu order. Added bounded owned solid-ID snapshots, strict written-output checks, operation-specific absence/error handling and native-version section layouts. A successful mutation acknowledgement does not prove design-strength persistence, save the model or provide rollback.

## Previous increment: 19 main signatures and six native observations

Incorporated the remaining **19 active, nonconflicting main-interface Property signatures** plus **six documented native observations**, bringing Property to **125 overloads**. Added section creation/assignment/read-back, explicit material conversion and captured-receiver uniform-thickness creation with complete matching XML IntelliSense. Native dispatch now covers **66 of 204 active candidates**, with **138 remaining**. See [all 25 section-completion contracts](Property-Sections.md).

Passed a Release build with zero warnings/errors and **4,156 offline tests**, including **225 new section cases**. Isolated installed-package verification compiles and runs **692 selected XML examples**, including all **250 Property interface/adapter examples**, through a fresh PackageReference-only consumer and cache. Managed stand-ins perform every example; live native mutation and engineering qualification remain pending.

Selected 47 reviewed section layouts per acquired STAAD 24/25/26 profile, reading native version metadata after wrapper initialization and rechecking it before returning packets. Corrected the source assignment query filtering an unfilled array. Preserved native identity/field labels, explicit missing-area results, operation-specific statuses and partial-write behavior. Added small section selection/data models without importing private catalog/UI services; reused the repository UnitsConverter for explicit modulus/weight-density conversion. Existing typed-return collisions (eight) and obsolete source overloads (five) remain documented exceptions.

The six native observations are GetSectionPropertyCount, GetThicknessPropertyCount, GetCountofSectionPropertyValuesEx, GetSectionPropertyType, GetSectionPropertyCountry and GetRecordForSection. All active main signatures that can coexist with the established API are now ported. This does not imply full private model/UI API parity or complete native dispatch coverage. Custom IOSProperty implementations must implement the additions.

## Previous increment: 25 material and release workflows

Passed a Release build with zero warnings/errors and **3,931 offline tests**, including **168 new workflow cases**. Isolated installed-package verification compiles and runs **642 selected XML examples**, including all **200 Property interface/adapter examples**, through a fresh PackageReference-only consumer and package cache. Examples run against managed native stand-ins; live mutation and engineering qualification remain pending.

Material creation records native Created/Updated outcomes separately from list IDs; legacy creation retains its explicitly documented E/G x1000 scale. Indexed material collections retain native names and consistent name equality/hash behavior. Release read-back preserves mixed native modes and partial-mode provenance; basic creation refuses unsupported partial-moment packets. Populations stage native reads before changing caller packets, degree overloads retain caller-apartment dispatch, and multi-step writes recheck binding lifetime around each native write.

All 25 signatures match the main interface. Custom IOSProperty implementations must supply the new members. Existing qualified single-material and release methods retain their return types; mutable collection/population methods use those checked observations internally.

## Previous increment: 25 management ports

Passed a Release solution build with zero warnings/errors and **3,763 offline tests**, including **189 new Property management cases**. The isolated installed-package verifier compiles and runs **592 selected XML examples**, including all **150 Property interface/adapter examples**, through a fresh PackageReference-only consumer and cache. Managed stand-ins exercise the examples without changing a native STAAD model. Live creation/assignment and engineering qualification for this increment remain pending.

The four material subset overloads originate in the main adapter; the other 21 originate in its interface as well. Signature defaults and parameter names are preserved. The expanded supporting packets omit the private unit-converter helpers. Interface expansion requires custom IOSProperty implementations to implement the new methods. Existing 25 typed observations keep their return packets and signatures.

## Previous increment: 25 inventory and assignment ports

At that preceding checkpoint the adapter exposed **50 public overloads** and adds routes to **19 further native names**, for **44 of 204 active native candidates** with **160 remaining**. Eight of the 212-name union remain excluded. This is dispatch-route coverage, not a claim of complete typed parsing or native qualification: the raw section-buffer route deliberately does not decode a version-specific layout. The archived multi-version review retains its original preimplementation coverage labels.

## Earlier foundation increment

Passed **3,322 offline tests**, including **422 new Property cases**, and a Release build with zero warnings/errors. All 50 interface/adapter XML examples compiled and ran through isolated installation of the local validation package; all 492 selected package examples passed. This is source and managed-stand-in verification; live native qualification remains pending. That foundation checkpoint used a separate validation archive; the new preview below includes both increments.

Adding these members to IOSProperty requires custom interface implementations to provide the new members. Existing wrapper/provider contracts and target frameworks are unchanged. The legacy ComObject escape hatch remains available; direct calls bypass the typed validation described here.

## Result, units and state rules

Each method returns PropertyQueryResult<T>. Inspect IsSuccessful before reading Data; an unsuccessful result exposes no payload and Data throws InvalidOperationException. NativeFunction identifies the decisive query, including a count/preflight call that prevented retrieval. NativeReturnCode and NativeBooleanReturn retain actual integral/Boolean evidence rather than applying a universal nonzero-success rule. Transport exceptions propagate, while unsupported types, undocumented codes and malformed/unwritten applicable outputs raise InvalidOperationException.

NoSpecification, NotAssigned, NotFound and NativeFailure have different meanings. A successful zero offset remains valid data; missing offsets never produce a fabricated zero vector. Named native material queries can use integer-zero success, Boolean success or returned-name success depending on the function. Assignment state is native 1=Unassigned / 2=Assigned.

Native numerical units are preserved. Weight density means force/volume, not mass/volume. Elastic/shear moduli and strengths retain force/area dimensions; thermal expansion retains inverse-temperature units; damping, Poisson and strength ratios are dimensionless. Member/plate offsets and section thicknesses retain length units, areas length-squared and inertia slots length-to-the-fourth. Wrapper input-unit fallback caches do not establish an observed Property-unit contract.

## Existing model and enum reuse

| Existing item | Reuse or necessary distinction |
| --- | --- |
| MemberEnd | Reused the existing Start=0 / End=1 enum; no duplicate member-end type. |
| Vector3D | Reused the existing WindowsBase value vector for independent local/global offsets. |
| NativeValues | Reused strict integral native decoding instead of Convert.ToInt32 rounding/coercion. |
| Main ElementNodeOffsetAxis | Ported exact Local=0 / Global=1 / Z=2 semantics with XML; did not reuse the oppositely numbered member OffsetAxis. |
| Main MaterialProperty | Reused E/Poisson/weight-density/thermal/damping and strength field meanings. Returned immutable observations; did not copy assumed engineering defaults, computed G/K substitutes, entity-assignment collections or proprietary catalog/unit dependencies. Native G is retained only when returned. |
| Main MemberReleases / Releases / Springs / MpFactors | Reused six-DOF order and MP concepts. Added per-DOF release modes and nullable applicable factors because Boolean Releases loses -1/-2/-3 and the source whole-release classification cannot represent independent mixed modes. Did not copy mutable parsing/event wiring or defaults for unavailable MP outputs. |
| Main OffsetSpecification / ElementNodeOffset | Reused offset, incidence-index and coordinate-selector semantics. Used existing MemberEnd plus Vector3D/status results rather than copying mutable assignment sets or collapsing absent and successful-zero offsets to a shared singleton. |
| Existing result patterns | Followed the existing named status/data pattern without coupling Property absence/status meanings to Output/Load enums or adding public tuples/ref/out. |

## Implemented APIs and choosing between them

### GetBetaAngle

~~~csharp
PropertyQueryResult<double> GetBetaAngle(int memberId);
~~~
Use to observe orientation about the analytical member axis; offsets and releases do not represent beta rotation. Returns the finite signed native angle unchanged, including zero. Native -3001 is NotFound. No degrees/radians conversion or managed local-axis reconstruction is performed.

### GetElementOffsetSpecificationCount

~~~csharp
PropertyQueryResult<int> GetElementOffsetSpecificationCount();
~~~
This counts native offset specifications, not plates, corners, nonzero vectors or assigned members. Zero is a valid count. Use GetPlateOffsetSpecification to inspect one plate corner and its stored convention. Undocumented negative counts are rejected.

### GetMemberReleaseSpecificationExtended

~~~csharp
PropertyQueryResult<MemberReleaseSpecification> GetMemberReleaseSpecificationExtended(int memberId, MemberEnd memberEnd);
~~~
Prefer this query when common MP or directional MPX/MPY/MPZ factors are needed. The basic GetMemberReleaseSpecification returns only six mode/parameter slots and cannot supply the separate extended factors. Native 1/true succeeds and 0/false fails. Modes 0/1/-1/-2/-3 are retained per DOF rather than converted to Boolean release flags. Unused spring/MP slots may remain unwritten and are represented as null; an applicable spring or factor must be written and finite. The native common MP factor is dimensionless; translational and rotational spring stiffness retain their different native dimensions.

### GetMemberReleaseSpecification

~~~csharp
PropertyQueryResult<MemberReleaseSpecification> GetMemberReleaseSpecification(int memberId, MemberEnd memberEnd);
~~~
Use for basic release/spring observations. Choose GetMemberReleaseSpecificationExtended for separate common MP and MPX/MPY/MPZ outputs; this basic query never invents zero for those missing outputs. Native 1/true succeeds and 0/false fails. FX/FY/FZ/MX/MY/MZ mode order and 0/1/-1/-2/-3 codes are preserved. Only applicable parameter slots are consumed; a spring or directional partial-factor slot must be finite. These are model specifications, not support reactions or member-end Output forces.

### GetMemberLocalOffset

~~~csharp
PropertyQueryResult<Vector3D> GetMemberLocalOffset(int memberId, MemberEnd memberEnd);
~~~
Choose this query when a local-coordinate offset is needed; GetMemberGlobalOffset asks STAAD for global components. The vector is an offset in native length units, not a position, displacement result or beta rotation. Native 0 succeeds; -6027 is NoSpecification, -3001 NotFound and -6026 InvalidReference. A successful zero vector stays successful and is not replaced by a shared no-offset singleton. No coordinate conversion or sign change is applied.

### GetMemberGlobalOffset

~~~csharp
PropertyQueryResult<Vector3D> GetMemberGlobalOffset(int memberId, MemberEnd memberEnd);
~~~
Choose this query for global components; GetMemberLocalOffset requests member-local components. No managed rotation is substituted. Native 0 succeeds; -6027 is NoSpecification, -3001 NotFound and -6026 InvalidReference. A successful zero vector differs from an absent specification. The returned Vector3D is an independent value in native length units. These are modeling offsets, not Output displacements.

### GetMemberSectionProperties

~~~csharp
PropertyQueryResult<BeamSectionProperties> GetMemberSectionProperties(int memberId);
~~~
Use for width, depth, axial/shear areas and the three named native inertia slots. GetMemberSectionPropertiesExtended additionally returns flange/web thickness; those fields are null here. This does not hydrate a section catalog or select a section shape. Native 1/true succeeds; 0/false cannot distinguish a missing member, unassigned property and general failure, so it returns NativeFailure without fabricated data. Native Ix/Iz descriptions conflict with conventional physical-axis labels; their slot names/order are preserved without assuming a torsion or principal-axis mapping. Length, area and inertia slots retain their respective native length, length-squared and length-to-the-fourth units.

### GetMemberSectionPropertiesExtended

~~~csharp
PropertyQueryResult<BeamSectionProperties> GetMemberSectionPropertiesExtended(int memberId);
~~~
Use instead of GetMemberSectionProperties when the two additional thickness outputs are required. No section shape, catalog identity, material or new property is inferred. Native 1/true succeeds; 0/false is an ambiguous failure and exposes no packet. All returned slots must be finite. Native Ix/Iz prose is inconsistent, so NativeIx/NativeIy/NativeIz retain native slot identity rather than a guessed torsional/principal interpretation. Values retain native length, area and fourth-power length units.

### GetMemberMaterialConstants

~~~csharp
PropertyQueryResult<MaterialConstants> GetMemberMaterialConstants(int memberId);
~~~
Use when the member is known but its material name is not. GetMaterialProperty instead selects a material by exact name; indexed isotropic queries select a material-list index and additionally return G. This native call returns E, Poisson ratio, weight density, thermal expansion and damping, not G or strength fields. ShearModulus is therefore null, never computed from E/Poisson. Native Boolean true succeeds; false does not distinguish a missing member from unavailable constants. WeightDensity is weight per volume, not mass density.

### GetPlateLocalOffset

~~~csharp
PropertyQueryResult<Vector3D> GetPlateLocalOffset(int plateId, int cornerIndex);
~~~
Choose this query for local components, GetPlateGlobalOffset for global components, or GetPlateOffsetSpecification for the stored selector and vector. cornerIndex is native incidence position 1/2/3/4, never a node ID; triangular-corner validity remains STAAD-controlled. Native 0 succeeds, -4001 is NotFound and -6027 NoSpecification. A successful zero vector is retained. No incidence lookup, coordinate transform or unit conversion occurs.

### GetPlateGlobalOffset

~~~csharp
PropertyQueryResult<Vector3D> GetPlateGlobalOffset(int plateId, int cornerIndex);
~~~
Use for global components; GetPlateLocalOffset returns local components and GetPlateOffsetSpecification returns the stored convention. The native Global function is called despite its help summary mistakenly saying local. cornerIndex is a one-based incidence position 1-4, not a node number. Native 0 succeeds, -4001 is NotFound and -6027 NoSpecification. A successful zero vector is not absence; no conversion or sign change occurs.

### GetPlateOffsetSpecification

~~~csharp
PropertyQueryResult<ElementOffsetSpecification> GetPlateOffsetSpecification(int plateId, int cornerIndex);
~~~
Use when the original Local/Global/Z selector is needed; the local/global offset queries instead request components in a particular coordinate frame. Native 1 supplies an offset, native 0 explicitly means NoSpecification, and -4001 means NotFound. This differs from the local/global getters, where 0 is success. Stored selectors are Local=0, Global=1 and Z=2, matching the main ElementNodeOffsetAxis enum; they must not be cast from its opposite member OffsetAxis numbering. No physical meaning or conversion beyond the native Z selector is inferred. A code-1 zero vector remains valid data.

### GetPlateSectionPropertyId

~~~csharp
PropertyQueryResult<int> GetPlateSectionPropertyId(int plateId);
~~~
Use to identify an assigned property, not to read its thickness values or count property definitions. Positive return values are property IDs; -4001 is NotFound and -6022 NotAssigned. Zero is undocumented and rejected. The method is documented in all three reviewed help sets but absent from inspected 2025/2026 CLR interfaces: a missing acquired dispatch member returns Unsupported. No alternate function name, different entity type or fallback call is attempted; other transport errors propagate.

### GetIsotropicMaterialAssignedMemberCount

~~~csharp
PropertyQueryResult<int> GetIsotropicMaterialAssignedMemberCount(string materialName);
~~~
### GetIsotropicMaterialAssignedMemberIds

~~~csharp
PropertyQueryResult<IEnumerable<int>> GetIsotropicMaterialAssignedMemberIds(string materialName, int maximumResults = 100000);
~~~
### GetMaterialProperty

~~~csharp
PropertyQueryResult<MaterialProperties> GetMaterialProperty(string materialName);
~~~
Use an exact material name obtained from an entity or native material observation. GetIsotropicMaterialProperty instead uses a zero-based list index and supplies native G; GetExtendedMaterialProperty also supplies strength fields. Native return 0 succeeds and -6023 is NotFound. These are different success semantics from the Boolean extended function. ShearModulus, Strengths and AssignmentState are null because this function does not return them. No default engineering values or computed G are substituted. Density remains weight per volume.

### GetBeamMaterialName

~~~csharp
PropertyQueryResult<string> GetBeamMaterialName(int memberId);
~~~
Use for name discovery before a named material query. This returns a name, not constants, an index, a property ID or a new assignment. Native spelling/case/whitespace are preserved. Empty or native-null text is EmptyOrUnavailable because the help supplies no way to distinguish no assignment from a missing/unavailable member. Non-string responses are rejected; no guessed material name is returned.

### GetElementMaterialName

~~~csharp
PropertyQueryResult<string> GetElementMaterialName(int plateId);
~~~
This native element query takes a plate ID. Use GetBeamMaterialName for members; do not pass a node, solid ID or material-list index. It returns the exact name, not constants or thickness. Empty/native-null text is EmptyOrUnavailable because the contract does not distinguish unassigned and unavailable entities. No fallback to a similarly named plate/material API is attempted.

### GetIsotropicMaterialCount

~~~csharp
PropertyQueryResult<int> GetIsotropicMaterialCount();
~~~
Use to bound the zero-based materialIndex accepted by GetIsotropicMaterialProperty and GetExtendedIsotropicMaterialProperty. This is not a member/plate assignment count. Zero is a valid empty definition inventory; undocumented negative or nonintegral counts are rejected. A count is only an observation, not a stable reservation of material identities.

### GetIsotropicMaterialProperty

~~~csharp
PropertyQueryResult<MaterialProperties> GetIsotropicMaterialProperty(int materialIndex);
~~~
Use for material enumeration, unlike GetMaterialProperty which selects by name. materialIndex is zero-based, not a native property ID or member ID. This function returns G directly; that native value is retained instead of the main MaterialProperty computed E/(2*(1+Poisson)) substitute. The native return is the material name, not a success flag. Null/empty name or an index outside the observed count is NotFound. Before/after material counts detect observed changes but not same-count reorderings. Strengths and AssignmentState are null.

### GetExtendedMaterialProperty

~~~csharp
PropertyQueryResult<MaterialProperties> GetExtendedMaterialProperty(string materialName);
~~~
Choose over GetMaterialProperty when Fy/Fu/Ry/Rt/Fcu are needed. GetExtendedIsotropicMaterialProperty selects a zero-based index and additionally returns native G; this name-based function omits G, so ShearModulus is null. Unlike basic named GetMaterialProperty, native Boolean true means success and false means failure. Strength stresses retain native force/area units; Ry/Rt are dimensionless. No material classification, design pass/fail, default strength or computed shear modulus is inferred.

### GetExtendedIsotropicMaterialProperty

~~~csharp
PropertyQueryResult<MaterialProperties> GetExtendedIsotropicMaterialProperty(int materialIndex);
~~~
Use for enumeration with native G and Fy/Fu/Ry/Rt/Fcu. GetExtendedMaterialProperty selects by name and lacks G; GetIsotropicMaterialProperty omits strengths. The native return is the material name, not a Boolean or numeric status. Null/empty name or an out-of-range observed index is NotFound. Coupled counts are checked before/after; no atomic snapshot or stable same-count ordering is promised. All supplied constants and strength fields must be written and finite; optional AssignmentState remains null.

### GetIsotropicMaterialPropertiesAssigned

~~~csharp
PropertyQueryResult<MaterialProperties> GetIsotropicMaterialPropertiesAssigned(int nativeMaterialNumber);
~~~
Use when the native assignment observation is required; the ordinary and extended indexed functions do not return it. nativeMaterialNumber is forwarded unchanged: this help says material number ID, unlike the explicitly zero-based indexed methods. Nonnegative input is accepted without guessed rebasing or a count-based ID/index conversion; qualify the selector on the acquired build. Native string name supplies success; null/empty means NotFound. Assignment output 1 means Unassigned and 2 Assigned, not Boolean 0/1; documented Double or integral forms are decoded exactly. Strengths remain null.

### GetOrthotropic2DMaterialCount

~~~csharp
PropertyQueryResult<int> GetOrthotropic2DMaterialCount();
~~~
Use for the 2D orthotropic inventory, not isotropic or 3D material counts and not element assignments. Zero is valid. The associated property getter documents a native material number rather than an explicit zero-based list index; this count does not establish a selector rebasing rule. Undocumented negative/nonintegral counts are rejected.

### GetOrthotropic2DMaterialProperties

~~~csharp
PropertyQueryResult<Orthotropic2DMaterialProperties> GetOrthotropic2DMaterialProperties(int nativeMaterialNumber);
~~~
Use for independent directional material constants; isotropic queries cannot substitute for them. nativeMaterialNumber is forwarded unchanged and must be qualified on the acquired build because the help does not establish its base. Native string name indicates success; null/empty is NotFound. Buffer sizes are E=2, Poisson=2, G=3, weight density=2, thermal expansion=2 and damping=2; each output must be finite and owned. Named first/second/third fields preserve native order because the help does not identify physical axis/plane ordering. Do not assume global X/Y or XY/YZ/ZX, derive G, or replace missing values with isotropic defaults.

## Native qualification still required

## Available functions

`AddControlDependentRelation()`, `AddUptPropertyAngle()`, `AddUptPropertyChannel()`, `AddUptPropertyDoubleAngle()`, `AddUptPropertyGeneral()`, `AddUptPropertyISection()`, `AddUptPropertyPipe()`, `AddUptPropertyPrismatic()`, `AddUptPropertyTee()`, `AddUptPropertyTube()`, `AddUptPropertyWideFlange()`, `AddUptPropertyWideFlangeComposite()`, `AddUptPropertyWideFlangeUnequal()`, `AssignBeamProperty()`, `AssignBetaAnglesToBeams()`, `AssignElementAttribute()`, `AssignElementSpecToPlate()`, `AssignMaterialProperties()`, `AssignMaterialProperty()`, `AssignMaterialToMember()`, `AssignMaterialToSolid()`, `AssignMemberAttribute()`, `AssignMemberEndOffset()`, `AssignMemberEndOffsets()`, `AssignOffsetSpecification()`, `AssignOffsetSpecifications()`, `AssignPlateElementThickness()`, `AssignPlateElementThicknesses()`, `AssignSectionProperties()`, `AssignSectionProperty()`, `AssignThicknessProperties()`, `AssignThicknessProperty()`, `CreateAndAssignMemberEndReleases()`, `CreateAndAssignOffsetSpecifications()`, `CreateAndAssignSectionProperties()`, `CreateAssignProfileProperty()`, `CreateBeamPropertyFromTable()`, `CreateBeamPropertyFromTableEx()`, `CreateBuiltUpBeamProperty()`, `CreateCompositeBeamPropertyFromTable()`, `CreateElementAttribute()`, `CreateElementIgnoreInplaneRotnSpec()`, `CreateElementLocalZOffsetSpec()`, `CreateElementNodeReleaseSpec()`, `CreateElementOffsetSpec()`, `CreateElementPlaneStressSpec()`, `CreateIsotropicMaterialAluminum()`, `CreateIsotropicMaterialConcrete()`, `CreateIsotropicMaterialPropertiesEx()`, `CreateIsotropicMaterialSteel()`, `CreateIsotropicMaterialTimber()`, `CreateMaterialProperties()`, `CreateMaterialPropertiesNoConversion()`, `CreateMaterialProperty()`, `CreateMaterialPropertyNoConversion()`, `CreateMemberAttribute()`, `CreateMemberCableSpec()`, `CreateMemberCableSpecEx()`, `CreateMemberCompressionSpec()`, `CreateMemberFireProofingSpec()`, `CreateMemberIgnoreStiffSpec()`, `CreateMemberInactiveSpec()`, `CreateMemberPartialReleaseSpec()`, `CreateMemberTensionSpec()`, `CreateMemberTrussSpec()`, `CreateOffsetSpecification()`, `CreateOffsetSpecifications()`, `CreatePlateElementThickness()`, `CreatePlateElementThicknesses()`, `CreatePlateThicknessProperties()`, `CreatePlateThicknessProperty()`, `CreatePrismaticCircleProperty()`, `CreatePrismaticGeneralProperty()`, `CreatePrismaticRectangleProperty()`, `CreatePrismaticTeeProperty()`, `CreatePrismaticTrapezoidalProperty()`, `CreatePropertyFromUptTable()`, `CreatePropertyFromUserTable()`, `CreateSectionProperties()`, `CreateSectionProperty()`, `CreateTaperedTubeProperty()`, `CreateUptTable()`, `CreateUptTableEx()`, `CreateWideFlangePropertyFromTable()`, `DeleteAllControlDependentRelations()`, `DeleteElementAttribute()`, `DeleteMaterial()`, `DeleteMemberAttribute()`, `DeleteMemberReleaseSpec()`, `DeleteMemberSpec()`, `DeleteProperty()`, `GetAllExtendedIsotropicMaterialProperties()`, `GetAllExtendedIsotropicMaterialPropertiesWithPopulatedAssignments()`, `GetAllExtendedMaterialPropertiesWithPopulatedAssignments()`, `GetAllIsotropicMaterialProperties()`, `GetAllIsotropicMaterialPropertiesWithPopulatedAssignments()`, `GetAllMaterialPropertiesWithPopulatedAssignments()`, `GetAllPlateElementThicknesses()`, `GetAllPlateElementThicknessesWithPopulatedAssignments()`, `GetAllSections()`, `GetAllSectionsWithAssignedBeamsList()`, `GetAssignedAttributeCount()`, `GetAveragePlateThickness()`, `GetBeamMaterialName()`, `GetBeamOffsetSpecification()`, `GetBeamProperty()`, `GetBeamSectionDisplayName()`, `GetBeamSectionName()`, `GetBeamSectionPropertyRefNo()`, `GetBeamSectionPropertyTypeNo()`, `GetBeamSectionPropertyValuesEx()`, `GetBeamsWithMissingSectionProperty()`, `GetBetaAngle()`, `GetCountofSectionPropertyValuesEx()`, `GetCountryTableNo()`, `GetElementCountByAttribute()`, `GetElementListByAttribute()`, `GetElementMaterialName()`, `GetElementOffsetSpecificationCount()`, `GetExtendedIsotropicMaterialProperty()`, `GetExtendedMaterialProperty()`, `GetFireProofDataForBeam()`, `GetFireProofedBeamCount()`, `GetFireProofedBeamList()`, `GetFireProofingSpecAssignedBeamCount()`, `GetFireProofingSpecAssignedBeamList()`, `GetFireProofingSpecCount()`, `GetFireProofingSpecDetails()`, `GetInactiveMemberCount()`, `GetIsotropicMaterialAssignedBeams()`, `GetIsotropicMaterialAssignedMemberCount()`, `GetIsotropicMaterialAssignedMemberIds()`, `GetIsotropicMaterialAssignedPlates()`, `GetIsotropicMaterialAssignedSolidCount()`, `GetIsotropicMaterialAssignedSolidIds()`, `GetIsotropicMaterialCount()`, `GetIsotropicMaterialPropertiesAssigned()`, `GetIsotropicMaterialProperty()`, `GetMaterialProperty()`, `GetMemberAttributeCount()`, `GetMemberAttributeList()`, `GetMemberCountByAttribute()`, `GetMemberGlobalOffset()`, `GetMemberListByAttribute()`, `GetMemberLocalOffset()`, `GetMemberMaterialConstants()`, `GetMemberReleaseSpecification()`, `GetMemberReleaseSpecificationExtended()`, `GetMemberSectionProperties()`, `GetMemberSectionPropertiesExtended()`, `GetMemberSpecification()`, `GetOrthotropic2DMaterialCount()`, `GetOrthotropic2DMaterialProperties()`, `GetOrthotropic3DMaterialCount()`, `GetOrthotropic3DMaterialProperties()`, `GetPlateGlobalOffset()`, `GetPlateLocalOffset()`, `GetPlateMaterialName()`, `GetPlateOffsetSpecification()`, `GetPlateSectionPropertyId()`, `GetPlateThicknessByPlateId()`, `GetPlateThicknessByThicknessPropertyId()`, `GetPlatesWithMissingThicknessProperty()`, `GetPropertyUniqueID()`, `GetPublishedProfileName()`, `GetRecordForSection()`, `GetSectionBy()`, `GetSectionByPropertyIdWithAssignedBeamsList()`, `GetSectionPropertiesForProfile()`, `GetSectionPropertyAssignedBeams()`, `GetSectionPropertyCount()`, `GetSectionPropertyCountry()`, `GetSectionPropertyList()`, `GetSectionPropertyName()`, `GetSectionPropertyType()`, `GetSectionPropertyValues()`, `GetSectionTableNo()`, `GetSectionalArea()`, `GetShapeCode()`, `GetSolidMaterialName()`, `GetStaadProfileName()`, `GetStandardProfileDBFolder()`, `GetThicknessPropertyAssignedPlates()`, `GetThicknessPropertyCount()`, `GetThicknessPropertyList()`, `GetTypeForIsotropicMaterial()`, `GetUnusedSectionsCount()`, `GetUnusedSectionsIds()`, `GetUserProvidedTableCount()`, `GetUserProvidedTableList()`, `GetUserProvidedTableNo()`, `GetUserProvidedTableSectionCount()`, `GetUserProvidedTableSectionProperties()`, `GetUserProvidedTableSectionPropertyCount()`, `GetUserProvidedTableSectionType()`, `PopulateBetaAngles()`, `PopulateMemberOffsets()`, `PopulateMemberReleases()`, `RemoveAllElementNodeReleaseSpec()`, `RemoveAllElementOffsetSpec()`, `RemoveAttribute()`, `RemoveElementIgnoreInplaneRotnSpecFromPlate()`, `RemoveElementNodeReleaseSpecFromPlate()`, `RemoveElementPlaneStressSpecFromPlate()`, `RemoveMaterialFromBeam()`, `RemoveMaterialFromPlate()`, `RemoveMaterialFromSolid()`, `RemoveMemberCableSpecFromBeam()`, `RemoveMemberCompressionSpecFromBeam()`, `RemoveMemberFireProofingSpecFromBeam()`, `RemoveMemberIgnoreStiffSpecFromBeam()`, `RemoveMemberInactiveSpecFromBeam()`, `RemoveMemberOffsetSpecFromBeam()`, `RemoveMemberReleaseSpecFromBeam()`, `RemoveMemberTensionSpecFromBeam()`, `RemoveMemberTrussSpecFromBeam()`, `RemovePropertyFromBeam()`, `RemovePropertyFromPlate()`, `RemovePropertyFromUptTable()`, `RemoveUptTable()`, `SetMaterialName()`, `SetPropertyUniqueID()`, `SetStandardProfileDBFolder()`, `SetTypeToIsotropicMaterial()`, `UpdatePropertiesToDesignSection()`.
