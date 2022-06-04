<?php

include_once 'vendor/autoload.php';

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
	//public string $TokenFilePath = null;

	public static function Authorize (
		Mode $mode,
		string $serviceAccountJsonFile,
		string $credentialsFile,
		string $tokensFile,
		string $name,
		array $scopes)
	{
		$result = false;

		// TODO: set to null if client is always returned
		//$client = new Google_Client();
		$client = null;

		switch ($mode)
		{
			case Mode::OAuth:
				break;
			case Mode::ServiceAccount:
				break;
			case Mode::Token:
				$client = AuthorizeByToken($tokensFile);
				break;
			}

		// Final fall back, request user confirmation through web page
		if ($client === null)
		{
			$client = self::RequestAuthorization(
				$credentialsFile, $tokensFile, $name, $scopes);
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

	private static function RequestAuthorization(
		string $credentialsFile, $tokensFile, string $name, array $scopes)
	{
		$client = new Google_Client();

		$client->setAccessType('offline');
		$client->setApplicationName($name);
		$client->setPrompt('select_account consent');
		$client->setScopes($scopes);

		$client->setAuthConfig($credentialsFile);

		$authorizationUrl = $client->createAuthUrl();
		$authorizationCode =
			self::PromptForAuthorizationCodeCli($authorizationUrl);

		$accessToken =
			$client->fetchAccessTokenWithAuthCode($authorizationCode);
		$client = self::SetAccessToken($client, $accessToken, $tokensFile);

		return $client;
	}

	private static function SetAccessToken($client, $tokens, $tokensFile)
	{
		$updatedClient = null;

		if ((is_array($accessToken)) &&
			(!array_key_exists('error', $accessToken)))
		{
			$client->setAccessToken($accessToken);
			$updatedClient = new $client;

			$json =  json_encode($accessToken);
			file_put_contents($tokensFile, $json);
		}

		return $updatedClient;
	}
}
