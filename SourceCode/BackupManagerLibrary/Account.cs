﻿/////////////////////////////////////////////////////////////////////////////
// <copyright file="Account.cs" company="James John McGuire">
// Copyright © 2017 - 2020 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

using Common.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace BackupManagerLibrary
{
	public class Account : IDisposable
	{
		private static readonly ILog Log = LogManager.GetLogger(
			System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private readonly IList<Directory> directories = new List<Directory>();

		private GoogleDrive googleDrive;

		public Account()
		{
			googleDrive = new GoogleDrive();
		}

		public string Email { get; set; }

		public string ServiceAccount { get; set; }

		public IList<Directory> Directories
		{
			get { return directories; }
		}

		/// <summary>
		/// Authenticating to Google using a Service account
		/// Documentation:
		/// https://developers.google.com/accounts/docs/OAuth2#serviceaccount.
		/// </summary>
		/// <returns>True upon success,false otherwise.</returns>
		public bool Authenticate()
		{
			bool authenticated = false;

			try
			{
				string userProfilePath = Environment.GetFolderPath(
					Environment.SpecialFolder.UserProfile);
				string accountsPath = userProfilePath + @"\GoogleDrive";

				if (System.IO.Directory.Exists(accountsPath))
				{
					string accountsFile = accountsPath + @"\" + ServiceAccount;

					authenticated = googleDrive.Authenticate(accountsFile);
					authenticated = true;
				}
			}
			catch (Exception exception) when
				(exception is ArgumentException ||
				exception is FileNotFoundException)
			{
				Log.Error(CultureInfo.InvariantCulture, m => m(
					exception.ToString()));
			}

			return authenticated;
		}

		public async Task BackUp()
		{
			bool authenticated = Authenticate();

			if (authenticated == true)
			{
				CleanUp();

				foreach (Directory directory in Directories)
				{
					string path = Environment.ExpandEnvironmentVariables(
						directory.Path);

					directory.ExpandExcludes();

					IList<Google.Apis.Drive.v3.Data.File> serverFiles =
						googleDrive.GetFiles(directory.Parent);

					await BackUp(directory, directory.Parent, path, serverFiles).
						ConfigureAwait(false);
				}
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				// dispose managed resources
				googleDrive.Dispose();
				googleDrive = null;
			}

			// free native resources
		}

		private async Task BackUp(
			Directory directory,
			string parent,
			string path,
			IList<Google.Apis.Drive.v3.Data.File> serverFiles)
		{
			try
			{
				if (System.IO.Directory.Exists(path))
				{
					bool processSubFolders = true;
					bool processFiles = true;

					if (directory.ExcludesContains(path))
					{
						Exclude exclude = directory.GetExclude(path);
						ExcludeType clause = exclude.ExcludeType;

						if (clause == ExcludeType.OnlyRoot)
						{
							processFiles = false;
						}
						else
						{
							processSubFolders = false;
						}
					}

					if (processSubFolders == true)
					{
						DirectoryInfo directoryInfo = new DirectoryInfo(path);
						FileInfo[] files = directoryInfo.GetFiles();

						Google.Apis.Drive.v3.Data.File serverFolder =
							GoogleDrive.GetFileInList(
								serverFiles, directoryInfo.Name);

						if (serverFolder == null)
						{
							serverFolder = googleDrive.CreateFolder(
								parent, directoryInfo.Name);
							System.Threading.Thread.Sleep(200);
						}
						else
						{
							serverFiles =
								googleDrive.GetFiles(serverFolder.Id);
							System.Threading.Thread.Sleep(200);
						}

						string[] subDirectories =
							System.IO.Directory.GetDirectories(path);

						foreach (string subDirectory in subDirectories)
						{
							await BackUp(
								directory, serverFolder.Id, subDirectory, serverFiles).
								ConfigureAwait(false);
						}

						if (processFiles == true)
						{
							foreach (FileInfo file in files)
							{
								BackUpFile(serverFolder, serverFiles, file);
							}
						}
					}
				}
			}
			catch (Exception exception) when
				(exception is ArgumentException ||
				exception is ArgumentNullException ||
				exception is DirectoryNotFoundException ||
				exception is FileNotFoundException ||
				exception is IndexOutOfRangeException ||
				exception is InvalidOperationException ||
				exception is NullReferenceException ||
				exception is IOException ||
				exception is PathTooLongException ||
				exception is System.Security.SecurityException ||
				exception is TargetException ||
				exception is UnauthorizedAccessException)
			{
				Log.Error(CultureInfo.InvariantCulture, m => m(
					exception.ToString()));
			}
		}

		private void BackUpFile(
			Google.Apis.Drive.v3.Data.File serverFolder,
			IList<Google.Apis.Drive.v3.Data.File> serverFiles,
			FileInfo file)
		{
			try
			{
				string message = "Checking: " + file.FullName;
				Log.Info(CultureInfo.InvariantCulture, m => m(
					message));

				Google.Apis.Drive.v3.Data.File serverFile =
						GoogleDrive.GetFileInList(
							serverFiles, file.Name);

				if (serverFile == null)
				{
					googleDrive.Upload(serverFolder.Id, file.FullName, null);
				}
				else
				{
					if (serverFile.ModifiedTime < file.LastWriteTime)
					{
						// local file is newer
						googleDrive.Upload(
							serverFolder.Id, file.FullName, serverFile.Id);
					}
				}
			}
			catch (Exception exception) when
				(exception is ArgumentNullException ||
				exception is DirectoryNotFoundException ||
				exception is FileNotFoundException ||
				exception is IOException ||
				exception is NullReferenceException ||
				exception is IndexOutOfRangeException ||
				exception is InvalidOperationException ||
				exception is UnauthorizedAccessException)
			{
				Log.Error(CultureInfo.InvariantCulture, m => m(
					exception.ToString()));
			}
		}

		private void CleanUp()
		{
			string[] files = Array.Empty<string>();

			foreach (string file in files)
			{
				googleDrive.Delete(file);
			}
		}
	}
}
