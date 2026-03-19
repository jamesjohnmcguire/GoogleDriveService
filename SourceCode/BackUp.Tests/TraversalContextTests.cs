/////////////////////////////////////////////////////////////////////////////
// <copyright file="TraversalContextTests.cs" company="James John McGuire">
// Copyright © 2017 - 2026 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.BackUp.Library.Tests;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DigitalZenWorks.BackUp.Library;
using NUnit.Framework;

/// <summary>
/// Provides a suite of unit tests for verifying the behavior of the
/// TraversalContext class, focusing on exclusion logic for file and
/// directory paths.
/// </summary>
/// <remarks>This test class includes tests that validate exclusion matching,
/// case sensitivity, type filtering, and global exclusion handling. The tests
/// ensure that TraversalContext correctly identifies paths to exclude based on
/// various criteria and platform-specific behaviors.</remarks>
[TestFixture]
internal sealed class TraversalContextTests
{
	private string clientsPath;
	private string existingDirectoryPath;
	private string existingFilePath;
	private List<string> globalExcludesWithDefaults = [];
	private string objPath;
	private Settings settngsWithDefaults;
	private string temporaryPath;
	private string testFixtureDirectory;
	private TraversalContext traversalContext;

	/// <summary>
	/// The one time setup method.
	/// </summary>
	[OneTimeSetUp]
	public void OneTimeSetUp()
	{
		string tempBaseDirectory = Path.GetTempPath();
		string randomName = Path.GetRandomFileName();

		testFixtureDirectory = Path.Combine(tempBaseDirectory, randomName);
		Directory.CreateDirectory(testFixtureDirectory);

		string settingsPath = Path.Combine(
			testFixtureDirectory,
			"Settings.json");

		string settingsData = "{\n\t\"GlobalExcludes\":\n" +
			"\t[\n\t\t\"_svn\", \".svn\", \".vs\", \"node_modules\", " +
			"\"obj\", \"vendor\"\n\t]\n}\n";

		File.WriteAllText(settingsPath, settingsData);

		SettingsManager settingsManager = new(settingsPath);
		settingsManager.Load();

		settngsWithDefaults = settingsManager.Settings;

		IReadOnlyCollection<string> defaultGlobalExcludes =
			settngsWithDefaults.GlobalExcludes;

		if (defaultGlobalExcludes != null)
		{
			globalExcludesWithDefaults = defaultGlobalExcludes.ToList();
		}
	}

	/// <summary>
	/// One time tear down method.
	/// </summary>
	[OneTimeTearDown]
	public void BaseOneTimeTearDown()
	{
		bool result = Directory.Exists(testFixtureDirectory);

		if (result == true)
		{
			Directory.Delete(testFixtureDirectory, true);
		}
	}

	/// <summary>
	/// Initializes the test environment by setting up temporary file paths for
	/// client data and object storage before each test is run.
	/// </summary>
	/// <remarks>This method is executed before each test to ensure a
	/// consistent and isolated environment. It creates temporary directories
	/// for storing client data and objects, which are used during the
	/// execution of the tests.</remarks>
	[SetUp]
	public void SetUp()
	{
		string tempBaseDirectory = Path.GetTempPath();
		string randomName = Path.GetRandomFileName();

		temporaryPath = Path.Combine(tempBaseDirectory, randomName);
		Directory.CreateDirectory(temporaryPath);

		existingDirectoryPath = Path.Combine(temporaryPath, "TestFolder");
		Directory.CreateDirectory(existingDirectoryPath);

		existingFilePath = Path.Combine(temporaryPath, "TestFile.txt");
		File.WriteAllText(existingFilePath, string.Empty);

		string root = Path.GetTempPath();
		clientsPath = Path.Combine(root, "Data", "Clients");
		objPath = Path.Combine(root, "Data", "obj");

		SettingsManager settingsManager = new();

		List<string> globalExcludes = [];

		Settings settings = settingsManager.Settings;

		if (settings.GlobalExcludes != null)
		{
			globalExcludes = settings.GlobalExcludes.ToList();
		}

		ICollection<Exclude> excludes = [];
		traversalContext = new(globalExcludes, excludes);
	}

