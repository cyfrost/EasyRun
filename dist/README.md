# EasyRun installer

`EasyRun-Setup-<version>.exe` is the current release, kept in the repository for direct download. The same file is attached to each [GitHub release](https://github.com/cyfrost/EasyRun/releases).

- Verify the download against `SHA256SUMS.txt`:
  ```powershell
  (Get-FileHash .\EasyRun-Setup-1.0.0.exe -Algorithm SHA256).Hash
  ```
- Not code-signed yet, so SmartScreen may warn: *More info → Run anyway*.
- Silent per-user install:
  ```powershell
  EasyRun-Setup-1.0.0.exe /S /CurrentUser
  ```

Built by `.\build.ps1 -UpdateDist` from `installer/EasyRun.nsi`. Don't edit files here by hand.
