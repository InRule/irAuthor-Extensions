rem Split the version off of the directory name
for %%I in (.) do for /F "tokens=1 delims=v" %%a in ("%%~nxI") do set extensionName=%%a
rem Remove trailing spaces
for /f "tokens=* delims= " %%A in ('echo %extensionName% ') do set extensionName=%%A
set extensionName=%extensionName:~0,-1%

echo Removing %extensionName%

rmdir /s /q "%localappdata%\InRule\irAuthor\ExtensionExchange\%extensionName%\"
rmdir /s /q "C:\Program Files (x86)\InRule\irAuthor\Extensions\%extensionName%\"