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

[TestFixture]
internal sealed class SettingsManagerTests
{
	private string tempDirectory;
	private string settingsPath;

	[SetUp]
	public void SetUp()
	{
		tempDirectory = Path.Combine(
			Path.GetTempPath(), Path.GetRandomFileName());
		Directory.CreateDirectory(tempDirectory);
		settingsPath = Path.Combine(tempDirectory, "Settings.json");
	}

	[TearDown]
	public void TearDown()
	{
		if (Directory.Exists(tempDirectory))
		{
			Directory.Delete(tempDirectory, recursive: true);
		}
	}

	// -------------------------------------------------------------------------
	// Constructor
	// -------------------------------------------------------------------------

	[Test]
	public void ConstructorValidPathCreatesInstance()
	{
		SettingsManager manager = new(settingsPath);

		Assert.That(manager, Is.Not.Null);
	}

	[Test]
	public void ConstructorDefaultPathCreatesInstance()
	{
		// Default constructor should not throw even if the default
		// settings file does not exist on this machine
		SettingsManager manager = new();

		Assert.That(manager, Is.Not.Null);
	}

	// -------------------------------------------------------------------------
	// Load — file existence
	// -------------------------------------------------------------------------

	[Test]
	public void LoadFileDoesNotExistReturnsNull()
	{
		SettingsManager manager = new(settingsPath);

		Settings result = manager.Load();

		Assert.That(result, Is.Null);
	}

	[Test]
	public void LoadFileExistsReturnsSettings()
	{
		Settings settings = new Settings();
		WriteSettings(settings);
		SettingsManager manager = new(settingsPath);

		Settings result = manager.Load();

		Assert.That(result, Is.Not.Null);
	}

	// -------------------------------------------------------------------------
	// Load — GlobalExcludes
	// -------------------------------------------------------------------------

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

	[Test]
	public void LoadWithEmptyGlobalExcludesReturnsEmptyList()
	{
		Settings settings = new() { GlobalExcludes = [] };
		WriteSettings(settings);

		SettingsManager manager = new(settingsPath);
		Settings result = manager.Load();

		Assert.That(result.GlobalExcludes, Is.Empty);
	}

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

	[Test]
	public void SettingsBeforeLoadIsNull()
	{
		SettingsManager manager = new(settingsPath);

		Assert.That(manager.Settings, Is.Null);
	}

	[Test]
	public void SettingsAfterLoadIsPopulated()
	{
		Settings settings = new Settings();
		WriteSettings(settings);
		SettingsManager manager = new(settingsPath);
		manager.Load();

		Assert.That(manager.Settings, Is.Not.Null);
	}

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

	// ------------------------------------------------------------------------
	// Helpers
	// ------------------------------------------------------------------------

	private void WriteSettings(Settings settings)
	{
		string json = JsonConvert.SerializeObject(settings);
		File.WriteAllText(settingsPath, json);
	}
}
