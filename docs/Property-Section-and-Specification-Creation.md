# Property section and specification creation

Passed a Release build with zero warnings/errors and **4,551 offline tests**, including **205 new cases**. The isolated PackageReference-only consumer verifies **792 selected XML examples**, including all **350 Property interface/adapter examples**. All 24 new mutation examples are guarded; dedicated managed checks exercise every new route through both the interface and concrete adapter. Verification does not change a live STAAD model.

Added explicit immutable wide-flange, general-prismatic, six-DOF and partial-factor inputs. Retained separate created section IDs, created specification IDs, zero-based fireproofing indices and zero-success status-only results. The void unique-ID setter returns InvokedWithoutAcknowledgement with no fabricated native code and IsSuccessful false; its exact requested property/text identity is retained. Existing signatures, current-material context and native units remain explicit.

## Native contracts and common implementation

The single non-partial OSPropertyAdapter delegates to PropertyCreationReader using the existing captured receiver, serialization gate and before/after binding checks. No new public ref/out, tuple, raw object or mutable array packets are exposed. Native calls run on the calling thread; the caller must arrange the correct apartment for actual COM use. A failed write or binding replacement may leave effects applied, without retry, hidden save or rollback.

## Result identities and acknowledgements

| Native return contract | Managed result |
| --- | --- |
| Positive created section reference | Created and CreatedPropertyId; specification identity fields remain null. |
| Positive created specification ID | Created and CreatedSpecificationId; section/index identity fields remain null. Zero is undocumented and rejected for these functions. |
| Partial-release / element-node-release zero OK | Succeeded, NativeReturnCode=0, no created ID/index. Positive responses are undocumented and rejected. |
| Control/dependent relation zero OK | Succeeded, separate ControlNodeId and owned dependent TargetIds; no created specification identity. |
| Fireproofing nonnegative creation index | Created and exact CreatedSpecificationIndex, including zero. No +1 adjustment and no CreatedSpecificationId. |
| SetPropertyUniqueID void | InvokedWithoutAcknowledgement; IsSuccessful=false; native return evidence and created identities null. This is completed invocation evidence, not a reported failure or persistence guarantee. |
| GetPropertyUniqueID string | Qualified immutable PropertyUniqueIdentifier; inspect IsSuccessful before Data. Empty/null is ambiguous absence, not a fabricated GUID or zero reference. |

All actual integral return codes are retained. Boolean, fractional, null and textual responses to integral contracts are rejected. Known native failure statuses have no created identity. A positive created ID does not establish that STAAD allocated a new globally unique definition rather than reused one.

## Immutable inputs and units

| Input family | Meaning |
| --- | --- |
| WideFlangeTableDefinition | Named factories for ST/D/T/CM/TC/BC/TB, country 1..16 and exact table name. Fresh four-double buffers retain applicable slots; BC uses selector eight and bottom dimensions. |
| GeneralPrismaticSectionDefinition | Explicit AX/AY/AZ/IX/IY/IZ/YD/ZD/YB/ZB in exact native order. AX positive, others nonnegative; zero AY/AZ retains ignore-shear-deformation meaning. |
| DegreeOfFreedomSelection | Immutable named FX/FY/FZ/MX/MY/MZ flags. Release calls interpret true as release; control/dependent calls interpret true as rigidity. Sent as integral 0/1, never COM Boolean guesses. |
| PartialReleaseFactors | Six explicit finite dimensionless factors in native DOF order, managed range 0..1. Distinct from MP/MPX/MPY/MPZ read-back modes. |
| TableSolidShape | PlateStrip=1, SolidRectangle=2, SolidRound=3, Round=4, Cable=5; distinct from TypeSpecification. |
| TaperedTubeShape | Round=0, Hexadecagonal=1, Dodecagonal=2, Octagonal=3, Hexagonal=4, Square=5. |
| CableDefinitionKind / CableTensionEnd | InitialTension=0 / UnstressedLength=1; tension end Unspecified=0, Start=1, End=2. The latter is not MemberEnd's 0/1 domain. |
| ControlDependencyMode | AllDirections=0, RigidXY=1, RigidYZ=2, RigidZX=3, SpecifiedDegreesOfFreedom=4. Four deliberately selects the native other-values branch. |
| FireProofingType | Block/BFP=1 or Contour/CFP=2; no fire analysis or assignment is activated. |

Every floating input is finite. Geometric dimensions are positive where documented by the managed signature; signed offsets and self-weight multipliers pass unchanged. No implicit unit conversion, section catalog lookup or derived area/inertia is performed. Material-dependent section creation uses the caller-established existing current material context without selecting a default. Engineering consistency, catalog availability and acquired-build behavior remain the caller's qualification responsibility.

Dependent-node inputs enumerate once, stop at 100001 items and reject counts outside 1..100000 before dispatch. Positive IDs must exclude the control node. Order/duplicates are preserved; an owned result copy survives native buffer changes. The relation operation changes model state directly; other definition creators do not silently assign members or plates.

## Functions

### CreateWideFlangePropertyFromTable

```csharp
PropertyMutationResult CreateWideFlangePropertyFromTable(WideFlangeTableDefinition definition)
```
Parameters:

- `definition`: Non-null immutable selection created with a named factory: Standard, Double, Tee, Composite, WithTopPlate, WithBottomPlate or WithTopAndBottomPlates. Country codes 1..16, exact native name and applicable numeric inputs are validated before dispatch.

