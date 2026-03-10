/////////////////////////////////////////////////////////////////////////////
// <copyright file="TraversalContextTests.cs" company="James John McGuire">
// Copyright © 2017 - 2026 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.BackUp.Library.Tests;

using System;
using System.Collections.Generic;
using System.IO;
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
	private static readonly HashSet<ExcludeType> FolderTypes =
	[
		ExcludeType.SubDirectory,
		ExcludeType.Global,
	];

	private static readonly HashSet<ExcludeType> FileTypes =
	[
		ExcludeType.File,
		ExcludeType.FileIgnore,
	];

	private string clientsPath;
	private string existingDirectoryPath;
	private string existingFilePath;
	private string objPath;
	private string root;
	private string tempDirectory;

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
		tempDirectory = Path.Combine(
			Path.GetTempPath(), Path.GetRandomFileName());
		Directory.CreateDirectory(tempDirectory);

		existingDirectoryPath = Path.Combine(tempDirectory, "TestFolder");
		Directory.CreateDirectory(existingDirectoryPath);

		existingFilePath = Path.Combine(tempDirectory, "TestFile.txt");
		File.WriteAllText(existingFilePath, string.Empty);

		root = Path.GetTempPath();
		clientsPath = Path.Combine(root, "Data", "Clients");
		objPath = Path.Combine(root, "Data", "obj");
	}

	// ------------------------------------------------------------------------
	// IsExcludeMatch — basic matching
	// ------------------------------------------------------------------------

	/// <summary>
	/// Verifies that an exact path match with an allowed exclusion type
	/// returns true when evaluated by the traversal context.
	/// </summary>
	/// <remarks>This test ensures that the exclusion logic correctly
	/// identifies an exact match for a subdirectory path as a valid exclusion
	/// according to the specified rules.</remarks>
	[Test]
	public void ExactPathMatchAllowedTypeReturnsTrue()
	{
		Exclude exclude = new(clientsPath, ExcludeType.SubDirectory);

		bool result = TraversalContext.IsExcludeMatch(
			clientsPath, exclude, FolderTypes);

		Assert.That(result, Is.True);
	}

	/// <summary>
	/// Verifies that exclusion matching for paths is case-insensitive on
	/// Windows operating systems by asserting a match regardless of case
	/// differences.
	/// </summary>
	/// <remarks>This test ensures that the exclusion logic correctly
	/// identifies a path as matching even when the case differs, but only on
	/// Windows. On non-Windows systems, the match is expected to be
	/// case-sensitive and not succeed if the case does not match.</remarks>
	[Test]
	[System.Diagnostics.CodeAnalysis.SuppressMessage(
		"Globalization",
		"CA1308:Normalize strings to uppercase",
		Justification = "It's just a test.")]
	public void ExactPathMatchCaseInsensitiveOnWindowsReturnsTrue()
	{
		string clientsPathLower = clientsPath.ToLowerInvariant();
		Exclude exclude = new(clientsPathLower, ExcludeType.SubDirectory);

		bool result = TraversalContext.IsExcludeMatch(
			clientsPath.ToUpperInvariant(), exclude, FolderTypes);

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
	/// Verifies that the exclusion logic does not incorrectly match a path
	/// that should not be excluded when using subdirectory exclusion criteria.
	/// </summary>
	/// <remarks>This test ensures that the traversal context correctly
	/// identifies non-matching paths and does not exclude them based on the
	/// provided exclusion settings. It helps confirm that only intended paths
	/// are excluded during traversal.</remarks>
	[Test]
	public void DifferentPathReturnsTrue()
	{
		Exclude exclude = new(clientsPath, ExcludeType.SubDirectory);

		bool result = TraversalContext.IsExcludeMatch(
			objPath, exclude, FolderTypes);

		Assert.That(result, Is.False);
	}

	// ------------------------------------------------------------------------
	// ExcludeType filtering
	// ------------------------------------------------------------------------

	/// <summary>
	/// Verifies that the exclusion logic returns false when the path type does
	/// not match any of the allowed types.
	/// </summary>
	/// <remarks>This test ensures that a path, even if it matches the
	/// specified value, is not excluded unless its type is included in the
	/// allowed set. It confirms correct behavior of the exclusion filter when
	/// the type constraint is not satisfied.</remarks>
	[Test]
	public void ExactPathMatchTypeNotInAllowedSetReturnsFalse()
	{
		// Path matches but type is not in the allowed set
		Exclude exclude = new(clientsPath, ExcludeType.File);

		bool result = TraversalContext.IsExcludeMatch(
			clientsPath, exclude, FolderTypes);

		Assert.That(result, Is.False);
	}

	/// <summary>
	/// Verifies that the exclusion logic correctly identifies an exact path
	/// match for a file type in the allowed set and returns true.
	/// </summary>
	/// <remarks>This test ensures that when a file exclusion is defined for a
	/// specific path and file type, the matching logic recognizes the path as
	/// excluded. It is important for validating that the exclusion mechanism
	/// behaves as expected for exact path matches within the allowed file
	/// types.</remarks>
	[Test]
	public void ExactPathMatchFileTypeInFileAllowedSetReturnsTrue()
	{
		Exclude exclude = new(clientsPath, ExcludeType.File);

		bool result = TraversalContext.IsExcludeMatch(
			clientsPath, exclude, FileTypes);

		Assert.That(result, Is.True);
	}

	// ------------------------------------------------------------------------
	// Global excludes — name-only matching is caller's responsibility
	// ------------------------------------------------------------------------

	/// <summary>
	/// Verifies that a global exclusion correctly matches a fully qualified
	/// path in the traversal context.
	/// </summary>
	/// <remarks>This test ensures that global excludes are expanded to fully
	/// qualified paths before matching occurs. It is important for callers to
	/// provide global excludes in their fully qualified form to guarantee
	/// accurate exclusion behavior during traversal.</remarks>
	[Test]
	public void GlobalExcludeFullPathMatchReturnsTrue()
	{
		// By the time IsExcludeMatch is called, global excludes should
		// already be expanded to fully qualified paths by the traversal
		// pipeline — so a full path match is the expected case.
		Exclude exclude = new(objPath, ExcludeType.Global);

		bool result = TraversalContext.IsExcludeMatch(
			objPath, exclude, FolderTypes);

		Assert.That(result, Is.True);
	}

	/// <summary>
	/// Verifies that a global exclusion does not match a different path,
	/// ensuring the exclusion logic correctly distinguishes between excluded
	/// and non-excluded paths.
	/// </summary>
	/// <remarks>This test ensures that when a path is globally excluded, the
	/// exclusion does not erroneously apply to unrelated paths. This helps
	/// maintain the accuracy and reliability of the exclusion mechanism within
	/// the traversal context.</remarks>
	[Test]
	public void GlobalExcludeDifferentPathReturnsFalse()
	{
		Exclude exclude = new(objPath, ExcludeType.Global);

		bool result = TraversalContext.IsExcludeMatch(
			clientsPath, exclude, FolderTypes);

		Assert.That(result, Is.False);
	}

	// -------------------------------------------------------------------------
	// IsExcludeMatch — basic matching
	// -------------------------------------------------------------------------

	/// <summary>
	/// Verifies that the exclusion logic correctly identifies an exact path
	/// match as valid when the exclusion type is set to SubDirectory.
	/// </summary>
	/// <remarks>This test ensures that the IsExcludeMatch method returns true
	/// when the provided directory path exactly matches an exclusion of type
	/// SubDirectory. It confirms that the matching logic behaves as expected
	/// for this scenario.</remarks>
	[Test]
	public void IsExcludeMatchExactPathMatchAllowedTypeReturnsTrue()
	{
		Exclude exclude = new(
			existingDirectoryPath, ExcludeType.SubDirectory);

		bool result = TraversalContext.IsExcludeMatch(
			existingDirectoryPath, exclude, FolderTypes);

		Assert.That(result, Is.True);
	}

	/// <summary>
	/// Verifies that the exclusion match does not occur when the provided path
	/// differs from the specified exclusion path.
	/// </summary>
	/// <remarks>This test ensures that the exclusion logic correctly identifies
	/// paths outside the excluded
	/// directory and does not falsely match them. It helps validate that only
	/// the intended paths are excluded according to the exclusion criteria.
	/// </remarks>
	[Test]
	public void IsExcludeMatchDifferentPathReturnsFalse()
	{
		string otherPath = Path.Combine(tempDirectory, "OtherFolder");
		Exclude exclude = new(
			existingDirectoryPath, ExcludeType.SubDirectory);

		bool result = TraversalContext.IsExcludeMatch(
			otherPath, exclude, FolderTypes);

		Assert.That(result, Is.False);
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
	public void IsExcludeMatchCaseSensitivityReflectsOperatingSystem()
	{
		Exclude exclude = new(
			existingDirectoryPath.ToLowerInvariant(),
			ExcludeType.SubDirectory);

		bool result = TraversalContext.IsExcludeMatch(
			existingDirectoryPath.ToUpperInvariant(), exclude, FolderTypes);

		if (OperatingSystem.IsWindows())
		{
			Assert.That(result, Is.True);
		}
		else
		{
			Assert.That(result, Is.False);
		}
	}

	// -------------------------------------------------------------------------
	// IsExcludeMatch — ExcludeType filtering
	// -------------------------------------------------------------------------

	/// <summary>
	/// The is exclude match type not in allowed set return false test.
	/// </summary>
	[Test]
	public void IsExcludeMatchTypeNotInAllowedSetReturnsFalse()
	{
		Exclude exclude = new(existingFilePath, ExcludeType.File);

		bool result = TraversalContext.IsExcludeMatch(
			existingFilePath, exclude, FolderTypes);

		Assert.That(result, Is.False);
	}

	/// <summary>
	/// The is exclude match file type in file allowed set return true test.
	/// </summary>
	[Test]
	public void IsExcludeMatchFileTypeInFileAllowedSetReturnsTrue()
	{
		Exclude exclude = new(existingFilePath, ExcludeType.File);

		bool result = TraversalContext.IsExcludeMatch(
			existingFilePath, exclude, FileTypes);

		Assert.That(result, Is.True);
	}

	/// <summary>
	/// The is exclude match file ignore type in file allowed set return true
	/// test.
	/// </summary>
	[Test]
	public void IsExcludeMatchFileIgnoreTypeInFileAllowedSetReturnsTrue()
	{
		Exclude exclude = new(existingFilePath, ExcludeType.FileIgnore);

		bool result = TraversalContext.IsExcludeMatch(
			existingFilePath, exclude, FileTypes);

		Assert.That(result, Is.True);
	}

	// ------------------------------------------------------------------------
	// NormalizePath — existing paths
	// ------------------------------------------------------------------------

	/// <summary>
	/// The normalize path existing file returns fully qualified path test.
	/// </summary>
	[Test]
	public void NormalizePathExistingFileReturnsFullyQualifiedPath()
	{
		string? result =
			TraversalContext.NormalizePath(existingFilePath, tempDirectory);

		Assert.That(result, Is.Not.Null);
		Assert.That(Path.IsPathFullyQualified(result!), Is.True);
	}

	/// <summary>
	/// The normalize path existing directory returns fully qualified path
	/// test.
	/// </summary>
	[Test]
	public void NormalizePathExistingDirectoryReturnsFullyQualifiedPath()
	{
		string? result = TraversalContext.NormalizePath(
			existingDirectoryPath, tempDirectory);

		Assert.That(result, Is.Not.Null);
		Assert.That(Path.IsPathFullyQualified(result!), Is.True);
	}

	/// <summary>
	/// The normalize path already fully qualified returns same path test.
	/// </summary>
	[Test]
	public void NormalizePathAlreadyFullyQualifiedReturnsSamePath()
	{
		string? result = TraversalContext.NormalizePath(
			existingDirectoryPath, tempDirectory);

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
			Directory.SetCurrentDirectory(tempDirectory);
			string relativePath = "TestFolder";

			string? result =
				TraversalContext.NormalizePath(relativePath, tempDirectory);

			Assert.That(result, Is.Not.Null);
			Assert.That(Path.IsPathFullyQualified(result!), Is.True);
		}
		finally
		{
			Directory.SetCurrentDirectory(originalDirectory);
		}
	}
}
