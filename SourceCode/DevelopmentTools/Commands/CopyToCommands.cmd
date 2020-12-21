SET utils=%USERPROFILE%\Data\Commands

CD %~dp0
cd ..\..\BackUpManager\bin\x64

xcopy /D /E /H /I /R /S /Y Release %utils%
