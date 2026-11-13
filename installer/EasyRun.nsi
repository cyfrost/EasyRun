; EasyRun installer (NSIS 3.x)
;
; Build:  makensis /DVERSION=1.0.0 /DSOURCE_DIR=<folder with built binaries> /DOUTPUT_FILE=<setup.exe> EasyRun.nsi
; (build.ps1 does this for you.)
;
; - Per-user install by default (no admin): %LOCALAPPDATA%\Programs\EasyRun
; - Administrators may choose an all-users install: C:\Program Files\EasyRun
; - Aliases are per-user (HKCU App Paths); nothing here needs admin for the default install.

Unicode true
ManifestDPIAware true
ManifestSupportedOS all
SetCompressor /SOLID lzma

!ifndef VERSION
  !define VERSION "1.0.0"
!endif
!ifndef SOURCE_DIR
  !error "Pass /DSOURCE_DIR=<folder containing EasyRun.exe and erun.exe>"
!endif
!ifndef OUTPUT_FILE
  !define OUTPUT_FILE "EasyRun-Setup-${VERSION}.exe"
!endif

!define PRODUCT_NAME "EasyRun"
!define PRODUCT_PUBLISHER "cyfrost"
!define PRODUCT_URL "https://github.com/cyfrost/EasyRun"
!define APP_EXE "EasyRun.exe"
!define CLI_EXE "erun.exe"
!define SETTINGS_KEY "Software\EasyRun"
!define UNINSTALL_KEY "Software\Microsoft\Windows\CurrentVersion\Uninstall\EasyRun"
!define LEGACY_UNINSTALL_KEY "Software\Microsoft\Windows\CurrentVersion\Uninstall\EasyRun"

Name "${PRODUCT_NAME}"
OutFile "${OUTPUT_FILE}"
BrandingText "${PRODUCT_NAME} ${VERSION}"
ShowInstDetails show
ShowUninstDetails show

VIProductVersion "${VERSION}.0"
VIAddVersionKey "ProductName" "${PRODUCT_NAME}"
VIAddVersionKey "ProductVersion" "${VERSION}"
VIAddVersionKey "FileVersion" "${VERSION}"
VIAddVersionKey "FileDescription" "${PRODUCT_NAME} Setup"
VIAddVersionKey "CompanyName" "${PRODUCT_PUBLISHER}"
VIAddVersionKey "LegalCopyright" "Copyright (c) 2018-2026 ${PRODUCT_PUBLISHER}"

!define MULTIUSER_EXECUTIONLEVEL Highest
!define MULTIUSER_MUI
!define MULTIUSER_INSTALLMODE_COMMANDLINE
!define MULTIUSER_INSTALLMODE_DEFAULT_CURRENTUSER
!define MULTIUSER_USE_PROGRAMFILES64
!define MULTIUSER_INSTALLMODE_INSTDIR "${PRODUCT_NAME}"
!define MULTIUSER_INSTALLMODE_INSTDIR_REGISTRY_KEY "${SETTINGS_KEY}"
!define MULTIUSER_INSTALLMODE_INSTDIR_REGISTRY_VALUENAME "InstallDir"
!define MULTIUSER_INSTALLMODE_DEFAULT_REGISTRY_KEY "${SETTINGS_KEY}"
!define MULTIUSER_INSTALLMODE_DEFAULT_REGISTRY_VALUENAME "InstallDir"

!include "MultiUser.nsh"
!include "MUI2.nsh"
!include "LogicLib.nsh"
!include "WinVer.nsh"
!include "WinMessages.nsh"

!define MUI_ICON "..\assets\EasyRun.ico"
!define MUI_UNICON "..\assets\EasyRun.ico"
!define MUI_ABORTWARNING
!define MUI_COMPONENTSPAGE_SMALLDESC

