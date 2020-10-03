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

		public IList<Exclude> Excludes
		{
			get { return excludes; }
		}
	}
}
