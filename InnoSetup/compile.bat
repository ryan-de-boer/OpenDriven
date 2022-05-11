@echo off
setlocal
call vars.bat

set FILE=OpenDrivenInstall

if not exist "%ISCC%" echo Error: Please install Inno Setup to %ISCC%, or change vars.bat to point to the newer version 1>&2 & exit /b 1
rem clean
del "Output\%FILE%.exe" /Q 2> NUL

"%ISCC%" "OpenDriven.iss" /F"%FILE%"

if not exist "Output\%FILE%.exe" echo Inno Setup failed to compile "%FILE%.exe" 1>&2 & exit /b 1
