SET utils=%USERPROFILE%\Data\Commands

CD %~dp0
cd ..\..\BackUpManager\bin

xcopy /D /E /H /I /R /S /Y debug %utils%
