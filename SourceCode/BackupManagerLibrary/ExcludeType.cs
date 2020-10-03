using System;
using System.Collections.Generic;
using System.Text;

namespace BackupManagerLibrary
{
	public enum ExcludeType
	{
		/// <summary>
		/// Only exclude the root directory.
		/// </summary>
		OnlyRoot = 0,

		/// <summary>
		/// Exclude the root directory and all sub directories.
		/// </summary>
		AllSubDirectories = 1
	}
}
