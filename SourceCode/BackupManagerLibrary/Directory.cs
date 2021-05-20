/////////////////////////////////////////////////////////////////////////////
// <copyright file="Directory.cs" company="James John McGuire">
// Copyright © 2017 - 2021 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;

namespace BackupManagerLibrary
{
	public class Directory
	{
		private readonly IList<Exclude> excludes = new List<Exclude>();

		public Directory()
		{
			Exclude exclude =
				new ("node_modules", ExcludeType.AllSubDirectories);
			excludes.Add(exclude);
			exclude = new ("obj", ExcludeType.AllSubDirectories);
			excludes.Add(exclude);
			exclude = new ("vendor", ExcludeType.AllSubDirectories);
			excludes.Add(exclude);
		}

		public string Path { get; set; }

		public string RootSharedFolderId { get; set; }

		public IList<Exclude> Excludes
		{
			get { return excludes; }
		}

		public IList<Exclude> ExpandExcludes()
		{
			foreach (Exclude exclude in excludes)
			{
				exclude.Path = Environment.ExpandEnvironmentVariables(
					exclude.Path);
			}

			return excludes;
		}

		public bool ExcludesContains(string path)
		{
			bool contains = false;

			foreach (Exclude exclude in excludes)
			{
				string checkPath = System.IO.Path.GetFullPath(path);

				var directoryInfo = System.IO.Directory.GetParent(path);

				string excludeCheckPath =
					System.IO.Path.GetFullPath(exclude.Path, directoryInfo.FullName);

				if (checkPath.Equals(
					excludeCheckPath, StringComparison.OrdinalIgnoreCase))
				{
					contains = true;
					break;
				}
			}

			return contains;
		}

		public Exclude GetExclude(string path)
		{
			Exclude foundExclude = null;

			foreach (Exclude exclude in excludes)
			{
				string checkPath = System.IO.Path.GetFullPath(path);

				var directoryInfo = System.IO.Directory.GetParent(path);

				string excludeCheckPath =
					System.IO.Path.GetFullPath(exclude.Path, directoryInfo.FullName);

				if (checkPath.Equals(
					excludeCheckPath, StringComparison.OrdinalIgnoreCase))
				{
					foundExclude = exclude;
					break;
				}
			}

			return foundExclude;
		}
	}
}
