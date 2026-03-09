/////////////////////////////////////////////////////////////////////////////
// <copyright file="ShouldSkipThisDirectoryTests.cs" company="James John McGuire">
// Copyright © 2017 - 2026 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.BackUp.Library.Tests;

using System.Collections.Generic;
using System.IO;
using DigitalZenWorks.BackUp.Library;
using NUnit.Framework;

/// <summary>
/// Provides unit tests for the ShouldSkipThisDirectory method under various
/// scenarios.
/// </summary>
[TestFixture]
internal class ShouldSkipThisDirectoryTests
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
	/// The null excludes returns false test.
	/// </summary>
	[Test]
	public void NullExcludesReturnsTrue()
	{
		bool result = BaseService.ShouldSkipThisDirectory(clientsPath, null);

		Assert.That(result, Is.False);
	}

	/// <summary>
	/// The empty excludes returns false test.
	/// </summary>
	[Test]
	public void EmptyExcludesReturnsTrue()
	{
		bool result = BaseService.ShouldSkipThisDirectory(clientsPath, []);

		Assert.That(result, Is.False);
	}

	// ------------------------------------------------------------------------
	// Fully qualified exclude vs fully qualified path
	// ------------------------------------------------------------------------

	/// <summary>
	/// The fully qualified exclude exact match return true test.
	/// </summary>
	[Test]
	public void FullyQualifiedExcludeExactMatchReturnsTrue()
	{
		ICollection<Exclude> excludes = [];

		Exclude exclude = new(clientsPath, ExcludeType.Keep);
		excludes.Add(exclude);

		bool result =
			BaseService.ShouldSkipThisDirectory(clientsPath, excludes);

		Assert.That(result, Is.True);
	}

	/// <summary>
	/// The fully qualified exclude case insensitive match returns true test.
	/// </summary>
	[Test]
	[System.Diagnostics.CodeAnalysis.SuppressMessage(
		"Globalization",
		"CA1308:Normalize strings to uppercase",
		Justification = "It's just a test.")]
	public void FullyQualifiedExcludeCaseInsensitiveMatchReturnsTrue()
	{
		ICollection<Exclude> excludes = [];

		string path = clientsPath.ToLowerInvariant();
		Exclude exclude = new(path, ExcludeType.Keep);
		excludes.Add(exclude);

		string checkPath = clientsPath.ToUpperInvariant();
		bool result =
			BaseService.ShouldSkipThisDirectory(checkPath, excludes);

		Assert.That(result, Is.True);
	}

	/// <summary>
	/// The fully qualified exclude different path returns false test.
	/// </summary>
	[Test]
	public void FullyQualifiedExcludeDifferentPathReturnsFalse()
	{
		ICollection<Exclude> excludes = [];

		Exclude exclude = new(clientsPath, ExcludeType.Keep);
		excludes.Add(exclude);

		bool result =
			BaseService.ShouldSkipThisDirectory(objPath, excludes);

		Assert.That(result, Is.False);
	}

	/// <summary>
	/// The relative exclude no name matches returns false test.
	/// </summary>
	[Test]
	public void RelativeExcludeNoNameMatchReturnsFalse()
	{
		ICollection<Exclude> excludes = [];

		Exclude exclude = new("obj", ExcludeType.Keep);
		excludes.Add(exclude);

		bool result =
			BaseService.ShouldSkipThisDirectory(clientsPath, excludes);

		Assert.That(result, Is.False);
	}

	/// <summary>
	/// The relative exclude partial name matches returns false test.
	/// </summary>
	[Test]
	public void RelativeExcludePartialNameMatchReturnsFalse()
	{
		// "obj" should not match "objstore" or "myobj"
		string partialMatch = Path.Combine(dataPath, "objstore");
		ICollection<Exclude> excludes = [];

		Exclude exclude = new("obj", ExcludeType.Keep);
		excludes.Add(exclude);

		bool result =
			BaseService.ShouldSkipThisDirectory(partialMatch, excludes);

		Assert.That(result, Is.False);
	}

	// ------------------------------------------------------------------------
	// Permissive cases
	// ------------------------------------------------------------------------

	/// <summary>
	/// The relative path with qualified exclude is permissive returns false
	/// test.
	/// </summary>
	[Test]
	public void RelativePathWithQualifiedExcludeIsPermissiveReturnsFalse()
	{
		// Cannot definitively match — must allow
		string relativePath = Path.Combine("Data", "Clients");
		ICollection<Exclude> excludes = [];

		Exclude exclude = new(clientsPath, ExcludeType.Keep);
		excludes.Add(exclude);

		bool result =
			BaseService.ShouldSkipThisDirectory(relativePath, excludes);

		Assert.That(result, Is.False);
	}

	// ------------------------------------------------------------------------
	// ExcludeType filtering
	// ------------------------------------------------------------------------

	/// <summary>
	/// The exclude type file is ignored returns false test.
	/// </summary>
	[Test]
	public void ExcludeTypeFileIsIgnoredReturnsFalse()
	{
		ICollection<Exclude> excludes = [];

		Exclude exclude = new(clientsPath, ExcludeType.SubDirectory);
		excludes.Add(exclude);

		bool result =
			BaseService.ShouldSkipThisDirectory(clientsPath, excludes);

		Assert.That(result, Is.False);
	}

	/// <summary>
	/// The exclude type fileIgnore is ignored returns false test.
	/// </summary>
	[Test]
	public void ExcludeTypeFileIgnoreIsIgnoredReturnsFalse()
	{
		ICollection<Exclude> excludes = [];

		Exclude exclude = new(clientsPath, ExcludeType.Global);
		excludes.Add(exclude);

		bool result =
			BaseService.ShouldSkipThisDirectory(clientsPath, excludes);

		Assert.That(result, Is.False);
	}

	/// <summary>
	/// The exclude type sub-directory is respected returns true test.
	/// </summary>
	[Test]
	public void ExcludeTypeSubDirectoryIsRespectedReturnsTrue()
	{
		ICollection<Exclude> excludes = [];

		Exclude exclude = new(clientsPath, ExcludeType.Keep);
		excludes.Add(exclude);

		bool result =
			BaseService.ShouldSkipThisDirectory(clientsPath, excludes);

		Assert.That(result, Is.True);
	}

	// ------------------------------------------------------------------------
	// Multiple excludes
	// ------------------------------------------------------------------------

	/// <summary>
	/// Multiple excludes one matches returns true test.
	/// </summary>
	[Test]
	public void MultipleExcludesOneMatchesReturnsTrue()
	{
		ICollection<Exclude> excludes = [];

		Exclude exclude1 = new(objPath, ExcludeType.Keep);
		Exclude exclude2 = new(nodeModulesPath, ExcludeType.Keep);

		string vsPath = Path.Combine(dataPath, ".vs");
		Exclude exclude3 = new(vsPath, ExcludeType.Keep);

		excludes.Add(exclude1);
		excludes.Add(exclude2);
		excludes.Add(exclude3);

		bool result =
			BaseService.ShouldSkipThisDirectory(nodeModulesPath, excludes);

		Assert.That(result, Is.True);
	}

	/// <summary>
	/// The multiple excludes none matches returns false test.
	/// </summary>
	[Test]
	public void MultipleExcludesNoneMatchReturnsFalse()
	{
		ICollection<Exclude> excludes = [];

		Exclude exclude1 = new(objPath, ExcludeType.Keep);
		Exclude exclude2 = new(nodeModulesPath, ExcludeType.Keep);

		string vsPath = Path.Combine(dataPath, ".vs");
		Exclude exclude3 = new(vsPath, ExcludeType.Keep);

		excludes.Add(exclude1);
		excludes.Add(exclude2);
		excludes.Add(exclude3);

		bool result =
			BaseService.ShouldSkipThisDirectory(clientsPath, excludes);

		Assert.That(result, Is.False);
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
		Exclude exclude1 = new(clientsPath, ExcludeType.SubDirectory);
		Exclude exclude2 = new(clientsPath, ExcludeType.Global);
		Exclude exclude3 = new(objPath, ExcludeType.Keep);

		excludes.Add(exclude1);
		excludes.Add(exclude2);
		excludes.Add(exclude3);

		bool result =
			BaseService.ShouldSkipThisDirectory(clientsPath, excludes);

		Assert.That(result, Is.False);
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
		// This test documents that ShouldSkipThisDirectory evaluates each
		// folder in isolation. A subfolder of an excluded folder is NOT
		// automatically excluded — the caller must honour the false result
		// on the parent and not recurse into it.
		string subFolder = Path.Combine(objPath, "Debug", "net8.0");
		ICollection<Exclude> excludes = [];

		Exclude exclude = new("obj", ExcludeType.Keep);
		excludes.Add(exclude);

		bool result = BaseService.ShouldSkipThisDirectory(subFolder, excludes);

		// The subfolder itself doesn't match "obj" by name,
		// so this returns true — subtree skipping is the caller's job.
		string message = "Subtree exclusion is the caller's " +
			"responsibility, not this method's.";

		Assert.That(result, Is.False, message);
	}
}
