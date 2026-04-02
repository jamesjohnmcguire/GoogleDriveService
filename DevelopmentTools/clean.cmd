@ECHO OFF
SETLOCAL

CD %~dp0
CD ..\SourceCode

IF EXIST 1\NUL RD /S /Q 1

CD BackUp.Library
IF EXIST bin\NUL RD /S /Q bin
IF EXIST obj\NUL RD /S /Q obj
CD ..

CD BackUp.Tests
IF EXIST bin\NUL RD /S /Q bin
IF EXIST obj\NUL RD /S /Q obj
CD ..

CD BackUpManager
IF EXIST bin\NUL RD /S /Q bin
IF EXIST obj\NUL RD /S /Q obj
CD ..

ENDLOCAL