Returns: Created with a separate positive CreatedPropertyId when STAAD returns a positive reference; the actual integral response and SectionDefinition scope are retained. Native -1 gives NativeFailure; zero and undocumented negatives throw.

When to use: create through the four-slot wide-flange native route while retaining ST=0, D=2, T=5, CM=6, TC=7, BC=8 and TB=9. Copies SP/CT/WP, FC/TH, CW/BW, CD/BT in exact order; unused slots are zero buffer padding. BC sends bottom dimensions in slots zero/one with selector eight. Caller must establish the intended current material context beforehand; no material is selected implicitly. Every call uses the captured Property binding and shared serialization gate, checked before and after access. Native mutations may be partial after a failure or transport/lifetime exception. No retry, implicit save, analysis, unit conversion, assignment, read-back or rollback occurs unless explicitly stated. Native acknowledgement does not certify persistence or engineering adequacy.

```csharp
bool applyChanges = false; // Enable only for a disposable/intended model and reviewed inputs.
if (applyChanges)
{
    var definition = StaadPro.Interop.Models.Property.WideFlangeTableDefinition.WithBottomPlate(Country.Indian, "ISMB300", 0.2, 0.01);
    var result = property.CreateWideFlangePropertyFromTable(definition);
    System.Console.WriteLine(result.Status);
    if (result.IsSuccessful) System.Console.WriteLine(result.NativeFunction);
}
```
### CreateBeamPropertyFromTableEx

```csharp
PropertyMutationResult CreateBeamPropertyFromTableEx(Country country, string sectionName, TableSolidShape shape)
```
Parameters:

- `country`: Reviewed native country code 1..37. The source-only Country.Chilean=38 and undefined selectors are rejected; catalog availability is not inferred.
- `sectionName`: Exact nonblank native table section name without NUL characters. Case/whitespace preserved; no database lookup.
- `shape`: Defined native selector: PlateStrip=1, SolidRectangle=2, SolidRound=3, Round=4 or Cable=5. Not a TypeSpecification modifier or property ID.

Returns: Created with a separate positive CreatedPropertyId when STAAD returns a positive reference; the actual integral response and SectionDefinition scope are retained. Native zero gives NativeFailure; negative/Boolean/fractional responses throw.

When to use: use the native solid-shape table route, whose TypeSpec declaration is documented as TypeSolidShape in the parameter table. It is different from the existing CreateBeamPropertyFromTable modifier/value route. Caller must establish intended current material context beforehand. Every call uses the captured Property binding and shared serialization gate, checked before and after access. Native mutations may be partial after a failure or transport/lifetime exception. No retry, implicit save, analysis, unit conversion, assignment, read-back or rollback occurs unless explicitly stated. Native acknowledgement does not certify persistence or engineering adequacy.

```csharp
bool applyChanges = false; // Enable only for a disposable/intended model and reviewed inputs.
if (applyChanges)
{
    var result = property.CreateBeamPropertyFromTableEx(Country.Indian, "ISMB300", StaadPro.Interop.Enums.Property.TableSolidShape.SolidRectangle);
    System.Console.WriteLine(result.Status);
    if (result.IsSuccessful) System.Console.WriteLine(result.NativeFunction);
}
```
### CreatePrismaticRectangleProperty

```csharp
PropertyMutationResult CreatePrismaticRectangleProperty(double depthY, double widthZ)
```
Parameters:

- `depthY`: YD depth along local Y. Finite positive value in the acquired native length convention; no conversion.
- `widthZ`: ZD width along local Z. Finite positive value in the acquired native length convention; no conversion.

Returns: Created with a separate positive CreatedPropertyId when STAAD returns a positive reference; the actual integral response and SectionDefinition scope are retained. Zero gives NativeFailure; other nonpositive responses are undocumented and throw.

When to use: create an explicit local-Y/local-Z rectangle without a table or private section catalog. Caller establishes the intended existing current material before creation. Every call uses the captured Property binding and shared serialization gate, checked before and after access. Native mutations may be partial after a failure or transport/lifetime exception. No retry, implicit save, analysis, unit conversion, assignment, read-back or rollback occurs unless explicitly stated. Native acknowledgement does not certify persistence or engineering adequacy.

```csharp
bool applyChanges = false; // Enable only for a disposable/intended model and reviewed inputs.
if (applyChanges)
{
    var result = property.CreatePrismaticRectangleProperty(0.4, 0.2);
    System.Console.WriteLine(result.Status);
    if (result.IsSuccessful) System.Console.WriteLine(result.NativeFunction);
}
```
### CreatePrismaticCircleProperty

```csharp
PropertyMutationResult CreatePrismaticCircleProperty(double diameter)
```
Parameters:

- `diameter`: Circle diameter YD, not radius. Finite positive value in the acquired native length convention; no conversion.

Returns: Created with a separate positive CreatedPropertyId when STAAD returns a positive reference; the actual integral response and SectionDefinition scope are retained. Zero gives NativeFailure; undocumented responses throw.

When to use: create a circular section without converting diameter to radius or synthesizing section constants. Caller establishes current material beforehand. Every call uses the captured Property binding and shared serialization gate, checked before and after access. Native mutations may be partial after a failure or transport/lifetime exception. No retry, implicit save, analysis, unit conversion, assignment, read-back or rollback occurs unless explicitly stated. Native acknowledgement does not certify persistence or engineering adequacy.

