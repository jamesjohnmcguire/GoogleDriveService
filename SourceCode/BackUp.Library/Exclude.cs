/////////////////////////////////////////////////////////////////////////////
// <copyright file="Exclude.cs" company="James John McGuire">
// Copyright © 2017 - 2026 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.BackUp.Library;

#nullable enable

/// <summary>
/// The Exclude class represents an item that should be excluded from backup.
/// </summary>
public class Exclude(string? path, bool keepOnRemote)
{
	/// <summary>
	/// Gets or sets path property.
	/// </summary>
	/// <value>Path property.</value>
	public string? Path { get; set; } = path;

	/// <summary>
	/// Gets or sets a value indicating whether to keep on remote property.
	/// </summary>
	/// <value>The value indicating whether to keep on remote property.</value>
	public bool KeepOnRemote { get; set; } = keepOnRemote;
}
