SET utils=%USERPROFILE%\Data\Commands

CD %~dp0
cd ..\..\Release

xcopy /D /E /H /I /R /S /Y win-x64 %utils%
