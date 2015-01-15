@ECHO OFF

SETLOCAL

SET VERSION=%1
if NOT "%VERSION%" == "" (
  SET VERSION=-BuildVersion '%VERSION%'
)

POWERSHELL "& { . '%~dp0build\build.ps1' %VERSION% }"
EXIT /B %ERRORLEVEL%