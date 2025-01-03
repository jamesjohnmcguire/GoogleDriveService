/////////////////////////////////////////////////////////////////////////////
// <copyright file="GoogleServiceAccount.cs" company="James John McGuire">
// Copyright © 2017 - 2025 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using GoogleDriveFile = Google.Apis.Drive.v3.Data.File;

namespace DigitalZenWorks.BackUp.Library
{
	/// <summary>
	/// Google service account class.
	/// </summary>
	public class GoogleServiceAccount : BaseService, IDisposable
	{
		private GoogleDrive googleDrive;

		/// <summary>
		/// Initializes a new instance of the
		/// <see cref="GoogleServiceAccount"/> class.
		/// </summary>
		/// <param name="account">The accound data.</param>
		/// <param name="logger">The logger interface.</param>
		public GoogleServiceAccount(
			Account account, ILogger<BackUpService> logger = null)
			: base(account, logger)
		{
			googleDrive = new GoogleDrive(logger);
		}

		/// <summary>
		/// Report server folder information.
		/// </summary>
		/// <param name="serverFolder">The server folder to report on.</param>
		public void ReportServerFolderInformation(
			GoogleDriveFile serverFolder)
		{
			if (serverFolder == null)
			{
				LogAction.Warning(
					Logger, "server folder is null", null);
			}
			else
			{
				string message = string.Format(
					CultureInfo.InvariantCulture,
					"Checking server file {0} {1}",
					serverFolder.Id,
					serverFolder.Name);
				LogAction.Information(Logger, message);

				if (serverFolder.Owners == null)
				{
					LogAction.Warning(
						Logger, "server folder owners null", null);
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

					LogAction.Information(Logger, ownersInfo);
				}

				if (serverFolder.Parents == null)
				{
					LogAction.Warning(
						Logger, "server folder parents is null", null);
				}
				else
				{
					IList<string> parents = serverFolder.Parents;

					string parentsInfo = "parents:";
					foreach (string item in parents)
					{
						parentsInfo += " " + item;
					}

					LogAction.Information(Logger, parentsInfo);
				}

				if (serverFolder.OwnedByMe == true)
				{
					LogAction.Information(Logger, "File owned by me");
				}
				else if (serverFolder.Shared == true)
				{
					LogAction.Information(Logger, "File shared with me");
				}
				else
				{
					LogAction.Information(
						Logger, "File is neither owned by or shared with me");
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
				string accountsFile = GetServiceAccountJsonFile();

				authenticated = googleDrive.Authorize(accountsFile);
			}
			catch (Exception exception) when
				(exception is ArgumentException ||
				exception is FileNotFoundException)
			{
				LogAction.Exception(Logger, exception);
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
				foreach (DriveMapping driveMapping in
					Account.DriveMappings)
				{
					try
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
						LogAction.Information(Logger, message);

						IList<GoogleDriveFile> serverFiles =
							await googleDrive.GetFilesAsync(
								driveParentFolderId, true).ConfigureAwait(false);

						DirectoryInfo parentDirectory = Directory.GetParent(path);

						string[] subDirectories =
							Directory.GetDirectories(parentDirectory.FullName);
						List<string> paths = [.. subDirectories];
						IList<string> driveMappingPaths = Account.DriveMappingPaths;

						RemoveAbandonedFolders(
							parentDirectory.FullName,
							paths,
							driveMappingPaths,
							serverFiles,
							driveMapping.Excludes,
							Account.CheckDriveMappings);

						serverFiles = await googleDrive.GetFilesAsync(
								driveParentFolderId, false).ConfigureAwait(false);

						await BackUp(
							driveParentFolderId,
							path,
							serverFiles,
							driveMapping.Excludes).ConfigureAwait(false);
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
						LogAction.Exception(Logger, exception);
					}
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
		/// Removes the abandoned folders.
		/// </summary>
		/// <param name="path">The path.</param>
		/// <param name="subDirectories">The sub directories.</param>
		/// <param name="driveMappings">The drive mappings.</param>
		/// <param name="serverFiles">The server files.</param>
		/// <param name="excludes">The excludes.</param>
		/// <param name="checkDriveMappings">Indicates whether or not to check
		/// the drive mappings.</param>
		/// <returns>The amount of files removed.</returns>
		public int RemoveAbandonedFolders(
			string path,
			IList<string> subDirectories,
			IList<string> driveMappings,
			IList<GoogleDriveFile> serverFiles,
			IList<Exclude> excludes,
			bool checkDriveMappings = false)
		{
			int removedFilesCount = 0;

			if (serverFiles != null)
			{
				foreach (GoogleDriveFile file in serverFiles)
				{
					bool removed = RemoveAbandonedFolder(
						file,
						path,
						subDirectories,
						driveMappings,
						excludes,
						checkDriveMappings);

					if (removed == true)
					{
						removedFilesCount++;
					}
				}
			}

			return removedFilesCount;
		}

		/// <summary>
		/// Back up with drive mapping.
		/// </summary>
		/// <param name="driveMapping">The drive mapping.</param>
		/// <returns>A task object.</returns>
		protected async Task BackUp(DriveMapping driveMapping)
		{
			if (driveMapping != null)
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
				LogAction.Information(Logger, message);

				IList<GoogleDriveFile> serverFiles =
					await googleDrive.GetFilesAsync(
						driveParentFolderId, false).ConfigureAwait(false);

				await BackUp(
					driveParentFolderId,
					path,
					serverFiles,
					driveMapping.Excludes).ConfigureAwait(false);
			}
		}

		/// <summary>
		/// Back up files method.
		/// </summary>
		/// <param name="driveParentId">The drive parent id.</param>
		/// <param name="path">The path to back up.</param>
		/// <param name="serverFiles">The list of server files.</param>
		/// <param name="excludes">The list of excludes.</param>
		protected void BackUpFiles(
			string driveParentId,
			string path,
			IList<GoogleDriveFile> serverFiles,
			IList<Exclude> excludes)
		{
			bool processFiles = ShouldProcessFiles(excludes, path);

			if (processFiles == true)
			{
				DirectoryInfo directoryInfo = new (path);

				FileInfo[] files = directoryInfo.GetFiles();

				if (IgnoreAbandoned == false)
				{
					RemoveAbandonedFiles(path, files, serverFiles, excludes);
				}

				foreach (FileInfo file in files)
				{
					BackUpFile(driveParentId, file, serverFiles, excludes);
				}
			}
		}

		/// <summary>
		/// Dispose method.
		/// </summary>
		/// <param name="disposing">Indicates currently disposing.</param>
		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				googleDrive.Dispose();
				googleDrive = null;
			}
		}

