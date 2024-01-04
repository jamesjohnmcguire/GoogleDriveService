# GoogleDriveService

This is basically a program to back up your files.

Currently, it uses Google Drive, with out the need of a 'Google Drive' folder.   It uses Google Service Accounts, so that it doesn't need to be prompted for authorization each time it is run.  Furthermore, you can customize the back up process.

It depends on a main configuration file called BackUp.json located in %AppData%\DigitalZenWorks\BackUpManager or home/.config/BackUpManager.  Currently, you need to put this file there, as well as the service account files, yourself.  It will the format like the following:

```json
[
	{
		"AccountType": "GoogleServiceAccount",
		"AccountIdentifier": "SomebodyServiceAccount.json",
		"DriveMappings":
		[
			{
				"DriveParentFolderId": "XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX",
				"Path": "%USERPROFILE%/",
				"Excludes":
				[
					{
						"Path": "%USERPROFILE%/InstanceData",
						"ExcludeType": 0
					},
					{
						"Path": "%USERPROFILE%/Temp",
						"ExcludeType": 1
					},
					{
						"Path": "%USERPROFILE%/SomeBigTemporaryFile.tmp",
						"ExcludeType": 2
					}
				]
			}
		]
	}
]
```

The json fields contain the following information:

| Field:              |                                                       |
| ------------------- | ----------------------------------------------------- |
| AccountType         | The account type.  Currently, only                    |
|                     | GoogleServiceAccount is supported.                    |
| AccountIdentifier   | A service specific identifier.  For Google service    |
|                     | accounts, this will be the name of the json           |
|                     | authorization file.  This file needs to be in the     |
|                     | same location as the main configuration file.  This   |
|                     | can be obtained from the Google Developer Console     |
|                     |  Credentials page.                                    |
| DriveMappings       | A list (array) of directories to back up.  Each one   |
|                     | will contain the following:                           |
| DriveParentFolderId | The Google Drive Id to back up to. This can be        |
|                     | obtained from the Google Drive page 'Share' option.   |
|                     | It is the main content of the 'Share' URL.            |
| Path                | The local PC path to back up.                         |
| Excludes            | A list of paths to NOT back up.                       |
|                     | There are 3 exclude types:                            |
|                     | 1: Do not back up this directory and all it's         |
|                     | sub-directories.                                      |
|                     | 2: Do not back up files in this directory, but        |
|                     | proceed to back up it's sub-directories.              |
|                     | 3: Do not back up this specific file.                 |

There can multiple service accounts specified.

There is an example at Support/Sample.BackUp.json
