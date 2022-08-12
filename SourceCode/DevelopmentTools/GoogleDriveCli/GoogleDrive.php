<?php
include_once 'vendor/autoload.php';
require_once "libraries/common/common.php";
require_once "libraries/common/debug.php";

use DigitalZenWorks\GoogleApiAuthorization\Authorizer;
use DigitalZenWorks\GoogleApiAuthorization\Mode;

defined('CREDENTIALS_FILE') or define('CREDENTIALS_FILE', 'ProjectCredentials.json');
defined('SHARED_FOLDER') or
	define('SHARED_FOLDER', '1Mnztj-6iGYfnI0EkXCV-y7cizfekg5X9');
defined('SERVICE_ACCOUNT_FILE') or
	define ('SERVICE_ACCOUNT_FILE', 'GoogleDriveServiceAccount.json');
defined('TOKEN_FILE') or define('TOKEN_FILE', 'Tokens.json');

class GoogleDrive
{
	protected $debug = null;

	private $client = null;
	private $coreSharedParentFolderId = null;
	private $service = null;
	private $serviceAccountFilePath = null;
	private $showOnlyFolders = false;
	private $showOnlyRootLevel = false;
	private $showParent = false;
	private $showShared = false;

	public function __construct(
		$debug, $options = [], $authorizationType = 'ServiceAccount')
	{
		$this->debug = $debug;

		foreach ($options as $key => $option)
		{
			$this->$key = $option;
		}

		$this->client = Authorizer::authorize(
			Mode::ServiceAccount,
			null,
			$this->serviceAccountFilePath,
			null,
			'Google Drive API File Uploader',
			['https://www.googleapis.com/auth/drive'],
			null);

		if ($this->client != null)
		{
			$this->GetCoreSharedParentFolderIdFromFile();

			$this->service = new Google_Service_Drive($this->client);
		}
	}

	public function About()
	{
		$this->debug->Show(Debug::DEBUG, "About begin");

		if ($this->service === null)
		{
			echo "ERROR: service object does not exist!";
		}
		else
		{
			$about = $this->service->about;

			$options =
			[
				'fields' => 'storageQuota',
				'prettyPrint' => true
			];

			$response = $about->get($options);

			print_r($response->storageQuota);
		}
	}

	public function DeleteAllFiles()
	{
		$response = $this->GetFiles(null);

		foreach ($response as $file)
		{
			try
			{
				echo "\033[36mDeleting id: " . 
					"$file->id Name $file->name\033[0m\r\n";
				$this->service->files->delete($file->id);
			}
			catch (Exception $exception)
			{
				$message = $exception->getMessage();
				echo "\033[31mError: $message\033[0m\r\n";
			}
		}

		return $response;
	}

	public function DeleteFile($fileId)
	{
		try
		{
			echo "\033[36mDeleting file with id: $fileId\033[0m\r\n";
			$this->service->files->delete($fileId);
		}
		catch (Exception $exception)
		{
			$message = $exception->getMessage();
			echo "\033[31mError: $message\033[0m\r\n";
		}
	}

	public function GetFile($fileId)
	{
		try
		{
			$fileFields = 'createdTime, id, mimeType, modifiedTime, name, ' .
				'ownedByMe, owners, parents, size, webContentLink';

			$options =
			[
				'fields' => "$fileFields"
			];

			echo "\033[36mGetting file with id: $fileId\033[0m\r\n";
			$file = $this->service->files->get($fileId, $options);

			print_r($file);
		}
		catch (Exception $exception)
		{
			$message = $exception->getMessage();
			echo "\033[31mError: $message\033[0m\r\n";
		}
	}

	public function ListFiles($parentId = null)
	{
		$files = $this->GetFiles(
			$parentId, $this->showOnlyFolders, $this->showOnlyRootLevel);

		$this->debug->Show(Debug::DEBUG, "Listing files");
		$this->debug->Show(Debug::DEBUG, "parent id: $parentId");

		echo "\r\n";

		if ($this->showShared === true)
		{
			$this->debug->Show(
				Debug::DEBUG, "Showing files also shared with me");
			echo "  ";

			if ($this->showParent == true)
			{
				echo "Id\t\t\t\t    Parent\t\tName\tOwner\r\n";
			}
			else
			{
				echo "Id\t\t\t\t  Name\tOwner\r\n";
			}
		}
		else
		{
			$this->debug->Show(Debug::DEBUG, "Showing files only owned by me");

			if ($this->showParent == true)
			{
				echo "Id\t\t\t\t    Parent\t\tName\r\n";
			}
			else
			{
				echo "Id\t\t\t\t  Name\r\n";
			}
		}

		foreach ($files as $file)
		{
			if ($this->showShared === true)
			{
				if ($file->ownedByMe === true)
				{
					echo "* ";
				}
				else
				{
					echo "  ";
				}
			}

			if (!empty($file->parents))
			{
				$parent = $file->parents[0];
			}
			else
			{
				$parent = '\033[31m <none> \033[0m';
			}

			if ($this->showParent == true)
			{
				echo "$file->id $parent $file->name\r\n";
			}
			else
			{
				if ($this->showShared === true)
				{
					$owner = $file->owners[0]->emailAddress;
					echo "$file->id $file->name\t$owner\r\n";
				}
				else
				{
					echo "$file->id $file->name\r\n";
				}
			}
		}

		$count = count($files);
		echo "\r\n";
		echo "total count: $count\r\n";

		return $files;
	}

