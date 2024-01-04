/////////////////////////////////////////////////////////////////////////////
// <copyright file="Program.cs" company="James John McGuire">
// Copyright © 2017 - 2024 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

using DigitalZenWorks.BackUp.Library;
using DigitalZenWorks.CommandLine.Commands;
using DigitalZenWorks.Common.VersionUtilities;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Configuration;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Threading.Tasks;

[assembly: CLSCompliant(true)]

namespace BackUpManager
{
	/// <summary>
	/// Back up manager program class.
	/// </summary>
	public static class Program
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

				arguments = UpdateArguments(arguments);

				CommandLineArguments commandLine = new (commands, arguments);

				if (commandLine.ValidArguments == false)
				{
					Log.Error(commandLine.ErrorMessage);
				}
				else
				{
					Command command = commandLine.Command;

					switch (command.Name)
					{
						case "backup":
							BackUpService backUpService =
								serviceProvider.GetService<
									DigitalZenWorks.BackUp.Library.BackUpService>();

							string configurationFile = GetConfigurationFile();

							bool ignoreAbandoned = command.DoesOptionExist(
								"i", "ignore-abandoned");
							backUpService.IgnoreAbandoned = ignoreAbandoned;

							await backUpService.BackUp(configurationFile).
								ConfigureAwait(false);
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

		private static string GetConfigurationFile()
		{
			string configurationFile = null;
			string baseDataDirectory = Environment.GetFolderPath(
				Environment.SpecialFolder.ApplicationData,
				Environment.SpecialFolderOption.Create);
			string accountsPath =
				baseDataDirectory + @"\DigitalZenWorks\BackUpManager";

			if (System.IO.Directory.Exists(accountsPath))
			{
				string accountsFile = accountsPath + @"\BackUp.json";

				if (System.IO.File.Exists(accountsFile))
				{
					configurationFile = accountsFile;
				}
			}

			return configurationFile;
		}

		private static void LogInitialization()
		{
			string applicationDataDirectory = @"DigitalZenWorks\BackUpManager";
			string baseDataDirectory = Environment.GetFolderPath(
				Environment.SpecialFolder.ApplicationData,
				Environment.SpecialFolderOption.Create) + @"\" +
				applicationDataDirectory;

			string logFilePath = baseDataDirectory + "\\BackUp.log";
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

		private static void ShowHelp(string additionalMessage)
		{
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
				Log.Logger.Information(header);
			}

			if (!string.IsNullOrWhiteSpace(additionalMessage))
			{
				Log.Logger.Information(additionalMessage);
			}
		}

		private static string[] UpdateArguments(string[] arguments)
		{
			if (arguments == null || arguments.Length == 0)
			{
				arguments = new string[1];
				arguments[0] = "backup";
			}
			else if (!arguments[0].Equals(
				"backup", StringComparison.Ordinal))
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

			return arguments;
		}
	}
}
