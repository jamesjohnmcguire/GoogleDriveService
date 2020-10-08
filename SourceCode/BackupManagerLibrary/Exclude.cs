/////////////////////////////////////////////////////////////////////////////
// <copyright file="Exclude.cs" company="James John McGuire">
// Copyright © 2017 - 2020 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;

namespace BackupManagerLibrary
{
	public class Exclude
	{
		public string Path { get; set; }

		public ExcludeType ExcludeType { get; set; }
	}
}
