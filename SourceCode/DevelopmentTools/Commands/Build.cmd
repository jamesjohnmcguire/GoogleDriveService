CD %~dp0
CD ..\..

REM IF "%1"=="release" CALL VersionUpdate BackUpManagerLibrary\BackupManagerLibrary.csproj
REM IF "%1"=="release" CALL VersionUpdate BackUpManager\BackupManager.csproj

CALL dotnet publish --configuration Release --runtime linux-x64 --self-contained true -p:PublishSingleFile=true -o Binaries\Linux MusicManager
CALL dotnet publish --configuration Release --runtime osx-x64 --self-contained true -p:PublishSingleFile=true -o Binaries\MacOS MusicManager
CALL dotnet publish --configuration Release --runtime win-x64 --self-contained true -p:PublishReadyToRun=true -p:PublishSingleFile=true --output Binaries\Windows MusicManager

IF "%1"=="release" GOTO release
GOTO end

:release
CD Bin\Release\AnyCPU

7z u BackUpManager.zip . -xr!*.json -xr!ref

hub release create -a BackUpManager.zip -m "%2" v%2

:end