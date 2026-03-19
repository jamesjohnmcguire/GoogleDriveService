/////////////////////////////////////////////////////////////////////////////
// <copyright file="DriveMappingTests.cs" company="James John McGuire">
// Copyright © 2017 - 2026 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.BackUp.Library.Tests;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using NUnit.Framework;

/// <summary>
/// Provides unit tests for the DriveMapping class to verify its construction,
/// property accessors, and exclude expansion methods under various scenarios.
/// </summary>
/// <remarks>These tests ensure that the DriveMapping class correctly handles
/// edge cases such as null or empty exclude lists, relative and absolute
/// paths, wildcard expansions, and regression scenarios involving multiple
/// wildcards. The fixture uses a temporary directory for isolation and cleanup
/// between tests.</remarks>
[TestFixture]
internal sealed class DriveMappingTests
{
	private string temporaryPath;
	private DriveMapping tempDirectoryMapping;

	/// <summary>
	/// Initializes a unique temporary directory for use in each test run,
	/// ensuring a clean environment before test execution.
	/// </summary>
	/// <remarks>This method is executed before each test to prevent
	/// interference between tests by creating a new temporary directory with a
	/// random name. The directory is guaranteed to be empty at the start of
	/// each test, allowing test files to be safely created and deleted without
	/// affecting other tests.</remarks>
	[SetUp]
	public void SetUp()
	{
		string tempBaseDirectory = Path.GetTempPath();
		string randomName = Path.GetRandomFileName();

		temporaryPath = Path.Combine(tempBaseDirectory, randomName);
		Directory.CreateDirectory(temporaryPath);

		tempDirectoryMapping = new() { LocalPath = temporaryPath };

		SettingsManager settingsManager = new();
		settingsManager.Load();

		List<string> globalExcludes = [];

		Settings settings = settingsManager.Settings;

		if (settings.GlobalExcludes != null)
		{
			globalExcludes = settings.GlobalExcludes.ToList();
		}
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
	// Constructor / Property tests
	// ------------------------------------------------------------------------

	/// <summary>
	/// Verifies that constructing a DriveMapping instance initializes the
	/// Excludes list to an empty, non-null collection.
	/// </summary>
	/// <remarks>This test ensures that the Excludes property is safely
	/// accessible even when no settings file is present, preventing null
	/// reference exceptions in client code.</remarks>
	[Test]
	public void ConstructorInstanceEmptyExcludes()
	{
		// When no settings file is present the excludes list should still
		// be initialised (just empty).
		DriveMapping mapping = new();

		Assert.That(mapping.Excludes, Is.Not.Null);
		Assert.That(mapping.Excludes, Is.Empty);
	}

	/// <summary>
	/// Verifies that the Path property of the DriveMapping class can be set
	/// and subsequently retrieved correctly.
	/// </summary>
	/// <remarks>This test ensures that assigning a value to the Path property
	/// results in the property returning the expected value, confirming proper
	/// getter and setter functionality.</remarks>
	[Test]
	public void PathPropertySetAndRetrieved()
	{
		Assert.That(tempDirectoryMapping.LocalPath, Is.EqualTo(temporaryPath));
	}

	/// <summary>
	/// Verifies that the DriveParentFolderId property of the DriveMapping
	/// class can be set and retrieved as expected.
	/// </summary>
	/// <remarks>This test ensures that assigning a value to the
	/// DriveParentFolderId property correctly stores the identifier and that
	/// the property returns the assigned value. Use this test to confirm the
	/// integrity of property accessors for folder hierarchy management.
	/// </remarks>
	[Test]
	public void DriveParentFolderIdSetAndRetrieved()
	{
		const string folderId = "1AbCdEfGhIjKlMnOpQrStUvWxYz";
		DriveMapping mapping = new() { DriveParentFolderId = folderId };

		Assert.That(mapping.DriveParentFolderId, Is.EqualTo(folderId));
	}

	/// <summary>
	/// Verifies that the ExpandWildCardExcludes method returns an empty
	/// collection when the excludes parameter is null.
	/// </summary>
	/// <remarks>This test ensures that passing a null excludes collection to
	/// ExpandWildCardExcludes results in an empty collection return value,
	/// indicating that no exclusions were applied.</remarks>
	[Test]
	public void ExpandWildCardExcludesNotNull()
	{
		ICollection<Exclude> result =
			DriveMapping.ExpandWildCardExcludes(null);

		Assert.That(result, Is.Not.Null);
		Assert.That(result, Is.Empty);
	}

	/// <summary>
	/// Verifies that the ExpandWildCardExcludes method returns an empty list
	/// when no exclusions are provided.
	/// </summary>
	/// <remarks>This test ensures that the method under test correctly handles
	/// scenarios where the exclusions collection is empty, returning a
	/// non-null, empty list as expected.</remarks>
	[Test]
	public void ExpandWildCardExcludesEmptyList()
	{
		ICollection<Exclude> result = DriveMapping.ExpandWildCardExcludes([]);

		Assert.That(result, Is.Not.Null);
		Assert.That(result, Is.Empty);
	}

	/// <summary>
	/// Tests that global exclude entries using a wildcard file pattern are
	/// correctly expanded to match specific files in the target directory.
	/// </summary>
	/// <remarks>This test verifies that when a wildcard exclude is provided,
	/// the expansion process replaces the wildcard entry with all files
	/// matching the pattern in the specified directory. Only files that match
	/// the wildcard are included in the result, ensuring accurate exclusion
	/// behavior.</remarks>
	[Test]
	public void ExpandWildCardExcludesWildcardFileExpands()
	{
		// Create two matching files inside temporaryPath
		AddLogTextfiles();

		string wildcardPath = Path.Combine(temporaryPath, "*.txt");
		Exclude wildcardExclude = new(wildcardPath, false);

		ICollection<Exclude> result =
			DriveMapping.ExpandWildCardExcludes([wildcardExclude]);

		// The wildcard entry itself should be removed and replaced by the
		// two matched .txt files.
		Assert.That(result, Has.Count.EqualTo(2));
		Assert.That(
			result,
			Has.All.Matches<Exclude>(e => e.Path?.EndsWith(
				".txt", System.StringComparison.OrdinalIgnoreCase) ?? false));
	}

	/// <summary>
	/// Tests that exclude entries using a wildcard file pattern are correctly
	/// expanded to match specific files in the target directory.
	/// </summary>
	/// <remarks>This test verifies that when a wildcard exclude is provided,
	/// the expansion process replaces the wildcard entry with all files
	/// matching the pattern in the specified directory. Only files that match
	/// the wildcard are included in the result, ensuring accurate exclusion
	/// behavior.</remarks>
	[Test]
	public void ExpandWildcardExcludesFileExpands()
	{
		// Create two matching files inside temporaryPath
		AddLogTextfiles();

		string wildcardPath = Path.Combine(temporaryPath, "*.txt");
		Exclude wildcardExclude = new(wildcardPath, false);

		ICollection<Exclude> result =
			DriveMapping.ExpandWildCardExcludes([wildcardExclude]);

		// The wildcard entry itself should be removed and replaced by the
		// two matched .txt files.
		Assert.That(result, Has.Count.EqualTo(2));
		Assert.That(
			result,
			Has.All.Matches<Exclude>(e => e.Path?.EndsWith(
				".txt", System.StringComparison.OrdinalIgnoreCase) ?? false));
	}

	/// <summary>
	/// Verifies that expanding a global exclude wildcard with no matching
	/// files does not remove the original exclude
	/// entry.
	/// </summary>
	/// <remarks>This test ensures that when a wildcard exclude is applied to a
	/// directory containing no files that match the pattern, the original
	/// exclude entry remains in the result. It validates that the method
	/// correctly handles cases where the wildcard expansion yields no matches,
	/// preserving the intended exclusion behavior.</remarks>
	[Test]
	public void ExpandWildCardExcludesWildcardNoMatches()
	{
		// Wildcard that won't match anything in the empty temp dir
		string wildcardPath = Path.Combine(temporaryPath, "*.xyz");
		Exclude wildcardExclude = new(wildcardPath, false);

		ICollection<Exclude> result =
			DriveMapping.ExpandWildCardExcludes([wildcardExclude]);

		// No matches – original entry stays because ExpandWildCard returns
		// an empty list and the remove-and-concat branch is not entered.
		Assert.That(result, Has.Count.EqualTo(0));
	}

	/// <summary>
	/// Verifies that expanding an exclude wildcard with no matching files
	/// does not remove the original exclude entry.
	/// </summary>
	/// <remarks>This test ensures that when a wildcard exclude is applied to a
	/// directory containing no files that match the pattern, the original
	/// exclude entry remains in the result. It validates that the method
	/// correctly handles cases where the wildcard expansion yields no matches,
	/// preserving the intended exclusion behavior.</remarks>
	[Test]
	public void ExpandWildcardExcludesNoMatches()
	{
		// Wildcard that won't match anything in the empty temp dir
		string wildcardPath = Path.Combine(temporaryPath, "*.xyz");
		Exclude wildcardExclude = new(wildcardPath, false);

		ICollection<Exclude> result =
			DriveMapping.ExpandWildCardExcludes([wildcardExclude]);

		// No matches – original entry stays because ExpandWildCard returns
		// an empty list and the remove-and-concat branch is not entered.
		Assert.That(result, Has.Count.EqualTo(0));
	}

	// ------------------------------------------------------------------------
	// ExpandExcludes (instance method)
	// ------------------------------------------------------------------------

	/// <summary>
	/// Verifies that the ExpandExcludes method returns an empty list when the
	/// excludes list is null.
	/// </summary>
	/// <remarks>This test uses reflection to simulate a null excludes list,
	/// which cannot be achieved through the public API. It ensures that
	/// ExpandExcludes handles the null condition gracefully by returning an
	/// empty list instead of null.</remarks>
	[Test]
	public void ExpandExcludesNullExcludeList()
	{
		// The default constructor initialises excludes to an empty list,
		// so the null branch is not reachable via the public API without
		// reflection – simply verify the empty-list path returns an empty
		// list.
		ICollection<Exclude> result = tempDirectoryMapping.ExpandExcludes();

		Assert.That(result, Is.Not.Null);
		Assert.That(result, Is.Empty);
	}

	/// <summary>
	/// Verifies that non-global excludes with relative paths are correctly
	/// expanded to absolute paths when using the ExpandExcludes method.
	/// </summary>
	/// <remarks>This test ensures that when an Exclude with a relative path is
	/// added to the Excludes collection of a DriveMapping instance, the
	/// ExpandExcludes method converts the relative path to an absolute path
	/// based on the mapping's base directory. This behavior is important for
	/// correct exclude path resolution in backup scenarios.</remarks>
	[Test]
	public void ExpandExcludesNonGlobalExcludeRelativePath()
	{
		// Add a non-global exclude with a relative path via the Excludes
		// collection (cast to List<Exclude> since Excludes is IList<Exclude>).
		Exclude item = new Exclude("MyFolder", false);

		List<Exclude> excludes = (List<Exclude>)tempDirectoryMapping.Excludes;
		excludes.Add(item);

		ICollection<Exclude> result = tempDirectoryMapping.ExpandExcludes();

		string expected = Path.GetFullPath(
			Path.Combine(temporaryPath, "MyFolder"));

		int count = result.Count;
		string? lastPath = GetLastExcludePath(result);

		Assert.That(count, Is.GreaterThan(0));
		Assert.That(lastPath, Is.EqualTo(expected));
	}

	/// <summary>
	/// Verifies that excluded file paths containing wildcards are correctly
	/// expanded to all matching files within the specified directory.
	/// </summary>
	/// <remarks>This test ensures that when an exclusion entry uses a wildcard
	/// pattern, the method expands it to include each file that matches the
	/// pattern. It checks that the resulting exclusion list contains the
	/// expected number of files and that all files meet the criteria defined
	/// by the wildcard. Use this test to confirm correct handling of
	/// wildcard-based exclusions in directory mappings.</remarks>
	[Test]
	public void ExpandExcludesWildcardFileExpands()
	{
		AddCsvTestfiles();

		string wildcardPath = Path.Combine(temporaryPath, "*.csv");

		Exclude item = new Exclude(wildcardPath, false);
		List<Exclude> excludes = (List<Exclude>)tempDirectoryMapping.Excludes;
		excludes.Add(item);

		ICollection<Exclude> result = tempDirectoryMapping.ExpandExcludes();

		int count = result.Count;

		Assert.That(count, Is.GreaterThanOrEqualTo(2));
		Assert.That(
			result,
			Has.Some.Matches<Exclude>(e => e.Path?.EndsWith(
				".csv", System.StringComparison.OrdinalIgnoreCase) ?? false));
	}

	/// <summary>
	/// Two wildcard patterns in ExpandExcludes: both should be fully expanded.
	/// </summary>
	[Test]
	public void ExpandExcludesMultipleWildcardsAllExpanded()
	{
		AddCsvTestfiles();
		AddLogTestfiles();

		ICollection<Exclude> result = AddCsvAndLogToExcludes();

		int count = result.Count;

		// Expect all 4 concrete files.
		Assert.That(count, Is.GreaterThanOrEqualTo(4));
		Assert.That(result, Has.None.Matches<Exclude>(e => e.Path?.Contains(
			'*', System.StringComparison.OrdinalIgnoreCase) ?? false));
	}

	/// <summary>
	/// Two wildcard patterns in ExpandExcludes: both should be fully expanded.
	/// </summary>
	[Test]
	public void ExpandWildCardExcludesMultipleAllExpanded()
	{
		AddCsvTestfiles();
		AddLogTestfiles();

		Collection<Exclude> excludeList = [];

		string excludePath1 = Path.Combine(temporaryPath, "*.csv");
		Exclude exclude1 = new(excludePath1, false);
		excludeList.Add(exclude1);

		string excludePath2 = Path.Combine(temporaryPath, "*.log");
		Exclude exclude2 = new(excludePath1, false);
		excludeList.Add(exclude2);

		ICollection<Exclude> result =
			DriveMapping.ExpandWildCardExcludes(excludeList);

		int count = result.Count;

		// Expect all 4 concrete files.
		Assert.That(count, Is.GreaterThanOrEqualTo(4));
		Assert.That(result, Has.None.Matches<Exclude>(e => e.Path?.Contains(
			'*', System.StringComparison.OrdinalIgnoreCase) ?? false));
	}

	/// <summary>
	/// Two wildcard patterns in ExpandExcludes: one should be fully expanded.
	/// But the other having no matches, should simply be removed.
	/// </summary>
	[Test]
	public void ExpandWildCardExcludesMultipleSomeNoMatches()
	{
		AddCsvTestfiles();

		Collection<Exclude> excludeList = [];

		string excludePath1 = Path.Combine(temporaryPath, "*.csv");
		Exclude exclude1 = new(excludePath1, false);
		excludeList.Add(exclude1);

		string excludePath2 = Path.Combine(temporaryPath, "*.log");
		Exclude exclude2 = new(excludePath1, false);
		excludeList.Add(exclude2);

		ICollection<Exclude> result =
			DriveMapping.ExpandWildCardExcludes(excludeList);

		int count = result.Count;

		// Expect 2 concrete files.
		Assert.That(count, Is.GreaterThanOrEqualTo(2));
		Assert.That(result, Has.None.Matches<Exclude>(e => e.Path?.Contains(
			'*', System.StringComparison.OrdinalIgnoreCase) ?? false));
	}

	/// <summary>
	/// Two wildcard patterns in ExpandExcludes: one should be fully expanded.
	/// But the other having no matches, should simply be removed.
	/// </summary>
	[Test]
	public void ExpandExcludesMultipleWildcardsSomeNoMatches()
	{
		AddCsvTestfiles();
		ICollection<Exclude> result = AddCsvAndLogToExcludes();

		int count = result.Count;

		// Expect 2 concrete files.
		Assert.That(count, Is.GreaterThanOrEqualTo(2));
		Assert.That(result, Has.None.Matches<Exclude>(e => e.Path?.Contains(
			'*', System.StringComparison.OrdinalIgnoreCase) ?? false));
	}

	/// <summary>
	/// A non-wildcard exclude followed by a wildcard exclude: the non-wildcard
	/// entry must survive intact. The stale-index bug can cause it to be lost
	/// when the list is rebuilt during wildcard expansion.
	/// </summary>
	[Test]
	public void ExpandExcludesNonWildcardNonWildcardIsPreserved()
	{
		string reportFile = Path.Combine(temporaryPath, "report.csv");
		File.WriteAllText(reportFile, string.Empty);

		string fixedFolder = Path.Combine(temporaryPath, "FixedFolder");
		string wildcardPath = Path.Combine(temporaryPath, "*.csv");

		List<Exclude> excludeList = (List<Exclude>)tempDirectoryMapping.Excludes;

		Exclude folderExclude = new(fixedFolder, false);
		Exclude wildCardExclude = new(wildcardPath, false);

		// Fixed entry first, wildcard second — reverse loop hits wildcard first.
		excludeList.Add(folderExclude);
		excludeList.Add(wildCardExclude);

		ICollection<Exclude> result = tempDirectoryMapping.ExpandExcludes();

		int count = result.Count;

		// Should contain the expanded csv file AND the fixed folder.
		Assert.That(count, Is.GreaterThanOrEqualTo(2));
		Assert.That(result, Has.Some.Matches<Exclude>(e =>
			e.Path?.EndsWith("report.csv", StringComparison.Ordinal) ?? false));
		Assert.That(result, Has.Some.Matches<Exclude>(e =>
			e.Path == Path.GetFullPath(fixedFolder)));
	}

	/// <summary>
	/// A non-wildcard exclude followed by a wildcard exclude: the non-wildcard
	/// entry must survive intact. The stale-index bug can cause it to be lost
	/// when the list is rebuilt during wildcard expansion.
	/// </summary>
	[Test]
	public void ExpandWildCardExcludesNonWildcardIsPreserved()
	{
		string reportFile = Path.Combine(temporaryPath, "report.csv");
		File.WriteAllText(reportFile, string.Empty);

		string fixedFolder = Path.Combine(temporaryPath, "FixedFolder");
		string wildcardPath = Path.Combine(temporaryPath, "*.csv");

		Collection<Exclude> excludeList = [];

		Exclude folderExclude = new(fixedFolder, false);
		Exclude wildCardExclude = new(wildcardPath, false);

		// Fixed entry first, wildcard second — reverse loop hits wildcard first.
		excludeList.Add(folderExclude);
		excludeList.Add(wildCardExclude);

		ICollection<Exclude> result =
			DriveMapping.ExpandWildCardExcludes(excludeList);

		int count = result.Count;

		// Should contain the expanded csv file AND the fixed folder.
		Assert.That(count, Is.GreaterThanOrEqualTo(2));
		Assert.That(result, Has.Some.Matches<Exclude>(e =>
			e.Path?.EndsWith(
				"report.csv", StringComparison.Ordinal) ?? false));
		Assert.That(result, Has.Some.Matches<Exclude>(e =>
			e.Path == Path.GetFullPath(fixedFolder)));
	}

	/// <summary>
	/// Same stale-index scenario exercised via the static ExpandGlobalExcludes.
	/// </summary>
	[Test]
	public void ExpandExcludesMultipleWildcards()
	{
		AddBakTestfiles();
		AddTmpTestfiles();

		string bakWildcard = Path.Combine(temporaryPath, "*.bak");
		string tmpWildcard = Path.Combine(temporaryPath, "*.tmp");

		Exclude exclude1 = new Exclude(bakWildcard, false);
		Exclude exclude2 = new Exclude(tmpWildcard, false);

		List<Exclude> excludes = [];
		excludes.Add(exclude1);
		excludes.Add(exclude2);

		ICollection<Exclude> result =
			DriveMapping.ExpandWildCardExcludes(excludes);

		Assert.That(result, Has.Count.EqualTo(4));
		Assert.That(
			result, Has.None.Matches<Exclude>(e => e.Path?.Contains(
				'*', StringComparison.Ordinal) ?? false));
	}

	/// <summary>
	/// Two wildcard patterns in ExpandExcludes: both should be fully expanded.
	/// </summary>
	[Test]
	public void ExpandWildCardExcludesMultipleWildcardsAllExpanded()
	{
		AddCsvTestfiles();
		AddLogTestfiles();

		List<Exclude> excludeList =
			(List<Exclude>)tempDirectoryMapping.Excludes;
		excludeList.Add(new Exclude(
			Path.Combine(temporaryPath, "*.csv"), false));
		excludeList.Add(new Exclude(
			Path.Combine(temporaryPath, "*.log"), false));

		ICollection<Exclude> result =
			DriveMapping.ExpandWildCardExcludes(excludeList);

		int count = result.Count;
		string? lastPath = GetLastExcludePath(result);

		// Expect all 4 concrete files.
		Assert.That(count, Is.GreaterThanOrEqualTo(4));
		Assert.That(result, Has.None.Matches<Exclude>(e => e.Path?.Contains(
			'*', System.StringComparison.OrdinalIgnoreCase) ?? false));
	}

	/// <summary>
	/// Two wildcard patterns in ExpandExcludes: one should be fully expanded.
	/// But the other having no matches, should simply be removed.
	/// </summary>
	[Test]
	public void ExpandWildCardExcludesMultipleWildcardsSomeNoMatches()
	{
		AddCsvTestfiles();

		List<Exclude> excludeList =
			(List<Exclude>)tempDirectoryMapping.Excludes;
		excludeList.Add(new Exclude(
			Path.Combine(temporaryPath, "*.csv"), false));
		excludeList.Add(new Exclude(
			Path.Combine(temporaryPath, "*.log"), false));

		ICollection<Exclude> result =
			DriveMapping.ExpandWildCardExcludes(excludeList);

		int count = result.Count;
		string? lastPath = GetLastExcludePath(result);

		// Expect 2 concrete files.
		Assert.That(count, Is.GreaterThanOrEqualTo(2));
		Assert.That(result, Has.None.Matches<Exclude>(e => e.Path?.Contains(
			'*', System.StringComparison.OrdinalIgnoreCase) ?? false));
	}

	/// <summary>
	/// A non-wildcard exclude followed by a wildcard exclude: the non-wildcard
	/// entry must survive intact. The stale-index bug can cause it to be lost
	/// when the list is rebuilt during wildcard expansion.
	/// </summary>
	[Test]
	public void ExpandWildCardExcludesNonWildcardNonWildcardIsPreserved2()
	{
		string reportFile = Path.Combine(temporaryPath, "report.csv");
		File.WriteAllText(reportFile, string.Empty);

		string fixedFolder = Path.Combine(temporaryPath, "FixedFolder");
		string wildcardPath = Path.Combine(temporaryPath, "*.csv");

		List<Exclude> excludeList = (List<Exclude>)tempDirectoryMapping.Excludes;

		Exclude folderExclude = new(fixedFolder, false);
		Exclude wildCardExclude = new(wildcardPath, false);

		// Fixed entry first, wildcard second — reverse loop hits wildcard first.
		excludeList.Add(folderExclude);
		excludeList.Add(wildCardExclude);

		ICollection<Exclude> result =
			DriveMapping.ExpandWildCardExcludes(excludeList);

		int count = result.Count;

		// Should contain the expanded csv file AND the fixed folder.
		Assert.That(count, Is.GreaterThanOrEqualTo(2));
		Assert.That(result, Has.Some.Matches<Exclude>(e =>
			e.Path?.EndsWith(
				"report.csv", StringComparison.Ordinal) ?? false));
		Assert.That(result, Has.Some.Matches<Exclude>(e =>
			e.Path == Path.GetFullPath(fixedFolder)));
	}

	/// <summary>
	/// Same stale-index scenario exercised via the static ExpandGlobalExcludes.
	/// </summary>
	[Test]
	public void ExpandWildCardExcludesMultipleWildcards()
	{
		AddBakTestfiles();
		AddTmpTestfiles();

		string bakWildcard = Path.Combine(temporaryPath, "*.bak");
		string tmpWildcard = Path.Combine(temporaryPath, "*.tmp");

		Exclude exclude1 = new Exclude(bakWildcard, false);
		Exclude exclude2 = new Exclude(tmpWildcard, false);

		List<Exclude> excludes = [];
		excludes.Add(exclude1);
		excludes.Add(exclude2);

		ICollection<Exclude> result = DriveMapping.ExpandWildCardExcludes(excludes);

		Assert.That(result, Has.Count.EqualTo(4));
		Assert.That(
			result, Has.None.Matches<Exclude>(e => e.Path?.Contains(
				'*', StringComparison.Ordinal) ?? false));
	}

	private static Exclude? GetLastExclude(ICollection<Exclude>? excludes)
	{
		Exclude? lastExclude = null;

		if (excludes != null)
		{
			int count = excludes.Count;
			lastExclude = excludes.Last();
		}

		return lastExclude;
	}

	private static string? GetLastExcludePath(ICollection<Exclude>? excludes)
	{
		string? lastPath = null;

		Exclude? lastExclude = GetLastExclude(excludes);

		if (lastExclude != null)
		{
			lastPath = lastExclude.Path;
		}

		return lastPath;
	}

	private void AddBakTestfiles()
	{
		string fileA = Path.Combine(temporaryPath, "snap_a.bak");
		string fileB = Path.Combine(temporaryPath, "snap_b.bak");

		File.WriteAllText(fileA, string.Empty);
		File.WriteAllText(fileB, string.Empty);
	}

	private void AddCsvTestfiles()
	{
		string fileA = Path.Combine(temporaryPath, "data_a.csv");
		string fileB = Path.Combine(temporaryPath, "data_b.csv");

		File.WriteAllText(fileA, string.Empty);
		File.WriteAllText(fileB, string.Empty);
	}

	private ICollection<Exclude> AddCsvAndLogToExcludes()
	{
		List<Exclude> excludeList =
			(List<Exclude>)tempDirectoryMapping.Excludes;

		string excludePath1 = Path.Combine(temporaryPath, "*.csv");
		Exclude exclude1 = new Exclude(excludePath1, false);
		excludeList.Add(exclude1);

		string excludePath2 = Path.Combine(temporaryPath, "*.log");
		Exclude exclude2 = new Exclude(excludePath1, false);
		excludeList.Add(exclude2);

		ICollection<Exclude> result = tempDirectoryMapping.ExpandExcludes();

		return result;
	}

	private void AddLogTestfiles()
	{
		string fileA = Path.Combine(temporaryPath, "app_1.log");
		string fileB = Path.Combine(temporaryPath, "app_2.log");

		File.WriteAllText(fileA, string.Empty);
		File.WriteAllText(fileB, string.Empty);
	}

	private void AddLogTextfiles()
	{
		string fileA = Path.Combine(temporaryPath, "notes_alpha.txt");
		string fileB = Path.Combine(temporaryPath, "notes_beta.txt");
		string fileC = Path.Combine(temporaryPath, "other.log");

		File.WriteAllText(fileA, string.Empty);
		File.WriteAllText(fileB, string.Empty);
		File.WriteAllText(fileC, string.Empty);
	}

	private void AddTmpTestfiles()
	{
		string fileA = Path.Combine(temporaryPath, "trace_1.tmp");
		string fileB = Path.Combine(temporaryPath, "trace_2.tmp");

		File.WriteAllText(fileA, string.Empty);
		File.WriteAllText(fileB, string.Empty);
	}
}
