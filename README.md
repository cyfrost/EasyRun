# EasyRun

[![build](https://github.com/cyfrost/EasyRun/actions/workflows/build.yml/badge.svg)](https://github.com/cyfrost/EasyRun/actions/workflows/build.yml)
[![license](https://img.shields.io/badge/license-MIT-orange.svg)](LICENSE)

**Give anything a short name you can launch from the Run box.** Press <kbd>Win</kbd>+<kbd>R</kbd>, type `vs`, and Visual Studio opens. Type `dl` and your Downloads folder opens. Type `fixprinter` and a maintenance script runs **as an admin account** — no UAC prompt, because you set it up once.

EasyRun turns short aliases into launches of programs, folders and commands, and can run chosen programs elevated as an administrator account you're allowed to use. Works on **Windows 10 and 11**; needs .NET Framework 4.8 (built into both).

![EasyRun](https://i.imgur.com/f3yqTBi.png)

## What it's for

| You type in Run (Win+R) | What happens |
|---|---|
| `vs` | Launches `C:\...\devenv.exe` |
| `dl` | Opens `C:\Users\you\Downloads` in Explorer |
| `serve` | Runs `python -m http.server 8080` in a folder you chose |
| `hosts` | Opens `C:\Windows\System32\drivers\etc\hosts` elevated, as an admin account |

An **alias** is just a short name. Behind it, EasyRun registers an entry under Windows' **App Paths** (the same mechanism that makes `notepad` and `winget` work from Run), so your alias works everywhere Windows resolves a command: the Run box, the address bar, `start <alias>`, and shortcuts.

## Download & install

Get the installer from [`dist/`](dist/) or the [latest release](https://github.com/cyfrost/EasyRun/releases/latest). It isn't code-signed yet, so SmartScreen may warn — choose *More info → Run anyway*.

- **Just for me** (default): no admin needed. Installs to `%LOCALAPPDATA%\Programs\EasyRun`; your aliases live in your own profile.
- **All users**: administrators only; installs to `C:\Program Files\EasyRun`.

Silent per-user install:

```bat
EasyRun-Setup-1.0.0.exe /S /CurrentUser
```

> **Upgrading from EasyRun 0.1 (2018)?** Uninstall it first from *Settings → Apps*. The old version demanded admin and stored everything machine-wide; v1.0 is per-user and needs no admin for ordinary aliases.

## Using it

### From the app

Open **EasyRun** from the Start menu. From the home window you can create an alias or an elevated shortcut, open the **Alias Manager** (see, run, edit, import/export every alias), review **Elevated tasks**, restore **Backups**, and toggle **Explorer integration**.

### From the command line (`erun`)

Tick "Add 'erun' to PATH" during install, then open a **new** terminal:

```bat
:: A program
erun add "C:\Program Files\Microsoft VS Code\Code.exe" --name code

:: A folder (opens in Explorer)
erun add "%USERPROFILE%\Downloads" --dir --name dl

:: A program with arguments (runs in a working directory you can set)
erun add "C:\Python312\python.exe" --name serve --args "-m http.server 8080"

:: See everything, filter, run, remove
erun list
erun run code
erun remove serve

:: Back up and move your aliases to another machine
erun export my-aliases.json
erun import my-aliases.json
```

`erun add .` with no `--name` uses the file or folder name. Add `--machine` to create an all-users alias (needs admin), and `--force` to overwrite. Every command returns a meaningful exit code (`0` ok, `2` bad input, `3` not found, `5` not permitted), so you can script it.

### From Explorer

Enable **Explorer integration**, then right-click any file or folder → **Add as EasyRun alias** (under *Show more options* on Windows 11). It's a per-user registry verb — no shell extension is loaded into Explorer.

## Elevated shortcuts (run as an admin account)

Some programs need administrator rights. If you're a standard user but hold credentials for a separate admin account (a common setup on managed machines), EasyRun can make an alias that launches a program **as that admin account**.

Create one from **New elevated shortcut**, pick the program and the admin account, and choose how it runs:

- **Scheduled task** — the program runs as the admin account with the password stored by Windows (in the LSA secret store, never by EasyRun). You're asked for the password **once**, at creation; the alias triggers it with no prompt afterwards. Best for scripts, service control and admin tools.
- **Run as (secondary logon)** — the program starts as the admin account in your current desktop session, so its window is visible. You're asked for the password **each time**. Best for GUI apps you need to see.
- **Automatic** (default) picks run-as for GUI programs and a task for everything else.

Use **Test run** in the dialog to launch the program as the admin account *before* saving, so you catch a wrong password or path immediately. Manage the tasks behind elevated shortcuts from **Elevated tasks** (run, or remove orphans left behind).

> **Why the 2018 version's "Trusted UAC" never worked for a standard user:** it registered a task that ran *as you* with "highest privileges," which can only elevate someone who is already an administrator. v1.0 runs the program as a *different* account that is an admin — the supported Windows mechanism. See [docs/2026-audit.md](docs/2026-audit.md).

## Good to know

- **Scope & shadowing.** User aliases live in `HKCU`, all-users in `HKLM`. The Run box checks your user aliases first, so a user alias of the same name **overrides** an all-users one. The Alias Manager flags this ("hidden"/"overrides") and the dialogs warn you.
- **Backups.** EasyRun saves an alias before overwriting or deleting it; restore from **Backups** or by re-importing. Backups never contain passwords.
- **Changes are immediate.** A new alias works in the next Run box / terminal you open.
- **Where things live.** Aliases: `HKCU\Software\Microsoft\Windows\CurrentVersion\App Paths`. Generated shortcuts and backups: `%LOCALAPPDATA%\EasyRun`.

## Troubleshooting

- **My alias doesn't run.** Open a new Run box (changes don't affect already-open programs). Check it in the Alias Manager; if it's "hidden", a user alias of the same name wins.
- **`erun` isn't found in my terminal.** Open a *new* terminal after installing, or re-run the installer and tick "Add 'erun' to PATH".
- **An elevated shortcut does nothing / asks for a password I don't have.** Elevated shortcuts run as the admin account you chose; you need its password. A scheduled-task shortcut may run a GUI program invisibly (session 0) — use the **Run as** mode for GUI apps.
- **SmartScreen blocked the installer.** It isn't signed yet: *More info → Run anyway*, or verify the SHA-256 in [`dist/SHA256SUMS.txt`](dist/).

## Building

Requirements: Visual Studio 2022+ or the .NET SDK, plus [NSIS 3](https://nsis.sourceforge.io/) for the installer. The .NET Framework 4.8 reference assemblies restore from NuGet.

```powershell
.\build.ps1                 # build, test, create artifacts\EasyRun-Setup-<version>.exe and a portable zip
.\build.ps1 -VerifyFormat   # also fail if 'dotnet format' would change anything (what CI runs)
.\build.ps1 -UpdateDist     # also refresh the installer kept in dist\
```

| Path | Output | Purpose |
|---|---|---|
| `src/EasyRun.Core` | `EasyRun.Core.dll` | Alias engine, App Paths store, shortcut/elevation/backup logic |
| `src/EasyRun.App` | `EasyRun.exe` | GUI, the elevated launcher, Windows implementations |
| `src/EasyRun.Cli` | `erun.exe` | Command-line tool |
| `tests/EasyRun.Tests` | | xUnit tests |
| `installer/` | | NSIS installer |
| `dist/` | `EasyRun-Setup-<version>.exe` | The current installer, kept in the repo |

All registry, alias, shortcut, elevation and backup logic lives in `EasyRun.Core` behind interfaces and is unit-tested against fakes; the GUI and CLI only call `AliasService`. See [CONTRIBUTING.md](CONTRIBUTING.md), [CHANGELOG.md](CHANGELOG.md) and [SECURITY.md](SECURITY.md).

## License

Copyright (c) 2018-2026 cyfrost. Licensed under the [MIT License](LICENSE).
