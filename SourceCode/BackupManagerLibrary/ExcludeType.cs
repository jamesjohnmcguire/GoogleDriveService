/////////////////////////////////////////////////////////////////////////////
// <copyright file="ExcludeType.cs" company="James John McGuire">
// Copyright © 2017 - 2022 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;

namespace BackupManagerLibrary
{
	/// <summary>
	/// Exclude type enum.
	/// </summary>
	public enum ExcludeType
	{
		/// <summary>
		/// Only exclude the root directory.
		/// </summary>
		OnlyRoot = 0,

		/// <summary>
		/// Exclude the root directory and all sub driveMappings.
		/// </summary>
		AllSubDirectories = 1,

		/// <summary>
		/// Exclude the specified file.
		/// </summary>
		File = 2,

		/// <summary>
		/// Keep the files in Google Drive, even if they don't exist locally.
		/// </summary>
		Keep = 3
	}
}
