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
	public class BackUpService : IBackUpService
	{
		private readonly ILogger<BackUpService> logger;

		private string parentId;
		private string path;

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
		/// Gets or Sets a value indicating whether to ignore abandoned files.
		/// </summary>
		/// <value>A value indicating whether to ignore abandoned files.</value>
		public bool IgnoreAbandoned { get; set; }

		/// <summary>
		/// Run method.
		/// </summary>
		/// <param name="configurationFile">The configuration file.</param>
		/// <returns>A task indicating completion.</returns>
		public async Task BackUp(string configurationFile)
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
						try
						{
							string name = accountData.AccountIdentifier;
							string message = "Backing up to account: " + name;
							LogAction.Information(logger, message);

							switch (accountData.AccountType)
							{
								case AccountType.GoogleServiceAccount:
								{
									using GoogleServiceAccount account =
										new (accountData, logger);

									account.IgnoreAbandoned = IgnoreAbandoned;

									await account.BackUp().
											ConfigureAwait(false);
									break;
								}

								default:
									break;
							}
						}
						catch (System.Net.Http.HttpRequestException exception)
						{
							LogAction.Error(logger, "HTTP Error", exception);
						}
					}
				}
			}
			catch (JsonException exception)
			{
				LogAction.Error(logger, "Accounts File Malformed", exception);
			}
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
		}
	}
}
