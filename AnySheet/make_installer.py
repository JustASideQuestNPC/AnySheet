'''
Generates and then runs the nsis script to turn my spaghetti into an installer.
'''

import subprocess
from pathlib import Path
from os import listdir, path

PUBLISH_PROJECTS = ('AnySheet', 'CrashHandler')
APP_PATH = Path.cwd() / 'publish'
MODULE_PATH = Path.cwd() / 'AnySheet' / 'Modules'

BASE_NSIS = '''
RequestExecutionLevel user ; for some reason the default level is admin?

!include "MUI2.nsh"
!include "UninstallLog.nsh"

Name "AnySheet"
OutFile "AnySheet_Setup.exe"

InstallDir "$APPDATA\\AnySheet"

Var StartMenuFolder

; installer ui pages
!insertmacro MUI_PAGE_WELCOME
!insertmacro MUI_PAGE_DIRECTORY
!define MUI_STARTMENUPAGE_REGISTRY_ROOT "HKCU" 
!define MUI_STARTMENUPAGE_REGISTRY_KEY "AnySheet" 
!define MUI_STARTMENUPAGE_REGISTRY_VALUENAME "Start Menu Folder"
!insertmacro MUI_PAGE_STARTMENU Application $StartMenuFolder
!insertmacro MUI_PAGE_INSTFILES
!insertmacro MUI_PAGE_FINISH

; uninstaller ui pages
; !insertmacro MUI_UNPAGE_WELCOME
!insertmacro MUI_UNPAGE_CONFIRM
!insertmacro MUI_UNPAGE_INSTFILES
; !insertmacro MUI_UNPAGE_FINISH

!insertmacro MUI_LANGUAGE "English"

; configs for only removing installed files
!define UninstLog "uninstall.log" ; log name
var UninstLog
; registry paths
!define REG_ROOT "HKLM"
!define REG_APP_PATH "SOFTWARE\\AnySheet"
; error string if you somehow deleted the log without deleting the rest of the app
LangString UninstLogMissing ${LANG_ENGLISH} "${UninstLog} not found!$\\r$\\nUninstallation cannot proceed!"

; define macros
!define AddItem "!insertmacro AddItem"
!define BackupFile "!insertmacro BackupFile"
!define BackupFiles "!insertmacro BackupFiles"
!define CopyFiles "!insertmacro CopyFiles"
!define CreateDirectory "!insertmacro CreateDirectory"
!define CreateShortcut "!insertmacro CreateShortcut"
!define File "!insertmacro File"
!define Rename "!insertmacro Rename"
!define RestoreFile "!insertmacro RestoreFile"
!define RestoreFiles "!insertmacro RestoreFiles"
!define SetOutPath "!insertmacro SetOutPath"
!define WriteRegDWORD "!insertmacro WriteRegDWORD"
!define WriteRegStr "!insertmacro WriteRegStr"
!define WriteUninstaller "!insertmacro WriteUninstaller"

Section -openlogfile
    CreateDirectory "$INSTDIR"
    IfFileExists "$INSTDIR\\${UninstLog}" +3
        FileOpen $UninstLog "$INSTDIR\\${UninstLog}" w
    Goto +4
        SetFileAttributes "$INSTDIR\\${UninstLog}" NORMAL
        FileOpen $UninstLog "$INSTDIR\\${UninstLog}" a
        FileSeek $UninstLog 0 END
SectionEnd

Section
    ${SetOutPath} $INSTDIR

    ; includes the crash handler and the main app
    Call BruteForceInstallFiles

    ${WriteUninstaller} "AnySheet_Uninstaller.exe"

    !insertmacro MUI_STARTMENU_WRITE_BEGIN Application
        
    ; Create shortcuts
    ${WriteRegStr} HKCU "Software\\Modern UI Test" "" $INSTDIR
    ${CreateDirectory} "$SMPROGRAMS\\$StartMenuFolder"
    ${CreateShortcut} "$SMPROGRAMS\\$StartMenuFolder\\AnySheet.lnk" "$INSTDIR\\AnySheet.exe" "" "" ""

    !insertmacro MUI_STARTMENU_WRITE_END

SectionEnd

Section Uninstall
    ; Can't uninstall if uninstall log is missing!
    IfFileExists "$INSTDIR\\${UninstLog}" +3
        MessageBox MB_OK|MB_ICONSTOP "$(UninstLogMissing)"
        Abort
    
    Push $R0
    Push $R1
    Push $R2
    SetFileAttributes "$INSTDIR\\${UninstLog}" NORMAL
    FileOpen $UninstLog "$INSTDIR\\${UninstLog}" r
    StrCpy $R1 -1
    
    GetLineCount:
        ClearErrors
        FileRead $UninstLog $R0
        IntOp $R1 $R1 + 1
        StrCpy $R0 $R0 -2
        Push $R0   
        IfErrors 0 GetLineCount
    Pop $R0
    
    LoopRead:
        StrCmp $R1 0 LoopDone
        Pop $R0
    
        IfFileExists "$R0\\*.*" 0 +3
        RMDir $R0  #is dir
        Goto +9
        IfFileExists $R0 0 +3
        Delete $R0 #is file
        Goto +6
        StrCmp $R0 "${REG_ROOT} ${REG_APP_PATH}" 0 +3
        DeleteRegKey ${REG_ROOT} "${REG_APP_PATH}" #is Reg Element
        Goto +3
        StrCmp $R0 "${REG_ROOT} ${UNINSTALL_PATH}" 0 +2
        DeleteRegKey ${REG_ROOT} "${UNINSTALL_PATH}" #is Reg Element
    
        IntOp $R1 $R1 - 1
        Goto LoopRead

    LoopDone:

    FileClose $UninstLog
    Delete "$INSTDIR\\${UninstLog}"
    Delete "$INSTDIR\\data.json"
    ; on the one hand, this will delete EVERYTHING in the logs folder even if i didn't install it.
    ; on the other hand, if you were stupid enough to put important files in the logs folder then
    ; tbh you kinda deserve this.
    RMDir /r "$INSTDIR\\logs"
    ; i'm not deleting the modules folder in case you've got custom ones in there
    Pop $R2
    Pop $R1
    Pop $R0
SectionEnd\n
'''

