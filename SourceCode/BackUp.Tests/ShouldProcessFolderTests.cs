/////////////////////////////////////////////////////////////////////////////
// <copyright file="ShouldProcessFolderTests.cs" company="James John McGuire">
// Copyright © 2017 - 2026 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.BackUp.Library.Tests;

using System.Collections.Generic;
using System.IO;
using DigitalZenWorks.BackUp.Library;
using NUnit.Framework;

/// <summary>
/// Provides unit tests for the ShouldProcessFolder method under various
/// scenarios.
/// </summary>
[TestFixture]
internal class ShouldProcessFolderTests
{
	private string root;
	private string dataPath;
	private string clientsPath;
	private string objPath;
	private string nodeModulesPath;

	/// <summary>
	/// Initializes various paths for use in each test run.
	/// </summary>
	[SetUp]
	public void SetUp()
	{
		// Build OS-agnostic paths from the temp directory root
		root = Path.GetTempPath();
		dataPath = Path.Combine(root, "Data");
		clientsPath = Path.Combine(dataPath, "Clients");
		objPath = Path.Combine(dataPath, "obj");
		nodeModulesPath = Path.Combine(dataPath, "node_modules");
	}

	// ------------------------------------------------------------------------
	// Null / empty guards — always permissive
	// ------------------------------------------------------------------------

	/// <summary>
	/// The null excludes returns true test.
	/// </summary>
	[Test]
	public void NullExcludesReturnsTrue()
	{
		bool result = BaseService.ShouldProcessFolder(null, clientsPath);

		Assert.That(result, Is.True);
	}

	/// <summary>
	/// The empty excludes returns true test.
	/// </summary>
	[Test]
	public void EmptyExcludesReturnsTrue()
	{
		bool result = BaseService.ShouldProcessFolder([], clientsPath);

		Assert.That(result, Is.True);
	}

	/// <summary>
	/// The null path returns true test.
	/// </summary>
	[Test]
	public void NullPathReturnsTrue()
	{
		ICollection<Exclude> excludes = [];

		Exclude exclude = new("anything", ExcludeType.SubDirectory);
		excludes.Add(exclude);

		bool result = BaseService.ShouldProcessFolder(excludes, null);

		Assert.That(result, Is.True);
	}

	/// <summary>
	/// The empty path return true test.
	/// </summary>
	[Test]
	public void EmptyPathReturnsTrue()
	{
		ICollection<Exclude> excludes = [];

		Exclude exclude = new("anything", ExcludeType.SubDirectory);
		excludes.Add(exclude);

		bool result = BaseService.ShouldProcessFolder(excludes, string.Empty);

		Assert.That(result, Is.True);
	}

	/// <summary>
	/// The nulll exclude entry is skipped, returns true test.
	/// </summary>
	[Test]
	public void NullExcludeEntryIsSkippedReturnsTrue()
	{
		ICollection<Exclude> excludes = [];
		excludes.Add(null);

		bool result = BaseService.ShouldProcessFolder(excludes, clientsPath);

		Assert.That(result, Is.True);
	}

	/// <summary>
	/// The exclude with null path is skipped, returns true.
	/// </summary>
	[Test]
	public void ExcludeWithNullPathIsSkippedReturnsTrue()
	{
		Exclude exclude = new(null, ExcludeType.SubDirectory);

		ICollection<Exclude> excludes = [];
		excludes.Add(exclude);

		bool result = BaseService.ShouldProcessFolder(excludes, clientsPath);

		Assert.That(result, Is.True);
	}

	// ------------------------------------------------------------------------
	// Fully qualified exclude vs fully qualified path
	// ------------------------------------------------------------------------

	/// <summary>
	/// The fully qualified exclude exact match return false test.
	/// </summary>
	[Test]
	public void FullyQualifiedExcludeExactMatchReturnsFalse()
	{
		ICollection<Exclude> excludes = [];

		Exclude exclude = new(clientsPath, ExcludeType.SubDirectory);
		excludes.Add(exclude);

		bool result = BaseService.ShouldProcessFolder(excludes, clientsPath);

		Assert.That(result, Is.False);
	}

	/// <summary>
	/// The fully qualified exclude case insensitive match returns false test.
	/// </summary>
	[Test]
	[System.Diagnostics.CodeAnalysis.SuppressMessage(
		"Globalization",
		"CA1308:Normalize strings to uppercase",
		Justification = "It's just a test.")]
	public void FullyQualifiedExcludeCaseInsensitiveMatchReturnsFalse()
	{
		ICollection<Exclude> excludes = [];

		string path = clientsPath.ToLowerInvariant();
		Exclude exclude = new(path, ExcludeType.SubDirectory);
		excludes.Add(exclude);

		string checkPath = clientsPath.ToUpperInvariant();
		bool result = BaseService.ShouldProcessFolder(excludes, checkPath);

		Assert.That(result, Is.False);
	}

