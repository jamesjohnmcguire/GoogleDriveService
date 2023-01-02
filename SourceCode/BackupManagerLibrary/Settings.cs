/////////////////////////////////////////////////////////////////////////////
// <copyright file="Settings.cs" company="James John McGuire">
// Copyright © 2017 - 2023 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackupManagerLibrary
{
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
