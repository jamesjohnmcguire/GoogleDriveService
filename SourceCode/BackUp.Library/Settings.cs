/////////////////////////////////////////////////////////////////////////////
// <copyright file="Settings.cs" company="James John McGuire">
// Copyright © 2017 - 2025 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.BackUp.Library
{
	using System.Collections.Generic;

	/// <summary>
	/// Represents a set of settings.
	/// </summary>
	public class Settings
	{
		/// <summary>
		/// Gets or sets a list of global excludes.
		/// </summary>
		/// <value>A list of global excludes.</value>
		public IReadOnlyList<string> GlobalExcludes { get; set; }
	}
}