		/// <summary>
		/// Remove abandoned files method.
		/// </summary>
		/// <param name="parentPath">The parent path.</param>
		/// <param name="files">The files to back up.</param>
		/// <param name="serverFiles">The list of server files.</param>
		/// <param name="excludes">The list of excludes.</param>
		protected void RemoveAbandonedFiles(
			string parentPath,
			FileInfo[] files,
			IList<GoogleDriveFile> serverFiles,
			IList<Exclude> excludes)
		{
			if (serverFiles != null)
			{
				bool skipThisDirectory = ShouldSkipThisDirectory(
					parentPath, excludes);

				if (skipThisDirectory == false)
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
								bool exists = files.Any(
									element => element.Name.Equals(
										serverFileName,
										StringComparison.Ordinal));

								if (exists == false)
								{
									googleDrive.Delete(serverFile);
								}
							}
						}
						catch (Google.GoogleApiException exception)
						{
							LogAction.Exception(Logger, exception);
						}
					}
				}
			}
		}

		/// <summary>
		/// Remove excluded items method.
		/// </summary>
		/// <param name="parentPath">The parent path.</param>
		/// <param name="serverFiles">The list of server files.</param>
		/// <param name="excludes">The list of excludes.</param>
		protected void RemoveExcludedItems(
			string parentPath,
			IList<GoogleDriveFile> serverFiles,
			IList<Exclude> excludes)
		{
			if (!string.IsNullOrWhiteSpace(parentPath) &&
				serverFiles != null &&
				excludes != null)
			{
				foreach (GoogleDriveFile serverFile in serverFiles)
				{
					foreach (Exclude exclude in excludes)
					{
						ExcludeType clause = exclude.ExcludeType;

						if (clause == ExcludeType.SubDirectory ||
							clause == ExcludeType.Global ||
							clause == ExcludeType.File)
						{
							string name = exclude.Path;
							string path = parentPath;
							bool isQualified =
								Path.IsPathFullyQualified(exclude.Path);

							if (isQualified == true)
							{
								name = Path.GetFileName(exclude.Path);
								path = Path.GetDirectoryName(exclude.Path);
							}

							if (serverFile.Name.Equals(
								name, StringComparison.OrdinalIgnoreCase) &&
								parentPath.Equals(
									path, StringComparison.OrdinalIgnoreCase))
							{
								googleDrive.Delete(serverFile);
							}
						}
					}
				}
			}
		}

		private static bool CheckForKeepExclude(
			string fileName, IList<Exclude> excludes)
		{
			bool keep = false;

			foreach (Exclude exclude in excludes)
			{
				string name = Path.GetFileName(exclude.Path);

				if (fileName.Equals(name, StringComparison.OrdinalIgnoreCase))
				{
					if (exclude.ExcludeType == ExcludeType.Keep ||
						exclude.ExcludeType == ExcludeType.FileIgnore)
					{
						keep = true;
						break;
					}
				}
			}

			return keep;
		}

		private static string GetNormalizedPath(string path)
		{
			path = path.Replace("\\", "/", StringComparison.OrdinalIgnoreCase);

			return path;
		}

		private async Task BackUp(
			string driveParentId,
			string path,
			IList<GoogleDriveFile> serverFiles,
			IList<Exclude> excludes)
		{
			try
			{
				if (System.IO.Directory.Exists(path))
				{
					bool processFolder = ShouldProcessFolder(excludes, path);

					if (processFolder == true)
					{
						GoogleDriveFile serverFolder =
							googleDrive.GetServerFolder(
								driveParentId, path, serverFiles);

						IList<GoogleDriveFile> thisServerFiles =
							await googleDrive.GetFilesAsync(
								serverFolder.Id, false).ConfigureAwait(false);

						string[] subDirectories =
							Directory.GetDirectories(path);
						List<string> paths = [.. subDirectories];

						IList<Exclude> expandExcludes =
							DriveMapping.ExpandGlobalExcludes(path, excludes);

						RemoveExcludedItems(
							path, thisServerFiles, expandExcludes);

						if (IgnoreAbandoned == false)
						{
							RemoveAbandonedFolders(
								path, paths, null, thisServerFiles, excludes);
						}

						DirectoryInfo directoryInfo = new (path);

						foreach (string subDirectory in subDirectories)
						{
							await BackUp(
								serverFolder.Id,
								subDirectory,
								thisServerFiles,
								excludes).ConfigureAwait(false);
						}

						BackUpFiles(
							serverFolder.Id, path, thisServerFiles, excludes);
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
				LogAction.Exception(Logger, exception);
			}
		}

		private void BackUpFile(
			string driveParentId,
			FileInfo file,
			IList<GoogleDriveFile> serverFiles,
			IList<Exclude> excludes)
		{
			try
			{
				bool checkFile = ShouldProcessFile(excludes, file.FullName);

				if (checkFile == true)
				{
					string fileName =
						GoogleDrive.SanitizeFileName(file.FullName);

					string message = string.Format(
						CultureInfo.InvariantCulture,
						"Checking: {0}",
						fileName);
					LogAction.Information(Logger, message);

					GoogleDriveFile serverFile =
							GoogleDrive.GetFileInList(serverFiles, file.Name);

					Upload(driveParentId, file, serverFile);
				}
				else
				{
					bool remove = ShouldRemoveFile(excludes, file.FullName);

					if (remove == true)
					{
						string message = string.Format(
							CultureInfo.InvariantCulture,
							"Excluding file from Server: {0}",
							file.FullName);
						LogAction.Information(Logger, message);

						RemoveExcludedFile(file, serverFiles);
					}
				}
			}
			catch (Exception exception) when
				(exception is ArgumentNullException ||
				exception is DirectoryNotFoundException ||
				exception is FileNotFoundException ||
				exception is FormatException ||
				exception is Google.GoogleApiException ||
				exception is IOException ||
				exception is NullReferenceException ||
				exception is IndexOutOfRangeException ||
				exception is InvalidOperationException ||
				exception is UnauthorizedAccessException)
			{
				LogAction.Exception(Logger, exception);
			}
		}

		private bool ExcludeKeepOrDeleteFile(
			GoogleDriveFile file, IList<Exclude> excludes)
		{
			bool removed = false;

			bool keep = true;

			if (excludes != null)
			{
				keep = CheckForKeepExclude(file.Name, excludes);
			}

			if (keep == false)
			{
				googleDrive.Delete(file);
				removed = true;
			}

			return removed;
		}

		private string GetServiceAccountJsonFile()
		{
			string serviceAccountJsonFile = null;

			try
			{
				string userProfilePath = Environment.GetFolderPath(
					Environment.SpecialFolder.UserProfile);
				string accountsPath = AccountsManager.DataPath;

				if (System.IO.Directory.Exists(accountsPath))
				{
					string accountsFile =
						accountsPath + @"\" + Account.AccountIdentifier;

					if (System.IO.File.Exists(accountsFile))
					{
						serviceAccountJsonFile = accountsFile;
					}
				}
			}
			catch (Exception exception) when
				(exception is ArgumentException ||
				exception is FileNotFoundException)
			{
				LogAction.Exception(Logger, exception);
			}

			return serviceAccountJsonFile;
		}

		private bool RemoveAbandonedFolder(
			GoogleDriveFile file,
			string path,
			IList<string> subDirectories,
			IList<string> driveMappings,
			IList<Exclude> excludes,
			bool checkDriveMappings = false)
		{
			bool removed = false;

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

					bool skipThisDirectory = ShouldSkipThisDirectory(
						folderPath, excludes);

					if (exists == false && skipThisDirectory == false)
					{
						removed = ExcludeKeepOrDeleteFile(file, excludes);
					}

					if (removed == false && checkDriveMappings == true)
					{
						path = GetNormalizedPath(path);
						folderPath = path + "/" + file.Name;

						exists = driveMappings.Any(
							element => element.Equals(
								folderPath, StringComparison.Ordinal));

						if (exists == false && skipThisDirectory == false)
						{
							removed = ExcludeKeepOrDeleteFile(file, excludes);
						}
					}
				}
			}
			catch (Google.GoogleApiException exception)
			{
				LogAction.Exception(Logger, exception);
			}

			return removed;
		}

		private void RemoveExcludedFile(
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
							googleDrive.Delete(serverFile);
							break;
						}
					}
				}
				catch (Google.GoogleApiException exception)
				{
					LogAction.Exception(Logger, exception);
				}
			}
		}

		private void Upload(
			string driveParentId,
			FileInfo file,
			GoogleDriveFile serverFile)
		{
			if (serverFile == null)
			{
				googleDrive.Upload(
					driveParentId, file.FullName, null);
			}
			else if (serverFile.ModifiedTimeDateTimeOffset < file.LastWriteTime)
			{
				// local file is newer
				googleDrive.Upload(
					driveParentId, file.FullName, serverFile.Id);
			}
		}
	}
}
