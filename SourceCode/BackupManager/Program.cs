/////////////////////////////////////////////////////////////////////////////
// <copyright file="Program.cs" company="James John McGuire">
// Copyright © 2017 - 2020 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

using BackupManagerLibrary;
using Common.Logging;
using Common.Logging.Configuration;
using Microsoft.Extensions.Configuration;
using System;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;

[assembly: log4net.Config.XmlConfigurator(Watch = true)]

namespace BackupManager
{
	public static class Program
	{
		//private static readonly ILog Log = LogManager.GetLogger(
		//	System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private static readonly ILog Log = LogManager.GetLogger(typeof(Program));

		public static void Main(string[] args)
		{
			Console.WriteLine("Backup Manager");

/*
			ExeConfigurationFileMap configFileMap = new ExeConfigurationFileMap();
			configFileMap.ExeConfigFilename = "App.Config";
			Configuration config = ConfigurationManager.OpenMappedExeConfiguration(configFileMap, ConfigurationUserLevel.None);

			ConfigurationSectionGroup sectionGroup = config.GetSectionGroup("common");
			// IConfigurationSection section = (IConfigurationSection)sectionGroup.Sections.Get("logging");

			LogConfiguration logConfiguration = new LogConfiguration();

			ConfigurationBuilder builder = new ConfigurationBuilder();
			IConfigurationRoot tester = builder.Build();
			//tester.GetSection("LogConfiguration").Bind(section);

			var common2 = ConfigurationManager.GetSection("common/logging");
			var settings = ConfigurationManager.GetSection("appSettings");
			var log4net = ConfigurationManager.GetSection("log4net");


			IConfigurationSection common = (IConfigurationSection)ConfigurationManager.GetSection("common");
			IConfiguration tester2 = (IConfiguration)ConfigurationManager.GetSection("common");

			IConfigurationRoot configuration2 = builder.Build();

			common.GetSection("common").Bind(logConfiguration);

			// Type type = typeof(Log4NetFactoryAdapter);
			var logConfiguration2 = new LogConfiguration();
			//configuration2.GetSection("LogConfiguration").Bind(section);
			//logConfiguration2.FactoryAdapter = section.
			LogManager.Configure(logConfiguration);
*/
			IConfiguration config = new ConfigurationBuilder()
					.AddJsonFile("AppSettings.json", optional: true, reloadOnChange: true)
					.Build();

			LogConfiguration logConfiguration = new LogConfiguration();
			config.GetSection("LogConfiguration").Bind(logConfiguration);
			LogManager.Configure(logConfiguration);

			// SetupLogging();

			Log.Info("Log testing.");
			Backup.Run();
		}

		private static void SetupLogging()
		{
			var props = new Common.Logging.Configuration.NameValueCollection
			{
				{ "configType", "FILE" },
				{ "configFile", "./log4net.config" }
			};

			LogManager.Adapter = new Common.Logging.Log4Net.Log4NetLoggerFactoryAdapter(props);
		}
	}
}
