/////////////////////////////////////////////////////////////////////////////
// <copyright file="ConfigurationInspector.Validation.cs" company="James John McGuire">
// Copyright © 2017 - 2026 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.BackUp.Reporting;

using System;
using System.Collections.Generic;
using System.IO;
using DigitalZenWorks.BackUp.Library;
using Newtonsoft.Json.Linq;

/// <summary>Validates account, mapping, and exclusion declarations.</summary>
public static partial class ConfigurationInspector
{
	private static void ReadAccount(
		ConfigurationReport report,
		JObject account,
		string location,
		string credentialDirectory,
		bool checkGit)
	{
		CheckProperties(
			report,
			account,
			location,
			["AccountType", "AccountIdentifier",
				"DriveMappings", "CheckDriveMappings"]);

		string type = ReadString(report, account, "AccountType", location);
		string identifier = ReadString(
			report, account, "AccountIdentifier", location);
		bool supported = type == "GoogleServiceAccount";

		CheckProvider(report, location, supported);

		if (supported && identifier.Length > 0)
		{
			CheckCredential(report, identifier, location, credentialDirectory);
		}

		CheckInactiveMappingSetting(report, account, location);

		ReadMappings(report, account, location, identifier, supported, checkGit);
	}

	private static void ReadMapping(
		ConfigurationReport report,
		JObject item,
		string location,
		string account,
		bool supported,
		bool checkGit)
	{
		CheckProperties(
			report,
			item,
			location,
			["Path", "DriveParentFolderId", "Excludes"]);

		string path = ReadString(report, item, "Path", location);
		string parent = ReadString(
			report, item, "DriveParentFolderId", location);
		string source = Normalize(report, path, location + ".Path");

		if (source == null)
		{
			return;
		}

		DriveMapping mapping = new()
		{
			LocalPath = path,
			DriveParentFolderId = parent,
		};

		ReviewedMapping reviewed = new()
		{
			Account = account,
			Location = location,
			Source = source,
			Mapping = mapping,
			Supported = supported && !File.Exists(source),
		};

		report.Mappings.Add(reviewed);

		CheckMappingPath(report, source, path, location);

		ReadExclusions(report, reviewed, item, location, checkGit);
	}

	private static void ReadExclusion(
		ConfigurationReport report,
		ReviewedMapping mapping,
		JObject item,
		string location,
		Dictionary<string, bool> paths,
		bool checkGit)
	{
		CheckProperties(
			report,
			item,
			location,
			["Path", "KeepOnRemote", "Reason", "Protection"]);

		string path = ReadString(report, item, "Path", location);
		bool keep = ReadBoolean(report, item, "KeepOnRemote", location);
		string reason = ReadString(report, item, "Reason", location, false);
		string protection = ReadString(
			report, item, "Protection", location, false);

		// Match ExpandExclude: the original mapping path is the base.
		string normalized = Normalize(
			report, path, location + ".Path", mapping.Mapping.LocalPath);

		if (normalized == null)
		{
			return;
		}

		Exclude rule = new(path, keep)
		{
			Reason = reason,
			Protection = protection,
			ConfigurationLocation = location,
		};

		mapping.Mapping.Excludes.Add(rule);

		CheckDuplicateExclusion(report, paths, normalized, keep, location);

		CheckExclusionPath(report, mapping.Source, normalized, path, location);

		CheckCleanupPolicy(report, keep, location);

		CheckProtection(report, protection, normalized, location, checkGit);
	}

	private static void CheckOverlaps(ConfigurationReport report)
	{
		foreach (ReviewedMapping mapping in report.Mappings)
		{
			int overlaps = 0;

			foreach (ReviewedMapping other in report.Mappings)
			{
				if (ReferenceEquals(mapping, other))
				{
					continue;
				}

				if (ContainsPath(mapping.Source, other.Source))
				{
					overlaps++;
				}
			}

			if (overlaps > 0)
			{
				string map007Message =
					$"This source contains or duplicates {overlaps} other " +
					"mapping root(s). Review exclusions and use explain " +
					"to check which mappings select a particular path.";

				report.Add(
					DiagnosticSeverity.Warning,
					"MAP007",
					mapping.Location,
					map007Message);
			}
		}
	}

