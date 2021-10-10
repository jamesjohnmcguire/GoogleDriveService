REM Examples
REM Command upload videos/jp/GreenChannelWeb/2020-10-25.mp4
REM Command list
REM Command deleteall
REM Command delete 1IcurHhUiafHbqw8qfw1UEGGLrGGFzDqH

@ECHO OFF
cd %~dp0
SET GOOGLE_APPLICATION_CREDENTIALS=%~dp0\GoogleDriveServiceAccount.json

php.exe GoogleDriveCommand.php %1 %2 %3
