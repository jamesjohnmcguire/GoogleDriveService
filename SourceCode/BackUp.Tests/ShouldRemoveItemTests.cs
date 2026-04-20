/////////////////////////////////////////////////////////////////////////////
// <copyright file="ShouldRemoveItemTests.cs" company="James John McGuire">
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
/// Provides unit tests for the ShouldRemoveItem method under various
/// scenarios.
/// </summary>
[TestFixture]
internal sealed class ShouldRemoveItemTests
{
	private string? root;
	private string? dataPath;
	private string? objPath;

	/// <summary>
	/// Initializes various paths for use in each test run.
	/// </summary>
	[SetUp]
	public void SetUp()
	{
		// Build OS-agnostic paths from the temp directory root
		root = Path.GetTempPath();
		dataPath = Path.Combine(root, "Data");
		objPath = Path.Combine(dataPath, "obj");
	}

	/// <summary>
	/// The exclude with KeepOnRemote set to false should return true,
	/// indicating that the item should be removed.
	/// </summary>
	[Test]
	public void ExcludeKeepFalseReturnsTrue()
	{
		ICollection<Exclude> excludes = [];

		Exclude exclude = new(objPath, false);
		excludes.Add(exclude);

		bool result = BaseService.ShouldRemoveItem(objPath!, excludes);

		Assert.That(result, Is.True);
	}

	/// <summary>
	/// The exclude with KeepOnRemote set to true should return false,
	/// indicating that the item should not be removed.
	/// </summary>
	[Test]
	public void ExcludeKeepTrueReturnsFalse()
	{
		ICollection<Exclude> excludes = [];

		Exclude exclude = new(objPath, true);
		excludes.Add(exclude);

		bool result = BaseService.ShouldRemoveItem(objPath!, excludes);

		Assert.That(result, Is.False);
	}

	/// <summary>
	/// The exclude missing from the collection should throw an
	/// ArgumentNullException, as an exclude was expected but not found.
	/// </summary>
	[Test]
	public void ExcludeMissingThrowsException()
	{
		ICollection<Exclude> excludes = [];

		Assert.Throws<ArgumentNullException>(() =>
		{
			BaseService.ShouldRemoveItem(objPath!, excludes);
		});
	}
}
