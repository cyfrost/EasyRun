# EasyRun — audit and rewrite plan

*Audit date: 2026-11-14. Scope: every source file in `EasyRun` and `Explintegrate`, the project/build files, the NSIS installer, the bundled binaries, and the copy installed in `C:\Program Files\EasyRun` on a test machine (a managed Entra/AzureAD-joined device, examined as a standard non-admin user).*

## TL;DR

EasyRun v0.1 is a 2018 .NET 4.0 WinForms app that creates "aliases" (entries under the **App Paths** registry key, so a short name typed in the Run box launches a program/folder). Its headline feature, **Trusted UAC (TUAC) "elevated shortcuts"**, is built on a scheduled-task trick that **cannot work for you as a standard user**, and several supporting bugs make it fail even where the trick is viable.

What I confirmed on this machine:

1. **The tester is a standard user** (not a member of Administrators). The machine's Administrators group contains only separate admin and service accounts.
2. **The TUAC scheduled tasks don't exist** any more (`schtasks /query /tn EasyRunUACDelegate` → *cannot find the file specified*), yet the generated `.cmd` files and the `er` alias are still installed and point at them. So the aliases are dead ends.
3. **A standard user can't even register a scheduled task here** — registering a trivial self-scoped task returns *Access is denied*. On this managed device, task creation is an admin-only operation.
4. **The elevation trick is the wrong tool for your case.** "Run with highest privileges" only grants the full admin token to a user who is *already* a local admin. A standard user has no admin token to be elevated to, so a task registered as you would still run unelevated. The only legitimate way to get admin here is to run as a *different account that is an admin* — which is exactly the **admin service account you said you're cleared to use**.
5. **The whole app demands admin to even start** (`requireAdministrator` manifest) and **stores every alias in HKLM** (`App Paths`), which a standard user can't write (confirmed *Requested registry access is not allowed*). So nothing in EasyRun works for you without entering admin credentials, and the elevated feature doesn't work even then.

