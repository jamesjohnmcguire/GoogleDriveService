/////////////////////////////////////////////////////////////////////////////
// <copyright file="BackupService.cs" company="James John McGuire">
// Copyright © 2017 - 2023 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

using Common.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

[assembly: CLSCompliant(false)]

namespace DigitalZenWorks.BackUp.Library
{
	/// <summary>
	/// Back up class.
	/// </summary>
	public static class BackupService
	{
		private static readonly ILog Log = LogManager.GetLogger(
			System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// Run method.
		/// </summary>
		/// <param name="configurationFile">The configuration file.</param>
		/// <returns>A task indicating completion.</returns>
		public static async Task Run(string configurationFile)
		{
			try
			{
				IList<Account> accounts =
					AccountsManager.LoadAccounts(configurationFile);

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
