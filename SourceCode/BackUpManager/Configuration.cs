/////////////////////////////////////////////////////////////////////////////
// <copyright file="Configuration.cs" company="James John McGuire">
// Copyright © 2017 - 2026 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace BackUpManager;

using System.IO;
using DigitalZenWorks.BackUp.Library;
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
		string accountsPath = GetDefaultDataLocation();
		string configurationFile = FindConfigurationFile(accountsPath);

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
		string defaultDataLocation = ApplicationPaths.GetDataDirectory();

		return defaultDataLocation;
	}

	/// <summary>Finds the configuration in the supplied data directory.</summary>
	/// <param name="accountsPath">The application data directory.</param>
	/// <returns>The existing configuration path, or null.</returns>
	internal static string FindConfigurationFile(string accountsPath)
	{
		string configurationFile = null;

		// Will use existing directory or create it.
		Directory.CreateDirectory(accountsPath);

		string accountsFile = Path.Combine(accountsPath, "BackUp.json");
		bool exists = File.Exists(accountsFile);

		if (exists == true)
		{
			configurationFile = accountsFile;
		}

		return configurationFile;
	}
}
