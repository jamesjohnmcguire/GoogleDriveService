/////////////////////////////////////////////////////////////////////////////
// <copyright file="SettingsManagerTests.cs" company="James John McGuire">
// Copyright © 2017 - 2026 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.BackUp.Library.Tests;

using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using NUnit.Framework;

/// <summary>
/// Provides unit tests for the SettingsManagerTests class.
/// </summary>
[TestFixture]
internal sealed class SettingsManagerTests
{
	private string tempDirectory;
	private string settingsPath;

	/// <summary>
	/// Initializes unique temporary directories for use in each test run.
	/// </summary>
	[SetUp]
	public void SetUp()
	{
		tempDirectory = Path.Combine(
			Path.GetTempPath(), Path.GetRandomFileName());
		Directory.CreateDirectory(tempDirectory);
		settingsPath = Path.Combine(tempDirectory, "Settings.json");
	}

	/// <summary>
	/// Cleans up resources by deleting the temporary directory, if it exists.
	/// </summary>
	[TearDown]
	public void TearDown()
	{
		if (Directory.Exists(tempDirectory))
		{
			Directory.Delete(tempDirectory, recursive: true);
		}
	}

	// ------------------------------------------------------------------------
	// Constructor
	// ------------------------------------------------------------------------

	/// <summary>
	/// The constructor with a valid path should create an instance of
	/// SettingsManager test.
	/// </summary>
	[Test]
	public void ConstructorValidPathCreatesInstance()
	{
		SettingsManager manager = new(settingsPath);

		Assert.That(manager, Is.Not.Null);
	}

	/// <summary>
	/// The default constructor should create an instance of SettingsManager
	/// test.
	/// </summary>
	[Test]
	public void ConstructorDefaultPathCreatesInstance()
	{
		// Default constructor should not throw even if the default
		// settings file does not exist on this machine
		SettingsManager manager = new();

		Assert.That(manager, Is.Not.Null);
	}

	// ------------------------------------------------------------------------
	// Load — file existence
	// ------------------------------------------------------------------------

	/// <summary>
	/// The load called twice returns consistent results test.
	/// </summary>
	[Test]
	public void LoadCalledTwiceReturnsConsistentResults()
	{
		IReadOnlyCollection<string> globalExcludes =
			["obj", "node_modules"];

		Settings settings = new();
		settings.GlobalExcludes = globalExcludes;

		WriteSettings(settings);

		SettingsManager manager = new(settingsPath);
		Settings first = manager.Load();
		Settings second = manager.Load();

		Assert.That(
			first.GlobalExcludes,
			Is.EquivalentTo(second.GlobalExcludes));
	}

	/// <summary>
	/// The load method should return an (empty) object when the settings file
	/// does not exist test.
	/// </summary>
	[Test]
	public void LoadFileDoesNotExistReturnsObject()
	{
		SettingsManager manager = new(settingsPath);

		Settings settings = manager.Load();

		Assert.That(settings, Is.Not.Null);
		Assert.That(settings.GlobalExcludes, Is.Null);
	}

	/// <summary>
	/// The load method should return a Settings object when the settings file
	/// test.
	/// </summary>
	[Test]
	public void LoadFileExistsReturnsSettings()
	{
		Settings settings = new Settings();
		WriteSettings(settings);
		SettingsManager manager = new(settingsPath);

		Settings result = manager.Load();

		Assert.That(result, Is.Not.Null);
	}

	// ------------------------------------------------------------------------
	// Load — GlobalExcludes
	// ------------------------------------------------------------------------

	/// <summary>
	/// The load with empty GlobalExcludes returns empty list test.
	/// </summary>
	[Test]
	public void LoadWithEmptyGlobalExcludesReturnsEmptyList()
	{
		Settings settings = new() { GlobalExcludes = [] };
		WriteSettings(settings);

		SettingsManager manager = new(settingsPath);
		Settings result = manager.Load();

		Assert.That(result.GlobalExcludes, Is.Empty);
	}

	/// <summary>
	/// The load with GlobalExcludes populates GlobalExcludes test.
	/// </summary>
	[Test]
	public void LoadWithGlobalExcludesPopulatesGlobalExcludes()
	{
		IReadOnlyCollection<string> globalExcludes =
			["obj", "node_modules", ".vs"];

		Settings settings = new();
		settings.GlobalExcludes = globalExcludes;
		WriteSettings(settings);

		SettingsManager manager = new(settingsPath);
		Settings result = manager.Load();

		Assert.That(result.GlobalExcludes, Is.Not.Null);
		Assert.That(result.GlobalExcludes, Has.Count.EqualTo(3));
	}

	/// <summary>
	/// The load with GlobalExcludes preserves values test.
	/// </summary>
	[Test]
	public void LoadWithGlobalExcludesPreservesValues()
	{
		IReadOnlyCollection<string> globalExcludes =
			["obj", "node_modules", ".vs"];

		Settings settings = new();
		settings.GlobalExcludes = globalExcludes;
		WriteSettings(settings);

		SettingsManager manager = new(settingsPath);
		Settings result = manager.Load();

		string[] expected = new[] { "obj", "node_modules", ".vs" };
		Assert.That(result.GlobalExcludes, Is.EquivalentTo(expected));
	}

	/// <summary>
	/// The load with null GlobalExcludes returns null test.
	/// </summary>
	[Test]
	public void LoadWithNullGlobalExcludesReturnsNullExcludes()
	{
		Settings settings = new() { GlobalExcludes = null };
		WriteSettings(settings);

		SettingsManager manager = new(settingsPath);
		Settings result = manager.Load();

		Assert.That(result.GlobalExcludes, Is.Null);
	}

	// -------------------------------------------------------------------------
	// Load — Settings property
	// -------------------------------------------------------------------------

	/// <summary>
	/// The settings property after load is populated test.
	/// </summary>
	[Test]
	public void SettingsAfterLoadIsPopulated()
	{
		Settings settings = new Settings();
		WriteSettings(settings);
		SettingsManager manager = new(settingsPath);
		manager.Load();

		Assert.That(manager.Settings, Is.Not.Null);
	}

	/// <summary>
	/// The settings property before load is null test.
	/// </summary>
	[Test]
	public void SettingsBeforeLoadIsNull()
	{
		SettingsManager manager = new(settingsPath);

		Assert.That(manager.Settings, Is.Not.Null);
		Assert.That(manager.Settings.GlobalExcludes, Is.Null);
	}

	private void WriteSettings(Settings settings)
	{
		string json = JsonConvert.SerializeObject(settings);
		File.WriteAllText(settingsPath, json);
	}
}
