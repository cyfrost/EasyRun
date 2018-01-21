;Language strings English
LangString SEC_CORE ${LANG_ENGLISH} "EasyRun Core (Required)"
LangString SEC_EXPLORER ${LANG_ENGLISH} "Explorer Integration"


LangString DESC_CORE ${LANG_ENGLISH} "The core component of EasyRun package"
LangString DESC_EXPLORER ${LANG_ENGLISH} "Enables/Disables the Windows Explorer integration module of EasyRun"


;Assign language strings to sections
!insertmacro MUI_FUNCTION_DESCRIPTION_BEGIN
!insertmacro MUI_DESCRIPTION_TEXT ${CORE} $(DESC_CORE)
!insertmacro MUI_DESCRIPTION_TEXT ${EXPLORER} $(DESC_EXPLORER)
!insertmacro MUI_FUNCTION_DESCRIPTION_END
