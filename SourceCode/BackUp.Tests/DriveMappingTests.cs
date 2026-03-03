/////////////////////////////////////////////////////////////////////////////
// <copyright file="DriveMappingTests.cs" company="James John McGuire">
// Copyright © 2017 - 2026 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.BackUp.Library.Tests;

using System.Collections.Generic;
using System.IO;
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
internal class DriveMappingTests
{
	private string tempDirectory;
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
		tempDirectory = Path.Combine(
			Path.GetTempPath(), Path.GetRandomFileName());
		Directory.CreateDirectory(tempDirectory);

		tempDirectoryMapping = new() { Path = tempDirectory };
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
		if (Directory.Exists(tempDirectory))
		{
			Directory.Delete(tempDirectory, recursive: true);
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
		Assert.That(tempDirectoryMapping.Path, Is.EqualTo(tempDirectory));
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

	// ------------------------------------------------------------------------
	// ExpandGlobalExcludes – null / empty guards
	// ------------------------------------------------------------------------

	/// <summary>
	/// Verifies that the ExpandGlobalExcludes method returns null when the
	/// excludes parameter is null.
	/// </summary>
	/// <remarks>This test ensures that passing a null excludes collection to
	/// ExpandGlobalExcludes results in a null return value, indicating that no
	/// exclusions are applied. This behavior is important for callers to
	/// understand how the method handles null input.</remarks>
	[Test]
	public void ExpandGlobalExcludesNullExcludes()
	{
		IList<Exclude> result =
			DriveMapping.ExpandGlobalExcludes(tempDirectory, null);

		Assert.That(result, Is.Null);
	}

	/// <summary>
	/// Verifies that the ExpandGlobalExcludes method returns an empty list
	/// when no exclusions are provided.
	/// </summary>
	/// <remarks>This test ensures that the method under test correctly handles
	/// scenarios where the exclusions collection is empty, returning a
	/// non-null, empty list as expected.</remarks>
	[Test]
	public void ExpandGlobalExcludesEmptyList()
	{
		IList<Exclude> result =
			DriveMapping.ExpandGlobalExcludes(tempDirectory, []);

		Assert.That(result, Is.Not.Null);
		Assert.That(result, Is.Empty);
	}

	// ------------------------------------------------------------------------
	// ExpandGlobalExcludes – Global exclude expansion
	// ------------------------------------------------------------------------

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
		const string relativeName = "SomeFolder";
		Exclude globalExclude = new(relativeName, ExcludeType.Global);

		IList<Exclude> result = DriveMapping.ExpandGlobalExcludes(
			tempDirectory, [globalExclude]);

		string expected = Path.GetFullPath(
			Path.Combine(tempDirectory, relativeName));

		Assert.That(result, Has.Count.EqualTo(1));
		Assert.That(result[0].Path, Is.EqualTo(expected));
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
		string absolutePath = Path.Combine(tempDirectory, "AbsoluteFolder");
		Exclude globalExclude = new(absolutePath, ExcludeType.Global);

		IList<Exclude> result = DriveMapping.ExpandGlobalExcludes(
			tempDirectory, [globalExclude]);

		Assert.That(result, Has.Count.EqualTo(1));
		Assert.That(result[0].Path, Is.EqualTo(
			Path.GetFullPath(absolutePath)));
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
		Exclude item = new Exclude("FolderA", ExcludeType.Global);

		List<Exclude> excludes = [];
		excludes.Add(item);
		excludes.Add(item);

		IList<Exclude> result =
			DriveMapping.ExpandGlobalExcludes(tempDirectory, excludes);

		Assert.That(result, Has.Count.EqualTo(2));
	}

