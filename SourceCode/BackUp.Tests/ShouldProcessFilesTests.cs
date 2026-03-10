/////////////////////////////////////////////////////////////////////////////
// <copyright file="ShouldProcessFilesTests.cs" company="James John McGuire">
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
/// Provides unit tests for the ShouldProcessFiles method under various
/// scenarios.
/// </summary>
[TestFixture]
internal sealed class ShouldProcessFilesTests
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
		Directory.CreateDirectory(dataPath);

		clientsPath = Path.Combine(dataPath, "Clients");
		Directory.CreateDirectory(clientsPath);

		objPath = Path.Combine(dataPath, "obj");
		Directory.CreateDirectory(objPath);

		nodeModulesPath = Path.Combine(dataPath, "node_modules");
	}

	[TearDown]
	public void TearDown()
	{
		if (Directory.Exists(dataPath))
		{
			Directory.Delete(dataPath, recursive: true);
		}
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
		bool result = BaseService.ShouldProcessFiles(clientsPath, null);

		Assert.That(result, Is.True);
	}

	/// <summary>
	/// The empty excludes returns true test.
	/// </summary>
	[Test]
	public void EmptyExcludesReturnsTrue()
	{
		bool result = BaseService.ShouldProcessFiles(clientsPath, []);

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

		Exclude exclude = new(clientsPath, ExcludeType.OnlyRoot);
		excludes.Add(exclude);

		bool result = BaseService.ShouldProcessFiles(clientsPath, excludes);

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
		Exclude exclude = new(path, ExcludeType.OnlyRoot);
		excludes.Add(exclude);

		string checkPath = clientsPath.ToUpperInvariant();
		bool result = BaseService.ShouldProcessFiles(checkPath, excludes);

		Assert.That(result, Is.False);
	}

	/// <summary>
	/// The fully qualified exclude different path returns true test.
	/// </summary>
	[Test]
	public void FullyQualifiedExcludeDifferentPathReturnsTrue()
	{
		ICollection<Exclude> excludes = [];

		Exclude exclude = new(clientsPath, ExcludeType.OnlyRoot);
		excludes.Add(exclude);

		bool result = BaseService.ShouldProcessFiles(objPath, excludes);

		Assert.That(result, Is.True);
	}

	/// <summary>
	/// The relative exclude no name matches returns false test.
	/// </summary>
	[Test]
	public void RelativeExcludeNoNameMatchReturnsTrue()
	{
		ICollection<Exclude> excludes = [];

		Exclude exclude = new("obj", ExcludeType.OnlyRoot);
		excludes.Add(exclude);

		bool result = BaseService.ShouldProcessFiles(clientsPath, excludes);

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
		Directory.CreateDirectory(partialMatch);

		ICollection<Exclude> excludes = [];

		Exclude exclude = new("obj", ExcludeType.OnlyRoot);
		excludes.Add(exclude);

		bool result = BaseService.ShouldProcessFiles(partialMatch, excludes);

		Directory.Delete(partialMatch);

		Assert.That(result, Is.True);
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

		bool result = BaseService.ShouldProcessFiles(clientsPath, excludes);

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

		bool result = BaseService.ShouldProcessFiles(clientsPath, excludes);

		Assert.That(result, Is.True);
	}

	/// <summary>
	/// The exclude type sub-directory is respected returns false test.
	/// </summary>
	[Test]
	public void ExcludeTypeSubDirectoryIsRespectedReturnsFalse()
	{
		ICollection<Exclude> excludes = [];

		Exclude exclude = new(clientsPath, ExcludeType.OnlyRoot);
		excludes.Add(exclude);

		bool result = BaseService.ShouldProcessFiles(clientsPath, excludes);

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

		Exclude exclude1 = new(objPath, ExcludeType.OnlyRoot);
		Exclude exclude2 = new(nodeModulesPath, ExcludeType.OnlyRoot);

		string vsPath = Path.Combine(dataPath, ".vs");
		Exclude exclude3 = new(vsPath, ExcludeType.OnlyRoot);

		excludes.Add(exclude1);
		excludes.Add(exclude2);
		excludes.Add(exclude3);

		bool result =
			BaseService.ShouldProcessFiles(nodeModulesPath, excludes);

		Assert.That(result, Is.False);
	}

	/// <summary>
	/// The multiple excludes none matches returns true test.
	/// </summary>
	[Test]
	public void MultipleExcludesNoneMatchReturnsTrue()
	{
		ICollection<Exclude> excludes = [];

		Exclude exclude1 = new(objPath, ExcludeType.OnlyRoot);
		Exclude exclude2 = new(nodeModulesPath, ExcludeType.OnlyRoot);

		string vsPath = Path.Combine(dataPath, ".vs");
		Exclude exclude3 = new(vsPath, ExcludeType.OnlyRoot);

		excludes.Add(exclude1);
		excludes.Add(exclude2);
		excludes.Add(exclude3);

		bool result = BaseService.ShouldProcessFiles(clientsPath, excludes);

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
		Exclude exclude3 = new(objPath, ExcludeType.OnlyRoot);

		excludes.Add(exclude1);
		excludes.Add(exclude2);
		excludes.Add(exclude3);

		bool result = BaseService.ShouldProcessFiles(clientsPath, excludes);

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
		// This test documents that ShouldProcessFiles evaluates each
		// folder in isolation. A subfolder of an excluded folder is NOT
		// automatically excluded — the caller must honour the false result
		// on the parent and not recurse into it.
		string subFolder = Path.Combine(objPath, "Debug", "net8.0");
		Directory.CreateDirectory(subFolder);

		ICollection<Exclude> excludes = [];

		Exclude exclude = new("obj", ExcludeType.OnlyRoot);
		excludes.Add(exclude);

		bool result = BaseService.ShouldProcessFiles(subFolder, excludes);

		Directory.Delete(subFolder);

		// The subfolder itself doesn't match "obj" by name,
		// so this returns true — subtree skipping is the caller's job.
		string message = "Subtree exclusion is the caller's " +
			"responsibility, not this method's.";

		Assert.That(result, Is.True, message);
	}
}
