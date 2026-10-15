/////////////////////////////////////////////////////////////////////////////
// <copyright file="ConfigurationCommandTests.cs" company="James John McGuire">
// Copyright © 2017 - 2026 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.BackUp.Library.Tests;

using System;
using System.Collections.Generic;
using System.IO;
using BackUpManager;
using DigitalZenWorks.CommandLine.Commands;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

/// <summary>Verifies review routing and machine-readable failures.</summary>
[TestFixture]
internal sealed class ConfigurationCommandTests
{
	/// <summary>
	/// Preserves explicit and implicit backup commands in the shared parser.
	/// </summary>
	/// <param name="text">The command text.</param>
	[TestCase("")]
	[TestCase("backup")]
	[TestCase("--ignore-abandoned")]
	public void BackupArgumentsUseSharedParser(string text)
	{
		string[] arguments = text.Split(
			' ', StringSplitOptions.RemoveEmptyEntries);

		CommandLineInstance commandLine = Commands.Parse(arguments);

		Assert.That(commandLine.ValidArguments, Is.True);
		Assert.That(commandLine.Command.Name, Is.EqualTo("backup"));
	}

	/// <summary>Accepts supported commands through full validation.</summary>
	/// <param name="text">The command text.</param>
	[TestCase("")]
	[TestCase("backup")]
	[TestCase("--ignore-abandoned")]
	[TestCase("help")]
	[TestCase("validate")]
	[TestCase("explain path")]
	public void SupportedCommandsPassValidation(string text)
	{
		string[] arguments = text.Split(
			' ', StringSplitOptions.RemoveEmptyEntries);
		CommandLineInstance commandLine = Commands.Parse(arguments);

		bool isValid = Commands.ValidateCommand(commandLine, out string error);

		Assert.That(isValid, Is.True);
		Assert.That(error, Is.Empty);
	}

	/// <summary>Rejects incomplete or unknown review options locally.</summary>
	/// <param name="text">The command text.</param>
	[TestCase("validate extra --json")]
	[TestCase("explain --json")]
	[TestCase("validate --typo --json")]
	[TestCase("validate --config --json")]
	[TestCase("config remove --json")]
	[TestCase("backup unexpected")]
	[TestCase("help unexpected")]
	[TestCase("validate unexpected --json")]
	[TestCase("explain first second --json")]
	[TestCase("explain path --settings --json")]
	public void InvalidReviewArgumentsProduceJson(string text)
	{
		string[] arguments = text.Split(
			' ', StringSplitOptions.RemoveEmptyEntries);

		using StringWriter output = new();

		CommandLineInstance commandLine = Commands.Parse(arguments);
		bool isValid = Commands.ValidateCommand(commandLine, out string error);
		CommandExitCode exitCode = ConfigurationCommands.WriteArgumentError(
			output, error, true);

		JObject report = JObject.Parse(output.ToString());
		Assert.That(isValid, Is.False);
		Assert.That(error, Is.Not.Empty);
		Assert.That(exitCode, Is.EqualTo(CommandExitCode.InvalidArguments));
		Assert.That(
			report["Diagnostics"]![0]!["Code"]!.Value<string>(),
			Is.EqualTo("CLI001"));
	}

	/// <summary>
	/// Reports inaccessible files as JSON with a failing exit code.
	/// </summary>
	[Test]
	public void MissingConfigurationReturnsFailureWithoutServiceStartup()
	{
		string missing = Path.Combine(
			Path.GetTempPath(), "absent-backup-review", "BackUp.json");
		string[] arguments =
			["validate", "--config", missing,
				"--settings", missing, "--json"];
		using StringWriter output = new();

		CommandLineInstance commandLine = Commands.Parse(arguments);
		bool isValid = Commands.ValidateCommand(commandLine, out string error);
		Assert.That(isValid, Is.True);
		Assert.That(error, Is.Empty);

		CommandExitCode exitCode = ConfigurationCommands.Run(
			commandLine.Command, output);

		JObject report = JObject.Parse(output.ToString());
		Assert.That(exitCode, Is.EqualTo(CommandExitCode.ConfigurationError));
		Assert.That(report["HasErrors"]!.Value<bool>(), Is.True);
		Assert.That(
			report["Diagnostics"]![0]!["Severity"]!.Value<string>(),
			Is.EqualTo("Error"));
	}

