/////////////////////////////////////////////////////////////////////////////
// <copyright file="BackUpService.cs" company="James John McGuire">
// Copyright © 2017 - 2023 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

using Common.Logging;
using Microsoft.Extensions.Logging;
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
	public class BackUpService
	{
		private static readonly ILog Log = LogManager.GetLogger(
			System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private readonly ILogger<BackUpService> logger;

		/// <summary>
		/// Initializes a new instance of the <see cref="BackUpService"/>
		/// class.
		/// </summary>
		/// <param name="logger">The logger interface.</param>
		public BackUpService(ILogger<BackUpService> logger = null)
		{
			this.logger = logger;
		}

		/// <summary>
		/// Run method.
		/// </summary>
		/// <param name="configurationFile">The configuration file.</param>
		/// <returns>A task indicating completion.</returns>
		public async Task Run(string configurationFile)
		{
			try
			{
				IList<Account> accounts =
					AccountsManager.LoadAccounts(configurationFile);

				if ((accounts == null) || (accounts.Count == 0))
				{
					logger.LogError("No accounts information");
					Log.Error("No accounts information");
				}
				else
				{
					foreach (Account account in accounts)
					{
						string name = account.ServiceAccount;
						string message = "Backing up to account: " + name;
						Log.Info(message);
						logger.LogInformation(message);

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