	private static StringComparer GetPathComparer()
	{
		if (OperatingSystem.IsWindows())
		{
			return StringComparer.OrdinalIgnoreCase;
		}

		return StringComparer.Ordinal;
	}

	private static void CheckProvider(
		ConfigurationReport report, string location, bool supported)
	{
		if (!supported)
		{
			string acc002Message =
				"Only GoogleServiceAccount currently runs backups. " +
				"Choose a supported provider.";

			report.Add(
				DiagnosticSeverity.Error,
				"ACC002",
				location + ".AccountType",
				acc002Message);
		}
	}

	private static void CheckCredential(
		ConfigurationReport report,
		string identifier,
		string location,
		string credentialDirectory)
	{
		if (identifier.IndexOfAny(['/', '\\', ':']) >= 0)
		{
			string acc003Message =
				"Use a credential filename in the application data " +
				"directory, without directory components.";

			report.Add(
				DiagnosticSeverity.Error,
				"ACC003",
				location + ".AccountIdentifier",
				acc003Message);
		}
		else
		{
			string credential = Path.Combine(
				credentialDirectory, identifier);

			if (!File.Exists(credential))
			{
				string acc003Message =
					"Credential file not found in the application " +
					"data directory. Check the filename.";

				report.Add(
					DiagnosticSeverity.Error,
					"ACC003",
					location + ".AccountIdentifier",
					acc003Message);
			}
		}
	}

	private static void CheckInactiveMappingSetting(
		ConfigurationReport report, JObject account, string location)
	{
		if (account["CheckDriveMappings"] != null)
		{
			ReadBoolean(report, account, "CheckDriveMappings", location);
			string cfg005Message =
				"This setting is inactive: sibling-folder cleanup is " +
				"compiled out under REVIEW. It provides no coverage check.";

			report.Add(
				DiagnosticSeverity.Warning,
				"CFG005",
				location + ".CheckDriveMappings",
				cfg005Message);
		}
	}

	private static void ReadMappings(
		ConfigurationReport report,
		JObject account,
		string location,
		string identifier,
		bool supported,
		bool checkGit)
	{
		if (account["DriveMappings"] is not JArray mappings)
		{
			report.Add(
				DiagnosticSeverity.Error,
				"CFG002",
				location + ".DriveMappings",
				"Expected an array of mappings.");
			return;
		}

		report.MappingCount += mappings.Count;

		for (int index = 0; index < mappings.Count; index++)
		{
			string mappingLocation = $"{location}.DriveMappings[{index}]";

			if (mappings[index] is JObject mapping)
			{
				ReadMapping(
					report,
					mapping,
					mappingLocation,
					identifier,
					supported,
					checkGit);
			}
			else
			{
				report.Add(
					DiagnosticSeverity.Error,
					"CFG002",
					mappingLocation,
					"Expected a mapping object.");
			}
		}
	}

	private static void CheckMappingPath(
		ConfigurationReport report, string source, string path, string location)
	{
		if (File.Exists(source))
		{
			string map003Message =
				"Individual file roots are unsupported and are skipped. " +
				"Map the containing directory and review its exclusions.";

			report.Add(
				DiagnosticSeverity.Error,
				"MAP003",
				location + ".Path",
				map003Message);
		}
		else if (!Directory.Exists(source))
		{
			string map004Message =
				"Source directory is missing or inaccessible; backup " +
				"will skip it. Check the path or attach its drive.";

			report.Add(
				DiagnosticSeverity.Warning,
				"MAP004",
				location + ".Path",
				map004Message);
		}

		if (!Path.IsPathFullyQualified(path) ||
			path != Environment.ExpandEnvironmentVariables(path))
		{
			string map005Message =
				"Prefer an absolute source path. Relative paths depend " +
				"on the working directory; exclusion expansion uses the " +
				"original source string.";

			report.Add(
				DiagnosticSeverity.Warning,
				"MAP005",
				location + ".Path",
				map005Message);
		}
	}

