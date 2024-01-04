/////////////////////////////////////////////////////////////////////////////
// <copyright file="Configuration.cs" company="James John McGuire">
// Copyright © 2017 - 2024 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackUpManager
{
	internal class Configuration
	{
		public static string GetConfigurationFile()
		{
			string configurationFile = null;
			string baseDataDirectory = Environment.GetFolderPath(
				Environment.SpecialFolder.ApplicationData,
				Environment.SpecialFolderOption.Create);
			string accountsPath =
				baseDataDirectory + @"\DigitalZenWorks\BackUpManager";

			if (System.IO.Directory.Exists(accountsPath))
			{
				string accountsFile = accountsPath + @"\BackUp.json";

				if (System.IO.File.Exists(accountsFile))
				{
					configurationFile = accountsFile;
				}
			}

			return configurationFile;
		}

		public static string GetDefaultDataLocation()
		{
			string defaultDataLocation = null;

			string baseDataDirectory = Environment.GetFolderPath(
				Environment.SpecialFolder.ApplicationData,
				Environment.SpecialFolderOption.Create);

			string applicationDataDirectory = @"DigitalZenWorks\BackUpManager";

			defaultDataLocation =
				Path.Combine(baseDataDirectory, applicationDataDirectory);

			return defaultDataLocation;
		}
	}
}