	/// <summary>
	/// The fully qualified exclude different path returns true test.
	/// </summary>
	[Test]
	public void FullyQualifiedExcludeDifferentPathReturnsTrue()
	{
		ICollection<Exclude> excludes = [];

		Exclude exclude = new(clientsPath, ExcludeType.SubDirectory);
		excludes.Add(exclude);

		bool result = BaseService.ShouldProcessFolder(excludes, objPath);

		Assert.That(result, Is.True);
	}

	// ------------------------------------------------------------------------
	// Relative exclude vs fully qualified path — name-only match
	// Covers the "obj anywhere in the tree" requirement
	// ------------------------------------------------------------------------

	/// <summary>
	/// The relative exclude matches folder name at any depth, returns false
	/// test.
	/// </summary>
	[Test]
	public void RelativeExcludeMatchesFolderNameAtAnyDepthReturnsFalse()
	{
		// "obj" should match regardless of where it appears in the tree
		string deepObjPath = Path.Combine(
			dataPath, "Projects", "MyApp", "obj");
		ICollection<Exclude> excludes = [];

		Exclude exclude = new("obj", ExcludeType.Global);
		excludes.Add(exclude);

		bool result = BaseService.ShouldProcessFolder(excludes, deepObjPath);

		Assert.That(result, Is.False);
	}

	/// <summary>
	/// The relative exclude matches shallow folder, returns false test.
	/// </summary>
	[Test]
	public void RelativeExcludeMatchesShallowFolderReturnsFalse()
	{
		ICollection<Exclude> excludes = [];

		Exclude exclude = new("obj", ExcludeType.Global);
		excludes.Add(exclude);

		bool result = BaseService.ShouldProcessFolder(excludes, objPath);

		Assert.That(result, Is.False);
	}

	/// <summary>
	/// The relative exclude no name matches returns false test.
	/// </summary>
	[Test]
	public void RelativeExcludeNoNameMatchReturnsTrue()
	{
		ICollection<Exclude> excludes = [];

		Exclude exclude = new("obj", ExcludeType.Global);
		excludes.Add(exclude);

		bool result = BaseService.ShouldProcessFolder(excludes, clientsPath);

		Assert.That(result, Is.True);
	}

	/// <summary>
	/// The relative exclude partial name matches returns true test.
	/// </summary>
	[Test]
	public void RelativeExcludePartialNameMatchReturnsTrue()
	{
		// "obj" should not match "objstore" or "myobj"
		string partialMatch = Path.Combine(dataPath, "objstore");
		ICollection<Exclude> excludes = [];

		Exclude exclude = new("obj", ExcludeType.Global);
		excludes.Add(exclude);

		bool result = BaseService.ShouldProcessFolder(excludes, partialMatch);

		Assert.That(result, Is.True);
	}

	// ------------------------------------------------------------------------
	// Permissive cases
	// ------------------------------------------------------------------------

	/// <summary>
	/// The relative path with qualified exclude is permissive returns true
	/// test.
	/// </summary>
	[Test]
	public void RelativePathWithQualifiedExcludeIsPermissiveReturnsTrue()
	{
		// Cannot definitively match — must allow
		string relativePath = Path.Combine("Data", "Clients");
		ICollection<Exclude> excludes = [];

		Exclude exclude = new(clientsPath, ExcludeType.SubDirectory);
		excludes.Add(exclude);

		bool result = BaseService.ShouldProcessFolder(excludes, relativePath);

		Assert.That(result, Is.True);
	}

	/// <summary>
	/// The relative path with relative exclude returns false test.
	/// </summary>
	[Test]
	public void RelativePathWithRelativeExcludeReturnsFalse()
	{
		ICollection<Exclude> excludes = [];

		Exclude exclude = new("obj", ExcludeType.Global);
		excludes.Add(exclude);

		string relativePath = Path.Combine("Data", "obj");

		bool result = BaseService.ShouldProcessFolder(excludes, relativePath);

		Assert.That(result, Is.False);
	}

	// ------------------------------------------------------------------------
	// ExcludeType filtering
	// ------------------------------------------------------------------------

