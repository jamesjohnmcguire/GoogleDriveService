/////////////////////////////////////////////////////////////////////////////
// <copyright file="ConfigurationCommands.cs" company="James John McGuire">
// Copyright © 2017 - 2026 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace BackUpManager;

using System;
using System.IO;
using DigitalZenWorks.BackUp.Reporting;
using DigitalZenWorks.CommandLine.Commands;

/// <summary>
/// Routes read-only configuration commands before service startup.
/// </summary>
internal static class ConfigurationCommands
{
	/// <summary>
	/// Executes a review command parsed by the command-line package.
	/// </summary>
	/// <param name="command">The validated review command.</param>
	/// <param name="output">The report output.</param>
	/// <returns>The command exit code.</returns>
	internal static CommandExitCode Run(Command command, TextWriter output)
	{
		string dataDirectory = GetDataDirectory();
		string configurationPath = Path.Combine(dataDirectory, "BackUp.json");
		string settingsPath = Path.Combine(dataDirectory, "Settings.json");

		configurationPath = GetFilePath(
			command, "c", "config", configurationPath);
		settingsPath = GetFilePath(command, "s", "settings", settingsPath);

		bool checkGit = command.DoesOptionExist("g", "check-git");
		bool json = command.DoesOptionExist("j", "json");

		ConfigurationReport report = ConfigurationInspector.Review(
			configurationPath, settingsPath, dataDirectory, checkGit);

		if (command.Name == "explain")
		{
			string path = command.Parameters[0];
			ConfigurationInspector.Explain(report, path);
		}

		ConfigurationReportWriter.Write(output, report, json);

		CommandExitCode exitCode = CommandExitCode.Success;

		if (report.HasErrors == true)
		{
			exitCode = CommandExitCode.ConfigurationError;
		}

		return exitCode;
	}

	/// <summary>
	/// Writes an argument error without starting backup services.
	/// </summary>
	/// <param name="output">The report output.</param>
	/// <param name="message">The parser or command validation error.</param>
	/// <param name="json">Whether to produce JSON.</param>
	/// <returns>The invalid arguments exit code.</returns>
	internal static CommandExitCode WriteArgumentError(
		TextWriter output, string message, bool json)
	{
		ConfigurationReport report = new();
		ConfigurationDiagnostic diagnostic = new();
		diagnostic.Severity = DiagnosticSeverity.Error;
		diagnostic.Code = "CLI001";
		diagnostic.Location = "Arguments";
		diagnostic.Message = message;

		report.Diagnostics.Add(diagnostic);
		ConfigurationReportWriter.Write(output, report, json);

		return CommandExitCode.InvalidArguments;
	}

	private static string GetDataDirectory()
	{
		string appData = Environment.GetFolderPath(
			Environment.SpecialFolder.ApplicationData);

		string directory = Path.Combine(
			appData, "DigitalZenWorks", "BackUpManager");
		return directory;
	}

	private static string GetFilePath(
		Command command, string shortName, string longName, string defaultPath)
	{
		string path = defaultPath;
		CommandOption option = command.GetOption(shortName, longName);

		if (option != null)
		{
			path = option.Parameter;
		}

		return path;
	}
}
