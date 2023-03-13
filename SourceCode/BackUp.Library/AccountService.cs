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

		protected static bool ShouldSkipThisDirectory(
			string parentPath, IList<Exclude> excludes)
		{
			bool skipThisDirectory = false;

			if (!string.IsNullOrWhiteSpace(parentPath) && excludes != null)
			{
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
			}

			return skipThisDirectory;
		}

		protected static bool ShouldProcessFile(
			IList<Exclude> excludes, string path)
		{
			bool processFile = true;

			if (excludes != null)
			{
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
			}

			return processFile;
		}

		protected static bool ShouldProcessFiles(
			IList<Exclude> excludes, string path)
		{
			bool processFiles = true;

			if (excludes != null)
			{
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
			}

			return processFiles;
		}

		protected static void RemoveAbandonedFiles(
			string parentPath,
			FileInfo[] files,
			IList<Exclude> excludes)
		{
			bool skipThisDirectory = ShouldSkipThisDirectory(
				parentPath, excludes);

			if (skipThisDirectory == false)
			{
			}
		}

		protected static void RemoveAbandonedFolders(string path, IList<Exclude> excludes)
		{
		}

		protected static void RemoveExcludedItems(
			string parentPath,
			IList<Exclude> excludes)
		{
			if (excludes != null)
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
					}
				}
			}
		}

		protected static bool ShouldProcessFolder(
			IList<Exclude> excludes, string path)
		{
			bool processSubFolders = true;

			if (excludes != null)
			{
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
			}

			return processSubFolders;
		}

		/// <summary>
		/// Back up with drive mapping.
		/// </summary>
		/// <param name="driveMapping">The drive mapping.</param>
		/// <returns></returns>
		protected virtual async Task BackUp(DriveMapping driveMapping)
		{
			if (driveMapping != null)
			{
				string driveParentFolderId = driveMapping.DriveParentFolderId;

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

				await BackUp(driveParentFolderId, path, driveMapping.Excludes).
					ConfigureAwait(false);
			}
		}

		protected void BackUpFiles(
			string driveParentId,
			string path,
			IList<Exclude> excludes)
		{
			bool processFiles = ShouldProcessFiles(excludes, path);

			if (processFiles == true)
			{
				DirectoryInfo directoryInfo = new (path);

				FileInfo[] files = directoryInfo.GetFiles();

				foreach (FileInfo file in files)
				{
					BackUpFile(driveParentId, file, excludes);
				}
			}
		}

		protected virtual void Remove(string hostFile)
		{
		}

		protected virtual void RemoveExcludedItem(
			string parentPath,
			string hostFile,
			IList<Exclude> excludes)
		{
			if (!string.IsNullOrWhiteSpace(parentPath) &&
				!string.IsNullOrWhiteSpace(hostFile) &&
				excludes != null)
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
		}

		private static void RemoveTopLevelAbandonedFiles(string path)
		{
			string[] localEntries = Directory.GetFileSystemEntries(
				path, "*", SearchOption.TopDirectoryOnly);

			foreach (string localEntry in localEntries)
			{
				FileInfo fileInfo = new (localEntry);

				string name = fileInfo.Name;
			}
		}

		private async Task BackUp(
			string driveParentId,
			string path,
			IList<Exclude> excludes)
		{
			try
			{
				if (System.IO.Directory.Exists(path))
				{
					bool processFolder = ShouldProcessFolder(excludes, path);

					if (processFolder == true)
					{
						string[] subDirectories =
							System.IO.Directory.GetDirectories(path);

						DirectoryInfo directoryInfo = new (path);

						foreach (string subDirectory in subDirectories)
						{
							await BackUp(
								"1",
								subDirectory,
								excludes).ConfigureAwait(false);
						}

						BackUpFiles("1", path, excludes);
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
				LogAction.Exception(logger, exception);
			}
		}

		private void BackUpFile(
			string driveParentId,
			FileInfo file,
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
				}
				else
				{
					string message = string.Format(
						CultureInfo.InvariantCulture,
						"Excluding file from Server: {0}",
						file.FullName);
					LogAction.Information(logger, message);
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
				LogAction.Exception(logger, exception);
			}
		}
	}
}
