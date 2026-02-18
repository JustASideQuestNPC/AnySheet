'''
Generates and then runs the nsis script to turn my spaghetti into an installer.
'''

import subprocess
from pathlib import Path
from os import listdir

PUBLISH_PROJECTS = ('AnySheet', 'CrashHandler')
APP_PATH = Path.cwd() / 'publish'
MODULE_PATH = Path.cwd() / 'AnySheet' / 'Modules'

def relative_path(path: str) -> Path:
    return Path.cwd() / path

print('starting build...\npublishing .net builds...')

for project in PUBLISH_PROJECTS:
    subprocess.run((
        f'dotnet publish {project}/{project}.csproj -c Release -r win-x64 --self-contained true ' +
         '-o publish'
    ), shell=True)

print('generating nsis script...\nreading main script...')
nsis_lines = []
with open(relative_path('WindowsInstaller.nsi')) as file:
    for line in file:
        if line.startswith('Function'):
            break
        nsis_lines.append(line)

print('generating file list...')
nsis_lines.append('Function BruteForceInstallFiles\n')
nsis_lines.append('    ; app files\n')

for filename in listdir(APP_PATH):
    nsis_lines.append(f'    File "publish\\{filename}"\n')
    nsis_lines.append(f'    FileWrite $UninstLog "$OUTDIR\\{filename}$\\r$\\n"\n')

nsis_lines.append('\n')
nsis_lines.append('    ; modules\n')
nsis_lines.append('    CreateDirectory "$INSTDIR\\Modules"\n')

for dirname in listdir(MODULE_PATH):
    if dirname != 'testModules':
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