```csharp
bool applyChanges = false; // Enable only for a disposable/intended model and reviewed inputs.
if (applyChanges)
{
    var result = property.CreatePrismaticCircleProperty(0.3);
    System.Console.WriteLine(result.Status);
    if (result.IsSuccessful) System.Console.WriteLine(result.NativeFunction);
}
```
### CreatePrismaticTeeProperty

```csharp
PropertyMutationResult CreatePrismaticTeeProperty(double totalDepth, double flangeWidth, double stemDepth, double stemWidth)
```
Parameters:

- `totalDepth`: YD total depth from top of flange to bottom of stem. Finite positive value in the acquired native length convention; no conversion.
- `flangeWidth`: ZD flange width. Finite positive value in the acquired native length convention; no conversion.
- `stemDepth`: YB stem depth; must be smaller than totalDepth. Finite positive value in the acquired native length convention; no conversion.
- `stemWidth`: ZB stem width; must not exceed flangeWidth. Finite positive value in the acquired native length convention; no conversion.

Returns: Created with a separate positive CreatedPropertyId when STAAD returns a positive reference; the actual integral response and SectionDefinition scope are retained. Zero gives NativeFailure; undocumented responses throw.

When to use: create a tee using total depth, flange width, stem depth and stem width in exact native order. No flange-thickness or area/inertia input is fabricated. Caller establishes current material beforehand. Every call uses the captured Property binding and shared serialization gate, checked before and after access. Native mutations may be partial after a failure or transport/lifetime exception. No retry, implicit save, analysis, unit conversion, assignment, read-back or rollback occurs unless explicitly stated. Native acknowledgement does not certify persistence or engineering adequacy.

```csharp
bool applyChanges = false; // Enable only for a disposable/intended model and reviewed inputs.
if (applyChanges)
{
    var result = property.CreatePrismaticTeeProperty(0.5, 0.3, 0.45, 0.02);
    System.Console.WriteLine(result.Status);
    if (result.IsSuccessful) System.Console.WriteLine(result.NativeFunction);
}
```
### CreatePrismaticTrapezoidalProperty

```csharp
PropertyMutationResult CreatePrismaticTrapezoidalProperty(double depth, double topWidth, double bottomWidth)
```
Parameters:

- `depth`: YD total depth. Finite positive value in the acquired native length convention; no conversion.
- `topWidth`: ZD width at the top fiber. Finite positive value in the acquired native length convention; no conversion.
- `bottomWidth`: ZB width at the bottom fiber. Finite positive value in the acquired native length convention; no conversion.

Returns: Created with a separate positive CreatedPropertyId when STAAD returns a positive reference; the actual integral response and SectionDefinition scope are retained. Zero gives NativeFailure; undocumented responses throw.

When to use: create a trapezoid without interchanging top and bottom widths or deriving an angle from them. Caller establishes current material beforehand. Every call uses the captured Property binding and shared serialization gate, checked before and after access. Native mutations may be partial after a failure or transport/lifetime exception. No retry, implicit save, analysis, unit conversion, assignment, read-back or rollback occurs unless explicitly stated. Native acknowledgement does not certify persistence or engineering adequacy.

```csharp
bool applyChanges = false; // Enable only for a disposable/intended model and reviewed inputs.
if (applyChanges)
{
    var result = property.CreatePrismaticTrapezoidalProperty(0.5, 0.2, 0.4);
    System.Console.WriteLine(result.Status);
    if (result.IsSuccessful) System.Console.WriteLine(result.NativeFunction);
}
```
### CreatePrismaticGeneralProperty

```csharp
PropertyMutationResult CreatePrismaticGeneralProperty(GeneralPrismaticSectionDefinition definition)
```
Parameters:

- `definition`: Non-null immutable AX, AY, AZ, IX, IY, IZ, YD, ZD, YB, ZB packet in this native order. AX is finite positive; other inputs finite nonnegative. AY/AZ zero retains native ignore-shear-deformation semantics. Areas use length squared, inertia/torsion length to fourth power, dimensions length.

Returns: Created with a separate positive CreatedPropertyId when STAAD returns a positive reference; the actual integral response and SectionDefinition scope are retained. Zero gives NativeFailure; -106/-108 give InvalidInput (native buffer representation/size rejected). Other undocumented responses throw.

When to use: supply complete general-section constants independently from any catalog. Sends a fresh rank-one ten-double buffer. Here native IX is documented as torsional constant; IY/IZ are the local bending inertias. YB/ZB retain the native tee/trapezoid meanings. No area/inertia consistency or engineering-adequacy check is inferred. Caller establishes current material beforehand. Every call uses the captured Property binding and shared serialization gate, checked before and after access. Native mutations may be partial after a failure or transport/lifetime exception. No retry, implicit save, analysis, unit conversion, assignment, read-back or rollback occurs unless explicitly stated. Native acknowledgement does not certify persistence or engineering adequacy.

```csharp
bool applyChanges = false; // Enable only for a disposable/intended model and reviewed inputs.
if (applyChanges)
{
    var definition = new StaadPro.Interop.Models.Property.GeneralPrismaticSectionDefinition(0.01, 0.002, 0.003, 0.000001, 0.00001, 0.0001, 0.3, 0.2, 0.25, 0.02);
    var result = property.CreatePrismaticGeneralProperty(definition);
    System.Console.WriteLine(result.Status);
    if (result.IsSuccessful) System.Console.WriteLine(result.NativeFunction);
}
```
### CreateTaperedTubeProperty

```csharp
PropertyMutationResult CreateTaperedTubeProperty(TaperedTubeShape shape, double startDepth, double endDepth, double thickness)
```
Parameters:

