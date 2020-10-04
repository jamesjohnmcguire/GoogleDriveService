/////////////////////////////////////////////////////////////////////////////
// <copyright file="Backup.cs" company="James John McGuire">
// Copyright © 2017 - 2020 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

using Common.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.IO;

namespace BackupManagerLibrary
{
	public static class Backup
	{
		private static readonly ILog Log = LogManager.GetLogger(
			System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public static void Run()
		{
			try
			{
				IList<Account> accounts = AccountsManager.LoadAccounts();

				if (accounts.Count > 0)
				{
					DisplayConfig();
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
			catch (Exception exception) when
				(exception is JsonException)
			{
				Log.Error(CultureInfo.InvariantCulture, m => m(
					exception.ToString()));
			}
		}

		private static void CustomInitialization()
		{
			try
			{
				Log.Info(CultureInfo.InvariantCulture, m => m(
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

					Log.Info(CultureInfo.InvariantCulture, m => m(
						"CustomInitialization: Copying files"));

					foreach (string file in files)
					{
						System.IO.FileInfo fileInfo = new FileInfo(file);

						string destination = fileInfo.DirectoryName +
							@"\Backups\" + fileInfo.Name;
						System.IO.File.Copy(file, destination);
					}
				}

				using Process outlook = new Process();
				outlook.StartInfo.FileName =
					@"C:\Program Files\Microsoft Office\root\Office16\OUTLOOK.EXE";
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
				Log.Error(CultureInfo.InvariantCulture, m => m(
					exception.ToString()));
			}
		}

		private static void DisplayConfig()
		{
			foreach (string key in ConfigurationManager.AppSettings)
			{
				string value = ConfigurationManager.AppSettings[key];

				Log.Info(CultureInfo.InvariantCulture, m => m(
					"Config: " + key + ": " + value));
			}
		}
	}
}
