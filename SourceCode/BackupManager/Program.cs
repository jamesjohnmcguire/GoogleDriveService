/////////////////////////////////////////////////////////////////////////////
// <copyright file="Program.cs" company="James John McGuire">
// Copyright © 2017 - 2020 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

using BackupManagerLibrary;
using Common.Logging;
using Serilog;
using Serilog.Configuration;
using Serilog.Events;
using System.Threading.Tasks;

namespace BackupManager
{
	public static class Program
	{
		private const string LogFilePath = "Backup.log";
		private const string OutputTemplate =
			"[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}";

		private static readonly ILog Log = LogManager.GetLogger(
			System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public static async Task Main(string[] args)
		{
			StartUp();

			Log.Info("Starting Backup Manager");

			await Backup.Run().ConfigureAwait(false);
		}

		private static void StartUp()
		{
			LoggerConfiguration configuration = new LoggerConfiguration();
			LoggerSinkConfiguration sinkConfiguration = configuration.WriteTo;
			sinkConfiguration.Console(LogEventLevel.Verbose, OutputTemplate);
			sinkConfiguration.File(
				LogFilePath, LogEventLevel.Verbose, OutputTemplate);
			Serilog.Log.Logger = configuration.CreateLogger();

			LogManager.Adapter = new Common.Logging.Serilog.SerilogFactoryAdapter();
		}
	}
}
