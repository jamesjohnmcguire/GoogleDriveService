SET utils=%USERPROFILE%\Data\Commands

CD %~dp0
cd ..\..\Bin\Release

xcopy /D /E /H /I /R /S /Y AnyCPU %utils%
