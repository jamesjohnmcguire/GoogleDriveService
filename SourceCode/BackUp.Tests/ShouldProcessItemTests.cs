/////////////////////////////////////////////////////////////////////////////
// <copyright file="ShouldProcessItemTests.cs" company="James John McGuire">
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
/// Provides unit tests for the ShouldProcessItem method under various
/// scenarios.
/// </summary>
[TestFixture]
internal sealed class ShouldProcessItemTests
{
	private string dataPath;
	private string nodeModulesPath;
	private string objPath;
	private string root;

	/// <summary>
	/// Initializes various paths for use in each test run.
	/// </summary>
	[SetUp]
	public void SetUp()
	{
		// Build OS-agnostic paths from the temp directory root
		root = Path.GetTempPath();
		dataPath = Path.Combine(root, "Data");
		nodeModulesPath = Path.Combine(dataPath, "node_modules");
		objPath = Path.Combine(dataPath, "obj");
	}

	/// <summary>
	/// The null excludes returns true test.
	/// </summary>
	[Test]
	public void NullExcludesReturnsTrue()
	{
		bool result = BaseService.ShouldProcessItem(dataPath, null);

		Assert.That(result, Is.True);
	}

	/// <summary>
	/// The empty excludes returns true test.
	/// </summary>
	[Test]
	public void EmptyExcludesReturnsTrue()
	{
		bool result = BaseService.ShouldProcessItem(dataPath, []);

		Assert.That(result, Is.True);
	}

	/// <summary>
	/// The empty excludes returns true test.
	/// </summary>
	[Test]
	public void PathNullEmptyExcludesReturnsFalse()
	{
		Assert.Throws<ArgumentNullException>(() =>
		{
			BaseService.ShouldProcessItem(null, []);
		});
	}

	/// <summary>
	/// The exclude matched return false test.
	/// </summary>
	[Test]
	public void ExcludeMatchReturnsFalse()
	{
		ICollection<Exclude> excludes = [];

		Exclude exclude = new(dataPath, false);
		excludes.Add(exclude);

		bool result = BaseService.ShouldProcessItem(dataPath, excludes);

		Assert.That(result, Is.False);
	}

	/// <summary>
	/// The exclude not exact match return true test.
	/// </summary>
	[Test]
	public void ExcludeNotExactMatchReturnsTrue()
	{
		// "obj" should not match "objstore" or "myobj"
		string partialMatch = Path.Combine(dataPath, "objstore");

		ICollection<Exclude> excludes = [];

		Exclude exclude = new("obj", false);
		excludes.Add(exclude);

		bool result = BaseService.ShouldProcessItem(partialMatch, excludes);

		Assert.That(result, Is.True);
	}

	/// <summary>
	/// The exclude not matched return true test.
	/// </summary>
	[Test]
	public void ExcludeNotMatchReturnsTrue()
	{
		ICollection<Exclude> excludes = [];

		Exclude exclude = new(objPath, false);
		excludes.Add(exclude);

		bool result = BaseService.ShouldProcessItem(dataPath, excludes);

		Assert.That(result, Is.True);
	}

	/// <summary>
	/// The multiple exclude matched return false test.
	/// </summary>
	[Test]
	public void MultipleExcludeMatchReturnsFalse()
	{
		ICollection<Exclude> excludes = [];

		Exclude exclude = new(dataPath, false);
		excludes.Add(exclude);
		Exclude exclude2 = new(objPath, false);
		excludes.Add(exclude2);

		bool result = BaseService.ShouldProcessItem(dataPath, excludes);

		Assert.That(result, Is.False);
	}

	/// <summary>
	/// The multiple not matched return true test.
	/// </summary>
	[Test]
	public void MultipleExcludeNotMatchReturnsTrue()
	{
		ICollection<Exclude> excludes = [];

		Exclude exclude = new(objPath, false);
		excludes.Add(exclude);
		Exclude exclude2 = new(nodeModulesPath, false);
		excludes.Add(exclude2);

		bool result = BaseService.ShouldProcessItem(dataPath, excludes);

		Assert.That(result, Is.True);
	}

	/// <summary>
	/// The relative excludes sub-directory excludes caller responsibility
	/// test.
	/// </summary>
	/// <remarks>This test documents that ShouldProcessItem evaluates each item
	/// in isolation. A subfolder of an excluded folder is NOT automatically
	/// excluded — the caller must honour the false result  on the parent and
	/// not recurse into it.</remarks>
	[Test]
	public void ExcludeInMiddleOfPathReturnsTrue()
	{
		string subFolder = Path.Combine(objPath, "Debug", "net8.0");

		ICollection<Exclude> excludes = [];
		Exclude exclude = new("obj", false);
		excludes.Add(exclude);

		bool result = BaseService.ShouldProcessItem(subFolder, excludes);

		// The subfolder itself doesn't match "obj" by name,
		// so this returns true — subtree skipping is the caller's job.
		string message = "Subtree exclusion is the caller's " +
			"responsibility, not this method's.";

		Assert.That(result, Is.True, message);
	}

	/// <summary>
	/// The path with different casings evaluates per operating system test.
	/// </summary>
	[Test]
	[System.Diagnostics.CodeAnalysis.SuppressMessage(
		"Globalization",
		"CA1308:Normalize strings to uppercase",
		Justification = "It's just a test.")]
	public void PathWithDifferentCasingsEvaluatesPerOperatingSystem()
	{
		ICollection<Exclude> excludes = [];

		string path = dataPath.ToLowerInvariant();
		Exclude exclude = new(path, false);
		excludes.Add(exclude);

		string checkPath = dataPath.ToUpperInvariant();
		bool result = BaseService.ShouldProcessItem(dataPath, excludes);

		if (OperatingSystem.IsWindows())
		{
			Assert.That(result, Is.False);
		}
		else
		{
			Assert.That(result, Is.True);
		}
	}
}
