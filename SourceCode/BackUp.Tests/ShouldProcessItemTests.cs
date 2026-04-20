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
	private string? dataPath;
	private string? nodeModulesPath;
	private string? objPath;
	private string? root;

	/// <summary>
	/// Initializes the test environment by creating required temporary
	/// directories before any tests are run.
	/// </summary>
	/// <remarks>This method is executed once before any tests in the test
	/// fixture. It sets up OS-agnostic paths for test data and dependencies,
	/// ensuring that the necessary directory structure exists for subsequent
	/// tests.</remarks>
	[OneTimeSetUp]
	public void OneTimeSetUp()
	{
		// Build OS-agnostic paths from the temp directory root
		root = Path.GetTempPath();
		dataPath = Path.Combine(root, "Data");
		nodeModulesPath = Path.Combine(dataPath, "node_modules");
		Directory.CreateDirectory(nodeModulesPath);

		objPath = Path.Combine(dataPath, "obj");
		Directory.CreateDirectory(objPath);
	}

	/// <summary>
	/// Performs cleanup operations after all tests in the test fixture have
	/// run by deleting temporary directories used during testing.
	/// </summary>
	/// <remarks>This method is intended to be used as a one-time teardown in
	/// test fixtures to ensure that any temporary files or directories created
	/// during test execution are removed. It should be decorated with the
	/// [OneTimeTearDown] attribute when used with NUnit.</remarks>
	[OneTimeTearDown]
	public void BaseOneTimeTearDown()
	{
		if (Directory.Exists(nodeModulesPath))
		{
			Directory.Delete(nodeModulesPath, recursive: true);
		}

		if (Directory.Exists(objPath))
		{
			Directory.Delete(objPath, recursive: true);
		}
	}

	/// <summary>
	/// The null excludes returns true test.
	/// </summary>
	[Test]
	public void NullExcludesReturnsTrue()
	{
		// Cannot convert null literal to non-nullable reference type.
#pragma warning disable CS8625
		bool result = BaseService.ShouldProcessItem(dataPath!, null);
#pragma warning restore CS8625

		Assert.That(result, Is.True);
	}

	/// <summary>
	/// The empty excludes returns true test.
	/// </summary>
	[Test]
	public void EmptyExcludesReturnsTrue()
	{
		bool result = BaseService.ShouldProcessItem(dataPath!, []);

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
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
			BaseService.ShouldProcessItem(null, []);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
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

		bool result = BaseService.ShouldProcessItem(dataPath!, excludes);

		Assert.That(result, Is.False);
	}

	/// <summary>
	/// The exclude not exact match return true test.
	/// </summary>
	[Test]
	public void ExcludeNotExactMatchReturnsTrue()
	{
		// "obj" should not match "objstore" or "myobj"
		string partialMatch = Path.Combine(dataPath!, "objstore");
		Directory.CreateDirectory(partialMatch);

		ICollection<Exclude> excludes = [];

		Exclude exclude = new("obj", false);
		excludes.Add(exclude);

		bool result = BaseService.ShouldProcessItem(partialMatch, excludes);

		if (Directory.Exists(partialMatch))
		{
			Directory.Delete(partialMatch);
		}

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

		bool result = BaseService.ShouldProcessItem(dataPath!, excludes);

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

		bool result = BaseService.ShouldProcessItem(dataPath!, excludes);

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

		bool result = BaseService.ShouldProcessItem(dataPath!, excludes);

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
		string subFolder = Path.Combine(objPath!, "Debug", "net8.0");
		Directory.CreateDirectory(subFolder);

		ICollection<Exclude> excludes = [];
		Exclude exclude = new("obj", false);
		excludes.Add(exclude);

		bool result = BaseService.ShouldProcessItem(subFolder, excludes);

		if (Directory.Exists(subFolder))
		{
			Directory.Delete(subFolder);
		}

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

		string path = dataPath!.ToLowerInvariant();
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
