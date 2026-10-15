/////////////////////////////////////////////////////////////////////////////
// <copyright file="ConfigurationReport.cs" company="James John McGuire">
// Copyright © 2017 - 2026 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.BackUp.Reporting;

using System.Collections.Generic;
using Newtonsoft.Json;

/// <summary>
/// Collects configuration diagnostics and optional local selection decisions.
/// </summary>
public sealed class ConfigurationReport
{
	/// <summary>Gets or sets the reviewed configuration path.</summary>
	public string ConfigurationPath { get; set; } = string.Empty;

	/// <summary>Gets or sets the separately reviewed settings path.</summary>
	public string SettingsPath { get; set; } = string.Empty;

	/// <summary>Gets the scope of this report.</summary>
	public string Scope { get; } = "LocalOnly; remote state is not verified.";

	/// <summary>Gets or sets the number of account entries.</summary>
	public int AccountCount { get; set; }

	/// <summary>Gets or sets the number of mapping entries.</summary>
	public int MappingCount { get; set; }

	/// <summary>Gets or sets the number of exclusion entries.</summary>
	public int ExclusionCount { get; set; }

	/// <summary>Gets or sets the optional path being explained.</summary>
	public string ExplainedPath { get; set; } = string.Empty;

	/// <summary>Gets the diagnostics.</summary>
	public IList<ConfigurationDiagnostic> Diagnostics { get; } = [];

	/// <summary>Gets the decisions for relevant mappings.</summary>
	public IList<MappingExplanation> Decisions { get; } = [];

	/// <summary>Gets a value indicating whether errors were found.</summary>
	public bool HasErrors
	{
		get
		{
			foreach (ConfigurationDiagnostic diagnostic in Diagnostics)
			{
				if (diagnostic.Severity == DiagnosticSeverity.Error)
				{
					return true;
				}
			}

			return false;
		}
	}

	/// <summary>Gets validated mappings used for local explanation.</summary>
	[JsonIgnore]
	internal IList<ReviewedMapping> Mappings { get; } = [];

	/// <summary>Gets validated global exclusion templates.</summary>
	[JsonIgnore]
	internal IList<string> GlobalExcludes { get; } = [];

	/// <summary>
	/// Adds a diagnostic without including raw document or credential contents.
	/// </summary>
	/// <param name="severity">The severity.</param>
	/// <param name="code">The stable code.</param>
	/// <param name="location">The configuration location.</param>
	/// <param name="message">The finding.</param>
	internal void Add(
		DiagnosticSeverity severity,
		string code,
		string location,
		string message)
	{
		ConfigurationDiagnostic diagnostic = new();
		diagnostic.Severity = severity;
		diagnostic.Code = code;
		diagnostic.Location = location;
		diagnostic.Message = message;

		Diagnostics.Add(diagnostic);
	}
}
