<?php
include_once 'vendor/autoload.php';
require_once "libraries/common/common.php";
require_once "libraries/common/debug.php";

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
	private $root = null;
	private $service = null;
	private $serviceAccountFilePath = null;

	public function __construct($debug, $authorizationType = 'ServiceAccount')
	{
		$this->debug = $debug;

		$this->client = $this->Authorize($authorizationType);

		$contents = file_get_contents(SERVICE_ACCOUNT_FILE);
		$data = json_decode($contents);

		if (property_exists($data, 'root'))
		{
			$this->root = $data->root;
		}

		$this->service = new Google_Service_Drive($this->client);
	}

	public function About()
	{
		$this->debug->Show(Debug::DEBUG, "About begin");

		$about = $this->service->about;

		$options =
		[
			'fields' => 'storageQuota',
			'prettyPrint' => true
		];

		$response = $about->get($options);

		print_r($response->storageQuota);
		exit();

		return $response;
	}

	public function DeleteAllFiles()
	{
		$response = $this->GetFiles();

		foreach ($response as $file)
		{
			printf("Deleting file: %s (%s)\r\n", $file->name, $file->id);
			$this->service->files->delete($file->id);
		}

		return $response;
	}

	public function DeleteFile($fileId)
	{
		printf("Deleting file with id: %s\r\n", $fileId);
		$this->service->files->delete($fileId);
	}

	public function ListFiles($parentId, $showParent = false)
	{
		$response = $this->GetFiles($parentId, true, false);

		$this->debug->Show(Debug::DEBUG, "Listing files");

		foreach ($response as $file)
		{
			if ($showParent == true)
			{
				printf("Found file: Id: %s Parent: %s Name: %s\r\n",
					$file->id, $file->parents[0], $file->name);
				}
			else
			{
				printf("Found file: Id: %s Name: %s\r\n",
					$file->id, $file->name);
				foreach($file->permissions as $user)
				{
					echo "role: $user->role email: $user->emailAddress\r\n";
				}
				echo "\r\n";
			}
		}

		return $response;
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

	private static function AuthorizeRequestUser($client)
	{
		$result = false;

		// Request authorization from the user.
		$client->setRedirectUri("urn:ietf:wg:oauth:2.0:oob");
		$authUrl = $client->createAuthUrl();
		printf("Open the following link in your browser:\n%s\n", $authUrl);
		print 'Enter verification code: ';
		$rawCode = fgets(STDIN);
		$authCode = trim($rawCode);

		// Exchange authorization code for an access token.
		$accessToken = $client->fetchAccessTokenWithAuthCode($authCode);
		$client->setAccessToken($accessToken);

		// Check to see if there was an error.
		if (array_key_exists('error', $accessToken))
		{
			$notice = join(', ', $accessToken);
			Debug::ShowStatic(Debug::ERROR, $notice);
		}
		else
		{
			$result = true;
		}

		return $result;
	}

	private function Authorize($authorizationType)
	{
		$this->serviceAccountFilePath = __DIR__ . '/' . SERVICE_ACCOUNT_FILE;

		$client = new Google_Client();

		$client->setApplicationName('Google Drive API Video Uploader');
		$client->setScopes(Google_Service_Drive::DRIVE_FILE);
		//$client->addScope("https://www.googleapis.com/auth/drive.file");
		$client->addScope("https://www.googleapis.com/auth/drive");
		$client->setAccessType('offline');

		if ($authorizationType == 'ServiceAccount')
		{
			$this->debug->Show(Debug::DEBUG,
				'Setting environment variable GOOGLE_APPLICATION_CREDENTIALS ' .
				"to $this->serviceAccountFilePath" .PHP_EOL);

			putenv(
				"GOOGLE_APPLICATION_CREDENTIALS=$this->serviceAccountFilePath");

			if (getenv('GOOGLE_APPLICATION_CREDENTIALS'))
			{
				$this->debug->Show(Debug::DEBUG, 'using default credentials ' .
					'from environment GOOGLE_APPLICATION_CREDENTIALS' .PHP_EOL);
				$client->useApplicationDefaultCredentials();
			}
			else
			{
				$this->debug->Show(Debug::DEBUG,
					 'Missing environment GOOGLE_APPLICATION_CREDENTIALS');
				$this->AuthorizeOAuth($client);
			}
		}
		else if ($authorizationType == 'OAuth')
		{
			$this->AuthorizeOAuth($client);
		}
		else
		{
			$this->debug->Show(Debug::DEBUG, 'Loading credentials from token');
			$client->setPrompt('select_account consent');
			$this->AuthorizeOAuth($client);
			$this->SetAccessToken($client);
		}

		return $client;
	}

	private function AuthorizeOAuth($client)
	{
		$credentialFile = null;

		$this->debug->Show(Debug::DEBUG, 'Loading oauth credentials file');

		$checkFile = __DIR__ . '/' . CREDENTIALS_FILE;
		if (file_exists($checkFile))
		{
			$credentialFile = $checkFile;
		}

		if (empty($credentialFile))
		{
			$this->debug->Show(Debug::ERROR, "Credentials file missing");
		}
		else
		{
			$accessToken = $this->GetAccessTokenFromJsonFile($credentialFile);

			if (!empty($accessToken))
			{
				$client->setAccessToken($accessToken);
			}

			$client->setAuthConfig($credentialFile);
		}

		return $credentialFile;
	}

	private function GetAccessTokenFromJsonFile($filePath)
	{
		$accessToken = null;

		if (file_exists($filePath))
		{
			$contents = file_get_contents($filePath);
			$jsonContents = json_decode($contents, true);

			if (array_key_exists('access_token', $jsonContents))
			{
				$accessToken = $jsonContents['access_token'];
			}
		}
		else
		{
			$this->debug->Show(Debug::DEBUG, 'JSON credentials file missing');
		}

		return $accessToken;
	}

	private function GetFiles(
		$parentId, $showOnlyFolders = false, $showOnlyRootLevel = false)
	{
		// returns empty array
		// $files = new Google_Service_Drive_FileList($this->client);
		// $response = $files->getFiles();

		$options =
		[
			'pageSize' => 200,
			'supportsAllDrives' => true,
			'fields' => "files(id, name, parents, permissions)"
		];

		if ($showOnlyFolders == true && $showOnlyRootLevel == true)
		{
			if ($this->root != null)
			{
				$options['q'] =
					"mimeType = 'application/vnd.google-apps.folder'" .
					" and '$this->root' in parents";
			}
			else
			{
				if (empty($parentId))
				{
					$parentId = 'root';
				}

				$options['q'] =
					"mimeType = 'application/vnd.google-apps.folder'" .
					" and '$parentId' in parents";
			}
		}
		else if ($showOnlyFolders == true)
		{
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
			if ($this->root != null)
			{
				$options['q'] = "'$this->root' in parents";
			}
			else
			{
				$options['q'] = "'root' in parents";
			}
		}

		$response = $this->service->files->listFiles($options);

		return $response;
	}

	// Load previously authorized token from a file, if it exists.
	// The file token.json stores the user's access and refresh tokens,
	// and is created automatically when the authorization flow completes
	// for the first time (below).
	private function SetAccessToken($client)
	{
		$result = false;

		$tokenPath = __DIR__ . '/' . TOKEN_FILE;

		$accessToken = $this->GetAccessTokenFromJsonFile($tokenPath);

		if (!empty($accessToken))
		{
			$client->setAccessToken($accessToken);
		}

		// If there is no previous token or it's expired.
		if ($client->isAccessTokenExpired())
		{
			// Refresh the token if possible, else fetch a new one.
			if ($client->getRefreshToken())
			{
				$refreshToken = $client->getRefreshToken();
				$client->fetchAccessTokenWithRefreshToken($refreshToken);

				$result = true;
			}
			else
			{
				$result = self::AuthorizeRequestUser($client);
			}
		}
		else
		{
			$result = true;
		}

		if ($result == true)
		{
			$this->UpdateJsonTokensFile($client, $tokenPath);
		}

		return $result;
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

	private function UpdateJsonTokensFile($client, $tokenPath)
	{
		// Save the token to a file.
		$path = dirname($tokenPath);
		if (!file_exists($path))
		{
			mkdir($path, 0700, true);
		}

		$data = json_encode($client->getAccessToken());
		file_put_contents($tokenPath, $data);
	}
}
