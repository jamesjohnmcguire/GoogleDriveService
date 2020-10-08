/////////////////////////////////////////////////////////////////////////////
// <copyright file="Directory.cs" company="James John McGuire">
// Copyright © 2017 - 2020 James John McGuire. All Rights Reserved.
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

		public string Path { get; set; }

		public string Parent { get; set; }

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
				string excludeCheckPath =
					System.IO.Path.GetFullPath(exclude.Path);

				if (checkPath.Equals(
					excludeCheckPath, StringComparison.OrdinalIgnoreCase))
				{
					contains = true;
					break;
				}
			}

			return contains;
		}
	}
}
