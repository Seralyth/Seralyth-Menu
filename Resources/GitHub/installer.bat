REM To use, hit Ctrl + S, then select "Save as type", and click "All files (*.*)". Make sure its name is "installer.bat".
REM If on Discord, hit the download button and run it.

@echo off
setlocal enabledelayedexpansion
chcp ANSI

cls
title Seralyth Menu Installer // [#---------] Getting directory
echo Seralyth Menu Installer // [#---------] Getting directory
color 0e

:: Thanks to tdcvoid for telling me the new path for Oculus
if exist "C:/Program Files (x86)/Steam/steamapps/common/Gorilla Tag" (
    set "gamePath=C:/Program Files (x86)/Steam/steamapps/common/Gorilla Tag"
     goto afterDriveSearch
)
else if exist "C:/Program Files/Meta Horizon/Software/Software/another-axiom-gorilla-tag" (
    set "gamePath=C:/Program Files/Meta Horizon/Software/Software/another-axiom-gorilla-tag"
     goto afterDriveSearch
)

:: Deep search made by Lucky & fixed by everest
if not defined gamePath (
     for %%D in (D E F G H I J K L M N O P Q R S T U V W X Y Z) do (
        if exist "%%D:/SteamLibrary/steamapps/common/Gorilla Tag" (
            set "gamePath=%%D:/SteamLibrary/steamapps/common/Gorilla Tag"
            goto afterDriveSearch
        )
        else if exist "%%D:/Steam/steamapps/common/Gorilla Tag" (
            set "gamePath=%%D:/Steam/steamapps/common/Gorilla Tag"
            goto afterDriveSearch
        )
    )
)

:afterDriveSearch

if not defined gamePath (
    color 0c
    echo Gorilla Tag directory not found.
    echo Please open a mod-help ticket in the Discord server and report this. (https://discord.gg/seralyth)
    pause
    exit /b
)

color 0e
cls
title Seralyth Menu Installer // [###-------] Downloading BepInEx
echo Seralyth Menu Installer // [###-------] Downloading BepInEx
curl -L "https://github.com/BepInEx/BepInEx/releases/download/v5.4.23.4/BepInEx_win_x64_5.4.23.4.zip" -o .tmp-BPNX54234.zip

powershell -command "Expand-Archive -Path 'BPNX54234.zip' -DestinationPath '%gamePath%' -Force"

cls
title Seralyth Menu Installer // [####------] Creating directories
echo Seralyth Menu Installer // [####------] Creating directories
mkdir %gamePath%/BepInEx/config
mkdir %gamePath%/BepInEx/plugins

cls
title Seralyth Menu Installer // [#####-----] Downloading latest config
echo Seralyth Menu Installer // [#####-----] Downloading latest config
curl https://github.com/Seralyth/Seralyth-Menu/raw/refs/heads/master/Resources/GitHub/BepInEx.cfg -o %gamePath%/BepInEx/config/BepInEx.cfg

cls
title Seralyth Menu Installer // [#######---] Downloading menu
echo Seralyth Menu Installer // [#######---] Downloading menu
for /f "tokens=*" %%i in ('powershell -Command "(Invoke-RestMethod -Uri 'https://api.github.com/repos/Seralyth/Seralyth-Menu/releases/latest').assets | Where-Object { $_.name -like '*.dll' } | Select-Object -ExpandProperty browser_download_url"') do (
    set pluginUrl=%%i
)

if "%pluginUrl%"=="" (
    color 0c
    echo Failed to get latest release of menu.
    echo Please open a mod-help ticket in the Discord server amd report this. (https://discord.gg/seralyth)
    pause
    exit /b
)

color 0e
curl -L "%pluginUrl%" -o %gamePath%/BepInEx/plugins/"Seralyth Menu.dll"

cls
title Seralyth Menu Installer // [##########] Finished
echo Seralyth Menu Installer // [##########] Finished
echo Congratulations, you now have the menu!

del ".tmp-BPNX54234.zip"

pause
