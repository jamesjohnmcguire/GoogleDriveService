/////////////////////////////////////////////////////////////////////////////
// <copyright file="DiagnosticSeverity.cs" company="James John McGuire">
// Copyright © 2017 - 2026 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.BackUp.Reporting;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

/// <summary>Describes the severity of a configuration finding.</summary>
[JsonConverter(typeof(StringEnumConverter))]
public enum DiagnosticSeverity
{
	/// <summary>The severity has not been assigned.</summary>
	Unknown,

	/// <summary>The finding provides additional context.</summary>
	Information,

	/// <summary>The finding warrants review but does not invalidate input.</summary>
	Warning,

	/// <summary>The input contains an error that must be corrected.</summary>
	Error,
}
