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
		private readonly IList<string> excludes = new List<string>();

		public string Path { get; set; }

		public IList<string> Excludes
		{
			get { return excludes; }
		}
	}
}
