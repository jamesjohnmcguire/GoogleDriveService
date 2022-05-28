<?php

include_once 'vendor/autoload.php';

defined('CREDENTIALS_FILE') or define('CREDENTIALS_FILE', 'credentials.json');

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
