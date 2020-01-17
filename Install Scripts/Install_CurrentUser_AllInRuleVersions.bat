rem Split the version off of the directory name
for %%I in (.) do for /F "tokens=1 delims=v" %%a in ("%%~nxI") do set extensionName=%%a
rem Remove trailing spaces
for /f "tokens=* delims= " %%A in ('echo %extensionName% ') do set extensionName=%%A
set extensionName=%extensionName:~0,-1%

echo Installing %extensionName%

if not exist "%localappdata%\InRule\irAuthor\ExtensionExchange\" mkdir "%localappdata%\InRule\irAuthor\ExtensionExchange\"
if not exist "%localappdata%\InRule\irAuthor\ExtensionExchange\%extensionName%\" mkdir "%localappdata%\InRule\irAuthor\ExtensionExchange\%extensionName%\"
for /f "delims=" %%f in ('where /R "%cd%" "*.dll"') do xcopy /i /y "%%f" "%localappdata%\InRule\irAuthor\ExtensionExchange\%extensionName%\"