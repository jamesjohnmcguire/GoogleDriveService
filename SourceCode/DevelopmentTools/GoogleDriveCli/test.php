<?php

include_once 'vendor/autoload.php';
require_once 'GoogleAuthorization.php';

defined('CREDENTIALS_FILE') or define('CREDENTIALS_FILE', 'credentials.json');

function TestRawRequestUser()
{
	$client = new Google_Client();

	$client->setAccessType('offline');
	$client->setApplicationName('Google Drive API Video Uploader');
	// TODO: Check if this is best scope
	$client->setPrompt('select_account consent');
	$client->setScopes(Google_Service_Drive::DRIVE_FILE);

	$client->addScope("https://www.googleapis.com/auth/drive");

	$credentialFile = __DIR__ . '/' . CREDENTIALS_FILE;

	$client->setAuthConfig($credentialFile);

	$authorizationUrl = $client->createAuthUrl();

	echo 'Open the following link in your browser:' . PHP_EOL;
	echo $authorizationUrl . PHP_EOL;
	echo 'Enter verification code: ';

	$authorizationCode = fgets(STDIN);
	$$authorizationCode = trim($authorizationCode);
	echo $authorizationCode . PHP_EOL;

	$accessToken = $client->fetchAccessTokenWithAuthCode($authorizationCode);
	echo "ACCESS TOKEN: " . PHP_EOL;
	print_r($accessToken);
	echo PHP_EOL;

	if (array_key_exists('error', $accessToken))
	{
		echo "ERROR:" . PHP_EOL;
	}
	else
	{
		$client->setAccessToken($accessToken);
		
		$json =  json_encode($accessToken);
		$credentialsFile = 'cretentials_new.json';
		echo "Saving to file: " . $credentialsFile . PHP_EOL;
		file_put_contents($credentialsFile, $json);
	}
}

function TestRequestUser()
{
	GoogleAuthorization::Authorize(
		Mode::Request,
		'',
		'credentials.json',
		'',
		'Google Drive API Video Uploader',
		['https://www.googleapis.com/auth/drive']);
}

TestRequestUser();
