/////////////////////////////////////////////////////////////////////////////
// <copyright file="ExcludeType.cs" company="James John McGuire">
// Copyright © 2017 - 2025 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.BackUp.Library
{
	/// <summary>
	/// Exclude type enum.
	/// </summary>
	public enum ExcludeType
	{
		/// <summary>
		/// Only exclude files in the root directory.
		/// </summary>
		OnlyRoot = 0,

		/// <summary>
		/// Exclude the specified directory relative to the root directory.
		/// </summary>
		SubDirectory = 1,

		/// <summary>
		/// Exclude the directory and all sub-directories, whenever the
		/// directory is present.
		/// </summary>
		Global = 2,

		/// <summary>
		/// Exclude the specified file.
		/// </summary>
		/// <remarks>If the remote file is present, it will be
		/// removed.</remarks>
		File = 3,

		/// <summary>
		/// Keep the files in Google Drive, even if they don't exist locally.
		/// </summary>
		Keep = 4,

		/// <summary>
		/// Exclude the specified file, ignore remote file.
		/// </summary>
		/// <remarks>Even if the remote file is present, it will not be
		/// removed.</remarks>
		FileIgnore = 5
	}
}
