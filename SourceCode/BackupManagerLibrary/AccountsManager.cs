/////////////////////////////////////////////////////////////////////////////
// <copyright file="AccountsManager.cs" company="James John McGuire">
// Copyright © 2017 - 2020 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace BackupManagerLibrary
{
	public static class AccountsManager
	{
		public static IList<Account> LoadAccounts()
		{
			IList<Account> accounts = null;

			string userProfilePath = Environment.GetFolderPath(
				Environment.SpecialFolder.UserProfile);
			string accountsPath = userProfilePath + @"\GoogleDrive";

			if (System.IO.Directory.Exists(accountsPath))
			{
				string accountsFile = accountsPath + @"\Accounts.json";
				string accountsText = File.ReadAllText(accountsFile);

				accounts = JsonConvert.DeserializeObject<IList<Account>>(accountsText);
			}

			return accounts;
		}
	}
}
