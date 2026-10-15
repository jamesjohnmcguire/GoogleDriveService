/////////////////////////////////////////////////////////////////////////////
// <copyright file="Program.cs" company="James John McGuire">
// Copyright © 2017 - 2026 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

[assembly: System.CLSCompliant(true)]

namespace BackUpManager;

using System;
using System.IO;
using System.Threading.Tasks;
using DigitalZenWorks.BackUp.Library;
using DigitalZenWorks.CommandLine.Commands;
using DigitalZenWorks.Common.VersionUtilities;
using LoggingService;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

/// <summary>
/// Back up manager program class.
/// </summary>
internal static class Program
{
	/// <summary>
	/// The program's main entry point.
	/// </summary>
	/// <param name="arguments">An array of arguments passed to
	/// the program.</param>
	/// <returns>A task indicating completion.</returns>
	public static async Task Main(string[] arguments)
	{
		CommandLineInstance commandLine = Commands.Parse(arguments);
		bool isValid = Commands.ValidateCommand(commandLine, out string error);

		if (isValid == false)
		{
			ProcessError(arguments, error);
		}
		else
		{
			await RunCommand(commandLine).ConfigureAwait(false);
		}
	}

	private static ServiceProvider ConfigureServices()
	{
		string applicationDataDirectory =
			Configuration.GetDefaultDataLocation();

		string logFilePath =
			Path.Combine(applicationDataDirectory, "BackUp.log");

		LogService.Configure(
			logFilePath, LogFileMode.FixedNameWithDailyArchives);

		ServiceCollection serviceCollection = new();

		serviceCollection.AddLogging(builder =>
		{
			builder.ClearProviders();
			builder.AddSerilog(Log.Logger);
		});

		serviceCollection.AddTransient<BackUpService>();

		ServiceProvider serviceProvider =
			serviceCollection.BuildServiceProvider();

		return serviceProvider;
	}

	private static void ConfigureServiceSettings(
		BackUpService backUpService, Command command)
	{
		SettingsManager settingsManager = new();
		Settings settings = settingsManager.Load();
		backUpService.Settings = settings;

		bool ignoreAbandoned = command.DoesOptionExist(
			"i", "ignore-abandoned");
		backUpService.IgnoreAbandoned = ignoreAbandoned;
	}

	private static void ProcessError(
		string[] arguments, string error)
	{
		bool json = Commands.ProcessJson(arguments);

		CommandExitCode exitCode = ConfigurationCommands.WriteArgumentError(
			Console.Out, error, json);
		Environment.ExitCode = (int)exitCode;

		if (json == false)
		{
			Commands.ShowHelp();
		}
	}

	private static async Task ProcessBackup(Command command)
	{
		try
		{
			using ServiceProvider serviceProvider = ConfigureServices();

			Microsoft.Extensions.Logging.ILogger logger =
				serviceProvider.GetRequiredService<ILogger<BackUpService>>();

			string version = VersionSupport.GetVersion();

			logger.Info($"Starting Back Up Manager Version: {version}");
			logger.Info($"Command is: {command.Name}");

			BackUpService backUpService =
				serviceProvider.GetService<BackUpService>();
			ConfigureServiceSettings(backUpService, command);

			string configurationFile =
				Configuration.GetConfigurationFile(command);
			await backUpService.BackUp(configurationFile).ConfigureAwait(false);
			Environment.ExitCode = (int)CommandExitCode.Success;
		}
		catch (Exception exception)
		{
			Log.Error(exception.ToString());

			throw;
		}
		finally
		{
			await Log.CloseAndFlushAsync().ConfigureAwait(false);
		}
	}

	private static async Task RunCommand(CommandLineInstance commandLine)
	{
		Command command = commandLine.Command;

		switch (command.Name)
		{
			case "backup":
				await ProcessBackup(command).ConfigureAwait(false);
				break;
			case "validate":
			case "explain":
				CommandExitCode exitCode = ConfigurationCommands.Run(
					command, Console.Out);
				Environment.ExitCode = (int)exitCode;
				break;
			case "help":
				Commands.ShowHelp();
				Environment.ExitCode = (int)CommandExitCode.Success;
				break;
			default:
				break;
		}
	}
}
