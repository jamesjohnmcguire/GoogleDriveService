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
		private const string InternalDataPath =
			@"\Data\ProgramData\GoogleDrive";

		private const string MainDataFile = @"\Accounts.json";

		private static readonly ILog Log = LogManager.GetLogger(
			System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private static readonly string UserProfilePath =
			Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

		public static string DataPath
		{
			get { return UserProfilePath + InternalDataPath; }
		}

		public static IList<Account> LoadAccounts()
		{
			IList<Account> accounts = null;

			string accountsPath = UserProfilePath + InternalDataPath;

			if (System.IO.Directory.Exists(accountsPath))
			{
				string accountsFile = accountsPath + MainDataFile;

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
