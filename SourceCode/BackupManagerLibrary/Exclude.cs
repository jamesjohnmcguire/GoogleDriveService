/////////////////////////////////////////////////////////////////////////////
// <copyright file="Exclude.cs" company="James John McGuire">
// Copyright © 2017 - 2021 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;

namespace BackupManagerLibrary
{
	public class Exclude
	{
		public Exclude(string path, ExcludeType excludeType)
		{
			Path = path;
			ExcludeType = excludeType;
		}

		public string Path { get; set; }

		public ExcludeType ExcludeType { get; set; }
	}
}
