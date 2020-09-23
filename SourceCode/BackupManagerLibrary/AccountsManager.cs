using Google.Apis.Drive.v3;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace BackupManagerLibrary
{
	public class AccountsManager
	{
		public IList<Account> LoadAccounts()
		{
			IList<Account> accounts = null;

			string userProfilePath = Environment.GetFolderPath(
				Environment.SpecialFolder.UserProfile);
			string accountsPath = userProfilePath + @"\GoogleDrive";

			if (System.IO.Directory.Exists(accountsPath))
			{
				string accountsFile = accountsPath + @"\Accounts.json";
				string accountsText = File.ReadAllText(accountsFile);

				accounts = JsonConvert.DeserializeObject<IList<Account>>(accountsText);
			}

			return accounts;
		}
	}
}
