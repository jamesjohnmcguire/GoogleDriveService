SET utils=%USERPROFILE%\Data\Commands

CD %~dp0
cd ..\..\BackUpManager\bin\debug

xcopy /D /E /H /I /R /S /Y netcoreapp3.1 %utils%
