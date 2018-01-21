Section $(SEC_CORE) Core
	
   SectionIn RO
	
	
   SetOutPath "$INSTDIR"
   SetOverwrite ifnewer
  nsExec::ExecToLog '"$SYSDIR\cmd.exe" /C "taskkill.exe -im EasyRun.exe /f"'
  ;add installation resources here
  File res\License.txt
  File res\EasyRun.exe
  File res\CommandLine.dll
  File res\Explintegrate.dll
  File res\Interop.IWshRuntimeLibrary.dll
  File res\Microsoft.Win32.TaskScheduler.dll

  
SetShellVarContext current
CreateDirectory "$SMPROGRAMS\${PRODUCT_NAME}"
CreateShortCut "$SMPROGRAMS\${PRODUCT_NAME}\${PRODUCT_NAME}.lnk" "$INSTDIR\${MAIN_EXECUTABLE_FILENAME}"
CreateShortCut "$SMPROGRAMS\${PRODUCT_NAME}\Uninstall ${PRODUCT_NAME}.lnk" "$INSTDIR\uninst.exe"
  

SectionEnd

Section $(SEC_EXPLORER) Explorer
;enables win explorer integration
;MessageBox MB_OK "gonna add"
ExecWait '$INSTDIR\EasyRun.exe -e'

;nsExec::ExecToLog '$SYSDIR\cmd.exe /c "$INSTDIR\EasyRun.exe /e true"'
;nsExec::ExecToLog '"$SYSDIR\cmd.exe" /C "$INSTDIR\EasyRun.exe /e"'
;MessageBox MB_OK "did add"
SectionEnd


;tentative section
Function createshortcut

SetShellVarContext current
CreateShortCut "$DESKTOP\${PRODUCT_NAME}.lnk" "$INSTDIR\${MAIN_EXECUTABLE_FILENAME}" ""


FunctionEnd

