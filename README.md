# EasyRun

[![build](https://github.com/cyfrost/EasyRun/actions/workflows/build.yml/badge.svg)](https://github.com/cyfrost/EasyRun/actions/workflows/build.yml)
[![license](https://img.shields.io/badge/license-MIT-orange.svg)](LICENSE)

Create short **aliases** you run from the Windows Run box (<kbd>Win</kbd>+<kbd>R</kbd>) — for programs, folders and commands — plus **elevated shortcuts** that run a program as an admin account.

Works on **Windows 10 and 11**. Needs .NET Framework 4.8 (built into Windows 10 1903+ and 11).

## Download

Get the installer from [`dist/`](dist/) or the [latest release](https://github.com/cyfrost/EasyRun/releases/latest). It's not code-signed yet, so SmartScreen may warn: choose *More info → Run anyway*.

- **Just for me** (default): no admin. Installs to `%LOCALAPPDATA%\Programs\EasyRun`; aliases go in your user profile.
- **All users** (administrators only): `C:\Program Files\EasyRun`.

> Upgrading from EasyRun 0.1 (2018)? Uninstall it first from *Settings → Apps*. It demanded admin and stored everything machine-wide; v1.0 is per-user.

## Aliases

From the EasyRun window, or the `erun` command:

```text
erun add <path>              Add a program alias
    --dir                      the path is a folder (opens in Explorer)
    --args "<args>"            make it a command with arguments
    --name <alias>             what you type in Run (default: the file/folder name)
    --machine                  all users (needs admin);  --force  overwrite
erun list [--user|--machine]
erun remove <alias> [--machine]
erun run <alias>
erun help
```

Aliases are stored under **App Paths** (`HKCU` by default), the same mechanism Windows uses for `notepad`, `code`, etc. Programs are registered directly; folders and commands use a small `.lnk` in `%LOCALAPPDATA%\EasyRun\aliases` so no console window flashes.

## Elevated shortcuts

Run a program as an admin account from an alias. Pick one of two mechanisms (EasyRun defaults sensibly per target):

| Mechanism | How it runs | Prompt | Best for |
|---|---|---|---|
| **Scheduled task** | A Task Scheduler task runs the program as the admin account with the highest privileges; Windows stores the password in LSA. The alias triggers it. | Once, at creation | Tools, scripts, maintenance |
| **Run as (secondary logon)** | EasyRun starts the program as the admin account in your current desktop session, so its window is visible. | Each run | GUI apps you need to see |

EasyRun **never stores the password itself** — a task hands it to Windows (LSA); run-as asks each time and remembers only the username. Creating a task needs admin (supply the admin account's credentials once); triggering it afterwards does not.

> This is why EasyRun 0.1's "Trusted UAC" never worked for a standard user: its task ran as *you*, and "highest privileges" can't elevate an account that isn't an administrator. v1.0 runs the program as a *different* account that is an admin. See [docs/2026-audit.md](docs/2026-audit.md).

## Explorer integration (optional)

Right-click a file or folder → **Add as EasyRun alias** (under *Show more options* on Windows 11). Per-user registry verbs; no shell-extension DLL is loaded into Explorer.

## Safety

- User aliases need no admin. Only all-users aliases and creating an elevated task do.
- The alias manager shows every alias (user + machine) and which it created, and removes its own `.lnk` files and tasks cleanly.
- Nothing runs inside Explorer; nothing is written to HKLM for a per-user install.

## Building

Requirements: Visual Studio 2022+ or the .NET SDK, plus [NSIS 3](https://nsis.sourceforge.io/) for the installer. The .NET Framework 4.8 reference assemblies restore from NuGet.

```powershell
.\build.ps1                 # build, test, create artifacts\EasyRun-Setup-<version>.exe and a portable zip
.\build.ps1 -VerifyFormat   # also fail if 'dotnet format' would change anything (what CI runs)
.\build.ps1 -UpdateDist     # also refresh the installer kept in dist\
```

| Path | Output | Purpose |
|---|---|---|
| `src/EasyRun.Core` | `EasyRun.Core.dll` | Alias engine, App Paths store, shortcut/elevation abstractions |
| `src/EasyRun.App` | `EasyRun.exe` | GUI, the elevated launcher, Windows implementations |
| `src/EasyRun.Cli` | `erun.exe` | Command-line tool |
| `tests/EasyRun.Tests` | | xUnit tests |
| `installer/` | | NSIS installer |
| `assets/` | | Icon |
| `dist/` | `EasyRun-Setup-<version>.exe` | The current installer, kept in the repo |

See [CONTRIBUTING.md](CONTRIBUTING.md) and [CHANGELOG.md](CHANGELOG.md).

## License

Copyright (c) 2018-2026 cyfrost. Licensed under the [MIT License](LICENSE).
