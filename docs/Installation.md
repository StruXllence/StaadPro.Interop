# Installing StaadPro.Interop.Core

This independent package targets **net8.0-windows only** and requires Windows Desktop/WPF, including for console applications. It has its own StaadPro.Interop.dll and StaadPro.Interop.Core namespaces. Bentley binaries are not bundled; live use requires a separately licensed STAAD.Pro installation. The .NET Framework package StaadPro.Interop is a different project.

## Project reference

~~~xml
<PropertyGroup>
  <TargetFramework>net8.0-windows</TargetFramework>
  <UseWPF>true</UseWPF>
</PropertyGroup>
<ItemGroup>
  <PackageReference Include="StaadPro.Interop.Core" Version="1.0.0-preview.20261105" />
</ItemGroup>
~~~

The source is prepared for a Core preview; public NuGet.org availability of this exact package is not confirmed. Until manual upload and public restore are verified, install the archive from a local feed.

~~~powershell
dotnet add package StaadPro.Interop.Core --version 1.0.0-preview.20261105 --source "C:/Path/To/Local/Feed"
~~~

After publication, use https://api.nuget.org/v3/index.json as the public feed. Include prerelease versions when searching in Visual Studio. A package restore/build can be verified without a STAAD installation; native operations need the installed Windows COM application.

## Acquire a running session

```csharp
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
```

This default call attaches without launching. Keep provider creation, use/disposal and subsequent native operations on the established Windows STA thread. The provider does not close STAAD when disposed and does not own wrappers already returned. See [session acquisition](OpenStaad-Session-Provider.md) for target paths, blank states, cancellation and configured launch fallback.

## Migrating the earlier testing package

Remove the old StaadPro.Interop.Core.Testing package and update assembly/namespace imports to StaadPro.Interop.Core. The old testing package used the previous StaadPro.Interop assembly and namespace. Preserve your application's native STA ownership and do not treat a target-framework suffix as certification for every Windows/STAAD servicing build. WPF/dynamic COM trimming and Native AOT are not qualified.

Read [Core development](Core-Development.md), [API reference](API-Reference.md), [release notes](Release-Notes.md) and [publishing](Publishing.md) for scope and operating limits.
