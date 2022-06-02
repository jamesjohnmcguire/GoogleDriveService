<?php

include_once 'vendor/autoload.php';

defined('CREDENTIALS_FILE') or define('CREDENTIALS_FILE', 'credentials.json');

enum Mode
{
	case None;
	case Discover;
	case OAuth;
	case Request;
	case ServiceAccount;
	case Token;
}

class GoogleAuthorization
{
	// TODO: Remove if going with just static approach
	private $client = null;
	private Mode $mode = Mode::None;
	public string $TokenFilePath = null;

	public static function Authorize (
		Mode $mode,
		string $serviceAccountJsonFile,
		string $accessTokenFile,
		string $tokenFile)
	{
		$result = false;

		// TODO: set to null if client is always returned
		//$client = new Google_Client();

		switch ($mode)
		{
			case OAuth:
				break;
			case ServiceAccount:
				break;
			case Token:
				$client = AuthorizeByToken($tokenFile);
				break;
			}

		// Final fall back, request user confirmation through web page
		if ($client === null)
		{
			$client == self::RequestAuthorization();
		}

		return $result;
	}

	private static function AuthorizeByOAuth()
	{
		$client = null;

		return $client;
	}

	private static function AuthorizeByToken($tokenFilePath)
	{	
		$client = null;
		$accessToken = self::AuthorizeByTokenFile($client, $tokenFilePath);

		if ($accessToken === null)
		{
			$accessToken = self::AuthorizeByTokenLocal($client);
		}

		$client = self::SetAccessToken($accessToken);

		return $client;
	}

	private static function AuthorizeByTokenLocal($client)
	{
		$tokenFilePath = 'token.json';

		$accessToken = self::AuthorizeByTokenFile($client, $tokenFilePath);

		return $accessToken;
	}

	private static function AuthorizeByTokenFile($client, $tokenFilePath)
	{
		$accessToken = null;

		if (file_exists($tokenFilePath))
		{
			$fileContents = file_get_contents($tokenPath);
			$accessToken = json_decode($fileContents, true);
		}

		return $accessToken;
	}

	private static function AuthorizeToken($client, $accessToken)
	{
		$result = false;

		// log

		$client->setPrompt('select_account consent');
		$client->setAccessToken($accessToken);

		$result = $client->isAccessTokenExpired();

		if ($result !== true)
		{
			// Refresh the token if possible, else fetch a new one.
			$refreshToken = $client->getRefreshToken();

			if ($refreshToken !== null)
			{
				$client->fetchAccessTokenWithRefreshToken($refreshToken);
				$result = true;
			}
			else
			{
				// Request authorization from the user.
				$authorizationCode =
					self::PromptForAuthorizationCodeCli($authorizationUrl);

				// Exchange authorization code for an access token.
				$accessToken =
					$client->fetchAccessTokenWithAuthCode($authorizationCode);
				$client->setAccessToken($accessToken);

				// Check to see if there was an error.
				if (array_key_exists('error', $accessToken))
				{
					$message = join(', ', $accessToken);
					throw new Exception($message);
				}

				$result = true;
			}
		}

		return $result;
	}

	private static function IsValidJson($string)
	{
		$isValidJson = false;

		json_decode($string);
		$check = json_last_error();

		if ($check === JSON_ERROR_NONE)
		{
			$isValidJson = true;
		}

		return $isValidJson;
	 }

	private static function PromptForAuthorizationCodeCli(
		string $authorizationUrl)
	{
		echo 'Open the following link in your browser:' . PHP_EOL;
		echo $authorizationUrl . PHP_EOL;
		echo 'Enter verification code: ';
		$authorizationCode = fgets(STDIN);
		$$authorizationCode = trim($authorizationCode);

		return $authorizationCode;
	}

	private static function RequestAuthorization()
	{
		$client = new Google_Client();

		$client->setAccessType('offline');
		$client->setApplicationName('Google Drive API Video Uploader');
		$client->setPrompt('select_account consent');
		// TODO: Check if this is best scope
		$client->setScopes(Google_Service_Drive::DRIVE_FILE);
		$client->addScope("https://www.googleapis.com/auth/drive");

		$client->setAuthConfig(CREDENTIALS_FILE);

		$authorizationUrl = $client->createAuthUrl();
		$authorizationCode =
			self::PromptForAuthorizationCodeCli($authorizationUrl);

		$accessToken = $client->fetchAccessTokenWithAuthCode($authorizationCode);
		$client = self::SetAccessToken($client, $accessToken);

		return $client;
	}

	private static function SetAccessToken($client, $accessToken)
	{
		$updatedClient = null;
		$isValid = IsValidJson($accessToken);

		if ((isValid == true) && (!array_key_exists('error', $accessToken)))
		{
			$client->setAccessToken($accessToken);
			$updatedClient = new $client;

			$json =  json_encode($accessToken);
			file_put_contents(CREDENTIALS_FILE, $json);
		}

		return $updatedClient;
	}
}
