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
