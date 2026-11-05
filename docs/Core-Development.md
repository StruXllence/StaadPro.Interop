# StaadPro.Interop.Core development

StaadPro.Interop.Core is a separate Windows .NET 8 library, not an additional target of the Framework project. Its solution, project, assembly, package ID and public namespaces use StaadPro.Interop.Core. The test project is StaadPro.Interop.Core.Tests.

## Branch and source relationship

The staadpro-interop-core branch was recreated directly from main at 20821c50c9e1e3a3955d7e111cdf906d5b921588. The previously developed session-provider implementation was preserved from main-wrapper-redesign at 0cb47b9726724075d39e23f8c3eca2fb908076f4 and applied as source changes, without merging that branch's history. The earlier implementation remains available on its original branch.

The Core project retains the current adapters, wrapper properties, managed models, XML documentation and offline fixtures from the main baseline. It adds the previously developed OpenStaadSessionProvider, acquisition models/environment, modern Windows native OLE discovery, STA ownership checks, retained roots and atomic target-file preparation. The existing public wrapper remains named OpenStaadWrapper; its namespace and assembly are Core-specific. Framework development is paused and its separate checkout and local drafts are preserved.

## Distinct identities

| Item | Core | Framework |
| --- | --- | --- |
| Active branch | staadpro-interop-core | main |
| Root folder | Staad Interop Core | Staad Interop |
| Solution | StaadPro.Interop.Core.sln | StaadPro.Interop.sln |
| Project | src/StaadPro.Interop.Core/StaadPro.Interop.Core.csproj | src/StaadPro.Interop/StaadPro.Interop.csproj |
| Package / DLL | StaadPro.Interop.Core | StaadPro.Interop |
| Namespace root | StaadPro.Interop.Core | StaadPro.Interop |
| Target | net8.0-windows only | net481 |

Core has no ProjectReference or package dependency on the Framework project. It owns its own source tree and produces only a Windows .NET 8 library asset. Native STAAD.Pro remains a separately installed Windows dependency; Core does not mean cross-platform OpenSTAAD. WPF remains necessary because existing entity contracts expose Windows Desktop geometry types.

## Build, test and package

~~~powershell
dotnet restore StaadPro.Interop.Core.sln
dotnet build StaadPro.Interop.Core.sln -c Release --no-restore
dotnet test tests/StaadPro.Interop.Core.Tests/StaadPro.Interop.Core.Tests.csproj -c Release --no-build --filter "TestCategory!=LiveIntegration"
dotnet pack src/StaadPro.Interop.Core/StaadPro.Interop.Core.csproj -c Release --no-build -o artifacts/release
./eng/Verify-CorePackage.ps1 -PackagePath artifacts/release/StaadPro.Interop.Core.1.0.0-preview.20261105.nupkg
~~~

Use the .NET 8 SDK and Windows Desktop/WPF runtime. CI builds only the Core solution on this branch, excludes live integration tests, packs the Core ID and checks a fresh PackageReference consumer. Package verification also rejects net481/multiple library assets, a dependency on the Framework package and old Framework assembly/namespace collisions. It compiles guarded provider documentation without starting STAAD.

The Framework path-query test assumed legacy rejection of embedded NUL characters. Core retains the lexical modern Path API behavior already handled by the earlier Core implementation; test expectations follow that runtime contract. No production path-query behavior was changed to satisfy a Framework-only assumption.

## Verification checkpoint

The Windows .NET 8 Release build passed with zero warnings/errors. All **7,263 offline tests** passed, including the inherited adapter suite and preserved acquisition tests. A fresh PackageReference-only consumer compiled and executed **1,317 selected XML examples**, checked the independent Core assembly/namespace, rejected a Framework dependency and verified the single Windows .NET 8/WPF asset. The provider example is compiled behind an explicit disabled guard, so package verification does not start STAAD.

## Native acquisition and qualification

Create, use and dispose OpenStaadSessionProvider on one Windows STA thread. Get returns the Core OpenStaadWrapper directly; Acquire returns a named result carrying that wrapper, process identity, outcome and diagnostics. A returned existing blank remains blank; it does not implicitly open the target into that instance. Default attachment does not launch. Native errors, discovery limits, synchronous COM hangs and external UI changes retain the documented boundaries.

The legacy provider also uses the preserved modern OLE discovery adapter in Core. The original Framework-only Marshal.GetActiveObject helper is not included in this project. The target-file mutex name remains shared with compatible acquisition clients so that a namespace rename does not defeat target coordination.

The full managed suite validates the renamed Core surface and preserved acquisition policies. Historical STAAD.Pro 2026 native evidence is retained as earlier evidence, not presented as fresh native qualification of this combined project. Run live integration tests only against controlled models with explicit intent; normal tests and package verification do not launch or mutate a STAAD instance.

## Development conventions

Keep adapter signatures in their interfaces and implementations in one non-partial adapter per domain. Reuse existing models/enums and named result classes; expose no tuples or public reference outputs. Keep helpers in coherent internal service folders. The external ReInvented.StaadPro.Interop source remains strictly read-only.

Use StruXllence as Git author/committer; package Authors remains Srinivasa Rao Masanam and publisher/company remains StruXllence. Every requested commit also includes a push unless explicitly instructed otherwise. Commit descriptions use past-tense wording without repeatedly prefacing statements with I.

The old StaadPro.Interop.Core.Testing artifacts remain historical tests of a previous assembly/namespace. They are not the new Core package and are not rebuilt or overwritten by this project. Publish the new Core ID only through the separately reviewed [publishing process](Publishing.md).