	/// <summary>
	/// Cleans up resources by deleting the temporary directory used during
	/// test execution, if it exists.
	/// </summary>
	/// <remarks>This method is typically called after each test to ensure that
	/// any temporary files or directories created during the test are removed.
	/// This helps prevent clutter and avoids interference with subsequent
	/// tests by maintaining a clean test environment.</remarks>
	[TearDown]
	public void TearDown()
	{
		if (Directory.Exists(temporaryPath))
		{
			Directory.Delete(temporaryPath, recursive: true);
		}
	}

	// ------------------------------------------------------------------------
	// IsExcludeMatch — basic matching
	// ------------------------------------------------------------------------

	/// <summary>
	/// Verifies that the exclusion logic does not incorrectly match a path
	/// that should not be excluded when using subdirectory exclusion criteria.
	/// </summary>
	/// <remarks>This test ensures that the traversal context correctly
	/// identifies non-matching paths.</remarks>
	[Test]
	public void IsExcludeMatchDifferentPathsReturnsFalse()
	{
		Exclude exclude = new(clientsPath, false);

		bool result = TraversalContext.IsExcludeMatch(objPath, exclude);

		Assert.That(result, Is.False);
	}

	/// <summary>
	/// Verifies that an exact path matchreturns true when evaluated by the
	/// traversal context.
	/// </summary>
	/// <remarks>This test ensures that the exclusion logic correctly
	/// identifies an exact match for a subdirectory path as a valid exclusion
	/// according to the specified rules.</remarks>
	[Test]
	public void IsExcludeMatchExactPathMatchReturnsTrue()
	{
		Exclude exclude = new(clientsPath, false);

		bool result = TraversalContext.IsExcludeMatch(clientsPath, exclude);

		Assert.That(result, Is.True);
	}

	/// <summary>
	/// Verifies that exclusion matching logic respects the case sensitivity
	/// rules of the underlying operating system.
	/// </summary>
	/// <remarks>This test ensures that exclusion matching is case-insensitive
	/// on Windows and case-sensitive on non-Windows platforms. It does so by
	/// comparing the behavior when the exclusion path and the tested path
	/// differ in letter casing.</remarks>
	[Test]
	[System.Diagnostics.CodeAnalysis.SuppressMessage(
		"Globalization",
		"CA1308:Normalize strings to uppercase",
		Justification = "It is just a test.")]
	public void IsExcludeMatchCaseSensitivityReflectsOperatingSystem()
	{
		Exclude exclude = new(
			existingDirectoryPath.ToLowerInvariant(), false);

		string existingDirectoryPathUpper =
			existingDirectoryPath.ToUpperInvariant();
		bool result = TraversalContext.IsExcludeMatch(
			existingDirectoryPathUpper, exclude);

		if (OperatingSystem.IsWindows())
		{
			Assert.That(result, Is.True);
		}
		else
		{
			Assert.That(result, Is.False);
		}
	}

	/// <summary>
	/// The expand global excludes has default amount test verifies that the
	/// traversal context correctly expands global excludes to their fully
	/// qualified paths and that the expected number of global excludes are
	/// present after expansion test.
	/// </summary>
	[Test]
	public void ExpandGlobalExcludesHasDefaultAmount()
	{
		Assert.That(traversalContext, Is.Not.Null);

		ICollection<Exclude>? globalExcludes =
			traversalContext.ExpandGlobalExcludes(temporaryPath);
		Assert.That(globalExcludes, Has.Count.EqualTo(0));
	}

