/////////////////////////////////////////////////////////////////////////////
// <copyright file="Program.cs" company="James John McGuire">
// Copyright © 2017 - 2021 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

using BackupManagerLibrary;
using Common.Logging;
using Serilog;
using Serilog.Configuration;
using Serilog.Events;
using System;
using System.Threading.Tasks;

[assembly: CLSCompliant(true)]

namespace BackupManager
{
	public static class Program
	{
		private static readonly ILog Log = LogManager.GetLogger(
			System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public static async Task Main(string[] args)
		{
			bool useCustomInitialization = true;
			LogInitialization();

			Log.Info("Starting Backup Manager");

			if ((args != null) && (args.Length > 0) &&
				args[0].Equals(
					"false", System.StringComparison.OrdinalIgnoreCase))
			{
				useCustomInitialization = false;
			}

			await Backup.Run(useCustomInitialization).ConfigureAwait(false);
		}

		private static void LogInitialization()
		{
			string logFilePath = "Backup.log";
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
	}
}
