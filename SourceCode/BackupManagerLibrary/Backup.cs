/////////////////////////////////////////////////////////////////////////////
// <copyright file="Backup.cs" company="James John McGuire">
// Copyright © 2017 - 2023 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

using Common.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

[assembly: CLSCompliant(false)]

namespace BackupManagerLibrary
{
	/// <summary>
	/// Back up class.
	/// </summary>
	public static class Backup
	{
		private static readonly ILog Log = LogManager.GetLogger(
			System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// Run method.
		/// </summary>
		/// <returns>A task indicating completion.</returns>
		public static async Task Run()
		{
			try
			{
				IList<Account> accounts = AccountsManager.LoadAccounts();

				if ((accounts == null) || (accounts.Count == 0))
				{
					Log.Error("No accounts information");
				}
				else
				{
					foreach (Account account in accounts)
					{
						string name = account.ServiceAccount;
						string message = "Backing up to account: " + name;
						Log.Info(message);

						using GoogleDriveBackUpService tester =
							new GoogleDriveBackUpService(
								account.ServiceAccount);

						IBackUpService backUpService = tester;

						var some = account.DriveMappings[0];
						backUpService.BackUp(some.Path, "test");

						await account.BackUp().ConfigureAwait(false);
					}
				}
			}
			catch (Exception exception) when
				(exception is JsonException)
			{
				Log.Error(exception.ToString());
			}
		}
	}
}