	private static void ReadExclusions(
		ConfigurationReport report,
		ReviewedMapping reviewed,
		JObject item,
		string location,
		bool checkGit)
	{
		JToken exclusionToken = item["Excludes"];

		if (exclusionToken == null)
		{
			return;
		}

		if (exclusionToken is not JArray exclusions)
		{
			report.Add(
				DiagnosticSeverity.Error,
				"CFG002",
				location + ".Excludes",
				"Expected an array of exclusion objects.");
			return;
		}

		report.ExclusionCount += exclusions.Count;
		Dictionary<string, bool> paths = new(GetPathComparer());

		for (int index = 0; index < exclusions.Count; index++)
		{
			string ruleLocation = $"{location}.Excludes[{index}]";

			if (exclusions[index] is JObject exclusion)
			{
				ReadExclusion(
					report,
					reviewed,
					exclusion,
					ruleLocation,
					paths,
					checkGit);
			}
			else
			{
				report.Add(
					DiagnosticSeverity.Error,
					"CFG002",
					ruleLocation,
					"Expected an exclusion object.");
			}
		}
	}

	private static void CheckDuplicateExclusion(
		ConfigurationReport report,
		Dictionary<string, bool> paths,
		string normalized,
		bool keep,
		string location)
	{
		if (paths.TryGetValue(normalized, out bool previous))
		{
			string code = "EXC002";
			string message = "Duplicate exclusion; keep one rule.";

			if (previous != keep)
			{
				code = "EXC003";
				message = "Conflicting KeepOnRemote values for one path. " +
					"Resolve the conflict before running backup.";
			}

			report.Add(
				DiagnosticSeverity.Warning,
				code,
				location,
				message);
		}
		else
		{
			paths.Add(normalized, keep);
		}
	}

	private static void CheckExclusionPath(
		ConfigurationReport report,
		string source,
		string normalized,
		string path,
		string location)
	{
		if (!ContainsPath(source, normalized))
		{
			report.Add(
				DiagnosticSeverity.Warning,
				"EXC004",
				location + ".Path",
				"Exclusion is outside this mapping. Check its path.");
		}

		if (path.Contains('*', StringComparison.Ordinal))
		{
			string exc005Message =
				"Wildcards expand only to existing local files. " +
				"They do not declare a directory subtree exclusion.";

			report.Add(
				DiagnosticSeverity.Warning,
				"EXC005",
				location + ".Path",
				exc005Message);
		}
	}

	private static void CheckCleanupPolicy(
		ConfigurationReport report, bool keep, string location)
	{
		if (!keep)
		{
			string pol001Message =
				"This rule permits removal of matching remote content; " +
				"--ignore-abandoned does not disable exclusion cleanup. " +
				"Use KeepOnRemote: true to retain excluded remote content.";

			report.Add(
				DiagnosticSeverity.Information,
				"POL001",
				location,
				pol001Message);
		}
	}

	private static void CheckProtection(
		ConfigurationReport report,
		string protection,
		string normalized,
		string location,
		bool checkGit)
	{
		if (protection == "GitRemote")
		{
			string git001Message =
				"Git protection is declared, not verified. A repository " +
				"does not protect unpushed changes or ignored local data.";

			report.Add(
				DiagnosticSeverity.Information,
				"GIT001",
				location,
				git001Message);

			if (checkGit)
			{
				LocalGitInspector.Review(report, normalized, location);
			}
		}
		else if (protection.Length > 0)
		{
			string exc006Message =
				"Unknown protection label. Use GitRemote for an " +
				"optional local Git check.";

			report.Add(
				DiagnosticSeverity.Warning,
				"EXC006",
				location + ".Protection",
				exc006Message);
		}
	}
}
