/////////////////////////////////////////////////////////////////////////////
// <copyright file="ConfigurationReportWriter.cs" company="James John McGuire">
// Copyright © 2017 - 2026 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.BackUp.Reporting;

using System;
using System.IO;
using Newtonsoft.Json;

/// <summary>Writes configuration reports as text or JSON.</summary>
public static class ConfigurationReportWriter
{
	/// <summary>Writes a report to the supplied output.</summary>
	/// <param name="output">The caller-owned output writer.</param>
	/// <param name="report">The report to write.</param>
	/// <param name="json">Whether to write JSON instead of text.</param>
	public static void Write(
		TextWriter output, ConfigurationReport report, bool json)
	{
		ArgumentNullException.ThrowIfNull(output);
		ArgumentNullException.ThrowIfNull(report);

		if (json)
		{
			string serialized = JsonConvert.SerializeObject(
				report, Formatting.Indented);
			output.WriteLine(serialized);
		}
		else
		{
			WriteSummary(output, report);
			WriteDecisions(output, report);
			WriteDiagnostics(output, report);
		}
	}

	private static void WriteSummary(
		TextWriter output, ConfigurationReport report)
	{
		output.WriteLine(report.Scope);
		output.WriteLine("Configuration: " + report.ConfigurationPath);
		output.WriteLine("Settings: " + report.SettingsPath);

		string summary = $"Accounts: {report.AccountCount}; " +
			$"mappings: {report.MappingCount}; " +
			$"exclusions: {report.ExclusionCount}";
		output.WriteLine(summary);

		if (report.ExplainedPath.Length > 0)
		{
			output.WriteLine();
			output.WriteLine("Path: " + report.ExplainedPath);
		}
	}

	private static void WriteDecisions(
		TextWriter output, ConfigurationReport report)
	{
		foreach (MappingExplanation decision in report.Decisions)
		{
			output.WriteLine();
			output.WriteLine(decision.Status + ": " + decision.Location);
			output.WriteLine("  Account: " + decision.Account);
			output.WriteLine("  Source: " + decision.Source);
			output.WriteLine("  Parent folder ID: " + decision.Destination);
			output.WriteLine("  " + decision.Message);

			if (decision.RuleLocation.Length > 0)
			{
				output.WriteLine("  Rule: " + decision.RuleLocation);
				output.WriteLine("  Reason: " + decision.Reason);
				output.WriteLine("  Protection: " + decision.Protection);
			}

			output.WriteLine("  " + decision.RemotePolicy);
		}
	}

	private static void WriteDiagnostics(
		TextWriter output, ConfigurationReport report)
	{
		output.WriteLine();

		int cleanupRules = 0;

		foreach (ConfigurationDiagnostic diagnostic in report.Diagnostics)
		{
			if (diagnostic.Code == "POL001")
			{
				cleanupRules++;
				continue;
			}

			string line = $"{diagnostic.Severity} {diagnostic.Code} " +
				$"{diagnostic.Location}: {diagnostic.Message}";
			output.WriteLine(line);
		}

		if (cleanupRules > 0)
		{
			string policy = $"Information POL001: {cleanupRules} exclusions " +
				"permit remote cleanup. Use --json for their locations.";
			output.WriteLine(policy);
		}
	}
}
