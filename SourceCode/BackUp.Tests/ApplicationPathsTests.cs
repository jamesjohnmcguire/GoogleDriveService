/////////////////////////////////////////////////////////////////////////////
// <copyright file="ApplicationPathsTests.cs" company="James John McGuire">
// Copyright © 2017 - 2026 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.BackUp.Library.Tests;

using System;
using System.IO;
using BackUpManager;
using NUnit.Framework;

/// <summary>Checks application paths using isolated local files.</summary>
[TestFixture]
internal sealed class ApplicationPathsTests
{
	private string root = string.Empty;

	/// <summary>Chooses a unique, initially absent data root.</summary>
	[SetUp]
	public void SetUp()
	{
		string name = Path.GetRandomFileName();
		root = Path.Combine(Path.GetTempPath(), "Backup Tests 日本語", name);
	}

	/// <summary>Removes only files created by this test.</summary>
	[TearDown]
	public void TearDown()
	{
		bool exists = Directory.Exists(root);

		if (exists == true)
		{
			Directory.Delete(root, recursive: true);
		}
	}

	/// <summary>Resolves absent data roots without filesystem writes.</summary>
	[Test]
	public void ResolvingMissingRootDoesNotCreateDirectory()
	{
		string result = ApplicationPaths.GetDataDirectory(root);
		string parent = Path.Combine(root, "DigitalZenWorks");
		string expected = Path.Combine(parent, "BackUpManager");
		bool exists = Directory.Exists(root);

		Assert.That(result, Is.EqualTo(expected));
		Assert.That(exists, Is.False);
	}

	/// <summary>Prevents an unavailable root becoming a relative path.</summary>
	/// <param name="directory">The unusable root.</param>
	[TestCase("")]
	[TestCase(" ")]
	[TestCase("relative")]
	public void InvalidRootIsRejected(string directory)
	{
		Assert.Throws<InvalidOperationException>(
			() => ApplicationPaths.GetDataDirectory(directory));
	}

	/// <summary>Uses the same platform location for configuration and accounts.</summary>
	[Test]
	public void ConfigurationAndAccountsUsePlatformDataDirectory()
	{
		string baseDirectory = Environment.GetFolderPath(
			Environment.SpecialFolder.ApplicationData,
			Environment.SpecialFolderOption.DoNotVerify);
		string expected = Path.Combine(
			baseDirectory, "DigitalZenWorks", "BackUpManager");

		Assert.That(AccountsManager.DataPath, Is.EqualTo(expected));
		Assert.That(Configuration.GetDefaultDataLocation(), Is.EqualTo(expected));
	}

	/// <summary>Finds configuration and credential files with native paths.</summary>
	[Test]
	public void FindsConfigurationAndCredentialsInApplicationDirectory()
	{
		string directory = ApplicationPaths.GetDataDirectory(root);
		Directory.CreateDirectory(directory);
		string configuration = Path.Combine(directory, "BackUp.json");
		string credential = Path.Combine(directory, "account 日本語.json");
		File.WriteAllText(configuration, "[]");
		File.WriteAllText(credential, "synthetic credential");

		string actualConfiguration = Configuration.FindConfigurationFile(
			directory);
		string actualCredential = GoogleServiceAccount.FindCredentialFile(
			directory, "account 日本語.json");

		Assert.That(actualConfiguration, Is.EqualTo(configuration));
		Assert.That(actualCredential, Is.EqualTo(credential));
	}

	/// <summary>Retains a remote folder that has a matching local folder.</summary>
	[Test]
	public void ExistingLocalFolderIsNotConsideredAbandoned()
	{
		string child = Path.Combine(root, "Child 日本語");
		Directory.CreateDirectory(child);
		Account data = new();
		Settings settings = new();
		using GoogleServiceAccount account = new(data, settings);
		Google.Apis.Drive.v3.Data.File remote = new()
		{
			Name = "Child 日本語",
			MimeType = "application/vnd.google-apps.folder",
			OwnedByMe = false,
		};

		int removed = account.RemoveAbandonedFolders(
			root, [child], [], [remote]);

		Assert.That(removed, Is.Zero);
	}

	/// <summary>
	/// Missing credentials remain absent without creating directories.
	/// </summary>
	[Test]
	public void MissingCredentialDoesNotCreateDirectory()
	{
		string result = GoogleServiceAccount.FindCredentialFile(
			root, "missing.json");
		bool exists = Directory.Exists(root);

		Assert.That(result, Is.Null);
		Assert.That(exists, Is.False);
	}
}