	/// <summary>Rejects values beyond the package's validation rules.</summary>
	/// <param name="name">The command name.</param>
	/// <param name="first">The first argument.</param>
	/// <param name="second">The second argument.</param>
	[TestCase("validate", "extra", "argument")]
	[TestCase("explain", "first", "second")]
	[TestCase("validate", "--config", " ")]
	[TestCase("explain", " ", "--json")]
	public void ApplicationRejectsArgumentsAcceptedByPackage(
		string name, string first, string second)
	{
		CommandLineInstance commandLine = Commands.Parse(
			[name, first, second]);

		Assert.That(commandLine.ValidArguments, Is.True);

		bool isValid = Commands.ValidateCommand(commandLine, out string error);

		Assert.That(isValid, Is.False);
		Assert.That(error, Is.Not.Empty);
	}

	/// <summary>Registers review commands with the existing package.</summary>
	[Test]
	public void PackageParsesReviewOptionsAndQuotedPaths()
	{
		IList<Command> definitions = Commands.GetCommands();
		string[] arguments =
			["explain", "directory with spaces", "--config", "backup file.json",
				"--settings", "settings file.json", "--json", "--check-git"];
		CommandLineInstance commandLine = new(definitions, arguments);

		Assert.That(commandLine.ValidArguments, Is.True);
		Assert.That(commandLine.Command.Name, Is.EqualTo("explain"));
		Assert.That(
			commandLine.Command.Parameters[0], Is.EqualTo("directory with spaces"));

		CommandOption configuration =
			commandLine.Command.GetOption("c", "config");
		Assert.That(configuration.Parameter, Is.EqualTo("backup file.json"));
		Assert.That(
			commandLine.Command.DoesOptionExist("j", "json"), Is.True);
		Assert.That(
			commandLine.Command.DoesOptionExist("g", "check-git"), Is.True);
	}

	/// <summary>Accepts validation options without a subcommand.</summary>
	[Test]
	public void ValidateUsesTopLevelCommand()
	{
		string[] arguments = ["validate", "--json", "-c", "file.json"];
		CommandLineInstance commandLine = Commands.Parse(arguments);

		Assert.That(commandLine.ValidArguments, Is.True);
		bool isValid = Commands.ValidateCommand(commandLine, out string error);

		Assert.That(isValid, Is.True);
		Assert.That(error, Is.Empty);
		Assert.That(commandLine.Command.Name, Is.EqualTo("validate"));
		Assert.That(commandLine.Command.Parameters, Is.Empty);
	}

	/// <summary>
	/// Does not turn an unknown command into a backup request.
	/// </summary>
	[Test]
	public void UnknownCommandIsRejected()
	{
		CommandLineInstance commandLine = Commands.Parse(["exlpain", "path"]);

		Assert.That(commandLine.ValidArguments, Is.False);
	}

	/// <summary>
	/// Builds help from the registered commands after the version.
	/// </summary>
	[Test]
	public void HelpIncludesAllCommandsAfterVersionHeader()
	{
		using StringWriter output = new();
		Commands.ShowHelp(output: output);
		string help = output.ToString();

		Assert.That(help, Does.Contain("backup"));
		Assert.That(help, Does.Contain("validate"));
		Assert.That(help, Does.Contain("explain"));
		Assert.That(help, Does.Contain("check-git"));

		int headerEnd = help.IndexOf(
			Environment.NewLine, StringComparison.Ordinal);
		int commandPosition = help.IndexOf("explain", StringComparison.Ordinal);
		Assert.That(headerEnd, Is.GreaterThan(0));
		Assert.That(commandPosition, Is.GreaterThan(headerEnd));
	}
}
