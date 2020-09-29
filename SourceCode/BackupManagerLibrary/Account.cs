/////////////////////////////////////////////////////////////////////////////
// <copyright file="Account.cs" company="James John McGuire">
// Copyright © 2017 - 2020 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace BackupManagerLibrary
{
	public class Account : IDisposable
	{
		private readonly IList<Directory> directories = new List<Directory>();
		private DriveService service;

		public string Email { get; set; }

		public string ServiceAccount { get; set; }

		public IList<Directory> Directories
		{
			get { return directories; }
		}

		/// <summary>
		/// Authenticating to Google using a Service account
		/// Documentation:
		/// https://developers.google.com/accounts/docs/OAuth2#serviceaccount.
		/// </summary>
		/// <returns>True upon success,false otherwise.</returns>
		public bool Authenticate()
		{
			bool authenticated = false;

			try
			{
				string userProfilePath = Environment.GetFolderPath(
				Environment.SpecialFolder.UserProfile);
				string accountsPath = userProfilePath + @"\GoogleDrive";

				if (System.IO.Directory.Exists(accountsPath))
				{
					string accountsFile = accountsPath + @"\" + ServiceAccount;
					string accountsText = File.ReadAllText(accountsFile);

					GoogleCredential credentialedAccount =
						GoogleCredential.FromFile(accountsFile);

					BaseClientService.Initializer initializer =
						new BaseClientService.Initializer();
					initializer.ApplicationName = "Backup Manager";
					initializer.HttpClientInitializer = credentialedAccount;

					service = new DriveService(initializer);

					if (service != null)
					{
						authenticated = true;
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine("Create service account DriveService failed" + ex.Message);
				throw new Exception("CreateServiceAccountDriveFailed", ex);
			}

			return authenticated;
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				// dispose managed resources
				service.Dispose();
				service = null;
			}

			// free native resources
		}
	}
}
