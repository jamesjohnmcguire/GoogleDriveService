CD %~dp0
CD ..\SourceCode

IF "%1"=="publish" GOTO publish
IF "%1"=="release" GOTO publish

:default
dotnet clean /p:Platform="Any CPU"
dotnet build /p:Platform="Any CPU"
dotnet test /p:Platform="Any CPU"
GOTO :EOF

:publish
IF EXIST Release\NUL rd /s /q Release

dotnet clean BackUpManager
dotnet publish --configuration Release --output Release\win-x64 -p:PublishReadyToRun=true -p:PublishSingleFile=true -restore --runtime win-x64 --self-contained BackUpManager

IF "%1"=="release" GOTO release
GOTO :EOF

:release
dotnet publish --configuration Release --output Release\linux-x64 -p:PublishReadyToRun=true -p:PublishSingleFile=true --runtime linux-x64 --self-contained BackUpManager
dotnet publish --configuration Release --output Release\osx-x64 -p:PublishReadyToRun=true -p:PublishSingleFile=true --runtime osx-x64 --self-contained BackUpManager

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
gh release create v%2 --notes %3 *.zip
