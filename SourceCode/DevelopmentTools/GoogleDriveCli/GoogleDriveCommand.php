<?php
// Examples
// GoogleDriveCommand upload videos/jp/GreenChannelWeb/2020-10-25.mp4
// GoogleDriveCommand deleteall
// GoogleDriveCommand delete 1IcurHhUiafHbqw8qfw1UEGGLrGGFzDqH

require_once "libraries/common/FileTools.php";
require_once "GoogleDrive.php";

$debugLevel = Debug::DEBUG;
$logFile = __DIR__ . '/LogFiles/GoogleDrive.log';
$debugger = new Debug($debugLevel, $logFile);

$command = null;
$data = null;

if (!empty($argv[1]))
{
    $command = $argv[1];
}

if (!empty($argv[2]))
{
    $data = $argv[2];
}

$googleDrive = new GoogleDrive($debugger);

echo "Command is: $command\r\n";
echo "Data is: $data\r\n";

switch ($command)
{
    case 'about':
        $googleDrive->About();
        break;
    case 'delete':
        $googleDrive->DeleteFile($data);
        break;
    case 'deleteall':
        $googleDrive->DeleteAllFiles();
        break;
    case 'list':
        break;
    case 'upload':
        $googleDrive->UploadFile($data);
        break;
    default:
        break;
}

$googleDrive->ListFiles($data);
