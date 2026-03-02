/////////////////////////////////////////////////////////////////////////////
// <copyright file="SettingsManager.cs" company="James John McGuire">
// Copyright © 2017 - 2026 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.BackUp.Library;

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

/// <summary>
/// Provides methods and properties for managing application settings.
/// </summary>
/// <remarks>This class facilitates the retrieval and storage of configuration
/// settings, ensuring that application preferences are easily accessible and
/// modifiable. It may include features such as loading settings from files,
/// saving user preferences, and validating configuration values.</remarks>
public class SettingsManager
{
	private readonly string settingsPath;
	private Settings settings;

	/// <summary>
	/// Initializes a new instance of the <see cref="SettingsManager"/> class,
	/// optionally specifying the path to the settings file.
	/// </summary>
	/// <remarks>If the provided settingsPath is null or empty, the default
	/// path is constructed using the application's data directory,
	/// specifically targeting 'DigitalZenWorks/BackUpManager/Settings.json'.
	/// </remarks>
	/// <param name="settingsPath">The optional path to the settings file. If
	/// null or empty, a default path is used based on the application's data
	/// directory.</param>
	public SettingsManager(string settingsPath = null)
	{
		if (string.IsNullOrEmpty(settingsPath))
		{
			string baseDataDirectory = Environment.GetFolderPath(
				Environment.SpecialFolder.ApplicationData,
				Environment.SpecialFolderOption.Create);
			this.settingsPath = Path.Combine(
				baseDataDirectory,
				"DigitalZenWorks/BackUpManager/Settings.json");
		}
		else
		{
			this.settingsPath = settingsPath;
		}
	}

	/// <summary>
	/// Gets the current settings configuration for the application.
	/// </summary>
	public Settings Settings => settings;

	/// <summary>
	/// Loads the application settings from a JSON file at the configured path.
	/// </summary>
	/// <remarks>If the settings file is present, its contents are read and
	/// deserialized into a <see cref="Settings"/> object. If the file is
	/// missing, the method returns the current settings instance, which may be
	/// uninitialized.</remarks>
	/// <returns>An instance of the <see cref="Settings"/> class containing
	/// the loaded settings. Returns the current settings instance if the
	/// settings file does not exist.</returns>
	public Settings Load()
	{
		bool exists = File.Exists(settingsPath);

		if (exists == true)
		{
			string settingsText = File.ReadAllText(settingsPath);
			settings = JsonConvert.DeserializeObject<Settings>(settingsText);
		}

		return settings;
	}
}
