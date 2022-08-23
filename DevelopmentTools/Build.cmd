CD %~dp0
CD ..\..

rd /s /q Release

dotnet publish --configuration Release -p:PublishSingleFile=true --runtime linux-x64 --self-contained -o Release\linux-x64 BackUpManager
dotnet publish --configuration Release -p:PublishSingleFile=true --runtime osx-x64 --self-contained -o Release\osx-x64 BackUpManager
dotnet publish --configuration Release -p:PublishReadyToRun=true;PublishSingleFile=true --runtime=win-x64 --self-contained --output Release\win-x64 BackUpManager

IF "%1"=="release" GOTO release
GOTO end

:release
CD Release\linux-x64
7z u BackUpManager-linux-x64.zip .
MOVE BackUpManager-linux-x64.zip ..

CD ..\osx-x64
7z u BackUpManager-osx-x64.zip .
MOVE BackUpManager-osx-x64.zip ..

CD ..\win-x64
7z u BackUpManager-win-x64.zip .
MOVE BackUpManager-win-x64.zip ..

CD ..
REM Unfortunately, the following command does not work from the windows command
REM console.  Use a bash terminal.
REM gh release create v%2 --notes %3 *.zip

REM Old style
REM hub release create -a BackUpManager.zip -m "%2" v%2

:end