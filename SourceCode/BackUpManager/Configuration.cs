/////////////////////////////////////////////////////////////////////////////
// <copyright file="Configuration.cs" company="James John McGuire">
// Copyright © 2017 - 2025 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace BackUpManager
{
	using System;
	using System.IO;
	using DigitalZenWorks.CommandLine.Commands;

	internal static class Configuration
	{
		public static string GetConfigurationFile(Command command)
		{
			string location;

			CommandOption optionFound = command.GetOption("c", "config");

			if (optionFound != null)
			{
				location = optionFound.Parameter;
			}
			else
			{
				location = GetDefaultConfigurationFile();
			}

			return location;
		}

		public static string GetDefaultConfigurationFile()
		{
			string configurationFile = null;

			string accountsPath = GetDefaultDataLocation();

			// Will use existing directory or create it.
			Directory.CreateDirectory(accountsPath);

			string accountsFile = accountsPath + @"\BackUp.json";

			if (File.Exists(accountsFile))
			{
				configurationFile = accountsFile;
			}

			return configurationFile;
		}

		public static string GetDefaultDataLocation()
		{
			string defaultDataLocation;

			string baseDataDirectory = Environment.GetFolderPath(
				Environment.SpecialFolder.ApplicationData,
				Environment.SpecialFolderOption.Create);

			string applicationDataDirectory = @"DigitalZenWorks\BackUpManager";

			defaultDataLocation =
				Path.Combine(baseDataDirectory, applicationDataDirectory);

			return defaultDataLocation;
		}
	}
}
