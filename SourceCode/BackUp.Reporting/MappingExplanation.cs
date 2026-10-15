/////////////////////////////////////////////////////////////////////////////
// <copyright file="MappingExplanation.cs" company="James John McGuire">
// Copyright © 2017 - 2026 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.BackUp.Reporting;

/// <summary>
/// Explains one mapping's local selection decision, not remote backup status.
/// </summary>
public sealed class MappingExplanation
{
	/// <summary>Gets or sets the account reference.</summary>
	public string Account { get; set; } = string.Empty;

	/// <summary>Gets or sets the mapping's configuration location.</summary>
	public string Location { get; set; } = string.Empty;

	/// <summary>Gets or sets the normalized source root.</summary>
	public string Source { get; set; } = string.Empty;

	/// <summary>
	/// Gets or sets the configured remote parent, not a verified ID.
	/// </summary>
	public string Destination { get; set; } = string.Empty;

	/// <summary>Gets or sets the local selection status.</summary>
	public MappingStatus Status { get; set; } = MappingStatus.Unknown;

	/// <summary>Gets or sets the decision explanation.</summary>
	public string Message { get; set; } = string.Empty;

	/// <summary>
	/// Gets or sets the matching exclusion's configuration location.
	/// </summary>
	public string RuleLocation { get; set; } = string.Empty;

	/// <summary>Gets or sets the declared reason for exclusion.</summary>
	public string Reason { get; set; } = string.Empty;

	/// <summary>Gets or sets the declared external protection.</summary>
	public string Protection { get; set; } = string.Empty;

	/// <summary>Gets or sets the remote cleanup policy description.</summary>
	public string RemotePolicy { get; set; } = string.Empty;
}
