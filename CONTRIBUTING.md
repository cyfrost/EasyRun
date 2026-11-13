# Contributing to EasyRun

EasyRun writes to the registry and can run programs elevated, so changes are held to a high bar.

## Getting started

Requirements: Windows 10/11, Visual Studio 2022+ or the .NET SDK, and [NSIS 3](https://nsis.sourceforge.io/) for the installer. The .NET Framework 4.8 reference assemblies come from NuGet.

```powershell
git clone https://github.com/cyfrost/EasyRun.git
cd EasyRun
dotnet test EasyRun.sln
.\build.ps1
```

## Before a pull request

1. `dotnet format EasyRun.sln` (CI runs `--verify-no-changes`).
2. `dotnet test EasyRun.sln`. The build treats warnings as errors with the .NET analyzers and `.editorconfig` rules on.
3. Add or update tests for behaviour changes, especially in `EasyRun.Core`.
4. If you touched a window, check it at 100% and 150% display scaling.

## Coding standards

- One type per file, named after the type. Private fields `_camelCase`; constants/static-readonly `PascalCase`.
- All registry, alias, shortcut and elevation logic lives in `EasyRun.Core` behind interfaces (`IAppPathsStore`, `IShortcutWriter`, `IElevationService`, `ICredentialPrompt`) and is unit-tested against fakes. The GUI and CLI only call `AliasService`.
- Default to **HKCU** (per-user). Writing HKLM or creating a scheduled task is admin-only — surface that, don't assume it.
- **Never commit a signing key, password, or credential.** EasyRun does not persist passwords.

## Testing the elevated feature

Creating an elevated **task** and triggering it needs admin rights and a real admin account, so it can't run in CI or as a standard user; it's covered by unit tests against `IElevationService` fakes. To validate manually: create an elevated-task alias supplying an admin account, confirm the task exists (`schtasks /query /tn EasyRun_<alias>`), then run the alias from <kbd>Win</kbd>+<kbd>R</kbd>.

## Releasing

1. Bump `<Version>` in `Directory.Build.props`, add a `CHANGELOG.md` entry.
2. `.\build.ps1 -UpdateDist` to refresh `dist/`.
3. Merge, then tag `vX.Y.Z`; GitHub Actions builds and attaches the installer to a release.