	/// <summary>
	/// The expand global excludes has default amount test verifies that the
	/// traversal context correctly expands global excludes to their fully
	/// qualified paths and that the expected number of global excludes are
	/// present after expansion test.
	/// </summary>
	[Test]
	public void ExpandGlobalExcludesHasDefaultAmountWithDefaults()
	{
		ICollection<Exclude> excludes = [];
		TraversalContext localTraversalContext = new(
			globalExcludesWithDefaults,
			excludes);

		ICollection<Exclude>? globalExcludes =
			localTraversalContext.ExpandGlobalExcludes(temporaryPath);
		Assert.That(globalExcludes, Has.Count.EqualTo(6));
	}

	/// <summary>
	/// Verifies that a global exclude entry with a relative path is correctly
	/// expanded to its full path when processed by the ExpandGlobalExcludes
	/// method.
	/// </summary>
	/// <remarks>This test ensures that the ExpandGlobalExcludes method
	/// resolves relative paths in global exclude entries based on the provided
	/// temporary directory, resulting in the expected absolute path. It
	/// validates correct path expansion behavior for global excludes.
	/// </remarks>
	[Test]
	public void ExpandGlobalExcludesRelativePathExpanded()
	{
		SettingsManager settingsManager = new();

		const string relativeName = "SomeFolder";
		string tempSubDirectory = Path.Combine(temporaryPath, relativeName);
		Directory.CreateDirectory(tempSubDirectory);

		Exclude globalExclude = new(relativeName, false);

		IReadOnlyCollection<string> globalExcludesRaw =
			settingsManager.Settings.GlobalExcludes;

		List<string> globalExcludes = [];

		Settings settings = settingsManager.Settings;

		if (settings.GlobalExcludes != null)
		{
			globalExcludes = settings.GlobalExcludes.ToList();
		}

		globalExcludes.Add(relativeName);

		ICollection<Exclude> excludes = [];
		TraversalContext localTraversalContext = new(
			globalExcludes,
			excludes);

		ICollection<Exclude>? result =
			localTraversalContext.ExpandGlobalExcludes(temporaryPath);

		string expected = Path.GetFullPath(
			Path.Combine(temporaryPath, relativeName));

		Exclude exclude = result!.LastOrDefault()!;
		string? lastPath = exclude.Path;

		Assert.That(result, Has.Count.EqualTo(1));
		Assert.That(lastPath, Is.EqualTo(expected));
	}

	/// <summary>
	/// Verifies that a global exclude entry with a relative path is correctly
	/// expanded to its full path when processed by the ExpandGlobalExcludes
	/// method.
	/// </summary>
	/// <remarks>This test ensures that the ExpandGlobalExcludes method
	/// resolves relative paths in global exclude entries based on the provided
	/// temporary directory, resulting in the expected absolute path. It
	/// validates correct path expansion behavior for global excludes.
	/// </remarks>
	[Test]
	public void ExpandGlobalExcludesRelativePathExpandedWithDefaults()
	{
		SettingsManager settingsManager = new();
		settingsManager.Load();

		const string relativeName = "SomeFolder";
		string tempSubDirectory = Path.Combine(temporaryPath, relativeName);
		Directory.CreateDirectory(tempSubDirectory);

		Exclude globalExclude = new(relativeName, false);

		IReadOnlyCollection<string> globalExcludesRaw =
			settingsManager.Settings.GlobalExcludes;

		List<string> globalExcludes = globalExcludesWithDefaults.ToList();
		globalExcludes.Add(relativeName);

		ICollection<Exclude> excludes = [];
		TraversalContext localTraversalContext = new(
			globalExcludes,
			excludes);

		ICollection<Exclude>? result =
			localTraversalContext.ExpandGlobalExcludes(temporaryPath);

		string expected = Path.GetFullPath(
			Path.Combine(temporaryPath, relativeName));

		Exclude exclude = result!.LastOrDefault()!;
		string? lastPath = exclude.Path;

		Assert.That(result, Has.Count.EqualTo(7));
		Assert.That(lastPath, Is.EqualTo(expected));
	}

