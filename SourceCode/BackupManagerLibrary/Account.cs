/////////////////////////////////////////////////////////////////////////////
// <copyright file="Account.cs" company="James John McGuire">
// Copyright © 2017 - 2021 James John McGuire. All Rights Reserved.
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
	/// <summary>
	/// Google Service Account class.
	/// </summary>
	public class Account : IDisposable
	{
		private static readonly ILog Log = LogManager.GetLogger(
			System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private readonly IList<Directory> directories = new List<Directory>();

		private GoogleDrive googleDrive;

		private int retries;

		/// <summary>
		/// Initializes a new instance of the <see cref="Account"/> class.
		/// </summary>
		public Account()
		{
			googleDrive = new GoogleDrive();
		}

		/// <summary>
		/// Gets or sets account email property.
		/// </summary>
		/// <value>Account email property.</value>
		public string Email { get; set; }

		/// <summary>
		/// Gets or sets service account property.
		/// </summary>
		/// <value>Service account property.</value>
		public string ServiceAccount { get; set; }

		/// <summary>
		/// Gets directories property.
		/// </summary>
		/// <value>Directories property.</value>
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
				string accountsPath = AccountsManager.DataPath;

				if (System.IO.Directory.Exists(accountsPath))
				{
					string accountsFile = accountsPath + @"\" + ServiceAccount;

					authenticated = googleDrive.Authenticate(accountsFile);
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

		/// <summary>
		/// Main back up method.
		/// </summary>
		/// <returns>A task indicating completion.</returns>
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
						googleDrive.GetFiles(directory.RootSharedFolderId);

					await BackUp(directory, directory.RootSharedFolderId, path, serverFiles).
						ConfigureAwait(false);
				}
			}
		}

		/// <summary>
		/// Dispose method.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Dispose method.
		/// </summary>
		/// <param name="disposing">Indicates currently disposing.</param>
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

		private static bool CheckProcessRootFolder(
			Directory directory, string path)
		{
			bool processFiles = true;

			if (directory.ExcludesContains(path))
			{
				Exclude exclude = directory.GetExclude(path);
				ExcludeType clause = exclude.ExcludeType;

				if (clause == ExcludeType.OnlyRoot)
				{
					processFiles = false;
				}
			}

			return processFiles;
		}

		private static bool CheckProcessSubFolders(
			Directory directory, string path)
		{
			bool processSubFolders = true;

			// Check for default ignore paths
			DirectoryInfo directoryInfo = new (path);
			string directoryName = directoryInfo.Name;

			if (directory.ExcludesContains(path) ||
				directory.ExcludesContains(directoryName))
			{
				Exclude exclude = directory.GetExclude(path);
				ExcludeType clause = exclude.ExcludeType;

				if (clause == ExcludeType.AllSubDirectories)
				{
					processSubFolders = false;
				}
			}

			return processSubFolders;
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
					DirectoryInfo directoryInfo = new (path);

					bool processSubFolders =
						CheckProcessSubFolders(directory, path);

					if (processSubFolders == true)
					{
						await ProcessSubFolders(
							directory, parent, path, serverFiles).
							ConfigureAwait(false);
					}
				}
			}
			catch (Exception exception) when
				(exception is ArgumentException ||
				exception is ArgumentNullException ||
				exception is DirectoryNotFoundException ||
				exception is FileNotFoundException ||
				exception is Google.GoogleApiException ||
				exception is IndexOutOfRangeException ||
				exception is InvalidOperationException ||
				exception is NullReferenceException ||
				exception is IOException ||
				exception is PathTooLongException ||
				exception is System.Security.SecurityException ||
				exception is TargetException ||
				exception is TaskCanceledException ||
				exception is UnauthorizedAccessException)
			{
				Log.Error(CultureInfo.InvariantCulture, m => m(
					exception.ToString()));
			}
		}

		private bool BackUpFile(
			Google.Apis.Drive.v3.Data.File serverFolder,
			IList<Google.Apis.Drive.v3.Data.File> serverFiles,
			FileInfo file,
			bool retry)
		{
			bool success = false;

			try
			{
				string fileName = GoogleDrive.SanitizeFileName(file.FullName);

				string message = string.Format(
					CultureInfo.InvariantCulture,
					"Checking: {0}",
					fileName);
				Log.Info(CultureInfo.InvariantCulture, m => m(
					message));

				Google.Apis.Drive.v3.Data.File serverFile =
						GoogleDrive.GetFileInList(
							serverFiles, file.Name);

				Upload(serverFolder, file, serverFile, retry);

				success = true;
			}
			catch (AggregateException exception)
			{
				Log.Error(CultureInfo.InvariantCulture, m => m(
					"AggregateException caught"));
				Log.Error(CultureInfo.InvariantCulture, m => m(
					exception.ToString()));

				foreach (Exception innerExecption in exception.InnerExceptions)
				{
					if (innerExecption is TaskCanceledException)
					{
						Log.Warn(CultureInfo.InvariantCulture, m => m(
							exception.ToString()));

						retries--;
					}
					else if (innerExecption is ArgumentNullException ||
						innerExecption is DirectoryNotFoundException ||
						innerExecption is FileNotFoundException ||
						innerExecption is FormatException ||
						innerExecption is IOException ||
						innerExecption is NullReferenceException ||
						innerExecption is IndexOutOfRangeException ||
						innerExecption is InvalidOperationException ||
						innerExecption is UnauthorizedAccessException)
					{
						retries = 0;
					}
					else
					{
						// Rethrow any other exception.
						throw;
					}
				}
			}
			catch (Exception exception) when
				(exception is ArgumentNullException ||
				exception is DirectoryNotFoundException ||
				exception is FileNotFoundException ||
				exception is FormatException ||
				exception is IOException ||
				exception is NullReferenceException ||
				exception is IndexOutOfRangeException ||
				exception is InvalidOperationException ||
				exception is UnauthorizedAccessException)
			{
				Log.Error(CultureInfo.InvariantCulture, m => m(
					exception.ToString()));

				retries = 0;
			}

			return success;
		}

		private void CleanUp()
		{
			string[] files = Array.Empty<string>();

			foreach (string file in files)
			{
				googleDrive.Delete(file);
			}
		}

		private void ProcessFiles(
			FileInfo[] files,
			Google.Apis.Drive.v3.Data.File serverFolder,
			IList<Google.Apis.Drive.v3.Data.File> serverFiles)
		{
			foreach (FileInfo file in files)
			{
				bool retry = false;
				bool success = false;
				retries = 2;

				while ((success == false) && (retries > 0))
				{
					success = BackUpFile(
						serverFolder, serverFiles, file, retry);

					if ((success == false) && (retries > 0))
					{
						retry = true;
						System.Threading.Thread.Sleep(200);
					}
				}
			}
		}

		private async Task ProcessSubFolders(
			Directory directory,
			string parent,
			string path,
			IList<Google.Apis.Drive.v3.Data.File> serverFiles)
		{
			DirectoryInfo directoryInfo = new (path);

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
					directory,
					serverFolder.Id,
					subDirectory,
					serverFiles).ConfigureAwait(false);
			}

			bool processFiles =
				CheckProcessRootFolder(directory, path);

			if (processFiles == true)
			{
				ProcessFiles(files, serverFolder, serverFiles);
			}
		}

		private void Upload(
			Google.Apis.Drive.v3.Data.File serverFolder,
			FileInfo file,
			Google.Apis.Drive.v3.Data.File serverFile,
			bool retry)
		{
			if (serverFile == null)
			{
				googleDrive.Upload(serverFolder.Id, file.FullName, null, retry);
			}
			else if (serverFile.ModifiedTime < file.LastWriteTime)
			{
				// local file is newer
				googleDrive.Upload(
					serverFolder.Id, file.FullName, serverFile.Id, retry);
			}
		}
	}
}