!define MUI_WELCOMEPAGE_TITLE_3LINES
!define MUI_WELCOMEPAGE_TEXT "Setup will install ${PRODUCT_NAME} ${VERSION}.$\r$\n$\r$\n${PRODUCT_NAME} lets you create short aliases for programs, folders and commands that you run from the Run box (Win+R), plus elevated shortcuts that run as an admin account.$\r$\n$\r$\nNo administrator rights are needed for a just-for-me install."
!insertmacro MUI_PAGE_WELCOME
!insertmacro MUI_PAGE_LICENSE "${SOURCE_DIR}\LICENSE.txt"
!insertmacro MULTIUSER_PAGE_INSTALLMODE
!insertmacro MUI_PAGE_COMPONENTS
!insertmacro MUI_PAGE_DIRECTORY
!insertmacro MUI_PAGE_INSTFILES
!define MUI_FINISHPAGE_TITLE_3LINES
!define MUI_FINISHPAGE_RUN "$INSTDIR\${APP_EXE}"
!define MUI_FINISHPAGE_TEXT "${PRODUCT_NAME} is installed.$\r$\n$\r$\n- Open ${PRODUCT_NAME} from the Start menu to create aliases.$\r$\n- In a new terminal, run 'erun help'.$\r$\n- Right-click a file or folder (if you enabled Explorer integration)."
!insertmacro MUI_PAGE_FINISH

!insertmacro MUI_UNPAGE_CONFIRM
!insertmacro MUI_UNPAGE_INSTFILES
!insertmacro MUI_LANGUAGE "English"

Function .onInit
  SetRegView 64
  ${IfNot} ${AtLeastWin10}
    MessageBox MB_OK|MB_ICONSTOP "${PRODUCT_NAME} ${VERSION} requires Windows 10 or Windows 11."
    Abort
  ${EndIf}
  ReadRegDWORD $0 HKLM "SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full" "Release"
  ${If} $0 < 528040
    MessageBox MB_OK|MB_ICONSTOP "${PRODUCT_NAME} needs .NET Framework 4.8 or later.$\r$\nInstall it from Windows Update, then run Setup again."
    Abort
  ${EndIf}
  StrCpy $9 $INSTDIR
  !insertmacro MULTIUSER_INIT
  ${If} $9 != ""
    StrCpy $INSTDIR $9
  ${EndIf}
FunctionEnd

Function un.onInit
  SetRegView 64
  !insertmacro MULTIUSER_UNINIT
FunctionEnd

; ---------------------------------------------------------------- sections

Section "${PRODUCT_NAME} (required)" SecCore
  SectionIn RO
  SetOutPath "$INSTDIR"
  SetOverwrite on
  nsExec::ExecToLog '"$SYSDIR\taskkill.exe" /im ${APP_EXE} /f'

  File "${SOURCE_DIR}\${APP_EXE}"
  File "${SOURCE_DIR}\${APP_EXE}.config"
  File "${SOURCE_DIR}\${CLI_EXE}"
  File "${SOURCE_DIR}\${CLI_EXE}.config"
  File "${SOURCE_DIR}\EasyRun.Core.dll"
  File "${SOURCE_DIR}\Microsoft.Win32.TaskScheduler.dll"
  File "${SOURCE_DIR}\LICENSE.txt"
  File "${SOURCE_DIR}\README.txt"

  WriteUninstaller "$INSTDIR\uninstall.exe"
  CreateShortcut "$SMPROGRAMS\${PRODUCT_NAME}.lnk" "$INSTDIR\${APP_EXE}" "" "$INSTDIR\${APP_EXE}" 0 SW_SHOWNORMAL "" "Create and manage Run-box aliases"

  WriteRegStr SHCTX "${SETTINGS_KEY}" "InstallDir" "$INSTDIR"
  WriteRegStr SHCTX "${UNINSTALL_KEY}" "DisplayName" "${PRODUCT_NAME}"
  WriteRegStr SHCTX "${UNINSTALL_KEY}" "DisplayVersion" "${VERSION}"
  WriteRegStr SHCTX "${UNINSTALL_KEY}" "Publisher" "${PRODUCT_PUBLISHER}"
  WriteRegStr SHCTX "${UNINSTALL_KEY}" "URLInfoAbout" "${PRODUCT_URL}"
  WriteRegStr SHCTX "${UNINSTALL_KEY}" "DisplayIcon" "$INSTDIR\${APP_EXE},0"
  WriteRegStr SHCTX "${UNINSTALL_KEY}" "InstallLocation" "$INSTDIR"
  WriteRegStr SHCTX "${UNINSTALL_KEY}" "UninstallString" '"$INSTDIR\uninstall.exe" /$MultiUser.InstallMode'
  WriteRegStr SHCTX "${UNINSTALL_KEY}" "QuietUninstallString" '"$INSTDIR\uninstall.exe" /$MultiUser.InstallMode /S'
  WriteRegDWORD SHCTX "${UNINSTALL_KEY}" "NoModify" 1
  WriteRegDWORD SHCTX "${UNINSTALL_KEY}" "NoRepair" 1
