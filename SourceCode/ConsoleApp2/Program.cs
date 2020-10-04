using BackupManagerLibrary;
using Common.Logging;
using Common.Logging.Configuration;

using Serilog;
using Serilog.Configuration;
using Serilog.Events;
using System;
using Common.Logging.Serilog;

namespace ConsoleApp2
{
	class Program
	{
		private static readonly ILog Logger = LogManager.GetLogger(
			System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		static void Main(string[] args)
		{
			Console.WriteLine("Hello World!");

			string logFilePath = "Tester.log";
			string outputTemplate = "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}";

			LoggerConfiguration configuration = new LoggerConfiguration();
			LoggerSinkConfiguration sinkConfiguration = configuration.WriteTo;
			sinkConfiguration.Console(LogEventLevel.Verbose, outputTemplate);
			sinkConfiguration.File(logFilePath, LogEventLevel.Verbose, outputTemplate);
			Log.Logger = configuration.CreateLogger();

			LogManager.Adapter = new Common.Logging.Serilog.SerilogFactoryAdapter();


			Logger.Error("Goodbye, Serilog.");

			Backup.Run();
		}
	}
}