	/// <summary>
	/// The exclude type file is ignored returns true test.
	/// </summary>
	[Test]
	public void ExcludeTypeFileIsIgnoredReturnsTrue()
	{
		ICollection<Exclude> excludes = [];

		Exclude exclude = new(clientsPath, ExcludeType.File);
		excludes.Add(exclude);

		bool result = BaseService.ShouldProcessFolder(excludes, clientsPath);

		Assert.That(result, Is.True);
	}

	/// <summary>
	/// The exclude type fileIgnore is ignored returns true test.
	/// </summary>
	[Test]
	public void ExcludeTypeFileIgnoreIsIgnoredReturnsTrue()
	{
		ICollection<Exclude> excludes = [];

		Exclude exclude = new(clientsPath, ExcludeType.FileIgnore);
		excludes.Add(exclude);

		bool result = BaseService.ShouldProcessFolder(excludes, clientsPath);

		Assert.That(result, Is.True);
	}

	/// <summary>
	/// The exclude type sub-directory is respected returns false test.
	/// </summary>
	[Test]
	public void ExcludeTypeSubDirectoryIsRespectedReturnsFalse()
	{
		ICollection<Exclude> excludes = [];

		Exclude exclude = new(clientsPath, ExcludeType.SubDirectory);
		excludes.Add(exclude);

		bool result = BaseService.ShouldProcessFolder(excludes, clientsPath);

		Assert.That(result, Is.False);
	}

	// ------------------------------------------------------------------------
	// Multiple excludes
	// ------------------------------------------------------------------------

	/// <summary>
	/// Multiple excludes one matches returns false test.
	/// </summary>
	[Test]
	public void MultipleExcludesOneMatchesReturnsFalse()
	{
		ICollection<Exclude> excludes = [];

		Exclude exclude1 = new("obj", ExcludeType.Global);
		Exclude exclude2 = new("node_modules", ExcludeType.Global);
		Exclude exclude3 = new(".vs", ExcludeType.Global);

		excludes.Add(exclude1);
		excludes.Add(exclude2);
		excludes.Add(exclude3);

		bool result =
			BaseService.ShouldProcessFolder(excludes, nodeModulesPath);

		Assert.That(result, Is.False);
	}

	/// <summary>
	/// The multiple excludes none matches returns true test.
	/// </summary>
	[Test]
	public void MultipleExcludesNoneMatchReturnsTrue()
	{
		ICollection<Exclude> excludes = [];

		Exclude exclude1 = new("obj", ExcludeType.Global);
		Exclude exclude2 = new("node_modules", ExcludeType.Global);
		Exclude exclude3 = new(".vs", ExcludeType.Global);

		excludes.Add(exclude1);
		excludes.Add(exclude2);
		excludes.Add(exclude3);

		bool result = BaseService.ShouldProcessFolder(excludes, clientsPath);

		Assert.That(result, Is.True);
	}

	/// <summary>
	/// The multiple excludes mixed types only relevant types match test.
	/// </summary>
	[Test]
	public void MultipleExcludesMixedTypesOnlyRelevantTypesMatch()
	{
		ICollection<Exclude> excludes = [];

		// File and FileIgnore excludes for clientsPath should not
		// prevent the folder from being processed
		Exclude exclude1 = new(clientsPath, ExcludeType.File);
		Exclude exclude2 = new(clientsPath, ExcludeType.FileIgnore);
		Exclude exclude3 = new("obj", ExcludeType.Global);

		excludes.Add(exclude1);
		excludes.Add(exclude2);
		excludes.Add(exclude3);

		bool result = BaseService.ShouldProcessFolder(excludes, clientsPath);

		Assert.That(result, Is.True);
	}

	// ------------------------------------------------------------------------
	// Subtree exclusion — documents caller responsibility
	// ------------------------------------------------------------------------

	/// <summary>
	/// The relative excludes sub-directory excludes caller responsibility
	/// test.
	/// </summary>
	[Test]
	public void RelativeExcludeSubfolderOfExcludedCallerResponsibility()
	{
		// This test documents that ShouldProcessFolder evaluates each
		// folder in isolation. A subfolder of an excluded folder is NOT
		// automatically excluded — the caller must honour the false result
		// on the parent and not recurse into it.
		string subFolder = Path.Combine(objPath, "Debug", "net8.0");
		ICollection<Exclude> excludes = [];

		Exclude exclude = new("obj", ExcludeType.Global);
		excludes.Add(exclude);

		bool result = BaseService.ShouldProcessFolder(excludes, subFolder);

		// The subfolder itself doesn't match "obj" by name,
		// so this returns true — subtree skipping is the caller's job.
		string message = "Subtree exclusion is the caller's " +
			"responsibility, not this method's.";

		Assert.That(result, Is.True, message);
	}
}
