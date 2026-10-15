/////////////////////////////////////////////////////////////////////////////
// <copyright file="ConfigurationInspector.cs" company="James John McGuire">
// Copyright © 2017 - 2026 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.BackUp.Reporting;

using System;
using System.IO;
using DigitalZenWorks.BackUp.Library;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

/// <summary>
/// Reviews configuration and explains selection without accessing providers.
/// </summary>
public static partial class ConfigurationInspector
{
	/// <summary>Reads configuration and produces local diagnostics.</summary>
	/// <param name="configurationPath">The configuration file.</param>
	/// <param name="settingsPath">The separate settings file.</param>
	/// <param name="credentialDirectory">The credential directory.</param>
	/// <param name="checkGit">
	/// Whether to inspect declared Git exclusions.
	/// </param>
	/// <returns>The review report.</returns>
	public static ConfigurationReport Review(
		string configurationPath,
		string settingsPath,
		string credentialDirectory,
		bool checkGit = false)
	{
		ConfigurationReport report = new()
		{
			ConfigurationPath = configurationPath,
			SettingsPath = settingsPath,
		};

		JToken document = ReadDocument(report, configurationPath, "$");
		ReadSettings(report, settingsPath);

		if (document is not JArray accounts)
		{
			if (document != null)
			{
				report.Add(
					DiagnosticSeverity.Error,
					"CFG002",
					"$",
					"Expected an array of accounts.");
			}

			return report;
		}

		ReadAccounts(report, accounts, credentialDirectory, checkGit);

		CheckOverlaps(report);
		return report;
	}

	private static JToken ReadDocument(
		ConfigurationReport report, string path, string location)
	{
		try
		{
			using StreamReader file = File.OpenText(path);
			using JsonTextReader reader = new(file);
			JsonLoadSettings settings = new()
			{
				DuplicatePropertyNameHandling =
					DuplicatePropertyNameHandling.Error,
			};

			JToken document = JToken.Load(reader, settings);

			if (reader.Read())
			{
				throw new JsonReaderException("Trailing content.");
			}

			return document;
		}
		catch (ArgumentException)
		{
			report.Add(
				DiagnosticSeverity.Error,
				"CFG001",
				location,
				"Invalid filename. Correct the file path.");
		}
		catch (JsonException)
		{
			report.Add(
				DiagnosticSeverity.Error,
				"CFG001",
				location,
				"Invalid JSON or duplicate property. Correct the document.");
		}
		catch (IOException)
		{
			report.Add(
				DiagnosticSeverity.Error,
				"CFG001",
				location,
				"Cannot read the file. Check its path and availability.");
		}
		catch (UnauthorizedAccessException)
		{
			report.Add(
				DiagnosticSeverity.Error,
				"CFG001",
				location,
				"Cannot read the file. Check its access permissions.");
		}

		return null;
	}

	private static void ReadSettings(
		ConfigurationReport report, string path)
	{
		JToken document = ReadDocument(report, path, "Settings");

		if (document == null)
		{
			return;
		}

		if (document is not JObject settings)
		{
			report.Add(
				DiagnosticSeverity.Error,
				"CFG002",
				"Settings",
				"Expected a settings object.");
			return;
		}

		CheckProperties(report, settings, "Settings", ["GlobalExcludes"]);

		ReadGlobalExcludes(report, settings);
	}

	private static void CheckProperties(
		ConfigurationReport report,
		JObject item,
		string location,
		string[] allowed)
	{
		foreach (JProperty property in item.Properties())
		{
			bool known = Array.IndexOf(allowed, property.Name) >= 0;

			if (known)
			{
				continue;
			}

			string code = "CFG003";
			string message = "Unknown property; check spelling and schema.";

			if (property.Name == "ExcludeType")
			{
				code = "CFG004";
				message = "Obsolete property; remove ExcludeType. " +
					"Exclusions are matched by path.";
			}

			report.Add(
				DiagnosticSeverity.Warning,
				code,
				location + "." + property.Name,
				message);
		}
	}

	private static string ReadString(
		ConfigurationReport report,
		JObject item,
		string name,
		string location,
		bool required = true)
	{
		JToken token = item[name];

		if (token == null && !required)
		{
			return string.Empty;
		}

		if (token == null || token.Type != JTokenType.String ||
			string.IsNullOrWhiteSpace(token.Value<string>()))
		{
			report.Add(
				DiagnosticSeverity.Error,
				"CFG002",
				location + "." + name,
				"Expected a nonempty string.");
			return string.Empty;
		}

		return token.Value<string>();
	}