- `shape`: Defined native shape: Round=0, Hexadecagonal=1, Dodecagonal=2, Octagonal=3, Hexagonal=4 or Square=5.
- `startDepth`: Section depth at member start. Finite positive value in the acquired native length convention; no conversion.
- `endDepth`: Section depth at member end. Finite positive value in the acquired native length convention; no conversion.
- `thickness`: Constant section thickness; twice this value must be smaller than both endpoint depths. Finite positive value in the acquired native length convention; no conversion.

Returns: Created with a separate positive CreatedPropertyId when STAAD returns a positive reference; the actual integral response and SectionDefinition scope are retained. Zero gives NativeFailure; undocumented responses throw.

When to use: create a tapered tube while preserving native start/end ordering and shape code. Does not reinterpret polygon dimensions as radii or infer endpoint connectivity. Caller establishes current material beforehand. Every call uses the captured Property binding and shared serialization gate, checked before and after access. Native mutations may be partial after a failure or transport/lifetime exception. No retry, implicit save, analysis, unit conversion, assignment, read-back or rollback occurs unless explicitly stated. Native acknowledgement does not certify persistence or engineering adequacy.

```csharp
bool applyChanges = false; // Enable only for a disposable/intended model and reviewed inputs.
if (applyChanges)
{
    var result = property.CreateTaperedTubeProperty(StaadPro.Interop.Enums.Property.TaperedTubeShape.Round, 0.4, 0.2, 0.01);
    System.Console.WriteLine(result.Status);
    if (result.IsSuccessful) System.Console.WriteLine(result.NativeFunction);
}
```
### CreateMemberPartialReleaseSpec

```csharp
PropertyMutationResult CreateMemberPartialReleaseSpec(MemberEnd memberEnd, DegreeOfFreedomSelection releasedDofs, PartialReleaseFactors factors)
```
Parameters:

- `memberEnd`: Defined native START=0 or END=1 selector; no member ID or geometry is implied.
- `releasedDofs`: Non-null immutable FX/FY/FZ/MX/MY/MZ selection. Sent as an owned six-Int32 array: false=0 (no release), true=1 (release); no basic/partial release-mode inference.
- `factors`: Non-null immutable finite dimensionless six-factor packet in FX/FY/FZ/MX/MY/MZ order. Managed range is zero through one. Every explicit factor is forwarded, including entries for unselected flags; no MP/MPX/MPY/MPZ substitution.

Returns: Succeeded for integral zero; -106/-108 give InvalidInput and -6020 NativeFailure. This reviewed contract returns status only, so CreatedSpecificationId and CreatedSpecificationIndex are always null. Positive returns are undocumented and throw.

When to use: request this distinct native partial-release route. Nearby specification creators return IDs, but this function documents zero OK; do not assign a fabricated zero or discover a last-created ID by count arithmetic. This method creates no analytical-member assignment. Every call uses the captured Property binding and shared serialization gate, checked before and after access. Native mutations may be partial after a failure or transport/lifetime exception. No retry, implicit save, analysis, unit conversion, assignment, read-back or rollback occurs unless explicitly stated. Native acknowledgement does not certify persistence or engineering adequacy.

```csharp
bool applyChanges = false; // Enable only for a disposable/intended model and reviewed inputs.
if (applyChanges)
{
    var dofs = new StaadPro.Interop.Models.Property.DegreeOfFreedomSelection(false, false, false, true, true, true);
    var factors = new StaadPro.Interop.Models.Property.PartialReleaseFactors(0, 0, 0, 0.1, 0.2, 0.3);
    var result = property.CreateMemberPartialReleaseSpec(MemberEnd.End, dofs, factors);
    System.Console.WriteLine(result.Status);
    if (result.IsSuccessful) System.Console.WriteLine(result.NativeFunction);
}
```
### CreateMemberTrussSpec

```csharp
PropertyMutationResult CreateMemberTrussSpec()
```
Returns: Created with a separate positive CreatedSpecificationId when STAAD returns a positive ID; the actual integral response and SpecificationDefinition scope are retained. Native -6010 gives NativeFailure. Zero is undocumented for this ID-returning function and throws rather than being exposed as an ID.

When to use: create the MEMBER TRUSS definition independently of assigning any member. An existing specification may be reused by STAAD; no new-unique allocation or analytical behavior is certified. Every call uses the captured Property binding and shared serialization gate, checked before and after access. Native mutations may be partial after a failure or transport/lifetime exception. No retry, implicit save, analysis, unit conversion, assignment, read-back or rollback occurs unless explicitly stated. Native acknowledgement does not certify persistence or engineering adequacy.

```csharp
bool applyChanges = false; // Enable only for a disposable/intended model and reviewed inputs.
if (applyChanges)
{
    var result = property.CreateMemberTrussSpec();
    System.Console.WriteLine(result.Status);
    if (result.IsSuccessful) System.Console.WriteLine(result.NativeFunction);
}
```
### CreateMemberInactiveSpec

```csharp
PropertyMutationResult CreateMemberInactiveSpec()
```
Returns: Created with a separate positive CreatedSpecificationId when STAAD returns a positive ID; the actual integral response and SpecificationDefinition scope are retained. Native -6011 gives NativeFailure. Zero is undocumented for this ID-returning function and throws rather than being exposed as an ID.

