/////////////////////////////////////////////////////////////////////////////
// <copyright file="Exclude.cs" company="James John McGuire">
// Copyright © 2017 - 2022 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;

namespace BackupManagerLibrary
{
	/// <summary>
	/// Exclude class.
	/// </summary>
	public class Exclude
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Exclude"/> class.
		/// </summary>
		/// <param name="path">The path to exclude.</param>
		/// <param name="excludeType">The exclude type.</param>
		public Exclude(string path, ExcludeType excludeType)
		{
			Path = path;
			ExcludeType = excludeType;
		}

		/// <summary>
		/// Gets or sets path property.
		/// </summary>
		/// <value>Path property.</value>
		public string Path { get; set; }

		/// <summary>
		/// Gets or sets exclude type property.
		/// </summary>
		/// <value>Exclude type property.</value>
		public ExcludeType ExcludeType { get; set; }
	}
}