SectionEnd

Section "Explorer right-click integration" SecShell
  DetailPrint "Registering the Explorer menu..."
  nsExec::ExecToLog '"$INSTDIR\${APP_EXE}" --register-shell'
  WriteRegDWORD SHCTX "${SETTINGS_KEY}" "ShellMenu" 1
SectionEnd

Section "Add 'erun' to PATH" SecPath
  ${If} $MultiUser.InstallMode == "AllUsers"
    StrCpy $1 "Machine"
  ${Else}
    StrCpy $1 "User"
  ${EndIf}
  DetailPrint "Adding $INSTDIR to the $1 PATH..."
  ; [Environment]::SetEnvironmentVariable writes the registry and broadcasts WM_SETTINGCHANGE for us.
  nsExec::ExecToLog 'powershell -NoProfile -ExecutionPolicy Bypass -Command "$$d=\"$INSTDIR\"; $$p=[Environment]::GetEnvironmentVariable(\"Path\",\"$1\"); if([string]::IsNullOrEmpty($$p)){$$p=$$d}elseif(($$p.Split([char]59) -inotcontains $$d)){$$p=$$p.TrimEnd([char]59)+[char]59+$$d}; [Environment]::SetEnvironmentVariable(\"Path\",$$p,\"$1\")"'
  WriteRegDWORD SHCTX "${SETTINGS_KEY}" "OnPath" 1
SectionEnd

!insertmacro MUI_FUNCTION_DESCRIPTION_BEGIN
  !insertmacro MUI_DESCRIPTION_TEXT ${SecCore} "EasyRun, the 'erun' command-line tool, and the alias engine."
  !insertmacro MUI_DESCRIPTION_TEXT ${SecShell} "Adds 'Add as EasyRun alias' when you right-click a file or folder (under Show more options on Windows 11)."
  !insertmacro MUI_DESCRIPTION_TEXT ${SecPath} "Lets you run 'erun' from any new terminal window."
!insertmacro MUI_FUNCTION_DESCRIPTION_END

; ---------------------------------------------------------------- uninstall

Section "Uninstall"
  nsExec::ExecToLog '"$SYSDIR\taskkill.exe" /im ${APP_EXE} /f'
  ${If} ${FileExists} "$INSTDIR\${APP_EXE}"
    nsExec::ExecToLog '"$INSTDIR\${APP_EXE}" --unregister-shell'
  ${EndIf}

  ReadRegDWORD $0 SHCTX "${SETTINGS_KEY}" "OnPath"
  ${If} $0 == 1
    ${If} $MultiUser.InstallMode == "AllUsers"
      StrCpy $1 "Machine"
    ${Else}
      StrCpy $1 "User"
    ${EndIf}
    nsExec::ExecToLog 'powershell -NoProfile -ExecutionPolicy Bypass -Command "$$d=\"$INSTDIR\"; $$p=[Environment]::GetEnvironmentVariable(\"Path\",\"$1\"); if($$p){$$n=($$p.Split([char]59) | Where-Object {$$_ -and ($$_ -ine $$d)}) -join [char]59; [Environment]::SetEnvironmentVariable(\"Path\",$$n,\"$1\")}"'
  ${EndIf}

  Delete "$SMPROGRAMS\${PRODUCT_NAME}.lnk"
  Delete "$INSTDIR\${APP_EXE}"
  Delete "$INSTDIR\${APP_EXE}.config"
  Delete "$INSTDIR\${CLI_EXE}"
  Delete "$INSTDIR\${CLI_EXE}.config"
  Delete "$INSTDIR\EasyRun.Core.dll"
  Delete "$INSTDIR\Microsoft.Win32.TaskScheduler.dll"
  Delete "$INSTDIR\LICENSE.txt"
  Delete "$INSTDIR\README.txt"
  Delete "$INSTDIR\uninstall.exe"
  RMDir "$INSTDIR"

  DeleteRegKey SHCTX "${UNINSTALL_KEY}"
  DeleteRegKey SHCTX "${SETTINGS_KEY}"
  DetailPrint "Aliases you created remain in your profile. Remove them from the Alias Manager or with 'erun remove' before uninstalling if you want them gone."
SectionEnd