There are also show-stopping build problems (absolute `HintPath`s into `C:\Users\diabl\…` and a stranger's Downloads folder, a committed code-signing **private key** `EasyRun_TemporaryKey.pfx`, committed `bin/obj/.suo`), and a long list of correctness bugs.

**Recommendation: a focused rewrite on .NET Framework 4.8** (in-box on Windows 10/11), keeping EasyRun's good ideas (alias manager, Run-box shortcuts, Explorer integration) but replacing the elevation feature with one that actually works for a standard user who holds separate admin credentials, and running the app per-user without demanding admin for everything. Details in Part 2.

---

## How Trusted UAC works today, and why it fails

The mechanism (see [`UACDelegate.cs`](https://github.com/cyfrost/EasyRun/blob/258dc078597582ed8d0b85a719fa7577e3a19d59/src/EasyRun/EasyRun/UACDelegate.cs) and `Form1.addERDelegate`):

1. Register a Task Scheduler task whose action is the target program, with `Principal.RunLevel = TaskRunLevel.Highest` and the **default principal (the current interactive user, interactive logon)**.
2. Write a `.cmd` file that runs `schtasks.exe /run /tn "<task>"`.
3. Create an **App Paths** alias (under HKLM) pointing at that `.cmd`, so typing the alias in the Run box triggers the task.

Why it doesn't work for you:

- **`Highest` ≠ elevation for a non-admin.** `TaskRunLevel.Highest` means "use the highest privileges *this principal* can get." For a member of Administrators that is the full, un-prompted admin token (the classic "skip UAC" trick). For a standard user it is just the standard token — there is no admin token to request — so the program runs with no more rights than normal. The feature is architecturally incapable of elevating a standard user. ([`UACDelegate.cs` Act()](https://github.com/cyfrost/EasyRun/blob/258dc078597582ed8d0b85a719fa7577e3a19d59/src/EasyRun/EasyRun/UACDelegate.cs))
- **You can't register the task at all.** Confirmed on this machine: a standard user gets *Access is denied* from `RegisterTaskDefinition`. So creating an elevated shortcut already requires admin.
- **The task is gone but the alias remains.** The installed `er` alias and the two `.cmd` files survive, but the tasks they call don't exist, so the shortcuts silently do nothing.
- **A name-mangling bug guarantees a mismatch.** The task is registered as `"EasyRun-" + filename`, where `filename` still includes the extension, and `res.removeExtension` uses `IndexOf('.')` (the *first* dot). The generated `.cmd` calls `schtasks /run /tn "EasyRun-EasyRun.exe"` while the task (if it existed) was named `EasyRun-EasyRun` — so even a successfully created task wouldn't be found. (Visible in the installed file `usr_meta\EasyRun-EasyRun.cmd`.)
- **Even when it "works," the window may never appear.** A task that runs as another account with a stored password runs in session 0, non-interactively — a GUI program would run invisibly. Any rewrite has to address visibility head-on.

**The correct mechanism for your situation:** register the task to run **as the admin service account**, `LogonType = Password` (Task Scheduler stores the password encrypted in LSA), `RunLevel = Highest`, and set the task's security descriptor so *you* (the interactive standard user) are allowed to read and run it. Creating it is a one-time admin action (you supply the service-account credentials once); triggering it afterwards needs no prompt. This is a supported Windows feature, not a bypass. The visibility trade-off is handled explicitly (see the plan).

---

## Part 1 — Issues

Severity: **S1** = feature broken / data-loss / security · **S2** = broken in common cases · **S3** = wrong or fragile · **S4** = hygiene.

### A. Elevation / Trusted UAC

| # | Sev | Issue | Where |
|---|---|---|---|
| A1 | **S1** | `RunLevel.Highest` with the interactive principal can't elevate a standard user; the feature can't do what it claims for non-admins. | [UACDelegate.cs](https://github.com/cyfrost/EasyRun/blob/258dc078597582ed8d0b85a719fa7577e3a19d59/src/EasyRun/EasyRun/UACDelegate.cs), Form1.addERDelegate |
| A2 | **S1** | Task name mismatch: task registered as `EasyRun-<name>` but the `.cmd` calls `EasyRun-<name>.exe`; `removeExtension` splits on the first `.`. The shortcut never finds its task. | UACDelegate.Act, [res.cs](https://github.com/cyfrost/EasyRun/blob/258dc078597582ed8d0b85a719fa7577e3a19d59/src/EasyRun/EasyRun/res.cs) |
| A3 | S2 | Task registration isn't verified; the `.cmd` and alias are written even if `RegisterTaskDefinition` failed, leaving dead shortcuts (exactly the state on this machine). | UACDelegate.Act |
| A4 | S2 | No way to list, edit or remove TUAC tasks from the app; the alias manager only knows App Paths, not the tasks behind them. Uninstalling leaves tasks behind. | aliases.cs, Installer |
| A5 | S3 | GUI programs launched by a stored-password task run in session 0 and are invisible; never explained. | — |
| A6 | S3 | `addERDelegate` makes EasyRun itself an elevated task aliased `er`, re-registering on every click with no idempotency. | Form1.addERDelegate |

### B. Privilege model and manifest

| # | Sev | Issue | Where |
|---|---|---|---|
| B1 | **S1** | `requestedExecutionLevel = requireAdministrator`: EasyRun can't launch at all for a standard user without admin credentials, even to add a simple alias or open the manager. | [app.manifest](https://github.com/cyfrost/EasyRun/blob/258dc078597582ed8d0b85a719fa7577e3a19d59/src/EasyRun/EasyRun/app.manifest) |
| B2 | **S1** | Every alias is written to **HKLM** `App Paths`, which a standard user can't write (confirmed). Aliases should default to **HKCU**, which needs no admin and which the Run box and ShellExecute honour. | [Form1.cs WriteAlias](https://github.com/cyfrost/EasyRun/blob/258dc078597582ed8d0b85a719fa7577e3a19d59/src/EasyRun/EasyRun/Form1.cs), CommandLineHandler, UACDelegate |
| B3 | S3 | The "Just me / All users" radio exists but `WriteAlias` is driven by a separate `UniversalAlias` flag that defaults to HKLM; the choice barely connects to behaviour. | Form1.cs |
| B4 | S3 | No DPI awareness; blurry on the high-DPI laptop default. | app.manifest |

### C. Alias and shortcut correctness

| # | Sev | Issue | Where |
|---|---|---|---|
| C1 | S2 | Aliases are force-suffixed `.exe` so App Paths resolves them; a folder/command alias is therefore stored as `name.exe` pointing at a `.cmd`. Fragile and confusing. | CommandLineHandler, Form1 |
| C2 | S2 | `NullReferenceException` is used as normal control flow to detect "alias doesn't exist" (`OpenSubKey(...).GetValue("")` on a missing key). Throws and is caught everywhere; breaks under any other error. | Form1, UACDelegate, CommandLineHandler |
| C3 | S2 | Directory shortcuts are implemented by generating a `.cmd` that does `cd` + `start .`; breaks for UNC paths, flashes a console, and leaves `.cmd` litter in `usr_meta`. | CommandLineHandler.CreateDirectoryShortcut, Form1.CheckSettings |
| C4 | S3 | `res.removeExtension`/`getRoot`/`getLocation` use `IndexOf('.')`, `Substring(0,2)`, `LastIndexOf('\\')` — all break on paths with dots, UNC prefixes, or forward slashes. | [res.cs](https://github.com/cyfrost/EasyRun/blob/258dc078597582ed8d0b85a719fa7577e3a19d59/src/EasyRun/EasyRun/res.cs) |
| C5 | S3 | The alias manager lists HKCU entries but then reads each value from **HKLM** (`RefreshList` opens HKCU to enumerate, HKLM to read), so user-scope aliases show blank/wrong targets and may be mis-deleted. | [aliases.cs RefreshList](https://github.com/cyfrost/EasyRun/blob/258dc078597582ed8d0b85a719fa7577e3a19d59/src/EasyRun/EasyRun/aliases.cs) |
| C6 | S3 | `DelItem` for an HKCU alias reads `target` from the HKLM key handle (`key`, which is null in that branch) — a latent NPE. | aliases.cs DelItem |
| C7 | S3 | `Explintegrate.SubKeyExists` has the **HKCU and HKLM cases swapped** (HKCU opens LocalMachine and vice-versa). Only unused by luck. | [Main.cs](https://github.com/cyfrost/EasyRun/blob/258dc078597582ed8d0b85a719fa7577e3a19d59/src/Explintegrate/Explintegrate/Main.cs) |
| C8 | S3 | Search (`StartSearch`) permanently deletes non-matching rows from the ListView, so clearing the box needs a full registry reload; selection colours are left yellow. | aliases.cs |
| C9 | S4 | `new Form1().SAFE_LOCATION_DIRECTORY` is used all over just to read one constant string — it constructs an entire hidden main form (running its constructor and registry checks) each time. | aliases, UACDelegate, CommandLineHandler |

### D. Explorer integration

| # | Sev | Issue | Where |
|---|---|---|---|
| D1 | S2 | Writes verbs to **HKCR** (`*\shell`, `Directory\shell`), which is HKLM-backed and needs admin; should be per-user `HKCU\Software\Classes`. | ExplorerIntegration, Explintegrate |
| D2 | S3 | The command is stored unquoted (`...\EasyRun.exe --add-file "%1"` with a space in `Program Files`), so the exe path isn't quoted — breaks parsing. | ExplorerIntegration.CreateIntegration |
| D3 | S3 | No separator/background/drive verbs; legacy `IContextMenu`-style only, hidden under "Show more options" on Windows 11. | config |

### E. Build, toolchain and security

| # | Sev | Issue | Where |
|---|---|---|---|
| E1 | **S1** | A code-signing **private key is committed**: `EasyRun_TemporaryKey.pfx` (plus `ManifestCertificateThumbprint`). Must be removed and rotated. | [EasyRun.csproj](https://github.com/cyfrost/EasyRun/blob/258dc078597582ed8d0b85a719fa7577e3a19d59/src/EasyRun/EasyRun/EasyRun.csproj), repo |
| E2 | **S1** | `HintPath`s are absolute to another person's machine: `C:\Users\diabl\Downloads\[IDM] Compressed\TaskScheduler\…` and `…\Documents\Visual Studio 2017\Projects\Explintegrate\…`. The project cannot build on any other machine. | EasyRun.csproj |
| E3 | S2 | Dependencies (CommandLineParser 1.9 from 2013, TaskScheduler 2.0, the `Interop.IWshRuntimeLibrary` COM interop) are vendored as loose DLLs / COM refs instead of NuGet `PackageReference`. | csproj, packages.config |
| E4 | S2 | `.suo`, `bin/`, `obj/`, `app.publish` and the built `setup_EasyRun-0.1.exe` are committed; no `.gitignore`. | repo |
| E5 | S3 | `FailSafeInit` hard-exits if any bundled DLL is missing, with a dead-end message; a NuGet/ILMerge build removes the need. | FailSafeInit.cs |
| E6 | S3 | Targets .NET 4.0 (out of support); `packages.config` marks CommandLineParser `requireReinstallation`. | csproj |
| E7 | S4 | Two nested `EasyRun.sln`, duplicate icons, `res.cs`/`UDialogs` copied ideas from PathPlusPlus but diverged. | repo |

### F. Installer

| # | Sev | Issue | Where |
|---|---|---|---|
| F1 | S2 | `RequestExecutionLevel none` relies on UAC installer-detection, which is disabled on managed/enterprise machines; HKLM writes and Program Files copies then fail silently. | [Installer.nsi](https://github.com/cyfrost/EasyRun/blob/258dc078597582ed8d0b85a719fa7577e3a19d59/src/NSIS_Installer/Installer.nsi) |
| F2 | S2 | Uninstaller runs `EasyRun.exe -r` (needs admin, and the exe demands admin) and never removes the scheduled tasks, the `usr_meta` folder, the aliases, or the Start-menu folder. | Installer.nsi |
| F3 | S3 | 64-bit `$INSTDIR` is forced in `.onInit`, no per-user option, previous-install uninstall isn't waited on or checked. | Installer.nsi |
| F4 | S4 | Ships `Microsoft.Win32.TaskScheduler.dll` + localized satellites and the COM interop as loose files. | Components.nsi |

### G. Error handling and UX

| # | Sev | Issue | Where |
|---|---|---|---|
| G1 | S3 | Every method wraps everything in `try/catch` that pops a MessageBox and continues, so one failure cascades into several dialogs and leaves half-made state. | throughout |
| G2 | S3 | Inconsistent dialog titles ("Error", "Confirmation", "EasyRun"), hand-built forms with pixel coordinates and PNG buttons, no DPI scaling, no mnemonics. | UDialogs, CommandLineHandler.askUser, forms |
| G3 | S3 | `IsWinXP`/XP branches, `WakeToRun`, battery settings and other XP-era cruft remain. | Form1 |
| G4 | S4 | Copy-pasted MIT header typo "FOR AND CLAIM" and the "supercow powers" description. | all files |

---

## Part 2 — Rewrite plan

### Guiding decisions

- **Runtime: .NET Framework 4.8, WinForms.** In-box on Windows 10 22H2 / 11; the forms port with little change. (Same choice, and rationale, as the PathPlusPlus rewrite.)
- **Per-user by default, admin only when the user asks for an all-users or elevated feature.** Run `asInvoker`. Aliases go to **HKCU App Paths** (no admin). "All users" is an explicit, shielded choice that elevates just that write.
- **One shared, tested core library** (`EasyRun.Core`) used by the GUI, the CLI and the tests. Registry/alias/task/credential logic lives there behind interfaces, exactly as `PathPlusPlus.Core` does.
- **Elevation that actually works for a standard user with separate admin creds** — see below; this is the one decision I want to confirm before building.

### The elevated-shortcut feature (the core redesign)

Primary mechanism — **"Run as a stored admin account" via Task Scheduler**:

- Creating an elevated shortcut is a one-time privileged action. EasyRun collects the **admin service-account** username + password in a secure dialog and registers a task with `Principal.UserId = <admin>`, `LogonType = Password`, `RunLevel = Highest`, action = the target (+ args + working dir).
- The task's **security descriptor is set (SDDL)** so the interactive user (your SID) may read and run it. Triggering is then a no-prompt `schtasks /run` behind a user-scope alias.
- The password is never stored by EasyRun; Task Scheduler keeps it encrypted in LSA. EasyRun can optionally cache the *username* (not the secret) to streamline creating several shortcuts in one session.
- Full management: list, edit, disable, and **remove** EasyRun tasks (the alias manager gains a "Elevated" column and a tasks view); uninstall removes them.

The visibility trade-off is explicit, and is the decision I'll confirm with you:

- **(Recommended) Hybrid:** for programs that just need admin rights to do their work (installers, service control, scripts, admin tools that draw their own window once elevated) use the stored-password task. For GUI programs you need to *see* in your own session, offer a second mode that uses **`CreateProcessWithLogonW`** (secondary logon, like `runas`) to start the target **as the admin account in your current desktop session**, so its window is visible; credentials are cached with **DPAPI** (per-user, machine-bound) if you opt in, else prompted. EasyRun picks the sensible default per target and lets you override.
- **Task-only:** simplest and most "set and forget," but GUI apps may run hidden in session 0.
- **Secondary-logon-only:** always visible in your session, but storing the admin password under DPAPI is a bigger security footprint than letting LSA hold it, and some `requireAdministrator` GUI apps still won't get the elevated token this way.

### Phase plan

| Phase | Work | Needs admin to test |
|---|---|---|
| 0 | Repo safety: remove the committed `.pfx` (and note it must be rotated), `.gitignore`, strip `bin/obj/.suo/app.publish/setup.exe`, delete absolute HintPaths. | no |
| 1 | `EasyRun.Core`: `AliasStore` (HKCU/HKLM App Paths, read+write+enumerate+delete, scope-correct), `ShortcutFactory` (file / folder / command targets without `.cmd` litter — folders via `explorer.exe`), validation, backups before destructive writes. Unit-tested against a fake registry. | no |
| 2 | `ElevationService`: scheduled-task create/trigger/list/remove with admin-account principal + stored password + task SDDL; `SecondaryLogonLauncher` (CreateProcessWithLogonW) + optional DPAPI credential store. Interfaces faked in tests; the Windows path documented as admin-only. | task *creation* needs admin |
| 3 | GUI rewrite (WinForms, DPI-aware, `asInvoker`, UAC shields): main window, alias manager, add-shortcut, add-elevated-shortcut, credential dialog, shared `Ui` helper like PathPlusPlus. | partially |
| 4 | `erun` CLI: `add`, `add-dir`, `add-elevated`, `list`, `remove`, `run`, `integrate`/`unintegrate`, with real exit codes and no MessageBoxes. | no |
| 5 | Explorer integration as per-user `HKCU\Software\Classes` verbs (no COM dll), quoted commands, drive/background verbs. Retire `Explintegrate`. | no |
| 6 | NSIS installer: per-user (no-admin) or all-users, `asInvoker` core, proper uninstall (aliases, tasks, `usr_meta`, Start menu). Portable zip. | no |
| 7 | Repo: `.editorconfig`, `.gitattributes`, analyzers (warnings-as-errors), `dotnet format` in CI, one-type-per-file, `CONTRIBUTING`/`CHANGELOG`, GitHub Actions build+release, installer shipped in `dist/`. | no |

### What I can and can't verify here

As a standard user with no admin token (and I will not ask for or use your admin password), I can fully test: alias create/list/edit/remove in **HKCU**, file/folder/command shortcuts, the CLI, Explorer verbs, and the whole UI. I **cannot** test the admin paths — registering a stored-password task and triggering it — because task creation is admin-only on this machine. Those are covered by unit tests against fakes and documented as needing an admin to validate; I'll give you an exact manual test script to run with the service account.

### Until then: safe workaround

To run a specific program elevated-as-the-admin-account today, from your user session:

```bat
runas /user:DOMAIN\adminaccount /savecred "\"C:\Path\To\App.exe\""
```

`/savecred` stores the credentials in Credential Manager so later runs don't prompt (note: `/savecred` caches them for *any* program the account launches — a known trade-off; the rewrite's task-based approach is tighter because each shortcut is a distinct, removable task).
