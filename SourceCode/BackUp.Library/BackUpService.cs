/////////////////////////////////////////////////////////////////////////////
// <copyright file="BackUpService.cs" company="James John McGuire">
// Copyright © 2017 - 2023 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

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
		/// Back up method.
		/// </summary>
		/// <param name="path">The path to back up.</param>
		/// <param name="serviceDestinationId">A service specific
		/// identifier for the destination.</param>
		public void BackUp(string path, string serviceDestinationId)
		{
			this.path = path;
			parentId = serviceDestinationId;

			//bool authenticated = Authorize();

			//if (authenticated == true)
			//{
			//}
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
					AccountsManager.LoadAccounts(configurationFile, logger);

				if ((accounts == null) || (accounts.Count == 0))
				{
					LogAction.Error(logger, "No accounts information", null);
				}
				else
				{
					foreach (Account accountData in accounts)
					{
						string name = accountData.ServiceAccount;
						string message = "Backing up to account: " + name;
						LogAction.Information(logger, message);

						AccountService account = new (accountData, logger);
						await account.BackUp().ConfigureAwait(false);
					}
				}
			}
			catch (JsonException exception)
			{
				LogAction.Error(logger, "No accounts information", exception);
			}
		}
	}
}
