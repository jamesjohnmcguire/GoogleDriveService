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
	/// <summary>
	/// Google Service Accounts Manager.
	/// </summary>
	public static class AccountsManager
	{
		private const string InternalDataPath =
			@"\DigitalZenWorks\BackUpManager";

		private const string MainDataFile = @"\BackUp.json";

		private static readonly ILog Log = LogManager.GetLogger(
			System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// Gets data path property.
		/// </summary>
		/// <value>Data path property.</value>
		public static string DataPath
		{
			get
			{
				string baseDataDirectory = Environment.GetFolderPath(
					Environment.SpecialFolder.ApplicationData,
					Environment.SpecialFolderOption.Create);
				string accountsPath = baseDataDirectory + InternalDataPath;
				return accountsPath;
			}
		}

		/// <summary>
		/// Load accounts method.
		/// </summary>
		/// <returns>A list of accounts.</returns>
		public static IList<Account> LoadAccounts()
		{
			IList<Account> accounts = null;

			string baseDataDirectory = Environment.GetFolderPath(
				Environment.SpecialFolder.ApplicationData,
				Environment.SpecialFolderOption.Create);
			string accountsPath = baseDataDirectory + InternalDataPath;

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