When to use: create the MEMBER INACTIVE definition independently of assigning any member. An existing specification may be reused by STAAD; no new-unique allocation or analytical behavior is certified. Every call uses the captured Property binding and shared serialization gate, checked before and after access. Native mutations may be partial after a failure or transport/lifetime exception. No retry, implicit save, analysis, unit conversion, assignment, read-back or rollback occurs unless explicitly stated. Native acknowledgement does not certify persistence or engineering adequacy.

```csharp
bool applyChanges = false; // Enable only for a disposable/intended model and reviewed inputs.
if (applyChanges)
{
    var result = property.CreateMemberInactiveSpec();
    System.Console.WriteLine(result.Status);
    if (result.IsSuccessful) System.Console.WriteLine(result.NativeFunction);
}
```
### CreateMemberTensionSpec

```csharp
PropertyMutationResult CreateMemberTensionSpec()
```
Returns: Created with a separate positive CreatedSpecificationId when STAAD returns a positive ID; the actual integral response and SpecificationDefinition scope are retained. Native -6012 gives NativeFailure. Zero is undocumented for this ID-returning function and throws rather than being exposed as an ID.

When to use: create the MEMBER TENSION definition independently of assigning any member. An existing specification may be reused by STAAD; no new-unique allocation or analytical behavior is certified. Every call uses the captured Property binding and shared serialization gate, checked before and after access. Native mutations may be partial after a failure or transport/lifetime exception. No retry, implicit save, analysis, unit conversion, assignment, read-back or rollback occurs unless explicitly stated. Native acknowledgement does not certify persistence or engineering adequacy.

```csharp
bool applyChanges = false; // Enable only for a disposable/intended model and reviewed inputs.
if (applyChanges)
{
    var result = property.CreateMemberTensionSpec();
    System.Console.WriteLine(result.Status);
    if (result.IsSuccessful) System.Console.WriteLine(result.NativeFunction);
}
```
### CreateMemberCompressionSpec

```csharp
PropertyMutationResult CreateMemberCompressionSpec()
```
Returns: Created with a separate positive CreatedSpecificationId when STAAD returns a positive ID; the actual integral response and SpecificationDefinition scope are retained. Native -6013 gives NativeFailure. Zero is undocumented for this ID-returning function and throws rather than being exposed as an ID.

When to use: create the MEMBER COMPRESSION definition independently of assigning any member. An existing specification may be reused by STAAD; no new-unique allocation or analytical behavior is certified. Every call uses the captured Property binding and shared serialization gate, checked before and after access. Native mutations may be partial after a failure or transport/lifetime exception. No retry, implicit save, analysis, unit conversion, assignment, read-back or rollback occurs unless explicitly stated. Native acknowledgement does not certify persistence or engineering adequacy.

```csharp
bool applyChanges = false; // Enable only for a disposable/intended model and reviewed inputs.
if (applyChanges)
{
    var result = property.CreateMemberCompressionSpec();
    System.Console.WriteLine(result.Status);
    if (result.IsSuccessful) System.Console.WriteLine(result.NativeFunction);
}
```
### CreateMemberIgnoreStiffSpec

```csharp
PropertyMutationResult CreateMemberIgnoreStiffSpec()
```
Returns: Created with a separate positive CreatedSpecificationId when STAAD returns a positive ID; the actual integral response and SpecificationDefinition scope are retained. Native -6014 gives NativeFailure. Zero is undocumented for this ID-returning function and throws rather than being exposed as an ID.

When to use: create the IGNORE STIFFNESS definition independently of assigning any member. An existing specification may be reused by STAAD; no new-unique allocation or analytical behavior is certified. Every call uses the captured Property binding and shared serialization gate, checked before and after access. Native mutations may be partial after a failure or transport/lifetime exception. No retry, implicit save, analysis, unit conversion, assignment, read-back or rollback occurs unless explicitly stated. Native acknowledgement does not certify persistence or engineering adequacy.

```csharp
bool applyChanges = false; // Enable only for a disposable/intended model and reviewed inputs.
if (applyChanges)
{
    var result = property.CreateMemberIgnoreStiffSpec();
    System.Console.WriteLine(result.Status);
    if (result.IsSuccessful) System.Console.WriteLine(result.NativeFunction);
}
```
### CreateMemberCableSpec

```csharp
PropertyMutationResult CreateMemberCableSpec(CableDefinitionKind definitionKind, double value)
```
Parameters:

- `definitionKind`: InitialTension=0 or UnstressedLength=1. This selector changes the dimension of value; no Boolean inversion or unit conversion.
- `value`: Finite nonnegative initial tension in native force units, or finite positive unstressed length in native length units, according to definitionKind.

Returns: Created with a separate positive CreatedSpecificationId when STAAD returns a positive ID; the actual integral response and SpecificationDefinition scope are retained. Native -6015 gives NativeFailure. Zero is undocumented for this ID-returning function and throws rather than being exposed as an ID.

When to use: create the basic cable definition without advanced tension-end/self-weight options. Does not activate cable analysis or assign a member. Every call uses the captured Property binding and shared serialization gate, checked before and after access. Native mutations may be partial after a failure or transport/lifetime exception. No retry, implicit save, analysis, unit conversion, assignment, read-back or rollback occurs unless explicitly stated. Native acknowledgement does not certify persistence or engineering adequacy.

```csharp
bool applyChanges = false; // Enable only for a disposable/intended model and reviewed inputs.
if (applyChanges)
{
    var result = property.CreateMemberCableSpec(StaadPro.Interop.Enums.Property.CableDefinitionKind.InitialTension, 25.0);
    System.Console.WriteLine(result.Status);
    if (result.IsSuccessful) System.Console.WriteLine(result.NativeFunction);
}
```
### CreateMemberCableSpecEx

