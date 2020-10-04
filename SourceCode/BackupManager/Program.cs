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

namespace BackupManager
{
	public static class Program
	{
		private static readonly ILog Log = LogManager.GetLogger(
			System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public static void Main(string[] args)
		{
			StartUp();

			Log.Info("Starting Backup Manager");

			Backup.Run();
		}

		private static void StartUp()
		{
			string logFilePath = "Tester.log";
			string outputTemplate = "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}";

			LoggerConfiguration configuration = new LoggerConfiguration();
			LoggerSinkConfiguration sinkConfiguration = configuration.WriteTo;
			sinkConfiguration.Console(LogEventLevel.Verbose, outputTemplate);
			sinkConfiguration.File(logFilePath, LogEventLevel.Verbose, outputTemplate);
			Serilog.Log.Logger = configuration.CreateLogger();

			LogManager.Adapter = new Common.Logging.Serilog.SerilogFactoryAdapter();
		}
	}
}
