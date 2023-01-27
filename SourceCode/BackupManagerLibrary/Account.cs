/////////////////////////////////////////////////////////////////////////////
// <copyright file="Account.cs" company="James John McGuire">
// Copyright © 2017 - 2023 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

using Common.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using GoogleDriveFile = Google.Apis.Drive.v3.Data.File;

namespace BackupManagerLibrary
{
	/// <summary>
	/// Google Service Account class.
	/// </summary>
	public class Account : IDisposable
	{
		private static readonly ILog Log = LogManager.GetLogger(
			System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private readonly IList<DriveMapping> driveMappings =
			new List<DriveMapping>();

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
		/// Gets driveMappings property.
		/// </summary>
		/// <value>DriveMappings property.</value>
		public IList<DriveMapping> DriveMappings
		{
			get { return driveMappings; }
		}

		/// <summary>
		/// Report server folder information.
		/// </summary>
		/// <param name="serverFolder">The server folder to report on.</param>
		public static void ReportServerFolderInformation(
			GoogleDriveFile serverFolder)
		{
			if (serverFolder == null)
			{
				Log.Warn("server folder is null");
			}
			else
			{
				string message = string.Format(
					CultureInfo.InvariantCulture,
					"Checking server file {0} {1}",
					serverFolder.Id,
					serverFolder.Name);
				Log.Info(message);

				if (serverFolder.Owners == null)
				{
					Log.Warn("server folder owners null");
				}
				else
				{
					IList<Google.Apis.Drive.v3.Data.User> owners =
						serverFolder.Owners;

					string ownersInfo = "owners:";
					foreach (var user in owners)
					{
						var item = user.EmailAddress;
						ownersInfo += " " + item;
					}

					Log.Info(ownersInfo);
				}

				if (serverFolder.Parents == null)
				{
					Log.Warn("server folder parents is null");
				}
				else
				{
					IList<string> parents = serverFolder.Parents;

					string parentsInfo = "parents:";
					foreach (string item in parents)
					{
						parentsInfo += " " + item;
					}

					Log.Info(parentsInfo);
				}

				if (serverFolder.OwnedByMe == true)
				{
					Log.Info("File owned by me");
				}
				else if (serverFolder.Shared == true)
				{
					Log.Info("File shared with me");
				}
				else
				{
					Log.Info("File is neither owned by or shared with me");
				}
			}
		}

		/// <summary>
		/// Authenticating to Google using a Service account
		/// Documentation:
		/// https://developers.google.com/accounts/docs/OAuth2#serviceaccount.
		/// </summary>
		/// <returns>True upon success,false otherwise.</returns>
		public bool Authorize()
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

					authenticated = googleDrive.Authorize(accountsFile);
				}
			}
			catch (Exception exception) when
				(exception is ArgumentException ||
				exception is FileNotFoundException)
			{
				Log.Error(exception.ToString());
			}

