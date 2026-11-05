![StaadPro.Interop.Core](https://raw.githubusercontent.com/StruXllence/StaadPro.Interop/staadpro-interop-core/docs/images/logo-with-text-400.png)

# StaadPro.Interop.Core

A strongly-typed Windows .NET wrapper for Bentley STAAD.Pro OpenSTAAD.

By **Srinivasa Rao Masanam**. Published by **StruXllence**. **Copyright (c) 2026 StruXllence**.

[![YouTube @StruXllence](https://img.shields.io/badge/YouTube-%40StruXllence-FF0000?style=flat-square&logo=youtube&logoColor=white)](https://www.youtube.com/@StruXllence) [![Website — live](https://img.shields.io/badge/Website-Live-16A34A?style=flat-square)](https://struxllence.com) [![Community forum](https://img.shields.io/badge/Community-Forum-0866C6?style=flat-square)](https://forum.struxllence.com) [![Revamp forum — preview](https://img.shields.io/badge/Revamp%20forum-Preview-7657B5?style=flat-square)](https://new-forum.struxllence.com)

The [StruXllence website](https://struxllence.com) is live. The revamp forum remains a preview.

## Version 1.0.0-preview.20261105

StaadPro.Interop.Core is a distinct **Windows .NET 8** project. It has its own solution, assembly, package ID and `StaadPro.Interop.Core` namespaces, and targets only `net8.0-windows`. The Framework project remains on main and is developed separately. This preview is prepared locally; public availability of this Core package is not confirmed.

The branch starts from main commit 20821c5 and preserves the existing OpenStaadSessionProvider, immutable acquisition options/results, process identity checks, target-file coordination and modern OLE discovery. It also retains the current managed adapters from main; source coverage does not certify every native release or engineering convention.

## Build and requirements

~~~powershell
dotnet build StaadPro.Interop.Core.sln -c Release
dotnet test tests/StaadPro.Interop.Core.Tests/StaadPro.Interop.Core.Tests.csproj -c Release --no-build --filter "TestCategory!=LiveIntegration"
~~~

Runtime use requires Windows, the .NET 8 Windows Desktop/WPF runtime and a separately installed, licensed STAAD.Pro application exposing the requested OpenSTAAD members. WPF is required by the current geometry types even for console consumers. There is no net481 asset in this package. The NuGet asset suffix net8.0-windows7.0 is a framework identifier, not a claim of Windows 7 runtime support.

## Attach to an existing session

~~~csharp
using System;
using StaadPro.Interop.Core.Services;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        using (var provider = new OpenStaadSessionProvider())
        using (var wrapper = provider.Get())
        {
            if (wrapper == null)
            {
                Console.WriteLine("No usable running STAAD session was acquired.");
                return;
            }
            Console.WriteLine(wrapper.Root.GetStaadFileFullPath());
        }
    }
}
~~~

Create, use and dispose the provider on the same Windows STA thread. The default call attaches without launching. Acquire() provides the returned wrapper plus named outcome and diagnostics. Disposing the provider does not close STAAD or dispose wrappers already returned. Keep subsequent COM calls on the session's appropriate apartment.

## API scope and documentation

The inherited implementation includes 143 Geometry overloads, 171 Load overloads, 249 common Property operations plus five optional catalog queries, 96 managed Output overloads, 27 Support operations, seventeen Commands methods, ten Design methods, twenty-nine Table methods, eighteen View methods and the reviewed Root/wrapper APIs. Examples and XML IntelliSense now use the independent Core namespaces. Native version/capability and operation-specific result contracts remain applicable.

[Core development and separation](docs/Core-Development.md) · [Installation](docs/Installation.md) · [Session provider](docs/OpenStaad-Session-Provider.md) · [API reference](docs/API-Reference.md) · [Publishing](docs/Publishing.md)

Native-only gaps, parked Load/Property work and engineering qualification remain explicit. Earlier Framework coverage/release reports are retained as source provenance; they are not evidence that every route is natively qualified on modern .NET. The old StaadPro.Interop.Core.Testing package used the previous assembly/namespace and is historical, not this package.

## Verification

Passed **7,263 offline tests** and **1,317 installed-package XML examples** on Windows .NET 8. The Release build and fresh consumer build reported zero warnings/errors. Native STAAD UI, COM servicing builds and engineering qualification remain separate.

## Contribute and licensing

Use [GitHub Issues](https://github.com/StruXllence/StaadPro.Interop/issues) for reproducible defects and the community forums for engineering discussions. Source, offline tests and documentation improvements are welcome. The project uses Apache-2.0 with applicable inherited notices; see [licensing](docs/Licensing.md). This independent project is neither affiliated with, sponsored by nor endorsed by Bentley Systems. Bentley, STAAD and STAAD.Pro names and marks belong to their respective owners. See [NOTICE](NOTICE.md) and [trademark guidance](TRADEMARKS.md).
