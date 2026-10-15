/////////////////////////////////////////////////////////////////////////////
// <copyright file="ConfigurationInspectorTests.cs" company="James John McGuire">
// Copyright © 2017 - 2026 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.BackUp.Library.Tests;

using System;
using System.IO;
using DigitalZenWorks.BackUp.Reporting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

/// <summary>
/// Reviews synthetic configurations without providers or real credentials.
/// </summary>
[TestFixture]
internal sealed class ConfigurationInspectorTests
{
	private string root = string.Empty;
	private string source = string.Empty;
	private string configurationPath = string.Empty;
	private string settingsPath = string.Empty;
	private JObject account = null!;
	private JObject mapping = null!;

	/// <summary>Creates isolated input files.</summary>
	[SetUp]
	public void SetUp()
	{
		string temporary = Path.GetTempPath();
		Guid identifier = Guid.NewGuid();
		string name = identifier.ToString();

		root = Path.Combine(temporary, "BackUp.Review.Tests", name);
		source = Path.Combine(root, "Source");
		Directory.CreateDirectory(source);

		configurationPath = Path.Combine(root, "BackUp.json");
		settingsPath = Path.Combine(root, "Settings.json");

		File.WriteAllText(settingsPath, "{\"GlobalExcludes\":[\"obj\"]}");
		string credential = Path.Combine(root, "account.json");
		File.WriteAllText(credential, "not a credential");

		mapping = new JObject();
		mapping["Path"] = source;
		mapping["DriveParentFolderId"] = "unverified-parent";

		account = new JObject();
		account["AccountType"] = "GoogleServiceAccount";
		account["AccountIdentifier"] = "account.json";
		account["DriveMappings"] = new JArray(mapping);
	}

	/// <summary>Removes only this test's generated directory.</summary>
	[TearDown]
	public void TearDown()
	{
		Directory.Delete(root, recursive: true);
	}

	/// <summary>Checks existence without parsing credential contents.</summary>
	[Test]
	public void ValidConfigurationDoesNotOpenCredentials()
	{
		ConfigurationReport report = Review();

		Assert.That(report.HasErrors, Is.False);
		Assert.That(report.AccountCount, Is.EqualTo(1));
		Assert.That(report.MappingCount, Is.EqualTo(1));
		Assert.That(report.Scope, Does.Contain("not verified"));
	}

	/// <summary>
	/// Rejects obsolete and misspelled fields at their locations.
	/// </summary>
	[Test]
	public void UnknownAndObsoleteFieldsAreReported()
	{
		mapping["Pth"] = source;
		JObject exclusion = Exclusion("obj", true);
		exclusion["ExcludeType"] = 1;
		mapping["Excludes"] = new JArray(exclusion);

		ConfigurationReport report = Review();

		AssertDiagnostic(report, "CFG003", "[0].DriveMappings[0].Pth");
		AssertDiagnostic(
			report,
			"CFG004",
			"[0].DriveMappings[0].Excludes[0].ExcludeType");
	}

	/// <summary>Rejects providers that would otherwise fall through.</summary>
	[Test]
	public void UnsupportedProviderIsAnError()
	{
		account["AccountType"] = "OneDrive";

		ConfigurationReport report = Review();

		AssertDiagnostic(report, "ACC002", "[0].AccountType");
		Assert.That(report.HasErrors, Is.True);
	}

	/// <summary>Reports the dormant cleanup flag without enabling it.</summary>
	[Test]
	public void CheckDriveMappingsReportsInactiveBehavior()
	{
		account["CheckDriveMappings"] = true;

		ConfigurationReport report = Review();

		AssertDiagnostic(report, "CFG005", "[0].CheckDriveMappings");
	}

	/// <summary>Recognizes file mappings that backup currently skips.</summary>
	[Test]
	public void IndividualFileMappingIsUnsupported()
	{
		string file = Path.Combine(source, "playlist.m3u");
		File.WriteAllText(file, "playlist");
		mapping["Path"] = file;

		ConfigurationReport report = Review();
		ConfigurationInspector.Explain(report, file);

		AssertDiagnostic(report, "MAP003", "[0].DriveMappings[0].Path");
		Assert.That(
			report.Decisions[0].Status,
			Is.EqualTo(MappingStatus.Unsupported));
	}

