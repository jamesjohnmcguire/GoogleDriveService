using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace BackupManagerLibrary
{
	public class Backup
	{
		public void Run()
		{
			AccountsManager accountManager = new AccountsManager();

			IList<Account> accounts = accountManager.LoadAccounts();

			if (accounts.Count > 0)
			{
				CustomInitialization();

				foreach (Account account in accounts)
				{
					bool authenticated = account.Authenticate();

					if (authenticated == true)
					{

					}
				}
			}
		}

		private static void CustomInitialization()
		{
			try
			{
				Process[] processes = Process.GetProcessesByName("outlook");

				foreach (Process process in processes)
				{
					process.CloseMainWindow();
					process.WaitForExit();
				}

				// copy PSTs to backup
				string profilePath = Environment.GetFolderPath(
					Environment.SpecialFolder.UserProfile);
				string dataPath = profilePath + @"\Data\ProgramData\Outlook";
				string backupPath = dataPath + @"\Backups";

				if (System.IO.Directory.Exists(dataPath))
				{
					if (!System.IO.Directory.Exists(backupPath))
					{
						System.IO.Directory.CreateDirectory(backupPath);
					}

					string[] files = System.IO.Directory.GetFiles(dataPath);

					foreach (string file in files)
					{
						System.IO.FileInfo fileInfo = new FileInfo(file);

						string destination =
							fileInfo.DirectoryName + @"\Backups\" + fileInfo.Name;
						System.IO.File.Copy(file, destination);
					}
				}

				Process outlook = new Process();
				outlook.StartInfo.FileName = "outlook.exe";
				outlook.Start();
			}
			catch (Exception exception) when
				(exception is ArgumentException ||
				exception is ArgumentNullException ||
				exception is InvalidOperationException ||
				exception is PlatformNotSupportedException ||
				exception is SystemException ||
				exception is Win32Exception)
			{
				// Log
			}
		}
	}
}