```csharp
PropertyMutationResult CreateMemberCableSpecEx(CableDefinitionKind definitionKind, double value, CableTensionEnd tensionEnd, double selfWeightFactorX, double selfWeightFactorY, double selfWeightFactorZ)
```
Parameters:

- `definitionKind`: InitialTension=0 or UnstressedLength=1 as specified by all three reviewed native parameter tables; despite the declaration spelling varbTension, this is sent as an integral selector.
- `value`: Finite nonnegative tension in native force units, or finite positive unstressed length in native length units.
- `tensionEnd`: Unspecified=0, Start=1 or End=2 for initial tension. Must be Unspecified for UnstressedLength; not the MemberEnd 0/1 domain.
- `selfWeightFactorX`: Finite signed dimensionless multiplier for the global-X self-weight component, forwarded unchanged.
- `selfWeightFactorY`: Finite signed dimensionless multiplier for the global-Y self-weight component, forwarded unchanged.
- `selfWeightFactorZ`: Finite signed dimensionless multiplier for the global-Z self-weight component, forwarded unchanged.

Returns: Created with a separate positive CreatedSpecificationId when STAAD returns a positive ID; the actual integral response and SpecificationDefinition scope are retained. Native -6015 gives NativeFailure. Zero is undocumented for this ID-returning function and throws rather than being exposed as an ID.

When to use: create the advanced cable definition without changing global vertical axis, enabling advanced analysis or assuming gravity direction. Explicit self-weight multipliers are not forces and are not normalized. Every call uses the captured Property binding and shared serialization gate, checked before and after access. Native mutations may be partial after a failure or transport/lifetime exception. No retry, implicit save, analysis, unit conversion, assignment, read-back or rollback occurs unless explicitly stated. Native acknowledgement does not certify persistence or engineering adequacy.

```csharp
bool applyChanges = false; // Enable only for a disposable/intended model and reviewed inputs.
if (applyChanges)
{
    var result = property.CreateMemberCableSpecEx(StaadPro.Interop.Enums.Property.CableDefinitionKind.InitialTension, 25.0, StaadPro.Interop.Enums.Property.CableTensionEnd.Start, 0.0, -1.0, 0.0);
    System.Console.WriteLine(result.Status);
    if (result.IsSuccessful) System.Console.WriteLine(result.NativeFunction);
}
```
### CreateElementNodeReleaseSpec

```csharp
PropertyMutationResult CreateElementNodeReleaseSpec(int nodeId, DegreeOfFreedomSelection releasedDofs)
```
Parameters:

- `nodeId`: Positive analytical node number as documented by this native function; not a plate corner index or member-end selector.
- `releasedDofs`: Non-null immutable FX/FY/FZ/MX/MY/MZ selection. Sent as an owned six-Int32 array: false=0 (no release), true=1 (release); no basic/partial release-mode inference.

Returns: Succeeded for integral zero; -106/-108 give InvalidInput and -6021 NativeFailure. TargetIds retains the requested node. No created specification ID/index is supplied; positive returns are undocumented and throw.

When to use: request the native node release definition without borrowing another creator's ID-return convention. No implicit plate assignment or corner/node remapping occurs. Every call uses the captured Property binding and shared serialization gate, checked before and after access. Native mutations may be partial after a failure or transport/lifetime exception. No retry, implicit save, analysis, unit conversion, assignment, read-back or rollback occurs unless explicitly stated. Native acknowledgement does not certify persistence or engineering adequacy.

```csharp
bool applyChanges = false; // Enable only for a disposable/intended model and reviewed inputs.
if (applyChanges)
{
    var dofs = new StaadPro.Interop.Models.Property.DegreeOfFreedomSelection(false, false, false, true, true, true);
    var result = property.CreateElementNodeReleaseSpec(11, dofs);
    System.Console.WriteLine(result.Status);
    if (result.IsSuccessful) System.Console.WriteLine(result.NativeFunction);
}
```
### CreateElementPlaneStressSpec

```csharp
PropertyMutationResult CreateElementPlaneStressSpec()
```
Returns: Created with a separate positive CreatedSpecificationId when STAAD returns a positive ID; the actual integral response and SpecificationDefinition scope are retained. Native -6018 gives NativeFailure. Zero is undocumented for this ID-returning function and throws rather than being exposed as an ID.

When to use: create the ELEMENT PLANE STRESS definition without assigning analytical elements. Use a separately reviewed assignment route and the returned positive specification ID when applying it. Every call uses the captured Property binding and shared serialization gate, checked before and after access. Native mutations may be partial after a failure or transport/lifetime exception. No retry, implicit save, analysis, unit conversion, assignment, read-back or rollback occurs unless explicitly stated. Native acknowledgement does not certify persistence or engineering adequacy.

```csharp
bool applyChanges = false; // Enable only for a disposable/intended model and reviewed inputs.
if (applyChanges)
{
    var result = property.CreateElementPlaneStressSpec();
    System.Console.WriteLine(result.Status);
    if (result.IsSuccessful) System.Console.WriteLine(result.NativeFunction);
}
```
### CreateElementIgnoreInplaneRotnSpec

```csharp
PropertyMutationResult CreateElementIgnoreInplaneRotnSpec()
```
Returns: Created with a separate positive CreatedSpecificationId when STAAD returns a positive ID; the actual integral response and SpecificationDefinition scope are retained. Native -6019 gives NativeFailure. Zero is undocumented for this ID-returning function and throws rather than being exposed as an ID.

