# Publishing StaadPro.Interop.Core

This project produces **StaadPro.Interop.Core**, a separate package containing one Windows .NET 8 library. It does not produce the Framework package or the historical Core.Testing ID. No upload is performed automatically.

| Metadata | Value |
| --- | --- |
| Package / assembly | StaadPro.Interop.Core |
| Preview version | 1.0.0-preview.20261105 |
| Authors | Srinivasa Rao Masanam |
| Company / publisher | StruXllence |
| Copyright | Copyright (c) 2026 StruXllence |
| License | Apache-2.0, with retained inherited notices |
| Target | net8.0-windows, Windows Desktop/WPF |
| Source branch | staadpro-interop-core |

## Prepare and verify

Keep the project ReleaseDocumentationVersion, NuGet README version and first release-notes version synchronized. Update README/release notes before every distribution build. Retain the adopted logos, author identity, live website links and applicable notices from every workstation. Do not override the Core package/assembly identity with the Framework or historical testing identity.

~~~powershell
dotnet build StaadPro.Interop.Core.sln -c Release
dotnet test tests/StaadPro.Interop.Core.Tests/StaadPro.Interop.Core.Tests.csproj -c Release --no-build --filter "TestCategory!=LiveIntegration"
$releaseVersion = '1.0.0-preview.20261105'
$releaseCommit = git rev-parse HEAD
dotnet pack src/StaadPro.Interop.Core/StaadPro.Interop.Core.csproj -c Release --no-build -p:PackageVersion=$releaseVersion -p:RepositoryCommit=$releaseCommit -p:RepositoryBranch=staadpro-interop-core -o artifacts/release
./eng/Verify-CorePackage.ps1 -PackagePath "artifacts/release/StaadPro.Interop.Core.$releaseVersion.nupkg"
Get-FileHash "artifacts/release/StaadPro.Interop.Core.$releaseVersion.nupkg" -Algorithm SHA256
~~~

Build a distribution from committed/pushed source, verify the exact archive through the fresh package consumer, and retain its checksum. Preview CI packages use their own version and isolated feed. They are validation artifacts, not a substitute for the reviewed public release. Native feature qualification remains separate from offline tests.

## Manual public upload

Sign in to the branded NuGet.org account, use Upload, and select the verified StaadPro.Interop.Core archive. Confirm the separate package ID, author, framework, dependency/Windows Desktop references, README and release notes before submission. Check ownership access to this new package ID; registry ownership is separate from Authors metadata. Do not overwrite or unlist the Framework package as part of this process.

Wait for validation/indexing, then confirm the exact version is present at https://api.nuget.org/v3-flatcontainer/staadpro.interop.core/index.json and restore it into a fresh net8.0-windows/WPF consumer from https://api.nuget.org/v3/index.json. A local archive, successful upload form or inherited Framework release number alone does not prove public availability.

If a key-based route is used later, configure its secret locally and keep it out of source control and chat. Previously published package bytes and license terms remain unchanged. See [installation](Installation.md), [Core architecture](Core-Development.md) and [runtime qualification limits](OpenStaad-Session-Provider.md).
