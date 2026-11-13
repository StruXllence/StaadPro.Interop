<img src="logo-with-text.png" alt="StaadPro.Interop" width="400" />

# StaadPro.Interop

By **Srinivasa Rao Masanam**. Published by **StruXllence**.

[![YouTube @StruXllence](https://img.shields.io/badge/YouTube-%40StruXllence-FF0000?style=flat-square&logo=youtube&logoColor=white)](https://www.youtube.com/@StruXllence) [![Website — live](https://img.shields.io/badge/Website-Live-16A34A?style=flat-square)](https://struxllence.com) [![Community forum](https://img.shields.io/badge/Community-Forum-0866C6?style=flat-square)](https://forum.struxllence.com) [![Revamp forum — preview](https://img.shields.io/badge/Revamp%20forum-Preview-7657B5?style=flat-square)](https://new-forum.struxllence.com)

The [StruXllence website](https://struxllence.com) is live. Join the community forum for structural-engineering discussions, explore the forum preview, or follow @StruXllence on YouTube. Report package defects through [GitHub Issues](https://github.com/StruXllence/StaadPro.Interop/issues).

**Automate STAAD.Pro from C# with strongly typed OpenSTAAD APIs.**

[![Build and Test](https://github.com/StruXllence/StaadPro.Interop/actions/workflows/build-and-test.yml/badge.svg)](https://github.com/StruXllence/StaadPro.Interop/actions/workflows/build-and-test.yml) [![NuGet preview](https://img.shields.io/nuget/vpre/StaadPro.Interop?style=flat-square)](https://www.nuget.org/packages/StaadPro.Interop) [![License: Apache-2.0](https://img.shields.io/badge/License-Apache%202.0-blue?style=flat-square)](LICENSE) [![Runtime: .NET Framework 4.8.1](https://img.shields.io/badge/.NET_Framework-4.8.1-512BD4?style=flat-square)](docs/Installation.md)

A strongly-typed .NET Framework wrapper for Bentley STAAD.Pro OpenSTAAD. Work with geometry, loads, properties, supports, commands, design queries, report tables and analysis results through managed interfaces and structural entities.

- **Build engineering tools in C#:** use typed nodes, beams, plates, load cases and result models.
- **Start from documented workflows:** explore the API guides and packaged XML IntelliSense examples.
- **Contribute without STAAD.Pro:** build the library and run offline unit tests without Bentley binaries or a STAAD license.

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

## Detached geometry diagnostics

Use `GetBeamsWithinRadius(...)`, `GetBoundingBox(...)` and `IsConnectedToBeam(...)` for detached radial filtering, named bounds and endpoint connectivity. Nearest searches, duplicate-node groups and zero-length checks are also available. These helpers inspect supplied entities without native model changes. See [geometry diagnostics](docs/Geometry-Diagnostics.md) for identity, tolerance, materialization and error contracts.

## Installation and runtime

Runtime requires **Windows, .NET Framework 4.8.1 (net481), and a licensed STAAD.Pro installation** exposing the APIs used by your application. The .NET SDK used to build the project is separate from the application's target framework.

The latest locally prepared preview is **1.0.0-preview.20261113**. Check [NuGet](https://www.nuget.org/packages/StaadPro.Interop) for versions already published; the current prepared archive is available for manual upload. Follow [installation instructions](docs/Installation.md) for local-feed and exact public-version commands. No Bentley binaries are bundled.

## Latest preview — 1.0.0-preview.20261113

Ported `GetBeamsWithinRadius`, `GetBoundingBox` and `IsConnectedToBeam` with comprehensive XML IntelliSense. Added materialized XZ midpoint filtering, immutable named bounds and captured ID-plus-distance connectivity, with finite geometry validation and no native changes. See [detached geometry contracts and examples](docs/Geometry-Diagnostics.md).

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
| Connect and manage models | [Wrapper](docs/Wrapper.md) · [Root](docs/Root.md) |
| Nodes, beams, plates and groups | [Geometry](docs/Geometry.md) |
| Load cases and assignments | [Load](docs/Load.md) · [query contracts](docs/Load-Queries.md) |
| Materials, sections and specifications | [Property](docs/Property.md) |
| Analysis results and steel-design observations | [Output](docs/Output.md) |
| Supports and analysis commands | [Support and Commands](docs/Commands-and-Support.md) |
| Design briefs and assignments | [Design](docs/Design.md) |
| Report tables and formatting | Table |
| Graphical orientation, zoom and entity visibility | [View](docs/View-Entity-Visibility.md) |

## View orientations

## Compatibility and preview status

The current source passed **7,292 offline tests**; the installed-package verifier checks **1,328 XML examples**, including **48 View examples**. These checks validate managed behavior and distribution, not every native STAAD build, COM response or engineering-unit convention. Consult method-specific version, result-state, unit and side-effect contracts before using results in an engineering workflow.

The package remains a preview. [Release notes](docs/Release-Notes.md) identify the API scope, and NuGet readiness records validation limits. Earlier public packages retain the license terms shipped with them.

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