When to use: create the ELEMENT IGNORE INPLANE ROTATION definition without assigning analytical elements. Use a separately reviewed assignment route and the returned positive specification ID when applying it. Every call uses the captured Property binding and shared serialization gate, checked before and after access. Native mutations may be partial after a failure or transport/lifetime exception. No retry, implicit save, analysis, unit conversion, assignment, read-back or rollback occurs unless explicitly stated. Native acknowledgement does not certify persistence or engineering adequacy.

```csharp
bool applyChanges = false; // Enable only for a disposable/intended model and reviewed inputs.
if (applyChanges)
{
    var result = property.CreateElementIgnoreInplaneRotnSpec();
    System.Console.WriteLine(result.Status);
    if (result.IsSuccessful) System.Console.WriteLine(result.NativeFunction);
}
```
### AddControlDependentRelation

```csharp
PropertyMutationResult AddControlDependentRelation(int controlNodeId, ControlDependencyMode mode, DegreeOfFreedomSelection rigidDofs, IEnumerable<int> dependentNodeIds)
```
Parameters:

- `controlNodeId`: Positive analytical control-node ID. Must not appear in dependentNodeIds.
- `mode`: AllDirections=0, RigidXY=1, RigidYZ=2, RigidZX=3 or SpecifiedDegreesOfFreedom=4. Four selects the documented other-values branch; no arbitrary native selector is guessed.
- `rigidDofs`: Non-null immutable explicit FX/FY/FZ/MX/MY/MZ rigidity flags. Sent as six integral 0/1 scalars in that order for every mode; only the explicit-DOF native mode uses them as its selection.
- `dependentNodeIds`: Finite sequence of 1..100000 positive analytical dependent-node IDs excluding controlNodeId. Enumerated once with a hard bound, copied and sent as a rank-one Int32 array. Input order/duplicates retained; no geometry lookup or deduplication.

Returns: Succeeded for integral zero, InvalidInput for -106 and NativeFailure for -6029. Retains separate control-node identity and owned dependent TargetIds. No specification ID/index or per-node completion result is inferred.

When to use: deliberately add the native relation to the acquired model, rather than creating a free-standing specification for later assignment. A native failure can leave part of the relation applied. The rigidity coordinate meanings follow the native selected mode; no transforms occur. Every call uses the captured Property binding and shared serialization gate, checked before and after access. Native mutations may be partial after a failure or transport/lifetime exception. No retry, implicit save, analysis, unit conversion, assignment, read-back or rollback occurs unless explicitly stated. Native acknowledgement does not certify persistence or engineering adequacy.

```csharp
bool applyChanges = false; // Enable only for a disposable/intended model and reviewed inputs.
if (applyChanges)
{
    var dofs = new StaadPro.Interop.Models.Property.DegreeOfFreedomSelection(true, true, true, false, false, false);
    var result = property.AddControlDependentRelation(11, StaadPro.Interop.Enums.Property.ControlDependencyMode.SpecifiedDegreesOfFreedom, dofs, new[] { 12, 13 });
    System.Console.WriteLine(result.Status);
    if (result.IsSuccessful) System.Console.WriteLine(result.NativeFunction);
}
```
### CreateElementOffsetSpec

```csharp
PropertyMutationResult CreateElementOffsetSpec(ElementNodeOffsetAxis coordinateSystem, int plateNodeIndex, double offsetX, double offsetY, double offsetZ)
```
Parameters:

- `coordinateSystem`: ElementNodeOffsetAxis.Local=0 or Global=1 only. Z=2 is rejected; this numbering differs from member OffsetAxis.
- `plateNodeIndex`: Native plate corner index 1..4, not an analytical node ID. The result retains this corner identity.
- `offsetX`: Finite signed X offset in native length units and the explicit coordinateSystem.
- `offsetY`: Finite signed Y offset in native length units and the explicit coordinateSystem.
- `offsetZ`: Finite signed Z offset in native length units and the explicit coordinateSystem.

Returns: Created with a separate positive CreatedSpecificationId when STAAD returns a positive ID; the actual integral response and SpecificationDefinition scope are retained. Native -6016 gives NativeFailure; zero and other undocumented values throw.

When to use: create the per-corner local/global offset definition without transforming coordinates, inventing a plate incidence list or applying the definition to a plate. Every call uses the captured Property binding and shared serialization gate, checked before and after access. Native mutations may be partial after a failure or transport/lifetime exception. No retry, implicit save, analysis, unit conversion, assignment, read-back or rollback occurs unless explicitly stated. Native acknowledgement does not certify persistence or engineering adequacy.

```csharp
bool applyChanges = false; // Enable only for a disposable/intended model and reviewed inputs.
if (applyChanges)
{
    var result = property.CreateElementOffsetSpec(StaadPro.Interop.Enums.Property.ElementNodeOffsetAxis.Local, 2, 0.0, -0.1, 0.2);
    System.Console.WriteLine(result.Status);
    if (result.IsSuccessful) System.Console.WriteLine(result.NativeFunction);
}
```
### CreateElementLocalZOffsetSpec

```csharp
PropertyMutationResult CreateElementLocalZOffsetSpec(double node1Offset, double node2Offset, double node3Offset, double node4Offset)
```
Parameters:

