/////////////////////////////////////////////////////////////////////////////
// <copyright file="Program.cs" company="James John McGuire">
// Copyright © 2017 - 2026 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

[assembly: System.CLSCompliant(true)]

namespace BackUpManager;

using System;
using System.Collections.Generic;
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
		try
		{
			ServiceProvider serviceProvider = ConfigureServices();

			Microsoft.Extensions.Logging.ILogger logger =
				serviceProvider.GetRequiredService<ILogger<BackUpService>>();

			string version = VersionSupport.GetVersion();

			logger.Info($"Starting Back Up Manager Version: {version}");

			IList<Command> commands = Commands.GetCommands();

			arguments = Commands.UpdateArguments(commands, arguments);

			CommandLineInstance commandLine = new(commands, arguments);

			if (commandLine.ValidArguments == false)
			{
				logger.Error(commandLine.ErrorMessage);
				commandLine.ShowHelp();
			}
			else
			{
				Command command = commandLine.Command;
				logger.Info($"Command is: {command.Name}");

				switch (command.Name)
				{
					case "backup":
						BackUpService backUpService =
							serviceProvider.GetService<BackUpService>();

						ConfigureServiceSettings(backUpService, command);

						string configurationFile =
							Configuration.GetConfigurationFile(command);

						await backUpService.BackUp(configurationFile).
							ConfigureAwait(false);
						break;
					case "help":
						Commands.ShowHelp();
						commandLine.ShowHelp();
						break;
				}
			}
		}
		catch (Exception exception)
		{
			Log.Error(exception.ToString());

			throw;
		}
	}

	private static ServiceProvider ConfigureServices()
	{
		string applicationDataDirectory =
			Configuration.GetDefaultDataLocation();

		string logFilePath =
			Path.Combine(applicationDataDirectory, "BackUp.log");

		LogService.Configure(logFilePath);

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
}
