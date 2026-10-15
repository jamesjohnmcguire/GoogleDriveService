/////////////////////////////////////////////////////////////////////////////
// <copyright file="ConfigurationReportWriterTests.cs" company="James John McGuire">
// Copyright © 2017 - 2026 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.BackUp.Library.Tests;

using System.IO;
using DigitalZenWorks.BackUp.Reporting;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

/// <summary>Checks the text and JSON report contracts.</summary>
[TestFixture]
internal sealed class ConfigurationReportWriterTests
{
	/// <summary>Summarizes cleanup rules while retaining other findings.</summary>
	[Test]
	public void TextSummarizesCleanupRulesAndExplainsSelection()
	{
		ConfigurationReport report = CreateReport();
		using StringWriter output = new();

		ConfigurationReportWriter.Write(output, report, false);
		string text = output.ToString();

		Assert.That(text, Does.Contain("Excluded: Mapping"));
		Assert.That(text, Does.Contain("Rule: Rule"));
		Assert.That(text, Does.Contain("Warning MAP004 Source: Missing source"));
		Assert.That(text, Does.Contain("2 exclusions permit remote cleanup"));
		Assert.That(text, Does.Not.Contain("first-cleanup-detail"));
	}

	/// <summary>Preserves individual findings and enum names in JSON.</summary>
	[Test]
	public void JsonIncludesAllCleanupRulesAndReadableStatuses()
	{
		ConfigurationReport report = CreateReport();
		using StringWriter output = new();

		ConfigurationReportWriter.Write(output, report, true);
		string text = output.ToString();
		JObject document = JObject.Parse(text);
		JArray diagnostics = (JArray)document["Diagnostics"]!;

		Assert.That(diagnostics, Has.Count.EqualTo(3));
		Assert.That(text, Does.Contain("first-cleanup-detail"));
		Assert.That(
			document["Decisions"]![0]!["Status"]!.Value<string>(),
			Is.EqualTo("Excluded"));
		Assert.That(
			diagnostics[0]!["Severity"]!.Value<string>(),
			Is.EqualTo("Information"));
	}

	private static ConfigurationReport CreateReport()
	{
		ConfigurationReport report = new();
		report.Add(
			DiagnosticSeverity.Information,
			"POL001",
			"First",
			"first-cleanup-detail");
		report.Add(
			DiagnosticSeverity.Information, "POL001", "Second", "Cleanup");
		report.Add(
			DiagnosticSeverity.Warning, "MAP004", "Source", "Missing source");

		MappingExplanation decision = new()
		{
			Status = MappingStatus.Excluded,
			Location = "Mapping",
			RuleLocation = "Rule",
		};
		report.Decisions.Add(decision);

		return report;
	}
}
