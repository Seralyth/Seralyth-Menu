REM To use, hit Ctrl + S, then select "Save as type", and click "All files (*.*)". Make sure its name is "installer.bat".
REM If on Discord, hit the download button and run it.

@echo off
setlocal enabledelayedexpansion
chcp ANSI

cls
title United Goyim College Fund Installer // [#---------] Getting directory
color 0e

:: Thanks to tdcvoid for telling me the new path for Oculus
set steamPath1="C:/Program Files (x86)/Steam/steamapps/common/Gorilla Tag"
set steamPath2="C:/Program Files/Meta Horizon/Software/Software/another-axiom-gorilla-tag" 

if exist %steamPath1% (
    set gamePath=%steamPath1%
) else if exist %steamPath2% (
    set gamePath=%steamPath2%
) else if exist %steamPath3% (
    set gamePath=%steamPath3%
) else if exist %steamPath4% (
    set gamePath=%steamPath4%
) else (
    color 0c
    set /p userPath=Gorilla Tag directory not found.
    pause
    exit /b
)

if not defined gamePath (
    for %%D in (E F G H I J K L M N O P Q R S T U V W X Y Z) do (
        if exist "%%D:/SteamLibrary/steamapps/common/Gorilla Tag" (
            set "gamePath=%%D:/SteamLibrary/steamapps/common/Gorilla Tag"
            goto afterDriveSearch
        )
        if exist "%%D:/Steam/steamapps/common/Gorilla Tag" (
            set "gamePath=%%D:/Steam/steamapps/common/Gorilla Tag"
            goto afterDriveSearch
        )
    )
)

color 0e
cls
title United Goyim College Fund Installer // [###-------] Downloading BepInEx
curl -L "https://github.com/BepInEx/BepInEx/releases/download/v5.4.23.4/BepInEx_win_x64_5.4.23.4.zip" -o BPNX54234.zip

powershell -command "Expand-Archive -Path 'BPNX54234.zip' -DestinationPath '%gamePath%' -Force"

cls
title United Goyim College Fund Installer // [####------] Creating directories
mkdir %gamePath%/BepInEx/config
mkdir %gamePath%/BepInEx/plugins

cls
title United Goyim College Fund Installer // [#####-----] Downloading latest config
curl https://github.com/iiDk-the-inactual/UnitedGoyimCollegeFund/raw/refs/heads/master/Resources/GitHub/BepInEx.cfg -o %gamePath%/BepInEx/config/BepInEx.cfg

cls
title United Goyim College Fund Installer // [#######---] Downloading menu
for /f "tokens=*" %%i in ('powershell -Command "(Invoke-RestMethod -Uri 'https://api.github.com/repos/iiDk-the-inactual/UnitedGoyimCollegeFund/releases/latest').assets | Where-Object { $_.name -like '*.dll' } | Select-Object -ExpandProperty browser_download_url"') do (
    set pluginUrl=%%i
)

if "%pluginUrl%"=="" (
    color 0c
    echo Failed to get latest release of menu, please report to Discord
    pause
    exit /b
)

color 0e
curl -L "%pluginUrl%" -o %gamePath%/BepInEx/plugins/"United Goyim College Fund.dll"

cls
title United Goyim College Fund Installer // [##########] Finished
echo Congratulations, you now have the menu!

del "BPNX54234.zip"

pause
