/////////////////////////////////////////////////////////////////////////////
// <copyright file="AccountService.cs" company="James John McGuire">
// Copyright © 2017 - 2023 James John McGuire. All Rights Reserved.
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
	/// Account Service class.
	/// </summary>
	public class AccountService
	{
		private readonly Account account;
		private readonly ILogger<BackUpService> logger;

		/// <summary>
		/// Initializes a new instance of the
		/// <see cref="AccountService"/> class.
		/// </summary>
		/// <param name="account">The accound data.</param>
		/// <param name="logger">The logger interface.</param>
		public AccountService(
			Account account, ILogger<BackUpService> logger = null)
		{
			this.logger = logger;
			this.account = account;
		}

		public ILogger<BackUpService> Logger { get => logger; }

		/// <summary>
		/// Gets the account data.
		/// </summary>
		public Account Account { get => account; }

		/// <summary>
		/// Gets the logger service.
		/// </summary>
		public ILogger<BackUpService> Logger { get => logger; }

		/// <summary>
		/// Authorize method.
		/// </summary>
		/// <returns>True upon success,false otherwise.</returns>
		public virtual bool Authorize()
		{
			return true;
		}

		/// <summary>
		/// Main back up method.
		/// </summary>
		/// <returns>A task indicating completion.</returns>
		public virtual async Task BackUp()
		{
			bool authenticated = Authorize();

			if (authenticated == true)
			{
				foreach (DriveMapping driveMapping in
					account.DriveMappings)
				{
					await BackUp(driveMapping).ConfigureAwait(false);
				}
			}
		}

		public virtual async Task BackUp(DriveMapping driveMapping)
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
			LogAction.Information(logger, message);

			await CreateTopLevelLink(
				driveParentFolderId, path).ConfigureAwait(false);

			IList<GoogleDriveFile> serverFiles =
				await googleDrive.GetFilesAsync(driveParentFolderId).
					ConfigureAwait(false);

			await BackUp(
				driveParentFolderId,
				path,
				serverFiles,
				driveMapping.Excludes).ConfigureAwait(false);
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
				googleDrive.Dispose();
				googleDrive = null;
			}
		}

		protected static bool ShouldSkipThisDirectory(
			string parentPath, IList<Exclude> excludes)
		{
			bool skipThisDirectory = false;

			foreach (Exclude exclude in excludes)
			{
				if (exclude.ExcludeType == ExcludeType.Keep)
				{
					if (parentPath.Equals(
						exclude.Path, StringComparison.OrdinalIgnoreCase))
					{
						skipThisDirectory = true;
						break;
					}
				}
			}

			return skipThisDirectory;
		}

		protected static bool ShouldProcessFile(
			IList<Exclude> excludes, string path)
		{
			bool processFile = true;

			foreach (Exclude exclude in excludes)
			{
				ExcludeType clause = exclude.ExcludeType;

				if (clause == ExcludeType.File)
				{
					if (exclude.Path.Equals(
						path, StringComparison.OrdinalIgnoreCase))
					{
						processFile = false;
						break;
					}
				}
			}

			return processFile;
		}

		private static bool ShouldProcessFiles(
			IList<Exclude> excludes, string path)
		{
			bool processFiles = true;

			foreach (Exclude exclude in excludes)
			{
				ExcludeType clause = exclude.ExcludeType;

				if (clause == ExcludeType.OnlyRoot)
				{
					if (exclude.Path.Equals(
						path, StringComparison.OrdinalIgnoreCase))
					{
						processFiles = false;
						break;
					}
				}
			}

			return processFiles;
		}

		protected static bool ShouldProcessFolder(
			IList<Exclude> excludes, string path)
		{
			bool processSubFolders = true;

			foreach (Exclude exclude in excludes)
			{
				ExcludeType clause = exclude.ExcludeType;

				if (clause == ExcludeType.AllSubDirectories)
				{
					bool isQualified =
						System.IO.Path.IsPathFullyQualified(exclude.Path);

					if (isQualified == true)
					{
						if (exclude.Path.Equals(
							path, StringComparison.OrdinalIgnoreCase))
						{
							processSubFolders = false;
							break;
						}
					}
					else
					{
						string name = Path.GetFileName(path);

						if (exclude.Path.Equals(
							name, StringComparison.OrdinalIgnoreCase))
						{
							processSubFolders = false;
							break;
						}
					}
				}
			}

			return processSubFolders;
		}

		protected override async Task BackUp(
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
							await googleDrive.GetFilesAsync(serverFolder.Id).
								ConfigureAwait(false);

						string[] subDirectories =
							System.IO.Directory.GetDirectories(path);

						RemoveExcludedItems(path, excludes, thisServerFiles);

						RemoveAbandonedFolders(
							path, subDirectories, thisServerFiles, excludes);

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
				googleDrive.LogAction.Exception(exception);
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
					LogAction.Information(logger, message);

					GoogleDriveFile serverFile =
							GoogleDrive.GetFileInList(serverFiles, file.Name);

					Upload(driveParentId, file, serverFile);
				}
				else
				{
					string message = string.Format(
						CultureInfo.InvariantCulture,
						"Excluding file from Server: {0}",
						file.FullName);
					LogAction.Information(logger, message);

					RemoveExcludedFile(file, serverFiles);
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
				googleDrive.LogAction.Exception(exception);
			}
		}

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

				RemoveAbandonedFiles(path, files, serverFiles, excludes);

				foreach (FileInfo file in files)
				{
					BackUpFile(driveParentId, file, serverFiles, excludes);
				}
			}
		}

		private void RemoveAbandonedFiles(
			string parentPath,
			FileInfo[] files,
			IList<GoogleDriveFile> serverFiles,
			IList<Exclude> excludes)
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
									serverFileName, StringComparison.Ordinal));

							if (exists == false)
							{
								googleDrive.Delete(serverFile);
							}
						}
					}
					catch (Google.GoogleApiException exception)
					{
						googleDrive.LogAction.Exception(exception);
					}
				}
			}
		}

		private void RemoveAbandonedFolders(string path, IList<Exclude> excludes)
		{
		}

		protected virtual void Remove(string hostFile)
		{
		}

		protected virtual void RemoveExcludedItem(
			string parentPath,
			IList<Exclude> excludes,
			string hostFile)
		{
			foreach (Exclude exclude in excludes)
			{
				ExcludeType clause = exclude.ExcludeType;

				if (clause == ExcludeType.AllSubDirectories ||
					clause == ExcludeType.File)
				{
					string name = exclude.Path;
					string path = parentPath;
					bool isQualified =
						System.IO.Path.IsPathFullyQualified(exclude.Path);

					if (isQualified == true)
					{
						name = Path.GetFileName(exclude.Path);
						path = Path.GetDirectoryName(exclude.Path);
					}

					if (hostFile.Equals(
						name, StringComparison.OrdinalIgnoreCase) &&
						parentPath.Equals(
							path, StringComparison.OrdinalIgnoreCase))
					{
						Remove(hostFile);
					}
				}
			}
		}

		protected void RemoveExcludedItems(
			string parentPath,
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
						string name = exclude.Path;
						string path = parentPath;
						bool isQualified =
							System.IO.Path.IsPathFullyQualified(exclude.Path);

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
						googleDrive.Delete(file);
					}
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
			else if (serverFile.ModifiedTime < file.LastWriteTime)
			{
				// local file is newer
				googleDrive.Upload(
					driveParentId, file.FullName, serverFile.Id);
			}
		}
	}
}
