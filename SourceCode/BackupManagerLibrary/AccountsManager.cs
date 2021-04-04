/////////////////////////////////////////////////////////////////////////////
// <copyright file="AccountsManager.cs" company="James John McGuire">
// Copyright © 2017 - 2021 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

using Common.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace BackupManagerLibrary
{
	public static class AccountsManager
	{
		private static readonly ILog Log = LogManager.GetLogger(
			System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private static string dataPath = @"\Data\ProgramData\GoogleDrive";
		private static string mainDataFile = @"\Accounts.json";
		private static string userProfilePath = Environment.GetFolderPath(
			Environment.SpecialFolder.UserProfile);

		public static string DataPath
		{
			get { return userProfilePath + dataPath; }
		}

		public static IList<Account> LoadAccounts()
		{
			IList<Account> accounts = null;

			string accountsPath = userProfilePath + dataPath;

			if (System.IO.Directory.Exists(accountsPath))
			{
				string accountsFile = accountsPath + mainDataFile;

				if (System.IO.File.Exists(accountsFile))
				{
					string accountsText = File.ReadAllText(accountsFile);

					accounts = JsonConvert.DeserializeObject<IList<Account>>(
						accountsText);
				}
				else
				{
					Log.Error(CultureInfo.InvariantCulture, m => m(
						"Accounts file doesn't exist"));
				}
			}
			else
			{
				Log.Error(CultureInfo.InvariantCulture, m => m(
					"Accounts path doesn't exist"));
			}

			return accounts;
		}
	}
}
