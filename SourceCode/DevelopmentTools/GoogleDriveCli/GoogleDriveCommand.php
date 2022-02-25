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
$options =
[
	'showOnlyFolders' => false,
	'showOnlyRootLevel' => false,
	'showParent' => false,
	'showShared' => false
];

if (!empty($argv[1]))
{
    $command = $argv[1];
}

if (!empty($argv[2]))
{
	$exists = array_key_exists($argv[2], $options);

	if ($exists === false)
	{
		$data = $argv[2];
	}
}

foreach ($argv as $argument)
{
	$exists = array_key_exists($argument, $options);

	if ($exists === true)
	{
		$options[$argument] = true;
	}
}

$googleDrive = new GoogleDrive($debugger, $options);

echo "Command is: $command\r\n";
echo "Data is: $data\r\n";

if ($googleDrive === null)
{
	echo "ERROR: google drive object does not exist!";
}
else
{
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
		case 'get':
			$googleDrive->GetFile($data);
			break;
		case 'help':
			echo "Examples\r\n";
			echo "Command get <file id>\r\n";
			echo "Command list\r\n";
			echo "Command list showParent showOnlyRootLevel showOnlyFolders showShared \r\n";
			echo "Command deleteall\r\n";
			echo "Command delete 1IcurHhUiafHbqw8qfw1UEGGLrGGFzDqH\r\n";
			break;
		case 'list':
			$googleDrive->ListFiles($data);
			break;
		case 'upload':
			$googleDrive->UploadFile($data);
			break;
		default:
			break;
	}
}
