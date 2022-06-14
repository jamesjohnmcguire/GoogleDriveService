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
		string $serviceAccountFile,
		string $credentialsFile,
		string $tokensFile,
		string $name,
		array $scopes)
	{
		$client = null;

		switch ($mode)
		{
			case Mode::OAuth:
				break;
			case Mode::ServiceAccount:
				$client = self::AuthorizeServiceAccount(
					$serviceAccountFile, $name, $scopes);
				break;
			case Mode::Token:
				$client = self::AuthorizeByToken(
					$credentialsFile, $tokensFile, $name, $scopes);
				break;
			}

		// Final fall back, prompt user for confirmation code through web page
		if ($client === null && PHP_SAPI === 'cli')
		{
			$client = self::RequestAuthorization(
				$credentialsFile, $tokensFile, $name, $scopes);
		}

		return $client;
	}

	private static function AuthorizeByOAuth()
	{
		$client = null;

		return $client;
	}

	private static function AuthorizeByToken(
		$credentialsFile, $tokensFilePath, $name, $scopes)
	{	
		$client = null;
		$accessToken = self::AuthorizeByTokenFile($client, $tokensFilePath);

		if ($accessToken === null)
		{
			$accessToken = self::AuthorizeByTokenLocal($client);
		}

		$client = self::SetClient($credentialsFile, $name, $scopes);

		$client = self::SetAccessToken($client, $accessToken, $tokensFilePath);

		return $client;
	}

	private static function AuthorizeByTokenLocal($client)
	{
		// last chance attempt of hard coded file name
		$tokenFilePath = 'token.json';

		$accessToken = self::AuthorizeByTokenFile($client, $tokenFilePath);

		return $accessToken;
	}

	private static function AuthorizeByTokenFile($client, $tokenFilePath)
	{
		$accessToken = null;

		if (file_exists($tokenFilePath))
		{
			$fileContents = file_get_contents($tokenFilePath);
			$accessToken = json_decode($fileContents, true);
		}
		else
		{
			echo 'WARNING: token file doesn\'t exist - ' . $tokenFilePath .
				PHP_EOL;
		}

		return $accessToken;
	}

	private static function AuthorizeServiceAccount(
		$serviceAccountFilePath, $name, $scopes)
	{
		$client = self::SetClient(null, $name, $scopes);

		if (file_exists($serviceAccountFilePath))
		{
			putenv('GOOGLE_APPLICATION_CREDENTIALS=' . $serviceAccountFilePath);
		}

		// else, nothing else to do...

		$client->useApplicationDefaultCredentials();

		return $client;
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
		$client = null;

		if (PHP_SAPI !== 'cli')
		{
			echo 'WARNING: Requesting user authorization only works at the ' .
			'command line' . PHP_EOL;
		}
		else
		{
			$client = self::SetClient($credentialsFile, $name, $scopes);

			$authorizationUrl = $client->createAuthUrl();
			$authorizationCode =
				self::PromptForAuthorizationCodeCli($authorizationUrl);
	
			$accessToken =
				$client->fetchAccessTokenWithAuthCode($authorizationCode);
			$client = self::SetAccessToken($client, $accessToken, $tokensFile);
		}

		return $client;
	}

	private static function SetAccessToken($client, $tokens, $tokensFile)
	{
		$updatedClient = null;

		if ((is_array($tokens)) &&
			(!array_key_exists('error', $tokens)))
		{
			$client->setAccessToken($tokens);
			$updatedClient = new $client;

			$json =  json_encode($tokens);
			file_put_contents($tokensFile, $json);
		}
		else if (array_key_exists('error', $tokens))
		{
			echo 'Error key exists in tokens' . PHP_EOL;
		}
		else
		{
			echo 'Tokens is not an array' . PHP_EOL;
		}


		return $updatedClient;
	}

	private static function SetClient(
		string $credentialsFile, string $name, array $scopes)
	{
		$client = new Google_Client();

		$client->setAccessType('offline');
		$client->setApplicationName($name);
		$client->setPrompt('select_account consent');
		$client->setScopes($scopes);

		if (!empty($credentialsFile))
		{
			$client->setAuthConfig($credentialsFile);
		}

		return $client;
	}
}
