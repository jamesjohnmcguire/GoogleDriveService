/////////////////////////////////////////////////////////////////////////////
// <copyright file="ReviewedMapping.cs" company="James John McGuire">
// Copyright © 2017 - 2026 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.BackUp.Reporting;

using DigitalZenWorks.BackUp.Library;

/// <summary>
/// Keeps the original mapping alongside its normalized review context.
/// </summary>
internal sealed class ReviewedMapping
{
	/// <summary>Gets or sets the account reference.</summary>
	public string Account { get; set; } = string.Empty;

	/// <summary>Gets or sets the JSON location.</summary>
	public string Location { get; set; } = string.Empty;

	/// <summary>Gets or sets the normalized source path.</summary>
	public string Source { get; set; } = string.Empty;

	/// <summary>Gets or sets the original mapping.</summary>
	public DriveMapping Mapping { get; set; } = new();

	/// <summary>
	/// Gets or sets a value indicating whether the mapping is supported.
	/// </summary>
	public bool Supported { get; set; }
}
