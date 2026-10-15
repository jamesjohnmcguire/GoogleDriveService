# Configuration review

Review commands run before logging and backup services start. They read local
configuration and filesystem state; they do not create application directories,
write BackUp.log, read credential contents, or contact storage providers.

## Commands

Inspection, report models, and text/JSON formatting live in
`SourceCode/BackUp.Reporting/`. The console application handles command
dispatch and exit codes. Reporting reuses the backup library's traversal
rules; backup execution does not depend on reporting.

```powershell
BackUpManager validate
BackUpManager validate --config "C:\Config\BackUp.json" --json
BackUpManager explain "D:\Data\Projects\Example"
BackUpManager explain "D:\Data\Projects\Example" --check-git
```

Both commands accept `--config` (`-c`), `--settings` (`-s`), `--json` (`-j`),
and `--check-git` (`-g`). Options follow the command name. For `explain`,
they may appear before or after the path; `validate` takes no positional
arguments. Parsing and generated help use DigitalZenWorks.CommandLine.Commands.
The application additionally rejects extra positional arguments and blank paths.
The default files are BackUp.json and Settings.json in
`%APPDATA%\DigitalZenWorks\BackUpManager` on Windows.

An alternative configuration path does **not** change the credential directory
or the settings default. Credentials are still looked up by filename in the
application data directory, matching backup behavior. `--settings` reviews an
alternative settings file; it is not a new backup option.

Exit codes: **0** means no validation errors (warnings may remain), **1** means
configuration errors, and **2** means invalid command arguments. Intentionally
excluded or uncovered paths produce explanations/warnings, not failure codes.
JSON output includes all per-rule details without log messages; console output
summarizes repetitive cleanup-policy findings.

## Reading a report

Each finding includes severity, code, configuration location, and next action.
For example, `[0].DriveMappings[2].Path` locates the third mapping in the first
account. `Settings.GlobalExcludes[0]` refers to the separate settings file.

The report counts configured entries, not all available service accounts.
Credential-file existence does not verify authorization, destination access,
quota, or successful uploads. Capacity and remote inventory checks are separate
future work; no fixed quota is assumed.

| Codes | Meaning |
| --- | --- |
| CFG001–CFG004 | Unreadable/invalid JSON, invalid values, unknown/obsolete fields |
| CFG005 | Inactive CheckDriveMappings setting |
| ACC002–ACC003 | Unsupported provider or unavailable credential reference |
| MAP002–MAP005 | Invalid, unsupported, missing, or context-dependent source |
| MAP007 | Duplicate or nested source mappings |
| EXC002–EXC006 | Duplicate/conflicting, misplaced, wildcard, or unknown protection rules |
| POL001–POL002 | Remote cleanup policy and global-rule limitations |
| COV001–COV002 | No applicable mapping or multiple local selections |
| GIT001–GIT009 | Declared Git protection and optional local Git findings |

Unknown fields are warnings so existing documents remain reviewable.
Malformed documents and unsupported providers are errors. Only
GoogleServiceAccount currently runs backups. Individual files cannot serve as
mapping roots; use directory mappings.

`CheckDriveMappings` is inactive because sibling-folder cleanup is compiled out
under `#if REVIEW`. Validation exposes this; it does not enable deletion.

## Explaining selection

`explain` reports every applicable mapping with its account credential filename,
source root, destination parent folder ID, and selection status. It checks
parent directories, so an excluded ancestor explains an excluded descendant.
It also reports missing items and skipped links/reparse points.

Statuses use `MappingStatus`: `Unknown`, `Unsupported`, `Selected`, `Excluded`,
`Unavailable`, `Invalid`, `Missing`, and `SkippedLink`. JSON preserves these
names as strings. `Missing` follows the filesystem existence checks; inspection
exceptions produce `Unavailable` when the filesystem reports them.

A `Selected` result describes local traversal only. It does not establish that
a file exists remotely. Errors elsewhere in the report still need attention.
The command examines one requested path; it does not inventory every file or
prove complete backup coverage.

The current traversal passes different global rules to directory and file
processing. Global directory exclusions such as `obj` block descent. A global
filename rule can still leave that file selected while allowing remote cleanup.
POL002 exposes this existing behavior. Use explicit mapping exclusions for
file-specific rules and inspect the result.

Wildcard exclusions use the existing local-file expansion logic. Unmatched
patterns disappear, and wildcard directory trees are not supported. Relative
paths depend on the process working directory, not the configuration location;
prefer absolute mapping roots.

## Intentional Git exclusions

An exclusion can document why it exists:

```json
{
  "Path": "Projects/Example",
  "KeepOnRemote": true,
  "Reason": "Source is pushed to a Git remote",
  "Protection": "GitRemote"
}
```

Reason and Protection are optional metadata. Existing exclusions keep their
behavior. Git repositories are never automatically excluded or treated as
verified backups.

`--check-git` inspects only exclusions explicitly marked `GitRemote`. It reports
configured remote names, changed/untracked files, absent upstream tracking, and
commits ahead of locally recorded upstream history. It never fetches or pushes.
The check concerns the containing working tree, so a nested excluded directory
may report changes elsewhere in that repository. Remote reachability, ignored
files, and actual remote contents remain unverified.

`KeepOnRemote: true` retains matching remote content under that exclusion's
policy. False, including an omitted value, permits exclusion cleanup.
`--ignore-abandoned` does not disable exclusion cleanup. Reports describe policy,
not a concrete deletion plan: determining actual targets requires remote
inventory. Other mappings can have different policies for the same source.
