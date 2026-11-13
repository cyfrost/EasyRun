EasyRun - run things from the Run box (Windows 10/11)
======================================================

Aliases
  Open EasyRun from the Start menu, or run "erun" in a terminal.
  Create a short alias for a program, a folder, or a command, then press
  Win+R and type the alias.

  erun add <path> [--dir] [--args "..."] [--name <alias>] [--machine]
  erun list
  erun remove <alias>
  erun run <alias>
  erun help

Elevated shortcuts (run as an admin account)
  From the EasyRun window: "New elevated shortcut". Choose the program and
  the admin account to run it as. Two ways to run it:
    - Scheduled task: Windows stores the password (in LSA); later runs need
      no prompt. Best for tools and scripts.
    - Run as (secondary logon): always visible on your desktop; asks for the
      password each time. Best for GUI apps.
  EasyRun never stores the password itself.

Explorer integration (optional)
  Right-click a file or folder and choose "Add as EasyRun alias"
  (under "Show more options" on Windows 11).

Notes
  - User aliases need no administrator rights.
  - "All users" aliases and creating an elevated task need admin.

Project page: https://github.com/cyfrost/EasyRun
