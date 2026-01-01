/////////////////////////////////////////////////////////////////////////////
// <copyright file="Configuration.cs" company="James John McGuire">
// Copyright © 2017 - 2026 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace BackUpManager
{
	using System;
	using System.IO;
	using DigitalZenWorks.CommandLine.Commands;

	/// <summary>
	/// The configuration class.
	/// </summary>
	internal static class Configuration
	{
		/// <summary>
		/// Retrieves the configuration file path based on the specified
		/// command.
		/// </summary>
		/// <param name="command">The command containing options that may
		/// specify a configuration file path.</param>
		/// <returns>The path to the configuration file. If the command includes a
		/// "config" option, its parameter is returned;
		/// otherwise, the default configuration file path is returned.</returns>
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

		/// <summary>
		/// Retrieves the default configuration file path for backup data.
		/// </summary>
		/// <remarks>This method ensures that the default data directory exists by
		/// creating it if necessary.</remarks>
		/// <returns>The file path of the default configuration file if it exists;
		/// otherwise, <see langword="null"/>.</returns>
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

		/// <summary>
		/// Gets the default data location for the application.
		/// </summary>
		/// <remarks>The method constructs the path by combining the user's
		/// application data folder with a subdirectory specific to the
		/// application.</remarks>
		/// <returns>A string representing the path to the default data
		/// directory for the application, located within the user's
		/// application data folder.</returns>
		public static string GetDefaultDataLocation()
		{
			string baseDataDirectory = Environment.GetFolderPath(
				Environment.SpecialFolder.ApplicationData,
				Environment.SpecialFolderOption.Create);

			const string applicationDataDirectory =
				@"DigitalZenWorks\BackUpManager";

			string defaultDataLocation =
				Path.Combine(baseDataDirectory, applicationDataDirectory);

			return defaultDataLocation;
		}
	}
}
