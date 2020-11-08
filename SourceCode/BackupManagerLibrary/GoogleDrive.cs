/////////////////////////////////////////////////////////////////////////////
// <copyright file="GoogleDrive.cs" company="James John McGuire">
// Copyright © 2017 - 2020 James John McGuire. All Rights Reserved.
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

		private DriveService driveService;

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
		/// Authenticating to Google using a Service account
		/// Documentation:
		/// https://developers.google.com/accounts/docs/OAuth2#serviceaccount.
		/// </summary>
		/// <param name="credentialsFile">A file contiaining the credentials
		/// information.</param>
		/// <returns>True upon success,false otherwise.</returns>
		public bool Authenticate(string credentialsFile)
		{
			bool authenticated = false;

			GoogleCredential credentialedAccount =
				GoogleCredential.FromFile(credentialsFile);
			credentialedAccount = credentialedAccount.CreateScoped(Scopes);

			BaseClientService.Initializer initializer =
				new BaseClientService.Initializer();
			initializer.ApplicationName = "Backup Manager";
			initializer.HttpClientInitializer = credentialedAccount;

			driveService = new DriveService(initializer);

			if (driveService != null)
			{
				authenticated = true;
			}

			return authenticated;
		}

		public Google.Apis.Drive.v3.Data.File CreateFolder(
			string parent, string folderName)
		{
			Google.Apis.Drive.v3.Data.File fileMetadata =
				new Google.Apis.Drive.v3.Data.File();

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

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		public void Delete(string id)
		{
			FilesResource.DeleteRequest request = driveService.Files.Delete(id);

			request.Execute();
		}

		public IList<Google.Apis.Drive.v3.Data.File> GetFiles(string parent)
		{
			IList<Google.Apis.Drive.v3.Data.File> files;
			FilesResource.ListRequest listRequest = driveService.Files.List();

			listRequest.Fields = "files(id, name, modifiedTime)";
			listRequest.Q = $"'{parent}' in parents";
			listRequest.PageSize = 1000;

			Google.Apis.Drive.v3.Data.FileList filesList = listRequest.Execute();
			files = filesList.Files;

			return files;
		}

		public void Upload(
			string folder, string filePath, string fileId, bool retry)
		{
			if (retry == true)
			{
				TimeSpan timeOut = driveService.HttpClient.Timeout +
					TimeSpan.FromSeconds(100);
				driveService.HttpClient.Timeout = timeOut;
			}

			FileInfo file = new FileInfo(filePath);

			Google.Apis.Drive.v3.Data.File fileMetadata =
				new Google.Apis.Drive.v3.Data.File();
			fileMetadata.Name = file.Name;

			using FileStream stream = new System.IO.FileStream(
				filePath, System.IO.FileMode.Open);

			string mimeType = MimeTypes.GetMimeType(file.Name);

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

		private static void UploadResponseReceived(Google.Apis.Drive.v3.Data.File file)
		{
			string message = file.Name + " was uploaded successfully";

			Log.Info(CultureInfo.InvariantCulture, m => m(
				message));
		}
	}
}
