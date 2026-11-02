<img src="logo-with-text.png" alt="StaadPro.Interop" width="400" />

# StaadPro.Interop

By **Srinivasa Rao Masanam**. Published by **StruXllence**.

[![YouTube @StruXllence](https://img.shields.io/badge/YouTube-%40StruXllence-FF0000?style=flat-square&logo=youtube&logoColor=white)](https://www.youtube.com/@StruXllence) [![Website — coming soon](https://img.shields.io/badge/Website-Coming%20soon-64748B?style=flat-square)](https://struxllence.com) [![Community forum](https://img.shields.io/badge/Community-Forum-0866C6?style=flat-square)](https://forum.struxllence.com) [![Revamp forum — preview](https://img.shields.io/badge/Revamp%20forum-Preview-7657B5?style=flat-square)](https://new-forum.struxllence.com)

The main website is coming soon. Join the community forum for structural-engineering discussions, explore the forum preview, or follow @StruXllence on YouTube. Report package defects through [GitHub Issues](https://github.com/StruXllence/StaadPro.Interop/issues).

**Automate STAAD.Pro from C# with strongly typed OpenSTAAD APIs.**

[![Build and Test](https://github.com/StruXllence/StaadPro.Interop/actions/workflows/build-and-test.yml/badge.svg)](https://github.com/StruXllence/StaadPro.Interop/actions/workflows/build-and-test.yml) [![NuGet preview](https://img.shields.io/nuget/vpre/StaadPro.Interop?style=flat-square)](https://www.nuget.org/packages/StaadPro.Interop) [![License: Apache-2.0](https://img.shields.io/badge/License-Apache%202.0-blue?style=flat-square)](LICENSE) [![Runtime: .NET Framework 4.8.1](https://img.shields.io/badge/.NET_Framework-4.8.1-512BD4?style=flat-square)](docs/Installation.md)

A strongly-typed .NET Framework wrapper for Bentley STAAD.Pro OpenSTAAD. Work with geometry, loads, properties, supports, commands, design queries, report tables and analysis results through managed interfaces and structural entities.

- **Build engineering tools in C#:** use typed nodes, beams, plates, load cases and result models.
- **Start from documented workflows:** explore the API guides and packaged XML IntelliSense examples.
- **Contribute without STAAD.Pro:** build the library and run offline unit tests without Bentley binaries or a STAAD license.

[Install](docs/Installation.md) · [API reference](docs/API-Reference.md) · [Contribute](CONTRIBUTING.md) · [Roadmap](ROADMAP.md) · [Report an issue](https://github.com/StruXllence/StaadPro.Interop/issues/new/choose)

## Installation and runtime

Runtime requires **Windows, .NET Framework 4.8.1 (net481), and a licensed STAAD.Pro installation** exposing the APIs used by your application. The .NET SDK used to build the project is separate from the application's target framework.

The latest locally prepared preview is **1.0.0-preview.20261102.1**. Check [NuGet](https://www.nuget.org/packages/StaadPro.Interop) for versions already published; the current prepared archive is available for manual upload. Follow [installation instructions](docs/Installation.md) for local-feed and exact public-version commands. No Bentley binaries are bundled.

## Quick start: read an open model

Open a licensed STAAD.Pro model first. In a Windows application targeting net481 and referencing StaadPro.Interop:

~~~csharp
using System;
using StaadPro.Interop.Enums;
using StaadPro.Interop.Services;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        using (var wrapper = OpenStaadWrapperProvider.GetRunning())
        {
            if (wrapper?.Load == null)
                throw new InvalidOperationException("Open a STAAD model exposing OpenSTAAD first.");

            foreach (var loadCase in wrapper.Load.GetLoadCasesOfType(LoadCaseType.PrimaryLoad))
                Console.WriteLine(loadCase.Id + ": " + loadCase.Title);
        }
    }
}
~~~

This attaches to an existing instance and reads primary load-case metadata. It does not create or activate a load case. Wrapper disposal releases managed references without closing STAAD.Pro. Serialize COM access on the application's established apartment; see [connection and lifecycle guidance](docs/Installation.md#acquire-a-specific-model).

## API guides

| Area | Explore |
| --- | --- |
| Connect and manage models | [Wrapper](docs/Wrapper-Porting.md) · [Root](docs/Root-Porting.md) |
| Nodes, beams, plates and groups | [Geometry](docs/Geometry-Porting.md) |
| Load cases and assignments | [Load](docs/Load-Porting.md) · [query contracts](docs/Load-Queries.md) |
| Materials, sections and specifications | [Property](docs/Property-Porting.md) |
| Analysis results and steel-design observations | [Output](docs/Output-Porting.md) |
| Supports and analysis commands | [Support and Commands](docs/Commands-and-Support-Porting.md) |
| Design briefs and assignments | [Design](docs/Design-Porting.md) |
| Report tables and formatting | [Table](docs/Table-Porting.md) |
| Graphical orientation | [View](docs/View-Orientations.md) |

## View orientations

Use `wrapper.View.ShowFront()`, `ShowBack()` or `ShowLeft()` for the acquired instance's active graphical direction. Each returns a named unacknowledged completion receipt; no model-coordinate change, analysis or implicit save occurs. See [the View guide](docs/View-Orientations.md). View currently exposes 3/70 main-source overloads; remaining work is listed in the [roadmap](ROADMAP.md).

## Compatibility and preview status

The current implementation passed **6,964 offline tests** and **1,286 installed-package XML examples**. These checks validate managed behavior and distribution, not every native STAAD build, COM response or engineering-unit convention. Consult method-specific version, result-state, unit and side-effect contracts before using results in an engineering workflow.

The package remains a preview. [Release notes](docs/Release-Notes.md) identify the API scope, and [NuGet readiness](docs/NuGet-Readiness.md) records validation limits. Earlier public packages retain the license terms shipped with them.

## Contribute and get help

Documentation improvements, offline regression tests, examples and reproducible bug reports are welcome. Read [CONTRIBUTING](CONTRIBUTING.md), fork the repository for your change, or [propose a focused issue](https://github.com/StruXllence/StaadPro.Interop/issues/new/choose). A STAAD license is needed only for live integration work.

~~~powershell
git clone https://github.com/StruXllence/StaadPro.Interop.git
cd StaadPro.Interop
dotnet build StaadPro.Interop.sln -c Release
dotnet test tests/StaadPro.Interop.Tests/StaadPro.Interop.Tests.csproj -c Release --no-build
~~~

Use the [primary forum](https://forum.struxllence.com) for engineering discussions and [GitHub Issues](https://github.com/StruXllence/StaadPro.Interop/issues) for package defects. Read the [code of conduct](CODE_OF_CONDUCT.md). If this library helps your workflow, star the repository to follow future work; useful examples and contributions help it grow.

## License and attribution

[Apache License 2.0](LICENSE). **Copyright (c) 2026 StruXllence**. Original author: **Srinivasa Rao Masanam**. Keep applicable license and attribution notices and mark modified files as required. The retained [MIT-origin notice](LICENSES/MIT-legacy.txt) applies where relevant. See [licensing guidance](docs/Licensing.md), [NOTICE](NOTICE.md), [citation](CITATION.cff) and [name/logo guidance](TRADEMARKS.md).

## Independence and trademarks

StaadPro.Interop is independent and is not affiliated with, sponsored by, endorsed by or officially supported by Bentley Systems, Incorporated or its subsidiaries or affiliates. Bentley, STAAD, STAAD.Pro and OpenSTAAD are trademarks or registered trademarks of their respective owners. Names identify interoperability and do not imply endorsement. A separately licensed STAAD.Pro installation is required; see [NOTICE](NOTICE.md).
