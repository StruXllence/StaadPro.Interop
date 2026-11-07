# Main-source porting status

**Reviewed: 7 November 2026.** There were **145 active main-source interface overloads pending** before the three View ports in this increment; **142 remain afterwards**. The main project was read-only.

## Count by domain

| Domain | Reference overloads | Covered before | Pending before | Pending after | Excluded obsolete/deprecated |
| --- | --- | --- | --- | --- | --- |
| Root | 23 | 23 | 0 | 0 | 0 |
| Geometry | 143 | 143 | 0 | 0 | 0 |
| Load | 115 | 115 | 0 | 0 | 0 |
| Property | 103 | 98 | 0 | 0 | 5 |
| Output | 86 | 48 | 38 | 38 | 0 |
| Support | 33 | 3 | 28 | 28 | 2 |
| Design | 7 | 7 | 0 | 0 | 0 |
| Commands | 21 | 17 | 3 | 3 | 1 |
| Table | 29 | 29 | 0 | 0 | 0 |
| View | 70 | 18 | 52 | 49 | 0 |
| IDEA StatiCa | 24 | 0 | 24 | 24 | 0 |
| **Total** | **654** | **501** | **145** | **142** | **8** |

Covered after this increment: **504** reference overloads. One managed replacement can cover more than one source signature; this measures usable source workflows and their overloads, not identical binary contracts or a count of public managed methods. New enabling methods are not subtracted again from this inventory.

## What is included

- Output: 38 pending overloads, comprising 33 asynchronous wrappers and five synchronous functions/conveniences: GetBucklingFactor(int), GetBucklingModeDisplacement(int, Node), both GetResultantForceAlongLineForPlateList overloads, and GetPlateStressAtPoint. Pending asynchronous APIs require caller-apartment design; the reference Task.Run implementation will not be copied without review.
- Support: 28 active source workflows remain, principally typed support-instance creation/assignment, entity population, grouped discovery and plate-mat compositions. The existing scalar count, supported-node ID and support-type observations cover their renamed source equivalents. Native primitives alone do not complete the higher-level workflows.
- Commands: CreateSteelDesignCommand, ValidateSteelDesignCommand and GetSteelDesignCommands remain. The deprecated PerformPDeltaAnalysisConverge overload is excluded, not reported as a missing supported API.
- View: 49 of 70 overloads remain after `ShowMembers(IEnumerable<Beam> beams)`, `HideMember(Beam beam)` and `HideMembers(IEnumerable<Beam> beams)`.
- IDEA StatiCa: all 24 domain-interface overloads remain. Prerequisite models, options, transformation and export helpers are dependencies, not extra domain functions added to this total.

## Existing replacements and exclusions

Output availability, buckling count, integer-node buckling displacement and support-reaction functions already have typed equivalents. TransformPlateCornerForcesToMemberLocalPlateResult is covered by the named PlateCornerMemberForces result, which retains the source identity and transformed components without mislabelling them as global forces. Those operations are not counted as missing solely because the names or return models differ.

Property has all 98 active interface routes represented, including the explicit native-binding/namespace adaptations and typed-return collisions. The four additional adapter-only material-assignment collection queries are present too. Five obsolete Property overloads are excluded: two inactive-member queries, two element-offset helpers and CreateMaterialPropertyEx. Two obsolete Support elastic-mat discovery overloads are excluded. Together with the deprecated Commands overload, there are eight exclusions.

## Separate work and counting limits

This is a count of reviewed main **domain-interface overloads**. It excludes constructors, wrapper/provider members already handled by their own parity review, properties, private helpers, implementation-service methods, future dependency APIs and native-only functions. It does not claim every public method in every private implementation class has been ported. Main-source porting has priority over native-only expansion.

The nine documented Property gates and fifteen supplemental Property candidates remain separate review work. Parked Load additions and qualification-driven changes still require explicit approval. The eight Support, thirteen Output and forty Design native gates overlap some source workflows and must not simply be added to the 142 count. Native COM, UI, marshaling and engineering qualification are separate from managed implementation coverage.

[Machine-readable method inventory and source fingerprints](data/OpenSTAAD-Main-Source-Porting-Status.json) · [Remaining-domain sequence](OpenSTAAD-Remaining-Adapters-Review-Index.md) · [View implementation](../View-Orientations.md)
