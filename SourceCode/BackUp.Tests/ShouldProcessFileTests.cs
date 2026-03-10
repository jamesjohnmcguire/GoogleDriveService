/////////////////////////////////////////////////////////////////////////////
// <copyright file="ShouldProcessFileTests.cs" company="James John McGuire">
// Copyright © 2017 - 2026 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.BackUp.Library.Tests;

using System.Collections.Generic;
using System.IO;
using DigitalZenWorks.BackUp.Library;
using NUnit.Framework;

/// <summary>
/// Provides unit tests for the ShouldProcessFile method under various
/// scenarios.
/// </summary>
[TestFixture]
internal sealed class ShouldProcessFileTests
{
	private string root;
	private string dataPath;
	private string objPath;
	private string nodeModulesPath;
	private string testFile;
	private string testFileOther;

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

		objPath = Path.Combine(dataPath, "obj");
		Directory.CreateDirectory(objPath);

		nodeModulesPath = Path.Combine(dataPath, "node_modules");

		testFile = Path.Combine(dataPath, "TestFile.txt");
		using (File.Create(testFile)) { }

		testFileOther = Path.Combine(dataPath, "TestFileOther.txt");
		using (File.Create(testFileOther)) { }
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
		bool result = BaseService.ShouldProcessFile(testFile, null);

		Assert.That(result, Is.True);
	}

	/// <summary>
	/// The empty excludes returns true test.
	/// </summary>
	[Test]
	public void EmptyExcludesReturnsTrue()
	{
		bool result = BaseService.ShouldProcessFile(testFile, []);

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

		Exclude exclude = new(testFile, ExcludeType.File);
		excludes.Add(exclude);

		bool result = BaseService.ShouldProcessFile(testFile, excludes);

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

		string path = testFile.ToLowerInvariant();
		Exclude exclude = new(path, ExcludeType.File);
		excludes.Add(exclude);

		string checkPath = testFile.ToUpperInvariant();
		bool result = BaseService.ShouldProcessFile(checkPath, excludes);

		Assert.That(result, Is.False);
	}

	/// <summary>
	/// The fully qualified exclude different path returns true test.
	/// </summary>
	[Test]
	public void FullyQualifiedExcludeDifferentPathReturnsTrue()
	{
		ICollection<Exclude> excludes = [];

		Exclude exclude = new(testFileOther, ExcludeType.File);
		excludes.Add(exclude);

		bool result = BaseService.ShouldProcessFile(testFile, excludes);

		Assert.That(result, Is.True);
	}

	/// <summary>
	/// The relative exclude no name matches returns false test.
	/// </summary>
	[Test]
	public void RelativeExcludeNoNameMatchReturnsTrue()
	{
		ICollection<Exclude> excludes = [];

		Exclude exclude = new("obj", ExcludeType.File);
		excludes.Add(exclude);

		bool result = BaseService.ShouldProcessFile(testFile, excludes);

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

		Exclude exclude = new(testFile, ExcludeType.SubDirectory);
		excludes.Add(exclude);

		bool result = BaseService.ShouldProcessFile(testFile, excludes);

		Assert.That(result, Is.True);
	}

	/// <summary>
	/// The exclude type fileIgnore is ignored returns true test.
	/// </summary>
	[Test]
	public void ExcludeTypeFileIgnoreIsIgnoredReturnsTrue()
	{
		ICollection<Exclude> excludes = [];

		Exclude exclude = new(testFile, ExcludeType.Global);
		excludes.Add(exclude);

		bool result = BaseService.ShouldProcessFile(testFile, excludes);

		Assert.That(result, Is.True);
	}

	/// <summary>
	/// The exclude type sub-directory is respected returns false test.
	/// </summary>
	[Test]
	public void ExcludeTypeSubDirectoryIsRespectedReturnsFalse()
	{
		ICollection<Exclude> excludes = [];

		Exclude exclude = new(testFile, ExcludeType.File);
		excludes.Add(exclude);

		bool result = BaseService.ShouldProcessFile(testFile, excludes);

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

		Exclude exclude1 = new(objPath, ExcludeType.File);
		Exclude exclude2 = new(nodeModulesPath, ExcludeType.File);

		string vsPath = Path.Combine(dataPath, ".vs");
		Exclude exclude3 = new(vsPath, ExcludeType.File);

		excludes.Add(exclude1);
		excludes.Add(exclude2);
		excludes.Add(exclude3);

		bool result =
			BaseService.ShouldProcessFile(nodeModulesPath, excludes);

		Assert.That(result, Is.False);
	}

	/// <summary>
	/// The multiple excludes none matches returns true test.
	/// </summary>
	[Test]
	public void MultipleExcludesNoneMatchReturnsTrue()
	{
		ICollection<Exclude> excludes = [];

		Exclude exclude1 = new(objPath, ExcludeType.File);
		Exclude exclude2 = new(nodeModulesPath, ExcludeType.File);

		string vsPath = Path.Combine(dataPath, ".vs");
		Exclude exclude3 = new(vsPath, ExcludeType.File);

		excludes.Add(exclude1);
		excludes.Add(exclude2);
		excludes.Add(exclude3);

		bool result = BaseService.ShouldProcessFile(testFile, excludes);

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
		Exclude exclude1 = new(testFile, ExcludeType.SubDirectory);
		Exclude exclude2 = new(testFile, ExcludeType.Global);
		Exclude exclude3 = new(objPath, ExcludeType.File);

		excludes.Add(exclude1);
		excludes.Add(exclude2);
		excludes.Add(exclude3);

		bool result = BaseService.ShouldProcessFile(testFile, excludes);

		Assert.That(result, Is.True);
	}
}