	/// <summary>Reports missing source paths.</summary>
	[Test]
	public void MissingSourceIsReported()
	{
		string missing = Path.Combine(source, "Missing");
		mapping["Path"] = missing;

		ConfigurationReport report = Review();
		ConfigurationInspector.Explain(report, missing);

		AssertDiagnostic(report, "MAP004", "[0].DriveMappings[0].Path");
		Assert.That(
			report.Decisions[0].Status, Is.EqualTo(MappingStatus.Missing));
	}

	/// <summary>Preserves readable status names in JSON reports.</summary>
	/// <param name="status">The mapping outcome.</param>
	[TestCase(MappingStatus.Unknown)]
	[TestCase(MappingStatus.Unsupported)]
	[TestCase(MappingStatus.Selected)]
	[TestCase(MappingStatus.Excluded)]
	[TestCase(MappingStatus.Unavailable)]
	[TestCase(MappingStatus.Invalid)]
	[TestCase(MappingStatus.Missing)]
	[TestCase(MappingStatus.SkippedLink)]
	public void MappingStatusSerializesAsName(MappingStatus status)
	{
		MappingExplanation explanation = new();
		explanation.Status = status;

		string json = JsonConvert.SerializeObject(explanation);
		JObject document = JObject.Parse(json);
		string expected = status.ToString();

		Assert.That(document["Status"]!.Type, Is.EqualTo(JTokenType.String));
		Assert.That(document["Status"]!.Value<string>(), Is.EqualTo(expected));
	}

	/// <summary>
	/// Tracks intentional exclusion through an excluded ancestor.
	/// </summary>
	[Test]
	public void GitExclusionExplainsDescendantWithoutCheckingGit()
	{
		string project = Path.Combine(source, "Project");
		Directory.CreateDirectory(project);
		string file = Path.Combine(project, "work.cs");
		File.WriteAllText(file, "local work");

		JObject exclusion = Exclusion("Project", true);
		exclusion["Reason"] = "Source tracked in Git";
		exclusion["Protection"] = "GitRemote";
		mapping["Excludes"] = new JArray(exclusion);

		ConfigurationReport report = Review();
		ConfigurationInspector.Explain(report, file);

		Assert.That(report.Decisions, Has.Count.EqualTo(1));
		MappingExplanation decision = report.Decisions[0];

		Assert.That(decision.Status, Is.EqualTo(MappingStatus.Excluded));
		Assert.That(decision.Reason, Is.EqualTo("Source tracked in Git"));
		Assert.That(decision.Protection, Is.EqualTo("GitRemote"));
		Assert.That(
			decision.RuleLocation,
			Is.EqualTo("[0].DriveMappings[0].Excludes[0]"));
		Assert.That(decision.RemotePolicy, Does.Contain("retained"));
		AssertDiagnostic(report, "GIT001", decision.RuleLocation);
		Assert.That(HasCode(report, "GIT002"), Is.False);
	}

	/// <summary>
	/// Explains global directory exclusions, including descendants.
	/// </summary>
	[Test]
	public void GlobalDirectoryExclusionExplainsDescendant()
	{
		string output = Path.Combine(source, "obj", "Debug");
		Directory.CreateDirectory(output);
		string file = Path.Combine(output, "generated.cs");
		File.WriteAllText(file, "generated");

		ConfigurationReport report = Review();
		ConfigurationInspector.Explain(report, file);

		Assert.That(
			report.Decisions[0].Status,
			Is.EqualTo(MappingStatus.Excluded));
		Assert.That(
			report.Decisions[0].RuleLocation,
			Is.EqualTo("Settings.GlobalExcludes[0]"));
	}

