rem Split the version off of the directory name
for %%I in (.) do for /F "tokens=1 delims=v" %%a in ("%%~nxI") do set extensionName=%%a
rem Remove trailing spaces
for /f "tokens=* delims= " %%A in ('echo %extensionName% ') do set extensionName=%%A
set extensionName=%extensionName:~0,-1%

echo Installing %extensionName%

if not exist "C:\Program Files (x86)\InRule\irAuthor\Extensions\" mkdir "C:\Program Files (x86)\InRule\irAuthor\Extensions\"
if not exist "C:\Program Files (x86)\InRule\irAuthor\Extensions\%extensionName%\" mkdir "C:\Program Files (x86)\InRule\irAuthor\Extensions\%extensionName%\"
for /f "delims=" %%f in ('where /R "%cd%" "*.dll"') do xcopy /i /y "%%f" "C:\Program Files (x86)\InRule\irAuthor\Extensions\%extensionName%\"