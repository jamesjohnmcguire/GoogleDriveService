/////////////////////////////////////////////////////////////////////////////
// <copyright file="AccountsManager.cs" company="James John McGuire">
// Copyright © 2017 - 2026 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.BackUp.Library;

using System.Collections.Generic;
using System.IO;
using LoggingService;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

/// <summary>
/// Google Service Accounts Manager.
/// </summary>
public static class AccountsManager
{
	/// <summary>
	/// Gets data path property.
	/// </summary>
	/// <value>Data path property.</value>
	public static string DataPath
	{
		get
		{
			string accountsPath = ApplicationPaths.GetDataDirectory();
			return accountsPath;
		}
	}

	/// <summary>
	/// Load accounts method.
	/// </summary>
	/// <param name="configurationFile">The configuration file.</param>
	/// <param name="logger">The logger interface.</param>
	/// <returns>A list of accounts.</returns>
	public static IList<Account> LoadAccounts(
		string configurationFile, ILogger<BackUpService> logger)
	{
		IList<Account> accounts = null;

		if (!string.IsNullOrWhiteSpace(configurationFile) &&
			System.IO.File.Exists(configurationFile))
		{
			string accountsText = File.ReadAllText(configurationFile);

			accounts =
				JsonConvert.DeserializeObject<IList<Account>>(accountsText);
		}
		else
		{
			logger.Error("Accounts file doesn't exist");
		}

		return accounts;
	}
}
