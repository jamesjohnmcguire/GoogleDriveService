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
using System.Reflection;
using System.Threading.Tasks;

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
		/// Should skip this directory method.
		/// </summary>
		/// <param name="parentPath">The parent path.</param>
		/// <param name="excludes">The list of excludes.</param>
		/// <returns>A value indicating whether to process the file
		/// or not.</returns>
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

		/// <summary>
		/// Should process file method.
		/// </summary>
		/// <param name="excludes">The list of excludes.</param>
		/// <param name="path">The path to process.</param>
		/// <returns>A value indicating whether to process the file
		/// or not.</returns>
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

		/// <summary>
		/// Should process files method.
		/// </summary>
		/// <param name="excludes">The list of excludes.</param>
		/// <param name="path">The path to process.</param>
		/// <returns>A value indicating whether to process the file
		/// or not.</returns>
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

		/// <summary>
		/// Should process folder method.
		/// </summary>
		/// <param name="excludes">The list of excludes.</param>
		/// <param name="path">The path to process.</param>
		/// <returns>A value indicating whether to process the file
		/// or not.</returns>
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
	}
}
