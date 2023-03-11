/////////////////////////////////////////////////////////////////////////////
// <copyright file="Program.cs" company="James John McGuire">
// Copyright © 2017 - 2023 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

using DigitalZenWorks.BackUp.Library;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Configuration;
using Serilog.Events;
using System;
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
		/// <returns>A task indicating completion.</returns>
		public static async Task Main()
		{
			try
			{
				ServiceProvider serviceProvider = ConfigureServices();

				string version = GetVersion();

				Log.Logger.Information(
					"Starting Back Up Manager Version: " + version);

				BackUpService backUpService = serviceProvider.GetService<
					DigitalZenWorks.BackUp.Library.BackUpService>();

				string configurationFile = GetConfigurationFile();

				await backUpService.Run(configurationFile).
					ConfigureAwait(false);
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

		private static FileVersionInfo GetAssemblyInformation()
		{
			FileVersionInfo fileVersionInfo = null;

			// Bacause single file apps have no assemblies,
			// get the information from the process.
			Process process = Process.GetCurrentProcess();

			string location = process.MainModule.FileName;

			if (!string.IsNullOrWhiteSpace(location))
			{
				fileVersionInfo = FileVersionInfo.GetVersionInfo(location);
			}

			return fileVersionInfo;
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

		private static string GetVersion()
		{
			string assemblyVersion = string.Empty;

			FileVersionInfo fileVersionInfo = GetAssemblyInformation();

			if (fileVersionInfo != null)
			{
				assemblyVersion = fileVersionInfo.FileVersion;
			}

			return assemblyVersion;
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
			FileVersionInfo fileVersionInfo = GetAssemblyInformation();

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
	}
}
