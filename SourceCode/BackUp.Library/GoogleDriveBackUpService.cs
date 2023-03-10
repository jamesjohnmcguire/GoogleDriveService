/////////////////////////////////////////////////////////////////////////////
// <copyright file="GoogleDriveBackUpService.cs" company="James John McGuire">
// Copyright © 2017 - 2023 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

namespace BackupManagerLibrary
{
	/// <summary>
	/// Google Drive back up service.
	/// </summary>
	public class GoogleDriveBackUpService : IBackUpService, IDisposable
	{
		private readonly string serviceAccountJsonFile;

		private GoogleDrive googleDrive;
		private string parentId;
		private string path;
		private GoogleDriveBackUpServiceData serviceData;

		/// <summary>
		/// Initializes a new instance of the <see
		/// cref="GoogleDriveBackUpService"/> class.
		/// </summary>
		/// <param name="serviceAccountJsonFile">The service account
		/// json file.</param>
		public GoogleDriveBackUpService(string serviceAccountJsonFile)
		{
			googleDrive = new GoogleDrive();
			this.serviceAccountJsonFile = serviceAccountJsonFile;
		}

		/// <summary>
		/// Back up method.
		/// </summary>
		/// <param name="path">The path to back up.</param>
		/// <param name="serviceDestinationId">A service specific
		/// identifier for the destination.</param>
		public void BackUp(string path, string serviceDestinationId)
		{
			this.path = path;
			parentId = serviceDestinationId;

			bool authenticated = Authorize();

			if (authenticated == true)
			{
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
				googleDrive.Dispose();
				googleDrive = null;
			}
		}

		private bool Authorize()
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
				GoogleDrive.LogException(exception);
			}

			return authenticated;
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
					string accountsFile = accountsPath + @"\" +
						serviceData.ServiceAccount;

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
				GoogleDrive.LogException(exception);
			}

			return serviceAccountJsonFile;
		}
	}
}
