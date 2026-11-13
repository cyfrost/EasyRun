# Changelog

The format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and versions follow [Semantic Versioning](https://semver.org/).

## [1.0.0] - 2026-11-14

A complete rewrite for Windows 10 and 11. Full analysis of v0.1 in [docs/2026-audit.md](docs/2026-audit.md).

### Added
- **Alias engine** for programs, folders and commands, stored under App Paths. Folders/commands use a generated `.lnk` (no console flash, correct working directory) instead of stray `.cmd` files.
- **`erun` command-line tool**: `add`, `list`, `remove`, `run`, `integrate`, with real exit codes.
- **Elevated shortcuts that work for a standard user**: run a program as a separate admin account, via a stored-password scheduled task (for tools) or secondary logon (visible, for GUI apps). EasyRun never stores the password itself.
- **Alias Manager** showing user and machine aliases, what EasyRun manages, with run/open/delete.
- **Per-user Explorer integration** (HKCU verbs), no COM DLL in Explorer.
- Automatic cleanup: removing an alias deletes its `.lnk` and scheduled task.

### Fixed
- "Trusted UAC" couldn't elevate a standard user at all (a "highest privileges" task still ran as the non-admin user), and failed to even register a task without admin. Elevation now runs the target as an admin account.
- The app demanded administrator rights to start, and wrote every alias to HKLM (unwritable by a standard user). It now runs as the invoker and defaults to per-user aliases.
- A task-name mangling bug (`EasyRun-<name>.exe` vs `EasyRun-<name>`) meant generated shortcuts never found their task.
- The alias manager enumerated HKCU but read values from HKLM, showing blank/wrong user aliases.
- `Explintegrate.SubKeyExists` had HKCU and HKLM swapped.
- Path parsing used first-dot / fixed-offset substrings that broke on UNC paths and names with dots.

### Removed
- The committed code-signing private key `EasyRun_TemporaryKey.pfx`. **It was public in the v0.1 history and must be treated as compromised and rotated.**
- Absolute `HintPath`s into another machine's folders (the project couldn't build elsewhere).
- The `Explintegrate` helper assembly, the TaskScheduler/COM loose-DLL dependencies (now NuGet), committed `bin/obj/.suo`, and XP-era code.

## [0.1] - 2018-01-22

Initial release.