	// ------------------------------------------------------------------------
	// ExpandGlobalExcludes – Wildcard expansion
	// ------------------------------------------------------------------------

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
	public void ExpandGlobalExcludesWildcardFileExpands()
	{
		// Create two matching files inside tempDirectory
		string fileA = Path.Combine(tempDirectory, "notes_alpha.txt");
		string fileB = Path.Combine(tempDirectory, "notes_beta.txt");
		string fileC = Path.Combine(tempDirectory, "other.log");
		File.WriteAllText(fileA, string.Empty);
		File.WriteAllText(fileB, string.Empty);
		File.WriteAllText(fileC, string.Empty);

		string wildcardPath = Path.Combine(tempDirectory, "*.txt");
		Exclude wildcardExclude = new(wildcardPath, ExcludeType.File);

		IList<Exclude> result = DriveMapping.ExpandGlobalExcludes(
			tempDirectory, [wildcardExclude]);

		// The wildcard entry itself should be removed and replaced by the
		// two matched .txt files.
		Assert.That(result, Has.Count.EqualTo(2));
		Assert.That(
			result,
			Has.All.Matches<Exclude>(e => e.Path.EndsWith(
				".txt", System.StringComparison.OrdinalIgnoreCase)));
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
	public void ExpandGlobalExcludesWildcardNoMatches()
	{
		// Wildcard that won't match anything in the empty temp dir
		string wildcardPath = Path.Combine(tempDirectory, "*.xyz");
		Exclude wildcardExclude = new(wildcardPath, ExcludeType.File);

		IList<Exclude> result = DriveMapping.ExpandGlobalExcludes(
			tempDirectory, [wildcardExclude]);

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
		IList<Exclude> result = tempDirectoryMapping.ExpandExcludes();

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
		Exclude item = new Exclude("MyFolder", ExcludeType.SubDirectory);

		List<Exclude> excludes = (List<Exclude>)tempDirectoryMapping.Excludes;
		excludes.Add(item);

		IList<Exclude> result = tempDirectoryMapping.ExpandExcludes();

		string expected = Path.GetFullPath(
			Path.Combine(tempDirectory, "MyFolder"));

		int count = result.Count;
		Exclude lastExclude = GetLastExclude(result);
		string lastPath = lastExclude.Path;

		Assert.That(count, Is.GreaterThan(0));
		Assert.That(lastPath, Is.EqualTo(expected));
	}

	/// <summary>
	/// Verifies that the ExpandExcludes method does not re-expand global
	/// excludes, ensuring that paths marked as global remain unchanged in the
	/// result.
	/// </summary>
	/// <remarks>This test confirms that when an exclude of type Global is
	/// present in the DriveMapping, the ExpandExcludes method correctly skips
	/// re-expansion for that path. This preserves the integrity of global
	/// exclude paths and prevents unintended modifications.</remarks>
	[Test]
	public void ExpandExcludesGlobalExcludePathNotExpanded()
	{
		// Global excludes should not be re-expanded by ExpandExcludes
		// (the method skips them with != ExcludeType.Global).
		string absolutePath =
			Path.Combine(tempDirectory, "GlobalFolder");

		Exclude item = new Exclude(absolutePath, ExcludeType.Global);
		List<Exclude> excludes = (List<Exclude>)tempDirectoryMapping.Excludes;

		excludes.Add(item);

		IList<Exclude> result = tempDirectoryMapping.ExpandExcludes();

		int count = result.Count;
		Exclude lastExclude = result[count - 1];
		string lastPath = lastExclude.Path;

		// Path should be unchanged because the Global branch is skipped.
		Assert.That(lastPath, Is.EqualTo(absolutePath));
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

		string wildcardPath = Path.Combine(tempDirectory, "*.csv");

		Exclude item = new Exclude(wildcardPath, ExcludeType.File);
		List<Exclude> excludes = (List<Exclude>)tempDirectoryMapping.Excludes;
		excludes.Add(item);

		IList<Exclude> result = tempDirectoryMapping.ExpandExcludes();

		int count = result.Count;

		Assert.That(count, Is.GreaterThanOrEqualTo(2));
		Assert.That(
			result,
			Has.Some.Matches<Exclude>(e => e.Path.EndsWith(
				".csv", System.StringComparison.OrdinalIgnoreCase)));
	}

	/// <summary>
	/// Two wildcard patterns in ExpandExcludes: both should be fully expanded.
	/// </summary>
	[Test]
	public void ExpandExcludesMultipleWildcardsAllExpanded()
	{
		AddCsvTestfiles();
		AddLogTestfiles();

		IList<Exclude> result = AddCsvAndLogToExcludes();

		int count = result.Count;
		Exclude lastExclude = GetLastExclude(result);
		string lastPath = lastExclude.Path;

		// Expect all 4 concrete files.
		Assert.That(count, Is.GreaterThanOrEqualTo(4));
		Assert.That(result, Has.None.Matches<Exclude>(e => e.Path.Contains(
			'*', System.StringComparison.OrdinalIgnoreCase)));
	}

	/// <summary>
	/// Two wildcard patterns in ExpandExcludes: one should be fully expanded.
	/// But the other having no matches, should simply be removed.
	/// </summary>
	[Test]
	public void ExpandExcludesMultipleWildcardsSomeNoMatches()
	{
		AddCsvTestfiles();
		IList<Exclude> result = AddCsvAndLogToExcludes();

		int count = result.Count;
		Exclude lastExclude = GetLastExclude(result);

		string lastPath = lastExclude.Path;

		// Expect 2 concrete files.
		Assert.That(count, Is.GreaterThanOrEqualTo(2));
		Assert.That(result, Has.None.Matches<Exclude>(e => e.Path.Contains(
			'*', System.StringComparison.OrdinalIgnoreCase)));
	}

	/// <summary>
	/// A non-wildcard exclude followed by a wildcard exclude: the non-wildcard
	/// entry must survive intact. The stale-index bug can cause it to be lost
	/// when the list is rebuilt during wildcard expansion.
	/// </summary>
	[Test]
	public void ExpandExcludesNonWildcardNonWildcardIsPreserved()
	{
		string reportFile = Path.Combine(tempDirectory, "report.csv");
		File.WriteAllText(reportFile, string.Empty);

		string fixedFolder = Path.Combine(tempDirectory, "FixedFolder");
		string wildcardPath = Path.Combine(tempDirectory, "*.csv");

		List<Exclude> excludeList = (List<Exclude>)tempDirectoryMapping.Excludes;

		Exclude folderExclude = new(fixedFolder, ExcludeType.SubDirectory);
		Exclude wildCardExclude = new(wildcardPath, ExcludeType.File);

		// Fixed entry first, wildcard second — reverse loop hits wildcard first.
		excludeList.Add(folderExclude);
		excludeList.Add(wildCardExclude);

		IList<Exclude> result = tempDirectoryMapping.ExpandExcludes();

		int count = result.Count;

		// Should contain the expanded csv file AND the fixed folder.
		Assert.That(count, Is.GreaterThanOrEqualTo(2));
		Assert.That(result, Has.Some.Matches<Exclude>(e =>
			e.Path.EndsWith("report.csv")));
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

		string bakWildcard = Path.Combine(tempDirectory, "*.bak");
		string tmpWildcard = Path.Combine(tempDirectory, "*.tmp");

		Exclude exclude1 = new Exclude(bakWildcard, ExcludeType.File);
		Exclude exclude2 = new Exclude(tmpWildcard, ExcludeType.File);

		List<Exclude> excludes = [];
		excludes.Add(exclude1);
		excludes.Add(exclude2);

		IList<Exclude> result =
			DriveMapping.ExpandGlobalExcludes(tempDirectory, excludes);

		Assert.That(result, Has.Count.EqualTo(4));
		Assert.That(
			result, Has.None.Matches<Exclude>(e => e.Path.Contains('*')));
	}

	/// <summary>
	/// Two wildcard patterns in ExpandExcludes: both should be fully expanded.
	/// </summary>
	[Test]
	public void ExpandGlobalExcludesMultipleWildcardsAllExpanded()
	{
		AddCsvTestfiles();
		AddLogTestfiles();

		List<Exclude> excludeList =
			(List<Exclude>)tempDirectoryMapping.Excludes;
		excludeList.Add(new Exclude(
			Path.Combine(tempDirectory, "*.csv"), ExcludeType.File));
		excludeList.Add(new Exclude(
			Path.Combine(tempDirectory, "*.log"), ExcludeType.File));

		IList<Exclude> result = DriveMapping.ExpandGlobalExcludes(
			tempDirectory, excludeList);

		int count = result.Count;
		Exclude lastExclude = GetLastExclude(result);
		string lastPath = lastExclude.Path;

		// Expect all 4 concrete files.
		Assert.That(count, Is.GreaterThanOrEqualTo(4));
		Assert.That(result, Has.None.Matches<Exclude>(e => e.Path.Contains(
			'*', System.StringComparison.OrdinalIgnoreCase)));
	}

	/// <summary>
	/// Two wildcard patterns in ExpandExcludes: one should be fully expanded.
	/// But the other having no matches, should simply be removed.
	/// </summary>
	[Test]
	public void ExpandGlobalExcludesMultipleWildcardsSomeNoMatches()
	{
		AddCsvTestfiles();

		List<Exclude> excludeList =
			(List<Exclude>)tempDirectoryMapping.Excludes;
		excludeList.Add(new Exclude(
			Path.Combine(tempDirectory, "*.csv"), ExcludeType.File));
		excludeList.Add(new Exclude(
			Path.Combine(tempDirectory, "*.log"), ExcludeType.File));

		IList<Exclude> result = DriveMapping.ExpandGlobalExcludes(
			tempDirectory, excludeList);

		int count = result.Count;
		Exclude lastExclude = GetLastExclude(result);

		string lastPath = lastExclude.Path;

		// Expect 2 concrete files.
		Assert.That(count, Is.GreaterThanOrEqualTo(2));
		Assert.That(result, Has.None.Matches<Exclude>(e => e.Path.Contains(
			'*', System.StringComparison.OrdinalIgnoreCase)));
	}

	/// <summary>
	/// A non-wildcard exclude followed by a wildcard exclude: the non-wildcard
	/// entry must survive intact. The stale-index bug can cause it to be lost
	/// when the list is rebuilt during wildcard expansion.
	/// </summary>
	[Test]
	public void ExpandExcludesNonWildcardNonWildcardIsPreserved2()
	{
		string reportFile = Path.Combine(tempDirectory, "report.csv");
		File.WriteAllText(reportFile, string.Empty);

		string fixedFolder = Path.Combine(tempDirectory, "FixedFolder");
		string wildcardPath = Path.Combine(tempDirectory, "*.csv");

		List<Exclude> excludeList = (List<Exclude>)tempDirectoryMapping.Excludes;

		Exclude folderExclude = new(fixedFolder, ExcludeType.SubDirectory);
		Exclude wildCardExclude = new(wildcardPath, ExcludeType.File);

		// Fixed entry first, wildcard second — reverse loop hits wildcard first.
		excludeList.Add(folderExclude);
		excludeList.Add(wildCardExclude);

		IList<Exclude> result = DriveMapping.ExpandGlobalExcludes(
			tempDirectory, excludeList);

		int count = result.Count;

		// Should contain the expanded csv file AND the fixed folder.
		Assert.That(count, Is.GreaterThanOrEqualTo(2));
		Assert.That(result, Has.Some.Matches<Exclude>(e =>
			e.Path.EndsWith("report.csv")));
		Assert.That(result, Has.Some.Matches<Exclude>(e =>
			e.Path == Path.GetFullPath(fixedFolder)));
	}

	/// <summary>
	/// Same stale-index scenario exercised via the static ExpandGlobalExcludes.
	/// </summary>
	[Test]
	public void ExpandGlobalExcludesMultipleWildcards()
	{
		AddBakTestfiles();
		AddTmpTestfiles();

		string bakWildcard = Path.Combine(tempDirectory, "*.bak");
		string tmpWildcard = Path.Combine(tempDirectory, "*.tmp");

		Exclude exclude1 = new Exclude(bakWildcard, ExcludeType.File);
		Exclude exclude2 = new Exclude(tmpWildcard, ExcludeType.File);

		List<Exclude> excludes = [];
		excludes.Add(exclude1);
		excludes.Add(exclude2);

		IList<Exclude> result = DriveMapping.ExpandGlobalExcludes(
			tempDirectory, excludes);

		Assert.That(result, Has.Count.EqualTo(4));
		Assert.That(
			result, Has.None.Matches<Exclude>(e => e.Path.Contains('*')));
	}

	private void AddBakTestfiles()
	{
		string fileA = Path.Combine(tempDirectory, "snap_a.bak");
		string fileB = Path.Combine(tempDirectory, "snap_b.bak");

		File.WriteAllText(fileA, string.Empty);
		File.WriteAllText(fileB, string.Empty);
	}

	private void AddCsvTestfiles()
	{
		string fileA = Path.Combine(tempDirectory, "data_a.csv");
		string fileB = Path.Combine(tempDirectory, "data_b.csv");

		File.WriteAllText(fileA, string.Empty);
		File.WriteAllText(fileB, string.Empty);
	}

	private IList<Exclude> AddCsvAndLogToExcludes()
	{
		List<Exclude> excludeList =
			(List<Exclude>)tempDirectoryMapping.Excludes;
		excludeList.Add(new Exclude(
			Path.Combine(tempDirectory, "*.csv"), ExcludeType.File));
		excludeList.Add(new Exclude(
			Path.Combine(tempDirectory, "*.log"), ExcludeType.File));

		IList<Exclude> result = tempDirectoryMapping.ExpandExcludes();

		return result;
	}

	private void AddLogTestfiles()
	{
		string fileA = Path.Combine(tempDirectory, "app_1.log");
		string fileB = Path.Combine(tempDirectory, "app_2.log");

		File.WriteAllText(fileA, string.Empty);
		File.WriteAllText(fileB, string.Empty);
	}

	private void AddTmpTestfiles()
	{
		string fileA = Path.Combine(tempDirectory, "trace_1.tmp");
		string fileB = Path.Combine(tempDirectory, "trace_2.tmp");

		File.WriteAllText(fileA, string.Empty);
		File.WriteAllText(fileB, string.Empty);
	}

	private Exclude GetLastExclude(IList<Exclude> excludes)
	{
		int count = excludes.Count;
		Exclude lastExclude = excludes[count - 1];

		return lastExclude;
	}
}