	private static bool ReadBoolean(
		ConfigurationReport report,
		JObject item,
		string name,
		string location)
	{
		JToken token = item[name];

		if (token == null)
		{
			return false;
		}

		if (token.Type != JTokenType.Boolean)
		{
			report.Add(
				DiagnosticSeverity.Error,
				"CFG002",
				location + "." + name,
				"Expected true or false.");
			return false;
		}

		return token.Value<bool>();
	}

	private static string Normalize(
		ConfigurationReport report,
		string path,
		string location,
		string root = null)
	{
		try
		{
			return TraversalContext.NormalizePath(path, root);
		}
		catch (ArgumentException)
		{
			report.Add(
				DiagnosticSeverity.Error,
				"MAP002",
				location,
				"Invalid local path. Correct its syntax.");
		}
		catch (NotSupportedException)
		{
			report.Add(
				DiagnosticSeverity.Error,
				"MAP002",
				location,
				"Unsupported local path syntax.");
		}
		catch (PathTooLongException)
		{
			report.Add(
				DiagnosticSeverity.Error,
				"MAP002",
				location,
				"The local path exceeds the filesystem limit.");
		}

		return null;
	}

	private static bool ContainsPath(string root, string path)
	{
		StringComparison comparison = StringComparison.Ordinal;

		if (OperatingSystem.IsWindows())
		{
			comparison = StringComparison.OrdinalIgnoreCase;
		}

		root = Path.TrimEndingDirectorySeparator(root);
		path = Path.TrimEndingDirectorySeparator(path);

		if (string.Equals(root, path, comparison))
		{
			return true;
		}

		if (!Path.EndsInDirectorySeparator(root))
		{
			root += Path.DirectorySeparatorChar;
		}

		return path.StartsWith(root, comparison);
	}

	private static void ReadAccounts(
		ConfigurationReport report,
		JArray accounts,
		string credentialDirectory,
		bool checkGit)
	{
		report.AccountCount = accounts.Count;

		for (int index = 0; index < accounts.Count; index++)
		{
			string location = $"[{index}]";

			if (accounts[index] is JObject account)
			{
				ReadAccount(
					report, account, location, credentialDirectory, checkGit);
			}
			else
			{
				report.Add(
					DiagnosticSeverity.Error,
					"CFG002",
					location,
					"Expected an account object.");
			}
		}
	}

	private static void ReadGlobalExcludes(
		ConfigurationReport report, JObject settings)
	{
		if (settings["GlobalExcludes"] is not JArray excludes)
		{
			report.Add(
				DiagnosticSeverity.Error,
				"CFG002",
				"Settings.GlobalExcludes",
				"Supply an array of global exclusion paths, or [].");
			return;
		}

		for (int index = 0; index < excludes.Count; index++)
		{
			ReadGlobalExclude(report, excludes[index], index);
		}

		if (excludes.Count > 0)
		{
			string pol002Message =
				"Global rules filter directory traversal and allow remote " +
				"cleanup. Current file traversal uses the preceding " +
				"directory's rules; global file exclusions can differ. " +
				"Review file-specific rules with explain.";

			report.Add(
				DiagnosticSeverity.Warning,
				"POL002",
				"Settings.GlobalExcludes",
				pol002Message);
		}
	}

	private static void ReadGlobalExclude(
		ConfigurationReport report, JToken item, int index)
	{
		if (item.Type != JTokenType.String ||
			string.IsNullOrWhiteSpace(item.Value<string>()))
		{
			report.Add(
				DiagnosticSeverity.Error,
				"CFG002",
				$"Settings.GlobalExcludes[{index}]",
				"Expected a nonempty path string.");
			report.GlobalExcludes.Add(string.Empty);
			return;
		}

		string value = item.Value<string>();

		string normalized = Normalize(
			report, value, $"Settings.GlobalExcludes[{index}]");

		if (normalized == null)
		{
			report.GlobalExcludes.Add(string.Empty);
		}
		else
		{
			report.GlobalExcludes.Add(value);
		}
	}
}