- `node1Offset`: Finite signed local-Z offset at native plate corner 1, in native length units. Corner order is retained; no analytical node ID is supplied.
- `node2Offset`: Finite signed local-Z offset at native plate corner 2, in native length units. Corner order is retained; no analytical node ID is supplied.
- `node3Offset`: Finite signed local-Z offset at native plate corner 3, in native length units. Corner order is retained; no analytical node ID is supplied.
- `node4Offset`: Finite signed local-Z offset at native plate corner 4, in native length units. Corner order is retained; no analytical node ID is supplied.

Returns: Created with a separate positive CreatedSpecificationId when STAAD returns a positive ID; the actual integral response and SpecificationDefinition scope are retained. Native -6016 gives NativeFailure; zero and undocumented values throw.

When to use: create an explicit offset for all four native plate corner slots. All four inputs are mandatory; no triangle-corner duplication, uniform-thickness assumption or global/local transformation is inferred. Every call uses the captured Property binding and shared serialization gate, checked before and after access. Native mutations may be partial after a failure or transport/lifetime exception. No retry, implicit save, analysis, unit conversion, assignment, read-back or rollback occurs unless explicitly stated. Native acknowledgement does not certify persistence or engineering adequacy.

```csharp
bool applyChanges = false; // Enable only for a disposable/intended model and reviewed inputs.
if (applyChanges)
{
    var result = property.CreateElementLocalZOffsetSpec(0.1, 0.2, -0.3, 0.4);
    System.Console.WriteLine(result.Status);
    if (result.IsSuccessful) System.Console.WriteLine(result.NativeFunction);
}
```
### CreateMemberFireProofingSpec

```csharp
PropertyMutationResult CreateMemberFireProofingSpec(FireProofingType fireProofingType, double thickness, double density)
```
Parameters:

- `fireProofingType`: Defined native Block/BFP=1 or Contour/CFP=2.
- `thickness`: Fireproofing thickness. Finite positive value in the acquired native length convention; no conversion.
- `density`: Finite nonnegative fireproofing density in the acquired native density convention; no mass/weight or force/length conversion is inferred.

Returns: Created for any nonnegative integral return, including zero, with CreatedSpecificationIndex set to that exact zero-based index. -1 gives NativeFailure. CreatedSpecificationId is always null; undocumented negatives/Boolean/fractional responses throw.

When to use: create a fireproofing definition without assuming its creation index is the ID or one-based index used by other native getters. Never add one or pass it as an assignment reference without separately qualified mapping. No member assignment or fire-analysis activation occurs. Every call uses the captured Property binding and shared serialization gate, checked before and after access. Native mutations may be partial after a failure or transport/lifetime exception. No retry, implicit save, analysis, unit conversion, assignment, read-back or rollback occurs unless explicitly stated. Native acknowledgement does not certify persistence or engineering adequacy.

```csharp
bool applyChanges = false; // Enable only for a disposable/intended model and reviewed inputs.
if (applyChanges)
{
    var result = property.CreateMemberFireProofingSpec(StaadPro.Interop.Enums.Property.FireProofingType.Block, 0.03, 12.0);
    System.Console.WriteLine(result.Status);
    if (result.IsSuccessful) System.Console.WriteLine(result.NativeFunction);
}
```
### GetPropertyUniqueID

```csharp
PropertyQueryResult<PropertyUniqueIdentifier> GetPropertyUniqueID(int propertyId)
```
Parameters:

- `propertyId`: Positive native property reference number, not a member ID or specification index.

Returns: Success with an immutable requested property ID and exact nonempty string. Null/DBNull/empty gives EmptyOrUnavailable because missing metadata and missing property cannot be distinguished. Non-string values throw.

When to use: inspect property metadata without assuming GUID syntax or global uniqueness. Case and whitespace are retained; the string is not parsed or converted into a numerical entity identity. Inspect IsSuccessful/Status before Data. Uses the captured binding/gate; no native mutation, conversion, save or hydration occurs.

```csharp
var result = property.GetPropertyUniqueID(37);
if (result.IsSuccessful) System.Console.WriteLine(result.Data.Value);
else System.Console.WriteLine(result.Status);
```
### SetPropertyUniqueID

```csharp
PropertyMutationResult SetPropertyUniqueID(int propertyId, string uniqueId)
```
Parameters:

- `propertyId`: Positive existing native property reference number, not a member ID or created specification index.
- `uniqueId`: Exact non-null string without embedded NUL characters. Empty and whitespace strings are intentionally forwarded unchanged; no GUID format, generation, collision check or normalization is imposed.

Returns: InvokedWithoutAcknowledgement after the native void call returns. IsSuccessful remains false because no success acknowledgement was supplied. NativeReturnCode/NativeBooleanReturn and all created identities are null. Requested PropertyId and UniqueId are retained; no persistence read-back is implied.

When to use: deliberately write the exact property metadata string through the reviewed void setter. A completed invocation is not a reported failure, but cannot certify persistence. Call GetPropertyUniqueID separately when read-back is required; this method does not automatically read or overwrite more state. Every call uses the captured Property binding and shared serialization gate, checked before and after access. Native mutations may be partial after a failure or transport/lifetime exception. No retry, implicit save, analysis, unit conversion, assignment, read-back or rollback occurs unless explicitly stated. Native acknowledgement does not certify persistence or engineering adequacy.

```csharp
bool applyChanges = false; // Enable only for a disposable/intended model and reviewed inputs.
if (applyChanges)
{
    var result = property.SetPropertyUniqueID(37, "reviewed-property-identifier");
    System.Console.WriteLine(result.Status);
    System.Console.WriteLine(result.PropertyId); // Void invocation supplies no success acknowledgement.
}
```
