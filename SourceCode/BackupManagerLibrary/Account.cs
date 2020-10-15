﻿/////////////////////////////////////////////////////////////////////////////
// <copyright file="Account.cs" company="James John McGuire">
// Copyright © 2017 - 2020 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

using Common.Logging;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace BackupManagerLibrary
{
	public class Account : IDisposable
	{
		private static readonly ILog Log = LogManager.GetLogger(
			System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private readonly IList<Directory> directories = new List<Directory>();
		private DriveService driveService;
		private GoogleDrive googleDrive;

		public Account()
		{
			googleDrive = new GoogleDrive();
		}

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

					authenticated = googleDrive.Authenticate(accountsFile);
					authenticated = true;
				}
			}
			catch (Exception exception) when
				(exception is ArgumentException ||
				exception is FileNotFoundException)
			{
				Log.Error(CultureInfo.InvariantCulture, m => m(
					exception.ToString()));
			}

			return authenticated;
		}

		public async Task BackUp()
		{
			bool authenticated = Authenticate();

			if (authenticated == true)
			{
				IList<Directory> directories = Directories;

				foreach (Directory directory in directories)
				{
					string path = Environment.ExpandEnvironmentVariables(
						directory.Path);

					directory.ExpandExcludes();

					await BackUp(path, directory).ConfigureAwait(false);
				}
			}
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
				driveService.Dispose();
				driveService = null;

				googleDrive.Dispose();
				googleDrive = null;
			}

			// free native resources
		}

		private async Task BackUp(string path, Directory directory)
		{
			try
			{
				if ((!directory.ExcludesContains(path)) &&
					System.IO.Directory.Exists(path))
				{
					DirectoryInfo directoryInfo = new DirectoryInfo(path);
					FileInfo[] files = directoryInfo.GetFiles();

					IList<Google.Apis.Drive.v3.Data.File> serverFiles =
						googleDrive.GetFiles();

					foreach (FileInfo file in files)
					{
						try
						{
							string message = "Checking: " + file.FullName;
							Log.Info(CultureInfo.InvariantCulture, m => m(
								message));

							await googleDrive.Upload(
								file.FullName, directory.Parent).
								ConfigureAwait(false);
						}
						catch (Exception exception) when
							(exception is ArgumentNullException ||
							exception is DirectoryNotFoundException ||
							exception is FileNotFoundException ||
							exception is IOException ||
							exception is NullReferenceException ||
							exception is IndexOutOfRangeException ||
							exception is InvalidOperationException ||
							exception is UnauthorizedAccessException)
						{
							Log.Error(CultureInfo.InvariantCulture, m => m(
								exception.ToString()));
						}
					}

					string[] subDirectories =
						System.IO.Directory.GetDirectories(path);

					foreach (string subDirectory in subDirectories)
					{
						await BackUp(subDirectory, directory).
							ConfigureAwait(false);
					}
				}
			}
			catch (Exception exception) when
				(exception is ArgumentException ||
				exception is ArgumentNullException ||
				exception is DirectoryNotFoundException ||
				exception is FileNotFoundException ||
				exception is IndexOutOfRangeException ||
				exception is InvalidOperationException ||
				exception is NullReferenceException ||
				exception is IOException ||
				exception is PathTooLongException ||
				exception is System.Security.SecurityException ||
				exception is TargetException ||
				exception is UnauthorizedAccessException)
			{
				Log.Error(CultureInfo.InvariantCulture, m => m(
					exception.ToString()));
			}
		}
	}
}