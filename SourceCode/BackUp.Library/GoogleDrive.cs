/////////////////////////////////////////////////////////////////////////////
// <copyright file="GoogleDrive.cs" company="James John McGuire">
// Copyright © 2017 - 2026 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.BackUp.Library;

using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Upload;
using LoggingService;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using GoogleDriveFile = Google.Apis.Drive.v3.Data.File;

/// <summary>
/// Google drive class.
/// </summary>
/// <remarks>
/// Initializes a new instance of the
/// <see cref="GoogleDrive"/> class.
/// </remarks>
/// <param name="logger">The logger interface.</param>
public class GoogleDrive(ILogger<BackUpService> logger = null)
	: IDisposable
{
	private static readonly string[] Scopes =
	[
		DriveService.Scope.Drive,
		DriveService.Scope.DriveAppdata,
		DriveService.Scope.DriveFile,
		DriveService.Scope.DriveMetadata,
		DriveService.Scope.DriveMetadataReadonly,
		DriveService.Scope.DriveReadonly,
		DriveService.Scope.DriveScripts
	];

	private readonly ILogger<BackUpService> logger = logger;

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
	public async Task<bool> Authorize(string credentialsFile)
	{
		bool authorized = false;

		if (!string.IsNullOrEmpty(credentialsFile) &&
			File.Exists(credentialsFile))
		{
			using var cts = new CancellationTokenSource();

			ServiceAccountCredential serviceAccountCredential =
				await CredentialFactory.FromFileAsync<ServiceAccountCredential>(
					credentialsFile, cts.Token).ConfigureAwait(false);

			credentialedAccount =
				serviceAccountCredential.ToGoogleCredential();

			credentialedAccount = credentialedAccount.CreateScoped(Scopes);

			initializer = new BaseClientService.Initializer();
			initializer.ApplicationName = "Back Up Service Manager";
			initializer.HttpClientInitializer = credentialedAccount;

			driveService = new DriveService(initializer);
			driveService.HttpClient.Timeout = TimeSpan.FromMinutes(3);

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
			logger.Error("GetFiles: parent is empty");

			string message = "StackTrace: " + Environment.StackTrace;
			logger.Error(message);
		}
		else
		{
			GoogleDriveFile fileMetadata = new();

			fileMetadata.Name = folderName;
			fileMetadata.MimeType = "application/vnd.google-apps.folder";

			List<string> parents = [parent];
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
			logger.Information(message);
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
		GoogleDriveFile fileMetadata = new();

		fileMetadata.Name = linkName;
		fileMetadata.MimeType = "application/vnd.google-apps.shortcut";
		GoogleDriveFile.ShortcutDetailsData shortCut = new();

		shortCut.TargetId = targetId;
		fileMetadata.ShortcutDetails = shortCut;

		List<string> parents = [parent];
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
		logger.Information(message);

		return file;
	}

	/// <summary>
	/// Delete method.
	/// </summary>
	/// <param name="file">The google drive file to delete.</param>
	public void Delete(GoogleDriveFile file)
	{
		if (file != null)
		{
			string fileName = SanitizeFileName(file.Name);

			if (file.OwnedByMe == true)
			{
				string message = $"Deleting file from Server: {fileName}";
				logger.Information(message);

				FilesResource.DeleteRequest request =
					driveService.Files.Delete(file.Id);
				request.Execute();

				Delay();
			}
			else
			{
				string message =
					$"Attempting to delete a file not owned by me: {fileName}";
				logger.Warning(message);
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
	/// Does drive item exist.
	/// </summary>
	/// <param name="parentId">The parent id to check in.</param>
	/// <param name="itemName">The name of the item.</param>
	/// <param name="mimeType">The mime type of the item.</param>
	/// <returns>The id of the item, if found, null otherwise.</returns>
	public async Task<string> DoesDriveItemExist(
		string parentId, string itemName, string mimeType)
	{
		string itemId = null;
		bool found = false;

		IList<GoogleDriveFile> serverFiles =
			await GetFilesAsync(parentId, false).ConfigureAwait(false);

		foreach (GoogleDriveFile file in serverFiles)
		{
			try
			{
				if (file.MimeType.Equals(mimeType, StringComparison.Ordinal))
				{
					found =
						file.Name.Equals(itemName, StringComparison.Ordinal);

					if (found == true)
					{
						itemId = file.Id;
						break;
					}
				}
			}
			catch (Google.GoogleApiException exception)
			{
				logger.Exception(exception);
			}
		}

		return itemId;
	}

	/// <summary>
	/// The does the drive root item exist method.  This is used to determine
	/// if the drive item exists, or if the drive has been deleted or
	/// permissions changed such.
	/// </summary>
	/// <param name="driveId">The drive id.</param>
	/// <returns>Indicates whether the item was found or not.</returns>
	public bool DoesDriveRootItemExist(string driveId)
	{
		bool found = false;

		try
		{
			FilesResource.GetRequest request = driveService.Files.Get(driveId);
			GoogleDriveFile file = request.Execute();

			if (file != null)
			{
				found = true;
			}
		}
		catch (Google.GoogleApiException exception)
		{
			logger.Exception(exception);
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
		GoogleDriveFile file = driveService.Files.Get(driveId).Execute();

		return file;
	}

	/// <summary>
	/// Get files async method.
	/// </summary>
	/// <param name="parent">The parent folder.</param>
	/// <param name="ownedByMe">Indicates whether to return only files
	/// owned by this account.</param>
	/// <returns>A list of files.</returns>
	public async Task<IList<GoogleDriveFile>> GetFilesAsync(
		string parent, bool ownedByMe)
	{
		List<GoogleDriveFile> files = null;

		if (string.IsNullOrWhiteSpace(parent))
		{
			logger.Error("GetFiles: parent is empty");

			string message = "StackTrace: " + Environment.StackTrace;
			logger.Error(message);
		}
		else
		{
			try
			{
				GoogleDriveFile serverFile = GetFileById(parent);

				files = [];
				FilesResource.ListRequest listRequest =
					GetListRequest(parent, ownedByMe);

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
							"Retrieved files from: {0} ({1}) count: {2}",
							parent,
							serverFile.Name,
							files.Count);
						logger.Information(message);
					}
					catch (Google.GoogleApiException exception)
					{
						logger.Exception(exception);
						listRequest.PageToken = null;
					}
				}
				while (!string.IsNullOrEmpty(listRequest.PageToken));
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
				logger.Exception(exception);
			}
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
		string directoryName = Path.GetFileName(path);
		GoogleDriveFile serverFolder =
			GetFileInList(serverFiles, directoryName);

		if (serverFolder == null)
		{
			serverFolder = CreateFolder(driveParentId, directoryName);
			Delay();
		}

		return serverFolder;
	}

	/// <summary>
	/// Move a drive file.
	/// </summary>
	/// <param name="file">The file.</param>
	/// <param name="destinationId">The id of destination folder.</param>
	public void MoveFile(GoogleDriveFile file, string destinationId)
	{
		if (file != null)
		{
			GoogleDriveFile placeHolder = new();

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
		FileInfo file = new(filePath);

		GoogleDriveFile fileMetadata = new();
		fileMetadata.Name = file.Name;

		using FileStream stream = new(filePath, System.IO.FileMode.Open);

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
			driveService?.Dispose();
			driveService = null;
		}

		// free native resources
	}

	private string GetMimeType(FileInfo file)
	{
		string mimeType = MimeTypes.GetMimeType(file.Name);

		// overrides for wrongly marked files
		string extension = file.Extension;

		if (extension.Equals(
			".gdoc", StringComparison.OrdinalIgnoreCase) ||
			extension.Equals(
				".gsheet", StringComparison.OrdinalIgnoreCase))
		{
			logger.Information("Changing mime type to application/json");
			mimeType = "application/json";
		}

		return mimeType;
	}

	private void UploadProgressChanged(IUploadProgress progress)
	{
		string message = string.Format(
			CultureInfo.InvariantCulture,
			"{0}: {1} bytes",
			progress.Status,
			progress.BytesSent);

		logger.Information(message);

		if (progress.Exception != null)
		{
			logger.Error(message);
			logger.Exception(progress.Exception);
		}
	}

	private void UploadResponseReceived(
		GoogleDriveFile file)
	{
		string fileName = SanitizeFileName(file.Name);
		string message = fileName + " was uploaded successfully";

		logger.Information(message);
	}

	private FilesResource.ListRequest GetListRequest(
		string driveParentId, bool ownedByMe)
	{
		FilesResource.ListRequest listRequest = driveService.Files.List();

		const string fileFields = "id, name, mimeType, modifiedTime, " +
			"ownedByMe, owners, parents, webContentLink";
		listRequest.Fields = string.Format(
			CultureInfo.InvariantCulture,
			"files({0}), nextPageToken",
			fileFields);
		listRequest.PageSize = 1000;

		if (ownedByMe == true)
		{
			listRequest.Q = $"'{driveParentId}' in parents and 'me' in owners";
		}
		else
		{
			listRequest.Q = $"'{driveParentId}' in parents";
		}

		return listRequest;
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
				logger.Error("AggregateException caught");
				logger.Exception(exception);

				foreach (Exception innerExecption in exception.InnerExceptions)
				{
					if (innerExecption is TaskCanceledException)
					{
						logger.Warning("TaskCanceledException caught");
						logger.Exception(innerExecption);

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
			List<string> parents = [folder];
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
