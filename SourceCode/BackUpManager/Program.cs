/////////////////////////////////////////////////////////////////////////////
// <copyright file="Program.cs" company="James John McGuire">
// Copyright © 2017 - 2025 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

[assembly: System.CLSCompliant(true)]

namespace BackUpManager
{
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.IO;
	using System.Threading.Tasks;
	using DigitalZenWorks.BackUp.Library;
	using DigitalZenWorks.CommandLine.Commands;
	using DigitalZenWorks.Common.VersionUtilities;
	using Microsoft.Extensions.DependencyInjection;
	using Serilog;
	using Serilog.Configuration;
	using Serilog.Events;

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

				string version = VersionSupport.GetVersion();

				Log.Logger.Information(
					"Starting Back Up Manager Version: " + version);

				IList<Command> commands = Commands.GetCommands();

				arguments = Commands.UpdateArguments(commands, arguments);

				CommandLineInstance commandLine = new (commands, arguments);

				if (commandLine.ValidArguments == false)
				{
					Log.Error(commandLine.ErrorMessage);
					commandLine.ShowHelp();
				}
				else
				{
					Command command = commandLine.Command;

					switch (command.Name)
					{
						case "backup":
							BackUpService backUpService =
								serviceProvider.GetService<BackUpService>();

							string configurationFile =
								Configuration.GetConfigurationFile(command);

							bool ignoreAbandoned = command.DoesOptionExist(
								"i", "ignore-abandoned");
							backUpService.IgnoreAbandoned = ignoreAbandoned;

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
			ServiceCollection serviceCollection = new ();

			serviceCollection.AddLogging(config => config.AddSerilog())
				.AddTransient<BackUpService>();

			ServiceProvider serviceProvider =
				serviceCollection.BuildServiceProvider();

			LogInitialization();

			return serviceProvider;
		}

		private static void LogInitialization()
		{
			string applicationDataDirectory =
				Configuration.GetDefaultDataLocation();
			string logFilePath =
				Path.Combine(applicationDataDirectory, "BackUp.log");

			string outputTemplate =
				"[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] " +
				"{Message:lj}{NewLine}{Exception}";

			LoggerConfiguration configuration = new ();
			LoggerSinkConfiguration sinkConfiguration = configuration.WriteTo;
			sinkConfiguration.Console(
				LogEventLevel.Verbose,
				outputTemplate,
				CultureInfo.CurrentCulture);
			sinkConfiguration.File(
				logFilePath,
				LogEventLevel.Verbose,
				outputTemplate,
				CultureInfo.CurrentCulture);
			Log.Logger = configuration.CreateLogger();
		}
	}
}
