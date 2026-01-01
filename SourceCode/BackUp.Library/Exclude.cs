/////////////////////////////////////////////////////////////////////////////
// <copyright file="Exclude.cs" company="James John McGuire">
// Copyright © 2017 - 2026 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.BackUp.Library
{
	/// <summary>
	/// Exclude class.
	/// </summary>
	public class Exclude(string path, ExcludeType excludeType)
	{
		/// <summary>
		/// Gets or sets path property.
		/// </summary>
		/// <value>Path property.</value>
		public string Path { get; set; } = path;

		/// <summary>
		/// Gets or sets exclude type property.
		/// </summary>
		/// <value>Exclude type property.</value>
		public ExcludeType ExcludeType { get; set; } = excludeType;
	}
}
