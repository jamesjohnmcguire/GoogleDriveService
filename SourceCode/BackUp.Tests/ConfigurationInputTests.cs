/////////////////////////////////////////////////////////////////////////////
// <copyright file="ConfigurationInputTests.cs" company="James John McGuire">
// Copyright © 2017 - 2026 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.BackUp.Library.Tests;

using System;
using System.IO;
using DigitalZenWorks.BackUp.Reporting;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

/// <summary>
/// Checks invalid input without exposing raw document contents.
/// </summary>
[TestFixture]
internal sealed class ConfigurationInputTests
{
	private string root = string.Empty;
	private string settings = string.Empty;

	/// <summary>Creates temporary settings.</summary>
	[SetUp]
	public void SetUp()
	{
		Guid identifier = Guid.NewGuid();
		string name = identifier.ToString();

		root = Path.Combine(Path.GetTempPath(), "BackUp.Input.Tests", name);
		Directory.CreateDirectory(root);
		settings = Path.Combine(root, "Settings.json");
		File.WriteAllText(settings, "{\"GlobalExcludes\":[]}");
	}

	/// <summary>Removes the generated input directory.</summary>
	[TearDown]
	public void TearDown()
	{
		Directory.Delete(root, recursive: true);
	}

	/// <summary>
	/// Returns a report instead of throwing for an invalid filename.
	/// </summary>
	[Test]
	public void InvalidConfigurationFilenameReturnsError()
	{
		string path = "invalid" + char.MinValue + ".json";

		ConfigurationReport report = ConfigurationInspector.Review(
			path, settings, root);

		Assert.That(report.HasErrors, Is.True);
	}

	/// <summary>Reports unusable document shapes.</summary>
	/// <param name="json">The invalid document.</param>
	[TestCase("null")]
	[TestCase("{}")]
	[TestCase("[null]")]
	[TestCase("[{\"DriveMappings\":null}]")]
	public void InvalidDocumentShapeReturnsError(string json)
	{
		string path = Path.Combine(root, "BackUp.json");
		File.WriteAllText(path, json);

		ConfigurationReport report = ConfigurationInspector.Review(
			path, settings, root);

		Assert.That(report.HasErrors, Is.True);
	}

	/// <summary>
	/// Preserves global rule locations after invalid entries.
	/// </summary>
	[Test]
	public void InvalidGlobalRuleDoesNotShiftLaterLocations()
	{
		string directory = Path.Combine(root, "obj");
		Directory.CreateDirectory(directory);
		File.WriteAllText(settings, "{\"GlobalExcludes\":[null,\"obj\"]}");

		JObject mapping = new();
		mapping["Path"] = root;
		mapping["DriveParentFolderId"] = "parent";

		JObject account = new();
		account["AccountType"] = "GoogleServiceAccount";
		account["AccountIdentifier"] = "account.json";
		account["DriveMappings"] = new JArray(mapping);

		JArray accounts = new(account);
		string path = Path.Combine(root, "BackUp.json");
		File.WriteAllText(path, accounts.ToString());
		string credential = Path.Combine(root, "account.json");
		File.WriteAllText(credential, "not credentials");

		ConfigurationReport report = ConfigurationInspector.Review(
			path, settings, root);
		ConfigurationInspector.Explain(report, directory);

		Assert.That(
			report.Decisions[0].RuleLocation,
			Is.EqualTo("Settings.GlobalExcludes[1]"));
	}
}
