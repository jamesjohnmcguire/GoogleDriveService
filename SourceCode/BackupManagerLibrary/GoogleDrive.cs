/////////////////////////////////////////////////////////////////////////////
// <copyright file="GoogleDrive.cs" company="James John McGuire">
// Copyright © 2017 - 2021 James John McGuire. All Rights Reserved.
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
		/// Get file in list method.
		/// </summary>
		/// <param name="files">List of files.</param>
		/// <param name="name">The file to retrieve.</param>
		/// <returns>The retrieved file.</returns>
		public static Google.Apis.Drive.v3.Data.File GetFileInList(
			IList<Google.Apis.Drive.v3.Data.File> files, string name)
		{
			Google.Apis.Drive.v3.Data.File file = null;

			if ((files != null) && (!string.IsNullOrWhiteSpace(name)))
			{
				foreach (Google.Apis.Drive.v3.Data.File driveFile in files)
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
		/// This is mainly used for logging, which tries to interpret those sympbols.
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
		/// <param name="credentialsFile">A file contiaining the credentials
		/// information.</param>
		/// <returns>True upon success,false otherwise.</returns>
		public bool Authenticate(string credentialsFile)
		{
			bool authenticated;

			credentialedAccount = GoogleCredential.FromFile(credentialsFile);
			credentialedAccount = credentialedAccount.CreateScoped(Scopes);

			initializer = new BaseClientService.Initializer();
			initializer.ApplicationName = "Backup Manager";
			initializer.HttpClientInitializer = credentialedAccount;

			driveService = new DriveService(initializer);

			authenticated = true;

			return authenticated;
		}

		/// <summary>
		/// Create folder method.
		/// </summary>
		/// <param name="parent">The parent of the folder.</param>
		/// <param name="folderName">The folder name.</param>
		/// <returns>A file object of the folder.</returns>
		public Google.Apis.Drive.v3.Data.File CreateFolder(
			string parent, string folderName)
		{
			Google.Apis.Drive.v3.Data.File fileMetadata = new ();

			fileMetadata.Name = folderName;
			fileMetadata.MimeType = "application/vnd.google-apps.folder";

			IList<string> parents = new List<string>();
			parents.Add(parent);
			fileMetadata.Parents = parents;

			FilesResource.CreateRequest request =
				driveService.Files.Create(fileMetadata);
			request.Fields = "id, name, parents";
			Google.Apis.Drive.v3.Data.File file = request.Execute();

			string message = "Folder ID: " + file.Id;
			Log.Info(CultureInfo.InvariantCulture, m => m(
				message));

			return file;
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
		/// Delete method.
		/// </summary>
		/// <param name="id">The id of the item to delete.</param>
		public void Delete(string id)
		{
			FilesResource.DeleteRequest request = driveService.Files.Delete(id);

			request.Execute();
		}

		/// <summary>
		/// Get files method.
		/// </summary>
		/// <param name="parent">The parent folder.</param>
		/// <returns>A list of files.</returns>
		public IList<Google.Apis.Drive.v3.Data.File> GetFiles(string parent)
		{
			List<Google.Apis.Drive.v3.Data.File> files = new ();
			FilesResource.ListRequest listRequest = driveService.Files.List();

			listRequest.Fields = "files(id, name, mimeType, modifiedTime), " +
				"nextPageToken";
			listRequest.Q = $"'{parent}' in parents";
			listRequest.PageSize = 1000;

			do
			{
				try
				{
					string message = string.Format(
						CultureInfo.InvariantCulture,
						"Retrieved files from: {0} count: {1}",
						parent,
						files.Count);
					Log.Info(CultureInfo.InvariantCulture, m => m(
						message));

					Google.Apis.Drive.v3.Data.FileList filesList =
						listRequest.Execute();
					files.AddRange(filesList.Files);

					listRequest.PageToken = filesList.NextPageToken;
				}
				catch (Google.GoogleApiException exception)
				{
					Log.Error(CultureInfo.InvariantCulture, m => m(
						exception.ToString()));
					listRequest.PageToken = null;
				}
			}
			while (!string.IsNullOrEmpty(listRequest.PageToken));

			return files;
		}

		/// <summary>
		/// Upload method.
		/// </summary>
		/// <param name="folder">The folder to upload to.</param>
		/// <param name="filePath">The file path to upload.</param>
		/// <param name="fileId">The file id.</param>
		/// <param name="retry">Indicates whether to retry
		/// upon failure.</param>
		public void Upload(
			string folder, string filePath, string fileId, bool retry)
		{
			if (retry == true)
			{
				TimeSpan timeOut = driveService.HttpClient.Timeout +
					TimeSpan.FromSeconds(100);

				// apparentely, we need to reset the drive service
				driveService = new DriveService(initializer);
				driveService.HttpClient.Timeout = timeOut;
			}

			FileInfo file = new (filePath);

			Google.Apis.Drive.v3.Data.File fileMetadata = new ();
			fileMetadata.Name = file.Name;

			using FileStream stream = new (filePath, System.IO.FileMode.Open);

			string mimeType = MimeTypes.GetMimeType(file.Name);

			// over rides for wrongly marked files
			string extension = file.Extension;
			if (extension.Equals(".gdoc", StringComparison.OrdinalIgnoreCase) ||
				extension.Equals(".gsheet", StringComparison.OrdinalIgnoreCase))
			{
				Log.Info("Changing mime type to application/json");
				mimeType = "application/json";
			}

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

		private static void UploadProgressChanged(IUploadProgress progress)
		{
			string message = string.Format(
				CultureInfo.InvariantCulture,
				"{0}: {1} bytes",
				progress.Status,
				progress.BytesSent);

			Log.Info(CultureInfo.InvariantCulture, m => m(
				message));

			if (progress.Exception != null)
			{
				message = progress.Exception.ToString();
				Log.Error(CultureInfo.InvariantCulture, m => m(
					message));
			}
		}

		private static void UploadResponseReceived(
			Google.Apis.Drive.v3.Data.File file)
		{
			string fileName = SanitizeFileName(file.Name);
			string message = fileName + " was uploaded successfully";

			Log.Info(CultureInfo.InvariantCulture, m => m(
				message));
		}
	}
}
