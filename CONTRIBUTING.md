# Contributing to StaadPro.Interop.Core

Thank you for your interest in contributing to **StaadPro.Interop.Core**! We welcome bug fixes, documentation improvements, unit tests, and feature enhancements.

Start with [good first issues](https://github.com/StruXllence/StaadPro.Interop/labels/good%20first%20issue) or [help wanted](https://github.com/StruXllence/StaadPro.Interop/labels/help%20wanted), or propose a focused documentation/example improvement. A STAAD.Pro installation is not required for offline contributions.

## Development Setup

1. **Prerequisites**:
   - Windows, .NET SDK 8.0+ and the .NET Framework 4.8.1 targeting tools
   - Visual Studio 2022 / JetBrains Rider / VS Code
   - Bentley STAAD.Pro (required only for live COM integration tests, not for unit tests)

2. **Clone and Build**:
   ```powershell
   git clone https://github.com/StruXllence/StaadPro.Interop.git
   cd StaadPro.Interop.Core
   dotnet build StaadPro.Interop.Core.sln -c Release
   ```

3. **Run Unit Tests**:
   ```powershell
   dotnet test tests/StaadPro.Interop.Core.Tests/StaadPro.Interop.Core.Tests.csproj -c Release
   ```

## Contribution Workflow

1. Fork the repository and create your feature branch: `git checkout -b feature/my-new-feature`
2. Commit your changes with concise, descriptive commit messages.
3. Ensure all offline unit tests pass.
4. Open a Pull Request referencing any related issues.

## Licensing contributions

Intentional contributions are provided under Apache License 2.0 unless explicitly stated otherwise, as described by section 5 of [LICENSE](LICENSE). Submit only material you have the right to contribute and preserve applicable upstream notices. Contributors retain their authorship; this project does not require a separate copyright assignment or contributor agreement for ordinary pull requests. See [licensing guidance](docs/Licensing.md).