	/// <summary>
	/// Verifies that expanding global excludes preserves the absolute path of
	/// a global exclude entry.
	/// </summary>
	/// <remarks>This test ensures that when an absolute path is specified as a
	/// global exclude, the expansion process maintains the path as absolute
	/// and does not alter its format. This is important for correct exclusion
	/// behavior when working with file system paths.</remarks>
	[Test]
	public void ExpandGlobalExcludesAbsolutePathIsKept()
	{
		string absolutePath = Path.Combine(temporaryPath, "AbsoluteFolder");
		Exclude globalExclude = new(absolutePath, false);

		ICollection<Exclude> originalExcludes = [];
		List<Exclude> excludesCopy = originalExcludes.ToList();
		excludesCopy.Add(globalExclude);

		SettingsManager settingsManager = new();

		List<string> globalExcludes = [];

		Settings settings = settingsManager.Settings;

		if (settings.GlobalExcludes != null)
		{
			globalExcludes = settings.GlobalExcludes.ToList();
		}

		TraversalContext localTraversalContext =
			new(globalExcludes, excludesCopy);

		ICollection<Exclude>? result =
			localTraversalContext.ExpandGlobalExcludes(temporaryPath);
		Exclude? exclude = result!.FirstOrDefault();

		Assert.That(result, Has.Count.EqualTo(1));
		Assert.That(exclude, Is.Not.Null);

		string newPath = Path.GetFullPath(absolutePath);
		Assert.That(exclude.Path, Is.EqualTo(newPath));
	}

	/// <summary>
	/// Verifies that expanding global excludes preserves the absolute path of
	/// a global exclude entry.
	/// </summary>
	/// <remarks>This test ensures that when an absolute path is specified as a
	/// global exclude, the expansion process maintains the path as absolute
	/// and does not alter its format. This is important for correct exclusion
	/// behavior when working with file system paths.</remarks>
	[Test]
	public void ExpandGlobalExcludesAbsolutePathIsKeptWithDefaults()
	{
		string absolutePath = Path.Combine(temporaryPath, "AbsoluteFolder");
		Exclude globalExclude = new(absolutePath, false);

		ICollection<Exclude> originalExcludes = [];
		List<Exclude> excludesCopy = originalExcludes.ToList();
		excludesCopy.Add(globalExclude);

		SettingsManager settingsManager = new();
		settingsManager.Load();

		TraversalContext localTraversalContext =
			new(globalExcludesWithDefaults, excludesCopy);

		ICollection<Exclude>? result =
			localTraversalContext.ExpandGlobalExcludes(temporaryPath);
		Exclude? exclude = result!.FirstOrDefault();

		Assert.That(result, Has.Count.EqualTo(7));
		Assert.That(exclude, Is.Not.Null);

		string newPath = Path.GetFullPath(absolutePath);
		Assert.That(exclude.Path, Is.EqualTo(newPath));
	}

	/// <summary>
	/// Verifies that the ExpandGlobalExcludes method correctly expands
	/// multiple global excludes and returns all expected excludes for the
	/// specified directory.
	/// </summary>
	/// <remarks>This test ensures that when multiple global excludes are
	/// provided, the method returns a list containing all of them. It is
	/// useful for validating that the exclude expansion logic accounts for all
	/// global entries as intended.</remarks>
	[Test]
	public void ExpandGlobalExcludesAllExpanded()
	{
		Exclude item = new Exclude("FolderA", false);

		ICollection<Exclude> originalExcludes = [];
		List<Exclude> excludesCopy = originalExcludes.ToList();
		excludesCopy.Add(item);
		excludesCopy.Add(item);

		SettingsManager settingsManager = new();

		List<string> globalExcludes = [];

		Settings settings = settingsManager.Settings;

		if (settings.GlobalExcludes != null)
		{
			globalExcludes = settings.GlobalExcludes.ToList();
		}

		TraversalContext localTraversalContext =
			new(globalExcludes, excludesCopy);

		ICollection<Exclude>? result =
			localTraversalContext.ExpandGlobalExcludes(temporaryPath);

		Assert.That(result, Has.Count.EqualTo(2));
	}