	/// <summary>
	/// Documents the existing direct-file global exclusion quirk.
	/// </summary>
	[Test]
	public void GlobalFileRulePreservesCurrentTraversalBehavior()
	{
		File.WriteAllText(
			settingsPath,
			"{\"GlobalExcludes\":[\"generated.txt\"]}");
		string file = Path.Combine(source, "generated.txt");
		File.WriteAllText(file, "generated");

		ConfigurationReport report = Review();
		ConfigurationInspector.Explain(report, file);

		Assert.That(
			report.Decisions[0].Status,
			Is.EqualTo(MappingStatus.Selected));
		AssertDiagnostic(report, "POL002", "Settings.GlobalExcludes");
	}

	/// <summary>
	/// Reports uncovered folders without inventing a destination.
	/// </summary>
	[Test]
	public void UnassignedDirectoryHasNoApplicableMapping()
	{
		string other = Path.Combine(root, "Other");
		Directory.CreateDirectory(other);

		ConfigurationReport report = Review();
		ConfigurationInspector.Explain(report, other);

		Assert.That(report.Decisions, Is.Empty);
		AssertDiagnostic(report, "COV001", other);
	}

	/// <summary>Reports all selections for overlapping mappings.</summary>
	[Test]
	public void OverlappingMappingsRemainVisible()
	{
		string child = Path.Combine(source, "Child");
		Directory.CreateDirectory(child);
		JObject second = (JObject)mapping.DeepClone();
		second["Path"] = child;
		JArray mappings = (JArray)account["DriveMappings"]!;
		mappings.Add(second);

		ConfigurationReport report = Review();
		ConfigurationInspector.Explain(report, child);

		Assert.That(report.Decisions, Has.Count.EqualTo(2));
		AssertDiagnostic(report, "MAP007", "[0].DriveMappings[0]");
		AssertDiagnostic(report, "COV002", child);
	}

	/// <summary>
	/// Flags contradictory cleanup rules while preserving order.
	/// </summary>
	[Test]
	public void ConflictingExclusionsAreReported()
	{
		mapping["Excludes"] = new JArray(
			Exclusion("obj", true), Exclusion("obj", false));

		ConfigurationReport report = Review();

		AssertDiagnostic(
			report,
			"EXC003",
			"[0].DriveMappings[0].Excludes[1]");
	}

	/// <summary>
	/// Turns malformed JSON into a diagnostic without raw contents.
	/// </summary>
	[Test]
	public void MalformedDocumentReturnsDiagnostic()
	{
		File.WriteAllText(configurationPath, "[{secret");

		ConfigurationReport report = ConfigurationInspector.Review(
			configurationPath, settingsPath, root);

		AssertDiagnostic(report, "CFG001", "$");
		Assert.That(report.HasErrors, Is.True);
	}

	/// <summary>
	/// Rejects duplicate JSON properties instead of silently replacing.
	/// </summary>
	[Test]
	public void DuplicateJsonPropertiesAreRejected()
	{
		File.WriteAllText(
			configurationPath,
			"[{\"AccountType\":\"OneDrive\",\"AccountType\":\"Other\"}]");

		ConfigurationReport report = ConfigurationInspector.Review(
			configurationPath, settingsPath, root);

		AssertDiagnostic(report, "CFG001", "$");
	}

	private static JObject Exclusion(string path, bool keep)
	{
		JObject exclusion = new();
		exclusion["Path"] = path;
		exclusion["KeepOnRemote"] = keep;

		return exclusion;
	}

	private static bool HasCode(ConfigurationReport report, string code)
	{
		foreach (ConfigurationDiagnostic diagnostic in report.Diagnostics)
		{
			if (diagnostic.Code == code)
			{
				return true;
			}
		}

		return false;
	}

	private static void AssertDiagnostic(
		ConfigurationReport report, string code, string location)
	{
		foreach (ConfigurationDiagnostic diagnostic in report.Diagnostics)
		{
			if (diagnostic.Code == code && diagnostic.Location == location)
			{
				return;
			}
		}

		Assert.Fail("Expected " + code + " at " + location);
	}

	private ConfigurationReport Review()
	{
		JArray accounts = new(account);
		string json = accounts.ToString();
		File.WriteAllText(configurationPath, json);

		return ConfigurationInspector.Review(
			configurationPath, settingsPath, root);
	}
}
