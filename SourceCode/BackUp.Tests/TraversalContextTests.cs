/////////////////////////////////////////////////////////////////////////////
// <copyright file="TraversalContextTests.cs" company="James John McGuire">
// Copyright © 2017 - 2026 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace BackUp.Tests;

using System;
using System.Collections.Generic;
using System.IO;
using DigitalZenWorks.BackUp.Library;
using NUnit.Framework;

[TestFixture]
public class TraversalContextTests
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

	private string root;
	private string clientsPath;
	private string objPath;

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
		root = Path.GetTempPath();
		clientsPath = Path.Combine(root, "Data", "Clients");
		objPath = Path.Combine(root, "Data", "obj");
	}

	// ------------------------------------------------------------------------
	// Basic matching
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
}
