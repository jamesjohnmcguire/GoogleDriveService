CD %~dp0
CD ..\..

IF EXIST Bin\Release\AnyCPU\NUL DEL /Q Bin\Release\AnyCPU\*.*

dotnet build --configuration Release

IF "%1"=="release" GOTO release
GOTO end

:release
CD Bin\Release\AnyCPU

7z u BackUpManager.zip . -xr!*.json -xr!ref

hub release create -a BackUpManager.zip -m "%2" v%2

:end