def relative_path(path: str) -> Path:
    return Path.cwd() / path

print('starting build...\npublishing .net builds...')

for project in PUBLISH_PROJECTS:
    subprocess.run((
        f'dotnet publish {project}/{project}.csproj -c Release -r win-x64 --self-contained true ' +
         '-o publish'
    ), shell=True)

print('generating nsis script...\ngenerating file list...')
nsis_lines = [BASE_NSIS]
nsis_lines.append('Function BruteForceInstallFiles\n')
nsis_lines.append('    ; app files\n')

for filename in listdir(APP_PATH):
    nsis_lines.append(f'    File "publish\\{filename}"\n')
    nsis_lines.append(f'    FileWrite $UninstLog "$OUTDIR\\{filename}$\\r$\\n"\n')

nsis_lines.append('\n')
nsis_lines.append('    ; modules\n')
nsis_lines.append('    CreateDirectory "$INSTDIR\\Modules"\n')

for dirname in listdir(MODULE_PATH):
    if dirname != 'testModules' and path.isdir(dirname):
        nsis_lines.append(f'    ; {dirname}\n')
        nsis_lines.append(f'    CreateDirectory "$INSTDIR\\Modules\\{dirname}"\n')
        for filename in listdir(MODULE_PATH / dirname):
            nsis_lines.append(
                f'    File "/oname=Modules\\{dirname}\\{filename}" ' +
                f'"AnySheet\\Modules\\{dirname}\\{filename}"\n')
            nsis_lines.append(
                f'    FileWrite $UninstLog "$OUTDIR\\Modules\\{dirname}\\{filename}$\\r$\\n"\n')
        nsis_lines.append('\n')

nsis_lines.pop()
nsis_lines.append('FunctionEnd\n')

print('writing nsis script...')
with open(relative_path('WindowsInstaller.nsi'), 'w') as file:
    file.writelines(nsis_lines)

print('running nsis script...')   
subprocess.run(('makensis WindowsInstaller.nsi'), shell=True)