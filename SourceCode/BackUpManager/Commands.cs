/////////////////////////////////////////////////////////////////////////////
// <copyright file="Commands.cs" company="James John McGuire">
// Copyright © 2017 - 2026 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace BackUpManager;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using DigitalZenWorks.CommandLine.Commands;
using DigitalZenWorks.Common.VersionUtilities;

/// <summary>
/// Commands class.
/// </summary>
internal static class Commands
{
	/// <summary>
	/// Gets the list of commands.
	/// </summary>
	/// <returns>The list of commands.</returns>
	public static IList<Command> GetCommands()
	{
		List<Command> commands = [];

		Command help = new("help");
		help.Description = "Show this information";
		commands.Add(help);

		List<CommandOption> options = [];

		CommandOption configFile = new("c", "config", true);
		options.Add(configFile);

		CommandOption ignoreAbandoned = new("i", "ignore-abandoned", false);
		options.Add(ignoreAbandoned);

		Command backup = new("backup", options, 0, "Back up files");
		commands.Add(backup);

		IList<CommandOption> configurationOptions = GetReviewOptions();
		Command configuration = new(
			"validate",
			configurationOptions,
			0,
			"Validate configuration locally");
		commands.Add(configuration);

		IList<CommandOption> explanationOptions = GetReviewOptions();
		Command explain = new(
			"explain", explanationOptions, 1, "Explain local backup selection");
		explain.Parameters.Add("path");
		commands.Add(explain);

		return commands;
	}

	/// <summary>
	/// Show help.
	/// </summary>
	/// <param name="additionalMessage">An additional message,
	/// if any.</param>
	/// <param name="output">The output writer; defaults to the console.</param>
	public static void ShowHelp(
		string additionalMessage = null, TextWriter output = null)
	{
		if (output == null)
		{
			output = Console.Out;
		}

		FileVersionInfo fileVersionInfo =
			VersionSupport.GetAssemblyInformation();

		if (fileVersionInfo != null)
		{
			string assemblyVersion = fileVersionInfo.FileVersion;
			string companyName = fileVersionInfo.CompanyName;
			string copyright = fileVersionInfo.LegalCopyright;
			string name = fileVersionInfo.FileName;

			string header = string.Format(
				CultureInfo.CurrentCulture,
				"{0} {1} {2} {3}",
				name,
				assemblyVersion,
				copyright,
				companyName);
			output.WriteLine(header);
		}

		if (!string.IsNullOrWhiteSpace(additionalMessage))
		{
			output.WriteLine(additionalMessage);
		}

		IList<Command> commands = GetCommands();
		CommandsSet commandsSet = new(commands);
		string help = commandsSet.GetHelp();

		output.WriteLine();
		output.WriteLine(help);
	}

	/// <summary>
	/// Update arguments.
	/// </summary>
	/// <param name="commands">The command.</param>
	/// <param name="arguments">The arguments.</param>
	/// <returns>The updated command arguments.</returns>
	public static string[] UpdateArguments(
		IList<Command> commands, string[] arguments)
	{
		if (arguments == null || arguments.Length == 0)
		{
			arguments = new string[1];
			arguments[0] = "backup";
		}
		else
		{
			string requestedCommand = arguments[0];
			bool exists = false;

			foreach (Command command in commands)
			{
				if (command.Name == requestedCommand)
				{
					exists = true;
					break;
				}
			}

			bool startsWithOption = requestedCommand.StartsWith('-');

			if (exists == false && startsWithOption == true)
			{
				int length = arguments.Length + 1;
				string[] newArguments = new string[length];
				newArguments[0] = "backup";

				for (int index = 0; index < arguments.Length; index++)
				{
					newArguments[index + 1] = arguments[index];
				}

				arguments = newArguments;
			}
		}

		return arguments;
	}

	/// <summary>
	/// Parses every command using the shared command-line package.
	/// </summary>
	/// <param name="arguments">The supplied arguments.</param>
	/// <returns>The package's parsed command and validation result.</returns>
	internal static CommandLineInstance Parse(string[] arguments)
	{
		IList<Command> commands = GetCommands();
		arguments = UpdateArguments(commands, arguments);

		CommandLineInstance commandLine = new(commands, arguments);

		return commandLine;
	}

	/// <summary>
	/// Checks the requested output format, even after parse errors.
	/// </summary>
	/// <param name="arguments">The supplied arguments.</param>
	/// <returns>Whether JSON output was requested.</returns>
	internal static bool ProcessJson(string[] arguments)
	{
		bool processJson = false;

		// The parser may stop before reaching the output-format option.
		int longPosition = Array.IndexOf(arguments, "--json");
		int shortPosition = Array.IndexOf(arguments, "-j");

		if (longPosition >= 0 || shortPosition >= 0)
		{
			processJson = true;
		}

		return processJson;
	}

	/// <summary>Validates a command and returns its first error.</summary>
	/// <param name="commandLine">The package's parsing result.</param>
	/// <param name="error">The error text, or empty when valid.</param>
	/// <returns>Whether the command is valid.</returns>
	internal static bool ValidateCommand(
		CommandLineInstance commandLine, out string error)
	{
		error = commandLine.ErrorMessage;

		if (commandLine.ValidArguments == true)
		{
			Command command = commandLine.Command;
			error = ValidateParameters(command);

			if (string.IsNullOrEmpty(error))
			{
				error = ValidateOptions(command);
			}
		}

		bool isValid = commandLine.ValidArguments &&
			string.IsNullOrEmpty(error);

		return isValid;
	}

	private static IList<CommandOption> GetReviewOptions()
	{
		List<CommandOption> options = [];

		CommandOption configuration = new("c", "config", true);
		options.Add(configuration);

		CommandOption settings = new("s", "settings", true);
		options.Add(settings);

		CommandOption json = new("j", "json", false);
		options.Add(json);

		CommandOption checkGit = new("g", "check-git", false);
		options.Add(checkGit);

		return options;
	}

	private static string ValidateOptions(Command command)
	{
		// The package checks that a value exists, but permits blank text.
		string error = string.Empty;

		foreach (CommandOption option in command.Options)
		{
			if (option.RequiresParameter == true &&
				string.IsNullOrWhiteSpace(option.Parameter))
			{
				error = "Expected a nonempty value for --" + option.LongName;
				break;
			}
		}

		return error;
	}

	private static string ValidateParameters(Command command)
	{
		// The package enforces minimum counts, not exact counts.
		string error = string.Empty;

		if (command.Name == "explain")
		{
			if (command.Parameters.Count != 1 ||
				string.IsNullOrWhiteSpace(command.Parameters[0]))
			{
				error = "Expected explain <path>.";
			}
		}
		else if (command.Parameters.Count != 0)
		{
			error = command.Name + " does not accept positional arguments.";
		}

		return error;
	}
}
