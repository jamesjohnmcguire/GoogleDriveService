/////////////////////////////////////////////////////////////////////////////
// <copyright file="BaseService.cs" company="James John McGuire">
// Copyright © 2017 - 2024 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;

namespace DigitalZenWorks.BackUp.Library
{
	/// <summary>
	/// Account Service class.
	/// </summary>
	public abstract class BaseService
	{
		private readonly Account account;
		private readonly ILogger<BackUpService> logger;

		/// <summary>
		/// Initializes a new instance of the
		/// <see cref="BaseService"/> class.
		/// </summary>
		/// <param name="account">The accound data.</param>
		/// <param name="logger">The logger interface.</param>
		protected BaseService(
			Account account, ILogger<BackUpService> logger = null)
		{
			this.logger = logger;
			this.account = account;
		}

		/// <summary>
		/// Gets the account data.
		/// </summary>
		/// <value>The account data.</value>
		public Account Account { get => account; }

		/// <summary>
		/// Gets or Sets a value indicating whether to ignore abandoned files.
		/// </summary>
		/// <value>A value indicating whether to ignore abandoned files.</value>
		public bool IgnoreAbandoned { get; set; }

		/// <summary>
		/// Gets the logger service.
		/// </summary>
		/// <value>The logger service.</value>
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

					if (clause == ExcludeType.File ||
						clause == ExcludeType.FileIgnore)
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

					if (clause == ExcludeType.SubDirectory ||
						clause == ExcludeType.Global ||
						clause == ExcludeType.FileIgnore)
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
		/// Should remove file method.
		/// </summary>
		/// <remarks>This method assumes the file has already been excluded
		/// from uploading.</remarks>
		/// <param name="excludes">The list of excludes.</param>
		/// <param name="path">The path to remove.</param>
		/// <returns>A value indicating whether to remove the file
		/// or not.</returns>
		protected static bool ShouldRemoveFile(
			IList<Exclude> excludes, string path)
		{
			bool removeFile = true;

			if (excludes != null)
			{
				foreach (Exclude exclude in excludes)
				{
					ExcludeType clause = exclude.ExcludeType;

					if (clause == ExcludeType.FileIgnore)
					{
						if (exclude.Path.Equals(
							path, StringComparison.OrdinalIgnoreCase))
						{
							removeFile = false;
							break;
						}
					}
				}
			}

			return removeFile;
		}
	}
}
