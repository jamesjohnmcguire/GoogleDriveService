using System;
using System.Collections.Generic;
using System.Text;

namespace BackupManagerLibrary
{
	public class Backup
	{
		public void Run()
		{
			AccountsManager accountManager = new AccountsManager();

			IList<Account> accounts = accountManager.LoadAccounts();

			foreach(Account account in accounts)
			{
				bool authenticated = account.Authenticate();

				if (authenticated == true)
				{

				}
			}
		}
	}
}
