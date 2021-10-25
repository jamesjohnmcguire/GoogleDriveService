/////////////////////////////////////////////////////////////////////////////
// <copyright file="Backup.cs" company="James John McGuire">
// Copyright © 2017 - 2021 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

using Common.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;

[assembly: CLSCompliant(false)]

namespace BackupManagerLibrary
{
	/// <summary>
	/// Back up class.
	/// </summary>
	public static class Backup
	{
		private static readonly ILog Log = LogManager.GetLogger(
			System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// Run method.
		/// </summary>
		/// <param name="useCustomInitialization">Indicates whether to use
		/// custom initialation.</param>
		/// <returns>A task indicating completion.</returns>
		public static async Task Run(bool useCustomInitialization)
		{
			try
			{
				IList<Account> accounts = AccountsManager.LoadAccounts();

				if ((accounts == null) || (accounts.Count == 0))
				{
					Log.Error(CultureInfo.InvariantCulture, m => m(
						"No accounts information"));
				}
				else
				{
					if (useCustomInitialization == true)
					{
						Log.Info("Using custom initialization");
						CustomInitialization();
					}

					foreach (Account account in accounts)
					{
						string name = account.ServiceAccount;
						string message = "Backing up to account: " + name;
						Log.Info(CultureInfo.InvariantCulture, m => m(
							message));

						await account.BackUp().ConfigureAwait(false);
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
				string backupPath = dataPath + @"\Backups\";

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
						System.IO.FileInfo fileInfo = new (file);

						string destination = backupPath + fileInfo.Name;
						System.IO.File.Copy(file, destination, true);
					}
				}

				using Process outlook = new ();
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
	}
}
