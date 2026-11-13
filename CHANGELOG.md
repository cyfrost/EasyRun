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

### Later in 1.0.0 (polish)
- **Export / import aliases** as JSON (`erun export` / `erun import`, and buttons in the Alias Manager). Secrets are never exported.
- **Edit** an alias in place from the Alias Manager, plus a right-click context menu (run, edit, open folder, copy name/target, delete) and keyboard shortcuts (Delete, Enter, F2, F5).
- Consistent button mnemonics and labels across every window; the add-alias dialog warns when the target doesn't exist.
- Added `SECURITY.md` documenting the compromised v0.1 signing key and the credential model.

### Later in 1.0.0 (features + hardening)
- **Backups with restore**: every overwrite or delete of an EasyRun alias is saved first; restore from the new **Backups** window or the Alias Manager (backups never contain passwords).
- **Elevated tasks view**: lists the scheduled tasks behind elevated shortcuts, flags orphans (no alias points at them), and lets you run or remove them or open Task Scheduler.
- **Test run**: in the elevated-shortcut dialog, launch the program as the admin account before saving, to confirm the credentials and path.
- **Duplicate / shadow detection**: the Alias Manager marks a user alias that overrides an all-users one (and the hidden all-users alias), and the create dialogs warn about it.
- Reworked the Alias Manager layout (create/manage actions on a toolbar) so no button is clipped; fixed credential-dialog disposal and a couple of dead code paths found in review.
- README rewritten with usage examples, troubleshooting, and how aliases work.

## [0.1] - 2018-01-22

Initial release.
