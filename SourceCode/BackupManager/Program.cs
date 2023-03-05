/////////////////////////////////////////////////////////////////////////////
// <copyright file="Program.cs" company="James John McGuire">
// Copyright © 2017 - 2023 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

using Common.Logging;
using DigitalZenWorks.BackUp.Library;
using Serilog;
using Serilog.Configuration;
using Serilog.Events;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Threading.Tasks;

[assembly: CLSCompliant(true)]

namespace BackupManager
{
	/// <summary>
	/// Back up manager program class.
	/// </summary>
	public static class Program
	{
		private static readonly ILog Log = LogManager.GetLogger(
			System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// The program's main entry point.
		/// </summary>
		/// <returns>A task indicating completion.</returns>
		public static async Task Main()
		{
			try
			{
				LogInitialization();
				string version = GetVersion();

				Log.Info("Starting Backup Manager Version: " + version);

				string configurationFile = GetConfigurationFile();
				await BackupService.Run(configurationFile).
					ConfigureAwait(false);
			}
			catch (Exception exception)
			{
				Log.Error(exception.ToString());

				throw;
			}
		}

		private static FileVersionInfo GetAssemblyInformation()
		{
			FileVersionInfo fileVersionInfo = null;

			// Bacause single file apps have no assemblies, get the information
			// from the process.
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

			string logFilePath = baseDataDirectory + "\\Backup.log";
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
			Serilog.Log.Logger = configuration.CreateLogger();

			LogManager.Adapter =
				new Common.Logging.Serilog.SerilogFactoryAdapter();
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
				Log.Info(header);
			}

			if (!string.IsNullOrWhiteSpace(additionalMessage))
			{
				Log.Info(additionalMessage);
			}
		}
	}
}
