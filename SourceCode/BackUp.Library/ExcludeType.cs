/////////////////////////////////////////////////////////////////////////////
// <copyright file="ExcludeType.cs" company="James John McGuire">
// Copyright © 2017 - 2023 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;

namespace DigitalZenWorks.BackUp.Library
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
		/// <remarks>If the remote file is present, it will be
		/// removed.</remarks>
		File = 2,

		/// <summary>
		/// Keep the files in Google Drive, even if they don't exist locally.
		/// </summary>
		Keep = 3,

		/// <summary>
		/// Exclude the specified file, ignore remote file.
		/// </summary>
		/// <remarks>Even if the remote file is present, it will not be
		/// removed.</remarks>
		FileIgnore = 4
	}
}