	/// <summary>
	/// Verifies that the ExpandGlobalExcludes method correctly expands
	/// multiple global excludes and returns all expected excludes for the
	/// specified directory.
	/// </summary>
	/// <remarks>This test ensures that when multiple global excludes are
	/// provided, the method returns a list containing all of them. It is
	/// useful for validating that the exclude expansion logic accounts for all
	/// global entries as intended.</remarks>
	[Test]
	public void ExpandGlobalExcludesAllExpandedWithDefaults()
	{
		Exclude item = new Exclude("FolderA", false);

		ICollection<Exclude> originalExcludes = [];
		List<Exclude> excludesCopy = originalExcludes.ToList();
		excludesCopy.Add(item);
		excludesCopy.Add(item);

		SettingsManager settingsManager = new();
		settingsManager.Load();

		TraversalContext localTraversalContext =
			new(globalExcludesWithDefaults, excludesCopy);

		ICollection<Exclude>? result =
			localTraversalContext.ExpandGlobalExcludes(temporaryPath);

		Assert.That(result, Has.Count.EqualTo(8));
	}

	// ------------------------------------------------------------------------
	// NormalizePath — existing paths
	// ------------------------------------------------------------------------

	/// <summary>
	/// The normalize path existing file returns fully qualified path test.
	/// </summary>
	[Test]
	public void NormalizePathReturnsFullyQualifiedPath()
	{
		string? result =
			TraversalContext.NormalizePath(existingFilePath, temporaryPath);

		Assert.That(result, Is.Not.Null);

		bool fullyQualified = Path.IsPathFullyQualified(result);

		Assert.That(fullyQualified, Is.True);
	}

	/// <summary>
	/// The normalize path non-existing directory returns true test.
	/// </summary>
	[Test]
	public void NormalizePathNonExistingDirectoryReturnsTrue()
	{
		string? result = TraversalContext.NormalizePath(objPath, temporaryPath);

		Assert.That(result, Is.Not.Null);

		bool fullyQualified = Path.IsPathFullyQualified(result);

		Assert.That(fullyQualified, Is.True);
	}

	/// <summary>
	/// The normalize path non-existing file returns true test.
	/// </summary>
	[Test]
	public void NormalizePathNonExistingFileReturnsTrue()
	{
		string? result =
			TraversalContext.NormalizePath("somefile.txt", temporaryPath);

		Assert.That(result, Is.Not.Null);

		bool fullyQualified = Path.IsPathFullyQualified(result);

		Assert.That(fullyQualified, Is.True);
	}

	/// <summary>
	/// The normalize path already fully qualified returns same path test.
	/// </summary>
	[Test]
	public void NormalizePathAlreadyFullyQualifiedReturnsSame()
	{
		string? result = TraversalContext.NormalizePath(
			existingDirectoryPath, temporaryPath);

		Assert.That(result, Is.EqualTo(existingDirectoryPath));
	}

	/// <summary>
	/// The normalize path relative path returns fully qualified path test.
	/// </summary>
	[Test]
	public void NormalizePathRelativePathReturnsFullyQualifiedPath()
	{
		string originalDirectory = Directory.GetCurrentDirectory();

		try
		{
			Directory.SetCurrentDirectory(temporaryPath);
			string relativePath = "TestFolder";

			string? result =
				TraversalContext.NormalizePath(relativePath, temporaryPath);

			Assert.That(result, Is.Not.Null);
			Assert.That(Path.IsPathFullyQualified(result!), Is.True);
		}
		finally
		{
			Directory.SetCurrentDirectory(originalDirectory);
		}
	}
}
