/////////////////////////////////////////////////////////////////////////////
// <copyright file="Settings.cs" company="James John McGuire">
// Copyright © 2017 - 2026 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.BackUp.Library;

using System.Collections.Generic;

/// <summary>
/// Represents a set of settings.
/// </summary>
public class Settings
{
	/// <summary>
	/// Gets or sets a collection of global excludes.
	/// </summary>
	/// <value>A collection of global excludes.</value>
	public IReadOnlyCollection<string> GlobalExcludes { get; set; }
}