	public function UploadFile($file)
	{
		if (file_exists($file))
		{
			$this->debug->Show(Debug::DEBUG, "Starting file upload of $file");

			$driveFile = new Google_Service_Drive_DriveFile();
			$driveFile->name = basename($file);

			$parents = [SHARED_FOLDER];
			$driveFile->setParents($parents);

			// defer so it doesn't immediately return.
			$this->client->setDefer(true);
			$request = $this->service->files->create($driveFile);

			$chunkSizeBytes = 20 * 1024 * 1024;

			// Create a media file upload to represent our upload process.
			$media = new Google_Http_MediaFileUpload(
				$this->client,
				$request,
				'video/mp4',
				null,
				true,
				$chunkSizeBytes);

			$fileSize = filesize($file);
			$media->setFileSize($fileSize);

			$status = false;
			$handle = fopen($file, "rb");

			if ($handle === false)
			{
				$this->debug->Show(Debug::ERROR, "Failed to open file: $file");
			}
			else
			{
				$index = 1;
				$endOfFile = feof($handle);

				while (($status === false) && ($endOfFile === false))
				{
					$uploadedAmount = $media->getProgress();
					$bytes =  number_format($uploadedAmount);
					$this->debug->Show(Debug::DEBUG,
						"Uploaded file chunk: $index - $bytes bytes");

					$chunk = self::GetFileChunk($handle, $chunkSizeBytes);
					$status = $media->nextChunk($chunk);

					$index++;
					$endOfFile = feof($handle);
				}

				fclose($handle);
				$this->debug->Show(Debug::DEBUG, "Upload complete");
			}

			// The final value of $status will be the data from the API
			// for the object that has been uploaded.
			$result = false;

			if ($status !== false)
			{
				$result = true;
				$this->debug->Show(Debug::DEBUG, "Uploaded file success");
			}
		}
	}

	private static function GetFileChunk($handle, $chunkSize)
	{
		$byteCount = 0;
		$giantChunk = "";
		$endOfFile = feof($handle);

		while ($endOfFile === false)
		{
			// fread will never return more than 8192 bytes
			/// if the stream is read buffered and 
			// it does not represent a plain file
			$chunk = fread($handle, 8192);
			$byteCount += strlen($chunk);
			$giantChunk .= $chunk;
			if ($byteCount >= $chunkSize)
			{
				return $giantChunk;
			}

			$endOfFile = feof($handle);
		}

		return $giantChunk;
	}

	private function GetFiles(
		$parentId, $showOnlyFolders = false, $showOnlyRootLevel = false)
	{
		// returns empty array
		// $files = new Google_Service_Drive_FileList($this->client);
		// $response = $files->getFiles();

		// Including 'permissions' in fields will limit the result set to 100.
		$fileFields =
			'id, mimeType, name, ownedByMe, owners, parents, webContentLink';

		$options =
		[
			'fields' => "files($fileFields), nextPageToken",
			'pageSize' => 1000,
			'q' => '',
			'supportsAllDrives' => true
		];


		if ($showOnlyFolders == true && $showOnlyRootLevel == true)
		{
			$options['q'] =
				"mimeType = 'application/vnd.google-apps.folder'" .
				" and 'root' in parents";
		}
		else if ($showOnlyFolders == true)
		{
			echo "showOnlyFolders is true\r\n";
			echo "parentId: $parentId\r\n";

			$options['q'] = "mimeType = 'application/vnd.google-apps.folder'";

			if (!empty($parentId))
			{
				$options['q'] =
					"mimeType = 'application/vnd.google-apps.folder'" .
					" and '$parentId' in parents";
			}

		}
		else if ($showOnlyRootLevel == true)
		{
			$options['q'] = "'root' in parents";
		}
		else
		{
			if (!empty($parentId))
			{
				$options['q'] = "'$parentId' in parents";
			}
		}

		if ($this->showShared == false)
		{
			if (empty($options['q']))
			{
				$options['q'] = "'me' in owners";
			}
			else
			{
				$options['q'] .= " and 'me' in owners";
			}
		}

		print_r($options);

		$files = [];
		$pageToken = null;
		do
		{
			try
			{
				if ($pageToken !== null)
				{
					$options['pageToken'] = $pageToken;
				}

				$response = $this->service->files->listFiles($options);

				$files = array_merge($files, $response->files);
				$pageToken = $response->getNextPageToken();
			}
			catch (Exception $exception)
			{
				$message = $exception->getMessage();
				echo "Error: $message\r\n";
				$pageToken = null;
			}
		} while ($pageToken !== null);

		return $files;
	}

	private function GetCoreSharedParentFolderIdFromFile()
	{
		if (!empty($this->serviceAccountFilePath)  && file_exists($this->serviceAccountFilePath))
		{
			$contents = file_get_contents($this->serviceAccountFilePath);
			$data = json_decode($contents);

			if (property_exists($data, 'core_shared_parent_folder_id'))
			{
				$this->coreSharedParentFolderId =
					$data->core_shared_parent_folder_id;
			}
		}
	}

	private function TransferOwnership($email, $file)
	{
		$newPermission = new Google_Service_Drive_Permission();
		$newPermission->setRole('owner');
		$newPermission->setType('user');
		$newPermission->setEmailAddress($email);
		$options = array('transferOwnership' => 'true');

		$this->service->permissions->create($file->id, $newPermission, $options);
	}
}
