# BackUpManager

BackUpManager is a configurable backup tool for selected local directories,
with support for multiple storage services planned. It maps directories to
accounts and destinations, with exclusions to control what gets backed up.

Google Drive is the first and currently only supported service, using service
accounts without an interactive login for each run. Planned additions include
personal Google accounts, OneDrive, and Dropbox.

## Platforms

BackUpManager uses .NET 10, with Windows, macOS, and Linux as intended target
platforms. The CI workflow builds and runs tests on Linux. Full backup operation
on macOS and Linux still needs path-handling fixes: some configuration and
credential paths contain hard-coded Windows separators. The local review
commands already construct their default paths using platform separators.

## Configuration

Configuration belongs in the user's application data directory, under
`DigitalZenWorks/BackUpManager`. The code asks .NET for the platform's base
directory using `Environment.GetFolderPath(SpecialFolder.ApplicationData)`:

| Platform | Application data base directory |
| --- | --- |
| Windows | `%APPDATA%` (normally `AppData\Roaming` under the user profile) |
| macOS | `~/Library/Application Support` |
| Linux | `$XDG_CONFIG_HOME`, or `~/.config` when unset or empty |

These are the locations used by the local review commands; the backup path
limitations above must be resolved before relying on them across platforms.
See [.NET's Unix folder resolution](https://source.dot.net/System.Private.CoreLib/src/runtime/src/libraries/System.Private.CoreLib/src/System/Environment.GetFolderPathCore.Unix.cs.html).

On Windows, place BackUp.json, Settings.json, and service-account credential
files in `%APPDATA%\DigitalZenWorks\BackUpManager`.
The `--config` option selects a different BackUp.json; credentials and settings
still use the application data directory.

`%APPDATA%` does not automatically translate to `~`. Here, `~` denotes the user's
home directory; the application does not expand it in JSON paths. Use absolute
paths appropriate to the machine. The example below uses Windows paths.

BackUp.json contains an array of accounts, each with directory mappings:

```json
[
	{
		"AccountType": "GoogleServiceAccount",
		"AccountIdentifier": "SomebodyServiceAccount.json",
		"DriveMappings": [
			{
				"DriveParentFolderId": "XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX",
				"Path": "C:/Users/Example/Data",
				"Excludes": [
					{
						"Path": "Temp",
						"KeepOnRemote": false,
						"Reason": "Temporary files"
					},
					{
						"Path": "Projects/Example",
						"KeepOnRemote": true,
						"Reason": "Source is pushed to a Git remote",
						"Protection": "GitRemote"
					}
				]
			}
		]
	}
]
```

Replace the sample source and destination with your own paths and folder IDs.
See [Support/Sample.BackUp.json](Support/Sample.BackUp.json).

| Field | Meaning |
| --- | --- |
| AccountType | Currently GoogleServiceAccount |
| AccountIdentifier | Credential filename in the application data directory |
| DriveMappings | Source directory and destination mappings |
| Path | Mapping root, or an exclusion path relative to that root |
| DriveParentFolderId | Destination parent folder ID |
| Excludes | Explicit paths excluded from traversal |
| KeepOnRemote | True retains excluded remote content; false permits cleanup |
| Reason | Optional explanation of an intentional exclusion |
| Protection | Optional GitRemote declaration; not proof of protection |

A directory exclusion also prevents traversal into its descendants.
`ExcludeType` is obsolete. An omitted `KeepOnRemote` defaults to false.

Settings.json supplies global directory exclusions, for example:

```json
{
  "GlobalExcludes": ["obj", "node_modules"]
}
```

## Review before running

```powershell
BackUpManager validate
BackUpManager explain "D:/Data/Projects/Example"
BackUpManager validate --json
BackUpManager explain "D:/Data/Projects/Example" --check-git
```

These commands run local checks without starting backup services or file logging.
Reports distinguish intentional exclusions, missing paths, unsupported mappings,
overlaps, and inactive configuration. Optional Git inspection uses local state
only. See [configuration review](Support/ConfigurationReview.md) for options,
exit codes, diagnostic codes, and current traversal limitations.

## Run a backup

```powershell
BackUpManager --ignore-abandoned --config "C:/Config/BackUp.json"
```

`--ignore-abandoned` prevents abandoned-item cleanup; it does not disable
explicit exclusion cleanup. Review KeepOnRemote policies before running.
`CheckDriveMappings` is currently inactive.

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
