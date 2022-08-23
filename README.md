# GoogleDriveService

This is basically a program to back up your files to google drive, with out the need of a 'Google Drive' folder.  Furthermore, you can customize the back up process.

Currently, it uses Google Service Accounts, so that it doesn't need to be prompted for authorization each time it is run.

It depends on a file called BackUp.json located in %AppData%\DigitalZenWorks\BackUpManager or home/.config/BackUpManager.  Currently, you need to put this file there, as well as the service account, yourself.  It will look something like the following:

```json
[
	{
		"Email": "somebody@example.com",
		"ServiceAccount": "SomebodyServiceAccount.json",
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
| Email               | An optional email account to associate.               |
| ServiceAccount      | Another json file containing the email address and    |
|                     | key of the service account.  This can be obtained     |
|                     | from the Google Developer Console Credentials page.   |
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

There is an example at Documentation/Sample.BackUp.json
