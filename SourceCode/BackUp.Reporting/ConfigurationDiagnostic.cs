/////////////////////////////////////////////////////////////////////////////
// <copyright file="ConfigurationDiagnostic.cs" company="James John McGuire">
// Copyright © 2017 - 2026 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.BackUp.Reporting;

/// <summary>
/// Describes a local configuration finding with a stable code and location.
/// </summary>
public sealed class ConfigurationDiagnostic
{
	/// <summary>
	/// Gets or sets the severity: Error, Warning, or Information.
	/// </summary>
	public DiagnosticSeverity Severity { get; set; } = DiagnosticSeverity.Unknown;

	/// <summary>Gets or sets the stable diagnostic code.</summary>
	public string Code { get; set; } = string.Empty;

	/// <summary>
	/// Gets or sets the JSON property location or local path.
	/// </summary>
	public string Location { get; set; } = string.Empty;

	/// <summary>Gets or sets the explanation and suggested action.</summary>
	public string Message { get; set; } = string.Empty;
}
