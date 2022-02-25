/////////////////////////////////////////////////////////////////////////////
// <copyright file="Program.cs" company="James John McGuire">
// Copyright © 2017 - 2022 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

using BackupManagerLibrary;
using Common.Logging;
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

				await Backup.Run().ConfigureAwait(false);
			}
			catch (Exception exception)
			{
				Log.Error(exception.ToString());

				throw;
			}
		}

		private static string GetVersion()
		{
			Assembly assembly = Assembly.GetExecutingAssembly();

			AssemblyName assemblyName = assembly.GetName();
			Version version = assemblyName.Version;
			string assemblyVersion = version.ToString();

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
			sinkConfiguration.Console(LogEventLevel.Verbose, outputTemplate);
			sinkConfiguration.File(
				logFilePath, LogEventLevel.Verbose, outputTemplate);
			Serilog.Log.Logger = configuration.CreateLogger();

			LogManager.Adapter =
				new Common.Logging.Serilog.SerilogFactoryAdapter();
		}

		private static void ShowHelp(string additionalMessage)
		{
			Assembly assembly = Assembly.GetExecutingAssembly();
			string location = assembly.Location;

			if (string.IsNullOrWhiteSpace(location))
			{
				// Single file apps have no assemblies.
				Process process = Process.GetCurrentProcess();
				location = process.MainModule.FileName;
			}

			if (!string.IsNullOrWhiteSpace(location))
			{
				FileVersionInfo versionInfo =
				FileVersionInfo.GetVersionInfo(location);

				string companyName = versionInfo.CompanyName;
				string copyright = versionInfo.LegalCopyright;

				AssemblyName assemblyName = assembly.GetName();
				string name = assemblyName.Name;
				Version version = assemblyName.Version;
				string assemblyVersion = version.ToString();

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
