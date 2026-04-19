/////////////////////////////////////////////////////////////////////////////
// <copyright file="BackUpService.cs" company="James John McGuire">
// Copyright © 2017 - 2026 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

[assembly: System.CLSCompliant(false)]

namespace DigitalZenWorks.BackUp.Library;

using System.Collections.Generic;
using System.Threading.Tasks;
using LoggingService;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

/// <summary>
/// Back up class.
/// </summary>
public class BackUpService(ILogger<BackUpService> logger = null)
	: IBackUpService
{
	private readonly ILogger<BackUpService> logger = logger;

	private string parentId;
	private string path;

	/// <summary>
	/// Gets or Sets a value indicating whether to ignore abandoned files.
	/// </summary>
	/// <value>A value indicating whether to ignore abandoned files.</value>
	public bool IgnoreAbandoned { get; set; }

	/// <summary>
	/// Gets or sets the application settings that configure the behavior of
	/// the application.
	/// </summary>
	/// <remarks>This property allows for the retrieval and modification of
	/// settings that affect application functionality. Changes to the settings
	/// will be reflected throughout the application.</remarks>
	public Settings Settings { get; set; }

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
				logger.Error("No accounts information");
			}
			else
			{
				foreach (Account accountData in accounts)
				{
					await BackUpAccount(accountData).ConfigureAwait(false);
				}
			}
		}
		catch (JsonException exception)
		{
			logger.Error("Accounts File Malformed");
			logger.Exception(exception);
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

	private async Task BackUpAccount(Account accountData)
	{
		try
		{
			string name = accountData.AccountIdentifier;
			string message = "Backing up to account: " + name;
			logger.Information(message);

			switch (accountData.AccountType)
			{
				case AccountType.GoogleServiceAccount:
					{
						using GoogleServiceAccount account =
							new(accountData, Settings, logger);

						account.IgnoreAbandoned = IgnoreAbandoned;

						await account.BackUp().ConfigureAwait(false);
						break;
					}

				default:
					break;
			}
		}
		catch (System.Net.Http.HttpRequestException exception)
		{
			logger.Error("HTTP Error");
			logger.Exception(exception);
		}
	}
}
