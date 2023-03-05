/////////////////////////////////////////////////////////////////////////////
// <copyright file="AccountsManager.cs" company="James John McGuire">
// Copyright © 2017 - 2023 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

using Common.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace DigitalZenWorks.BackUp.Library
{
	/// <summary>
	/// Google Service Accounts Manager.
	/// </summary>
	public static class AccountsManager
	{
		private const string InternalDataPath =
			@"\DigitalZenWorks\BackUpManager";

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
		/// <param name="configurationFile">The configuration file.</param>
		/// <returns>A list of accounts.</returns>
		public static IList<Account> LoadAccounts(string configurationFile)
		{
			IList<Account> accounts = null;

			if (!string.IsNullOrWhiteSpace(configurationFile) &&
				System.IO.File.Exists(configurationFile))
			{
				string accountsText = File.ReadAllText(configurationFile);

				accounts = JsonConvert.DeserializeObject<IList<Account>>(
					accountsText);
			}
			else
			{
				Log.Error("Accounts file doesn't exist");
			}

			return accounts;
		}
	}
}