			return authenticated;
		}

		/// <summary>
		/// Main back up method.
		/// </summary>
		/// <returns>A task indicating completion.</returns>
		public async Task BackUp()
		{
			bool authenticated = Authorize();

			if (authenticated == true)
			{
				foreach (DriveMapping driveMapping in DriveMappings)
				{
					string driveParentFolderId =
						driveMapping.DriveParentFolderId;

					string path = Environment.ExpandEnvironmentVariables(
						driveMapping.Path);
					path = Path.GetFullPath(path);

					driveMapping.ExpandExcludes();

					string message = string.Format(
						CultureInfo.InvariantCulture,
						"Checking: \"{0}\" with Parent Id: {1}",
						path,
						driveParentFolderId);
					Log.Info(message);

					await CreateTopLevelLink(
						driveParentFolderId, path).ConfigureAwait(false);

					string parentPath = Path.GetDirectoryName(path);

					IList<GoogleDriveFile> serverFiles =
						await googleDrive.GetFilesAsync(driveParentFolderId).
							ConfigureAwait(false);

					RemoveTopLevelAbandonedFiles(
						serverFiles, parentPath);

					string directoryName = Path.GetFileName(path);
					GoogleDriveFile serverFolder =
						GoogleDrive.GetFileInList(serverFiles, directoryName);

					await BackUp(
						driveMapping, serverFolder.Id, path).
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

		private static bool CheckProcessFile(
			DriveMapping driveMapping, string path)
		{
			bool processFile = true;

			foreach (Exclude exclude in driveMapping.Excludes)
			{
				ExcludeType clause = exclude.ExcludeType;

				if (clause == ExcludeType.File &&
					exclude.Path.Equals(
						path, StringComparison.OrdinalIgnoreCase))
				{
					processFile = false;
				}
			}

			return processFile;
		}

		private static bool CheckProcessRootFolder(
			DriveMapping driveMapping, string path)
		{
			bool processFiles = true;

			if (driveMapping.ExcludesContains(path))
			{
				Exclude exclude = driveMapping.GetExclude(path);
				ExcludeType clause = exclude.ExcludeType;

				if (clause == ExcludeType.OnlyRoot)
				{
					processFiles = false;
				}
			}

			return processFiles;
		}

		private static bool ShouldProcessFolder(
			DriveMapping driveMapping, string path)
		{
			bool processSubFolders = true;

			string directoryName = Path.GetDirectoryName(path);

			foreach (Exclude exclude in driveMapping.Excludes)
			{
				ExcludeType clause = exclude.ExcludeType;

				if (clause == ExcludeType.AllSubDirectories)
				{
					if (exclude.Path.Equals(
						directoryName, StringComparison.OrdinalIgnoreCase))
					{
						processSubFolders = false;
					}
				}
			}

			return processSubFolders;
		}

		private static void Delay()
		{
			System.Threading.Thread.Sleep(190);
		}

		private async Task BackUp(
			DriveMapping driveMapping,
			string driveParentId,
			string path)
		{
			try
			{
				if (System.IO.Directory.Exists(path))
				{
					bool processFolder =
						ShouldProcessFolder(driveMapping, path);

					if (processFolder == true)
					{
						IList<GoogleDriveFile> serverFiles =
							await googleDrive.GetFilesAsync(driveParentId).
								ConfigureAwait(false);

						RemoveExcludedItemsFromServer(
							driveMapping.Excludes, serverFiles);

						string[] subDirectories =
							System.IO.Directory.GetDirectories(path);

						RemoveAbandonedFolders(
							path, subDirectories, serverFiles);

						DirectoryInfo directoryInfo = new (path);

						foreach (string subDirectory in subDirectories)
						{
							string directoryName =
								Path.GetFileName(subDirectory);
							GoogleDriveFile serverFolder =
								GoogleDrive.GetFileInList(
									serverFiles, directoryName);

							if (serverFolder == null)
							{
								Log.Error("Server Folder is null for:" +
									subDirectory);
							}
							else
							{
								await BackUp(
									driveMapping,
									serverFolder.Id,
									subDirectory).ConfigureAwait(false);
							}
						}

						bool processFiles =
							CheckProcessRootFolder(driveMapping, path);

						if (processFiles == true)
						{
							FileInfo[] files = directoryInfo.GetFiles();

							GoogleDriveFile serverFolder =
								GoogleDrive.GetFileInList(
									serverFiles, directoryInfo.Name);

							ProcessFiles(
								path,
								driveMapping,
								files,
								driveParentId,
								serverFiles);
						}
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
				exception is System.Net.Http.HttpRequestException ||
				exception is System.Net.Sockets.SocketException ||
				exception is System.Security.SecurityException ||
				exception is TargetException ||
				exception is TaskCanceledException ||
				exception is UnauthorizedAccessException)
			{
				Log.Error(exception.ToString());
			}
		}

		private bool BackUpFile(
			string driveParentId,
			IList<GoogleDriveFile> serverFiles,
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
				Log.Info(message);

				GoogleDriveFile serverFile =
						GoogleDrive.GetFileInList(serverFiles, file.Name);

				Upload(driveParentId, file, serverFile, retry);

				success = true;
			}
			catch (AggregateException exception)
			{
				Log.Error("AggregateException caught");
				Log.Error(exception.ToString());

				foreach (Exception innerExecption in exception.InnerExceptions)
				{
					if (innerExecption is TaskCanceledException)
					{
						Log.Warn(exception.ToString());

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
				Log.Error(exception.ToString());

				retries = 0;
			}

			return success;
		}

		/// <summary>
		/// Create top level link.
		/// </summary>
		/// <remarks>This helps in maintaining the service accounts, as
		/// without it, files tend to fall into the 'black hole' of
		/// the service account.</remarks>
		/// <param name="targetId">The target id.</param>
		/// <param name="path">The local path being mapped.</param>
		/// <returns>A task indicating completion.</returns>
		private async Task CreateTopLevelLink(
			string targetId, string path)
		{
			try
			{
				string name = Path.GetFileName(path);

				string linkName = name + ".lnk";

				bool found = await googleDrive.DoesDriveItemExist(
					"root", linkName, "application/vnd.google-apps.shortcut").
					ConfigureAwait(false);

				if (found == false)
				{
					googleDrive.CreateLink("root", linkName, targetId);
				}
			}
			catch (Exception exception) when
				(exception is Google.GoogleApiException ||
				exception is System.Net.Http.HttpRequestException ||
				exception is System.Net.Sockets.SocketException)
			{
				Log.Error(exception.ToString());
			}
		}

		private void DeleteFromDrive(GoogleDriveFile file)
		{
			if (file.OwnedByMe == true)
			{
				string fileName = GoogleDrive.SanitizeFileName(file.Name);

				string message = string.Format(
					CultureInfo.InvariantCulture,
					"Deleting file from Server: {0}",
					fileName);
				Log.Info(message);

				googleDrive.Delete(file.Id);
				Delay();
			}
		}

		private void ProcessFiles(
			string path,
			DriveMapping driveMapping,
			FileInfo[] files,
			string driveParentId,
			IList<GoogleDriveFile> serverFiles)
		{
			bool skipThisDirectory = false;

			if (driveMapping.ExcludesContains(path))
			{
				Exclude exclude = driveMapping.GetExclude(path);
				ExcludeType clause = exclude.ExcludeType;

				if (clause == ExcludeType.Keep)
				{
					// Files have been marked as keep.
					skipThisDirectory = true;
				}
			}

			if (skipThisDirectory == false)
			{
				RemoveAbandonedFiles(files, serverFiles);
			}

			foreach (FileInfo file in files)
			{
				bool retry = false;
				bool success = false;
				retries = 2;

				while ((success == false) && (retries > 0))
				{
					bool checkFile =
						CheckProcessFile(driveMapping, file.FullName);

					if (checkFile == true)
					{
						success = BackUpFile(
							driveParentId, serverFiles, file, retry);

						if ((success == false) && (retries > 0))
						{
							retry = true;
							Delay();
						}
					}
					else
					{
						string message = string.Format(
							CultureInfo.InvariantCulture,
							"Excluding file from Server: {0}",
							file.FullName);
						Log.Info(message);

						RemoveAbandonedFile(file, serverFiles);

						success = true;
					}
				}
			}
		}

		private void RemoveAbandonedFile(
			FileInfo file, IList<GoogleDriveFile> serverFiles)
		{
			foreach (GoogleDriveFile serverFile in serverFiles)
			{
				try
				{
					if (!serverFile.MimeType.Equals(
						"application/vnd.google-apps.folder",
						StringComparison.Ordinal))
					{
						string serverFileName = serverFile.Name;

						if (serverFileName.Equals(
							file.Name, StringComparison.Ordinal))
						{
							DeleteFromDrive(serverFile);
							break;
						}
					}
				}
				catch (Google.GoogleApiException exception)
				{
					Log.Error(exception.ToString());
				}
			}
		}

		private void RemoveAbandonedFiles(
			FileInfo[] files,
			IList<GoogleDriveFile> serverFiles)
		{
			foreach (GoogleDriveFile file in serverFiles)
			{
				try
				{
					if (!file.MimeType.Equals(
						"application/vnd.google-apps.folder",
						StringComparison.Ordinal))
					{
						string fileName = file.Name;
						bool exists = files.Any(element => element.Name.Equals(
							fileName, StringComparison.Ordinal));

						if (exists == false)
						{
							DeleteFromDrive(file);
						}
					}
				}
				catch (Google.GoogleApiException exception)
				{
					Log.Error(exception.ToString());
				}
			}
		}

		private void RemoveAbandonedFolders(
			string path,
			string[] subDirectories,
			IList<GoogleDriveFile> serverFiles)
		{
			foreach (GoogleDriveFile file in serverFiles)
			{
				try
				{
					if (file.MimeType.Equals(
						"application/vnd.google-apps.folder",
						StringComparison.Ordinal))
					{
						string folderPath = path + @"\" + file.Name;
						bool exists =
							subDirectories.Any(element => element.Equals(
								folderPath, StringComparison.Ordinal));

						if (exists == false)
						{
							DeleteFromDrive(file);
						}
					}
				}
				catch (Google.GoogleApiException exception)
				{
					Log.Error(exception.ToString());
				}
			}
		}

		private void RemoveExcludedItemsFromServer(
			IList<Exclude> excludes,
			IList<GoogleDriveFile> serverFiles)
		{
			foreach (GoogleDriveFile serverFile in serverFiles)
			{
				foreach (Exclude exclude in excludes)
				{
					ExcludeType clause = exclude.ExcludeType;

					if (clause == ExcludeType.AllSubDirectories ||
						clause == ExcludeType.File)
					{
						if (serverFile.Name.Equals(
							exclude.Path, StringComparison.OrdinalIgnoreCase))
						{
							DeleteFromDrive(serverFile);
						}
					}
				}
			}
		}

		private void RemoveTopLevelAbandonedFiles(
			IList<GoogleDriveFile> serverFiles,
			string path)
		{
			string[] localEntries = Directory.GetFileSystemEntries(
				path, "*", SearchOption.TopDirectoryOnly);

			int count = serverFiles.Count;

			for (int index = count - 1; index >= 0; index--)
			{
				GoogleDriveFile file = serverFiles[index];
				if (file.OwnedByMe == true)
				{
					bool found = false;

					foreach (string localEntry in localEntries)
					{
						FileInfo fileInfo = new (localEntry);

						string name = fileInfo.Name;

						if (name.Equals(
							file.Name, StringComparison.OrdinalIgnoreCase))
						{
							found = true;
							break;
						}
					}

					if (found == false)
					{
						DeleteFromDrive(file);
					}
				}
			}
		}

		private void Upload(
			string driveParentId,
			FileInfo file,
			GoogleDriveFile serverFile,
			bool retry)
		{
			if (serverFile == null)
			{
				googleDrive.Upload(
					driveParentId, file.FullName, null, retry);
			}
			else if (serverFile.ModifiedTime < file.LastWriteTime)
			{
				// local file is newer
				googleDrive.Upload(
					driveParentId, file.FullName, serverFile.Id, retry);
			}
		}
	}
}
