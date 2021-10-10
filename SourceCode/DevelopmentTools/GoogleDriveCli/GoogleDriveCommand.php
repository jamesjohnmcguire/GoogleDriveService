<?php
// Examples
// GoogleDriveCommand upload videos/jp/GreenChannelWeb/2020-10-25.mp4
// GoogleDriveCommand deleteall
// GoogleDriveCommand delete 1IcurHhUiafHbqw8qfw1UEGGLrGGFzDqH

require_once "libraries/common/debug.php";
require_once "GoogleDrive.php";

$debugLevel = Debug::DEBUG;
$logFile = __DIR__ . '/LogFiles/GoogleDrive.log';
$debugger = new Debug($debugLevel, $logFile);

$command = null;
$data = null;
$showParent = false;
$showOnlyFolders = false;
$showOnlyRootLevel = false;

if (!empty($argv[1]))
{
    $command = $argv[1];
}

if (!empty($argv[2]))
{
    $data = $argv[2];
}

foreach ($argv as $argument)
{
	if ($argument == 'showParent')
	{
		$showParent = true;
	}
	else if ($argument == 'showOnlyFolders')
	{
		$showOnlyFolders = true;
	}
	else if ($argument == 'showOnlyRootLevel')
	{
		$showOnlyRootLevel = true;
	}
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

$googleDrive->ListFiles(
	$data, $showParent, $showOnlyFolders, $showOnlyRootLevel);
