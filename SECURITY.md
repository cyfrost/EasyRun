# Security

## ⚠️ The v0.1 signing key is compromised

EasyRun 0.1 committed its Authenticode signing key, `EasyRun_TemporaryKey.pfx`, to the repository. It has therefore been public for the life of the project and **must be treated as compromised**:

- Revoke it if it was ever issued by a CA, and stop using it.
- Anything signed with it can no longer be trusted as authentic.
- The v1.0 rewrite removes the key from the tree and never signs with a committed key. Release signing (when added) must use a secret kept outside the repository, e.g. a GitHub Actions secret or Azure Trusted Signing.

The key remains in git history; rewriting public history is disruptive, so the practical remedy is revocation + rotation rather than history surgery.

## How EasyRun handles credentials

- EasyRun **never stores an admin password**. For an elevated **task**, the password is handed to Windows Task Scheduler, which keeps it in the LSA secret store; for **run-as** shortcuts it is requested each time. Only the admin **username** is cached (per process), never the secret.
- Aliases default to the per-user registry (HKCU). Writing all-users aliases or creating an elevated task requires administrator rights.
- Exported alias files (`erun export`) contain definitions only — never passwords.

## Reporting a vulnerability

Please open a private security advisory on the GitHub repository, or a regular issue for non-sensitive reports. Include your EasyRun version (`erun version`) and Windows build (`winver`).
