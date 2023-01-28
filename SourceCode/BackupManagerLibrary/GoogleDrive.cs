/////////////////////////////////////////////////////////////////////////////
// <copyright file="GoogleDrive.cs" company="James John McGuire">
// Copyright © 2017 - 2023 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

using Common.Logging;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Upload;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;

using GoogleDriveFile = Google.Apis.Drive.v3.Data.File;

namespace BackupManagerLibrary
{
	/// <summary>
	/// Google drive class.
	/// </summary>
	public class GoogleDrive : IDisposable
	{
		private static readonly ILog Log = LogManager.GetLogger(
			System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private static readonly string[] Scopes =
		{
			DriveService.Scope.Drive,
			DriveService.Scope.DriveAppdata,
			DriveService.Scope.DriveFile,
			DriveService.Scope.DriveMetadata,
			DriveService.Scope.DriveMetadataReadonly,
			DriveService.Scope.DriveReadonly,
			DriveService.Scope.DriveScripts
		};

		// Included as member, as sometimes there is a
		// need to recreate service.
		private GoogleCredential credentialedAccount;
		private DriveService driveService;

		// Included as member, as sometimes there is a
		// need to recreate service.
		private BaseClientService.Initializer initializer;

		/// <summary>
		/// Create a very slight delay.
		/// </summary>
		public static void Delay()
		{
			System.Threading.Thread.Sleep(190);
		}

		/// <summary>
		/// Get file in list method.
		/// </summary>
		/// <param name="files">List of files.</param>
		/// <param name="name">The file to retrieve.</param>
		/// <returns>The retrieved file.</returns>
		public static GoogleDriveFile GetFileInList(
			IList<GoogleDriveFile> files, string name)
		{
			GoogleDriveFile file = null;

			if ((files != null) && (!string.IsNullOrWhiteSpace(name)))
			{
				foreach (GoogleDriveFile driveFile in files)
				{
					if (name.Equals(driveFile.Name, StringComparison.Ordinal))
					{
						file = driveFile;
						break;
					}
				}
			}

			return file;
		}

		/// <summary>
		/// Sanitize file name method.  Currently, just removes '{' and '{'.
		/// This is mainly used for logging, which tries to interpret
		/// those sympbols.
		/// </summary>
		/// <param name="fileName">The file name to sanitize.</param>
		/// <returns>The sanitized file name.</returns>
		public static string SanitizeFileName(string fileName)
		{
			if (!string.IsNullOrWhiteSpace(fileName))
			{
				fileName = fileName.Replace(
					"{", string.Empty, StringComparison.OrdinalIgnoreCase);
				fileName = fileName.Replace(
					"}", string.Empty, StringComparison.OrdinalIgnoreCase);
			}

			return fileName;
		}

		/// <summary>
		/// Authenticating to Google using a Service account
		/// Documentation:
		/// https://developers.google.com/accounts/docs/OAuth2#serviceaccount.
		/// </summary>
		/// <param name="credentialsFile">A file containing the credentials
		/// information.</param>
		/// <returns>True upon success,false otherwise.</returns>
		public bool Authorize(string credentialsFile)
		{
			bool authorized = false;

			if (!string.IsNullOrEmpty(credentialsFile) &&
				File.Exists(credentialsFile))
			{
				credentialedAccount =
					GoogleCredential.FromFile(credentialsFile);
				credentialedAccount = credentialedAccount.CreateScoped(Scopes);

				initializer = new BaseClientService.Initializer();
				initializer.ApplicationName = "Backup Manager";
				initializer.HttpClientInitializer = credentialedAccount;

				driveService = new DriveService(initializer);

				authorized = true;
			}

			return authorized;
		}

		/// <summary>
		/// Create folder method.
		/// </summary>
		/// <param name="parent">The parent of the folder.</param>
		/// <param name="folderName">The folder name.</param>
		/// <returns>A file object of the folder.</returns>
		public GoogleDriveFile CreateFolder(
			string parent, string folderName)
		{
			GoogleDriveFile file = null;

			if (string.IsNullOrWhiteSpace(parent))
			{
				Log.Error("GetFiles: parent is empty");
				Log.Error("StackTrace: " + Environment.StackTrace);
			}
			else
			{
				GoogleDriveFile fileMetadata = new ();

				fileMetadata.Name = folderName;
				fileMetadata.MimeType = "application/vnd.google-apps.folder";

				IList<string> parents = new List<string>();
				parents.Add(parent);
				fileMetadata.Parents = parents;

				FilesResource.CreateRequest request =
					driveService.Files.Create(fileMetadata);
				request.Fields = "id, name, parents";
				file = request.Execute();

				string message = string.Format(
					CultureInfo.InvariantCulture,
					"Created Folder ID: {0} Name {1}",
					file.Id,
					file.Name);
				Log.Info(message);
			}

			return file;
		}

		/// <summary>
		/// Create link method.
		/// </summary>
		/// <param name="parent">The parent of the folder.</param>
		/// <param name="linkName">The link name.</param>
		/// <param name="targetId">The target of the link.</param>
		/// <returns>A file object of the folder.</returns>
		public GoogleDriveFile CreateLink(
			string parent, string linkName, string targetId)
		{
			GoogleDriveFile fileMetadata = new ();

			fileMetadata.Name = linkName;
			fileMetadata.MimeType = "application/vnd.google-apps.shortcut";
			GoogleDriveFile.ShortcutDetailsData shortCut =
				new ();

			shortCut.TargetId = targetId;
			fileMetadata.ShortcutDetails = shortCut;

			IList<string> parents = new List<string>();
			parents.Add(parent);
			fileMetadata.Parents = parents;

			FilesResource.CreateRequest request =
				driveService.Files.Create(fileMetadata);
			request.Fields = "id, name, parents";
			GoogleDriveFile file = request.Execute();

			string message = string.Format(
				CultureInfo.InvariantCulture,
				"Created Link Id: {0} of: Name {1}",
				file.Id,
				file.Name);
			Log.Info(message);

			return file;
		}

		/// <summary>
		/// Delete method.
		/// </summary>
		/// <param name="id">The id of the item to delete.</param>
		public void Delete(string id)
		{
			FilesResource.DeleteRequest request =
				driveService.Files.Delete(id);

			request.Execute();
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
		/// Does drive item exist.
		/// </summary>
		/// <param name="parentId">The parent id to check in.</param>
		/// <param name="itemName">The name of the item.</param>
		/// <param name="mimeType">The mime type of the item.</param>
		/// <returns>Indicates whether the item was found or not.</returns>
		public async Task<bool> DoesDriveItemExist(
			string parentId, string itemName, string mimeType)
		{
			bool found = false;

			IList<GoogleDriveFile> serverFiles =
				await GetFilesAsync(parentId).ConfigureAwait(false);

			foreach (GoogleDriveFile file in serverFiles)
			{
				try
				{
					if (file.MimeType.Equals(
						mimeType, StringComparison.Ordinal))
					{
						found = file.Name.Equals(
							itemName, StringComparison.Ordinal);
						if (found == true)
						{
							break;
						}
					}
				}
				catch (Google.GoogleApiException exception)
				{
					Log.Error(exception.ToString());
				}
			}

			return found;
		}

		/// <summary>
		/// Get google drive file by id.
		/// </summary>
		/// <param name="driveId">The id of the google file.</param>
		/// <returns>The file object.</returns>
		public GoogleDriveFile GetFileById(string driveId)
		{
			GoogleDriveFile file =
				driveService.Files.Get(driveId).Execute();

			return file;
		}

		/// <summary>
		/// Get files async method.
		/// </summary>
		/// <param name="parent">The parent folder.</param>
		/// <returns>A list of files.</returns>
		public async Task<IList<GoogleDriveFile>> GetFilesAsync(
			string parent)
		{
			List<GoogleDriveFile> files = null;

			if (string.IsNullOrWhiteSpace(parent))
			{
				Log.Error("GetFiles: parent is empty");
				Log.Error("StackTrace: " + Environment.StackTrace);
			}
			else
			{
				files = new ();
				FilesResource.ListRequest listRequest =
					driveService.Files.List();

				string fileFields = "id, name, mimeType, modifiedTime, " +
					"ownedByMe, owners, parents, webContentLink";
				listRequest.Fields = string.Format(
					CultureInfo.InvariantCulture,
					"files({0}), nextPageToken",
					fileFields);
				listRequest.PageSize = 1000;
				listRequest.Q = $"'{parent}' in parents";

				do
				{
					try
					{
						Google.Apis.Drive.v3.Data.FileList filesList =
							await listRequest.ExecuteAsync()
							.ConfigureAwait(false);

						files.AddRange(filesList.Files);

						listRequest.PageToken = filesList.NextPageToken;

						string message = string.Format(
							CultureInfo.InvariantCulture,
							"Retrieved files from: {0} count: {1}",
							parent,
							files.Count);
						Log.Info(message);
					}
					catch (Google.GoogleApiException exception)
					{
						Log.Error(exception.ToString());
						listRequest.PageToken = null;
					}
				}
				while (!string.IsNullOrEmpty(listRequest.PageToken));
			}

			return files;
		}

		/// <summary>
		/// Get server folder.
		/// </summary>
		/// <param name="driveParentId">The drive parent id.</param>
		/// <param name="path">The local path.</param>
		/// <param name="serverFiles">A list of parents files.</param>
		/// <returns>A GoogleDriveFile mapping path.</returns>
		public GoogleDriveFile GetServerFolder(
			string driveParentId,
			string path,
			IList<GoogleDriveFile> serverFiles)
		{
			string directoryName =
				Path.GetFileName(path);
			GoogleDriveFile serverFolder =
				GetFileInList(
					serverFiles, directoryName);

			if (serverFolder == null)
			{
				serverFolder = CreateFolder(
					driveParentId, directoryName);
				Delay();
			}

			return serverFolder;
		}

		/// <summary>
		/// Move a drive file.
		/// </summary>
		/// <param name="file">The file.</param>
		/// <param name="destinationId">The id of destination folder.</param>
		public void MoveFile(
			GoogleDriveFile file, string destinationId)
		{
			if (file != null)
			{
				GoogleDriveFile placeHolder = new ();

				FilesResource.UpdateRequest updateRequest =
					driveService.Files.Update(placeHolder, file.Id);

				updateRequest.AddParents = destinationId;
				updateRequest.RemoveParents = file.Parents[0];
				updateRequest.Execute();
			}
		}

		/// <summary>
		/// Upload method.
		/// </summary>
		/// <param name="folder">The folder to upload to.</param>
		/// <param name="filePath">The file path to upload.</param>
		/// <param name="fileId">The file id.</param>
		public void Upload(string folder, string filePath, string fileId)
		{
			FileInfo file = new (filePath);

			GoogleDriveFile fileMetadata = new ();
			fileMetadata.Name = file.Name;

			using FileStream stream = new (filePath, System.IO.FileMode.Open);

			string mimeType = GetMimeType(file);

			Upload(folder, fileId, fileMetadata, stream, mimeType);
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
				driveService.Dispose();
				driveService = null;
			}

			// free native resources
		}

		private static string GetMimeType(FileInfo file)
		{
			string mimeType = MimeTypes.GetMimeType(file.Name);

			// overrides for wrongly marked files
			string extension = file.Extension;
			if (extension.Equals(
				".gdoc", StringComparison.OrdinalIgnoreCase) ||
				extension.Equals(
					".gsheet", StringComparison.OrdinalIgnoreCase))
			{
				Log.Info("Changing mime type to application/json");
				mimeType = "application/json";
			}

			return mimeType;
		}

		private static void UploadProgressChanged(IUploadProgress progress)
		{
			string message = string.Format(
				CultureInfo.InvariantCulture,
				"{0}: {1} bytes",
				progress.Status,
				progress.BytesSent);

			Log.Info(message);

			if (progress.Exception != null)
			{
				message = progress.Exception.ToString();
				Log.Error(message);
			}
		}

		private static void UploadResponseReceived(
			GoogleDriveFile file)
		{
			string fileName = SanitizeFileName(file.Name);
			string message = fileName + " was uploaded successfully";

			Log.Info(message);
		}

		private void Upload(
			string folder,
			string fileId,
			GoogleDriveFile fileMetadata,
			FileStream stream,
			string mimeType)
		{
			bool success = false;
			int retries = 2;

			do
			{
				try
				{
					if (retries < 2)
					{
						TimeSpan timeOut = driveService.HttpClient.Timeout +
							TimeSpan.FromSeconds(100);

						// apparentely, we need to reset the drive service
						driveService = new DriveService(initializer);
						driveService.HttpClient.Timeout = timeOut;

						Delay();
					}

					UploadCore(folder, fileId, fileMetadata, stream, mimeType);

					success = true;
				}
				catch (AggregateException exception)
				{
					Log.Error("AggregateException caught");
					Log.Error(exception.ToString());

					foreach (Exception innerExecption in
						exception.InnerExceptions)
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
			}
			while (success == false && retries > 0);
		}

		private void UploadCore(
			string folder,
			string fileId,
			GoogleDriveFile fileMetadata,
			FileStream stream,
			string mimeType)
		{
			if (string.IsNullOrWhiteSpace(fileId))
			{
				IList<string> parents = new List<string>();
				parents.Add(folder);
				fileMetadata.Parents = parents;

				FilesResource.CreateMediaUpload request =
					driveService.Files.Create(fileMetadata, stream, mimeType);

				request.Fields = "id, name, parents";

				request.ProgressChanged += UploadProgressChanged;
				request.ResponseReceived += UploadResponseReceived;

				request.Upload();
			}
			else
			{
				FilesResource.UpdateMediaUpload request =
					driveService.Files.Update(
						fileMetadata, fileId, stream, mimeType);

				request.Fields = "id, name, parents";

				request.ProgressChanged += UploadProgressChanged;
				request.ResponseReceived += UploadResponseReceived;
				request.Upload();
			}
		}
	}
}
