# GoogleDriveService

This is basically a program to back up your files.

Currently, it uses Google Drive, with out the need of a 'Google Drive' folder.   It uses Google Service Accounts, so that it doesn't need to be prompted for authorization each time it is run.  Furthermore, you can customize the back up process.

The main configuration file is `BackUp.json`. Place it and your service-account
credential files in the application data directory:

| Platform | Default directory |
| --- | --- |
| Windows | `%APPDATA%\DigitalZenWorks\BackUpManager` |
| Linux | `$XDG_CONFIG_HOME/DigitalZenWorks/BackUpManager`, or `~/.config/DigitalZenWorks/BackUpManager` |
| macOS | `~/Library/Application Support/DigitalZenWorks/BackUpManager` |

The application uses .NET's platform-specific `ApplicationData` location.
See [the .NET macOS folder behavior](https://learn.microsoft.com/en-us/dotnet/core/compatibility/core-libraries/8.0/getfolderpath-unix).
`Settings.json` and `BackUp.log` use this same directory.
An explicit `--config` path changes only the configuration file, not the
credential or settings directory. Use native paths for each operating system.
The program does not expand `~` in JSON or in the `--config` argument.
Shell expansion happens before the program receives command-line arguments;
prefer absolute paths when in doubt. JSON source and exclusion paths expand
existing environment variables using .NET's `%NAME%` syntax; variable names
such as `USERPROFILE` are platform-specific.

Windows path concatenation has been replaced with platform-aware joins.
The CI matrix covers Windows, Linux, and macOS; live cloud backup operation
on each platform requires separate integration verification.

Configuration example:

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
|                     | default application data directory. This             |
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

## Logging

BackupManager writes current log entries to
`%AppData%\DigitalZenWorks\BackUpManager\BackUp.log` on Windows.
On the first logged event of a new local day, the previous file becomes
`BackUpyyyyMMdd.log`. Existing archive names receive a numbered suffix
instead of being overwritten. The newest 31 archives are retained.

The library supports both dated active files and a fixed active filename.
See [file logging](SourceCode/LoggingFoundation/LoggingService/README.md)
for retention, restart, and ownership details. There is no command-line
setting for the logging mode yet.
