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

		private static string[] scopes =
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
			credentialedAccount = credentialedAccount.CreateScoped(scopes);

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

		public IList<Google.Apis.Drive.v3.Data.File> GetFiles(string parent)
		{
			IList<Google.Apis.Drive.v3.Data.File> files = null;
			FilesResource.ListRequest listRequest = driveService.Files.List();

			listRequest.Fields = "files(id, name, modifiedTime)";
			listRequest.Q = $"'{parent}' in parents";

			Google.Apis.Drive.v3.Data.FileList filesList = listRequest.Execute();
			files = filesList.Files;

			return files;
		}

		public async Task Upload(string filePath, string folder)
		{
			FileInfo file = new FileInfo(filePath);

			Google.Apis.Drive.v3.Data.File fileMetadata =
				new Google.Apis.Drive.v3.Data.File();
			fileMetadata.Name = file.Name;

			IList<string> parents = new List<string>();
			parents.Add(folder);
			fileMetadata.Parents = parents;

			using FileStream stream = new System.IO.FileStream(
				filePath, System.IO.FileMode.Open);

			string mimeType = MimeTypes.GetMimeType(file.Name);

			FilesResource.CreateMediaUpload request =
				driveService.Files.Create(fileMetadata, stream, mimeType);
			request.Fields = "id, name, parents";

			request.ProgressChanged += UploadProgressChanged;
			request.ResponseReceived += UploadResponseReceived;

			await request.UploadAsync().ConfigureAwait(false);
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
				"{0}: {1}",
				progress.Status,
				progress.BytesSent);

			Log.Info(CultureInfo.InvariantCulture, m => m(
				message));

			if (progress.Exception != null)
			{
				Log.Error(CultureInfo.InvariantCulture, m => m(
					progress.Exception.ToString()));
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
