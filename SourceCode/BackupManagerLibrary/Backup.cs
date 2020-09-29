using Common.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;

namespace BackupManagerLibrary
{
	public class Backup
	{
		private static readonly ILog log = LogManager.GetLogger
			(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

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
				log.Info(CultureInfo.InvariantCulture, m => m(
					"CustomInitialization"));
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

					log.Info(CultureInfo.InvariantCulture, m => m(
						"CustomInitialization: Copying files"));

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
				log.Error(CultureInfo.InvariantCulture, m => m(
					exception.ToString()));
			}
		}
	}
}
