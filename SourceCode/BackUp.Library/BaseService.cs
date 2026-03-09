/////////////////////////////////////////////////////////////////////////////
// <copyright file="BaseService.cs" company="James John McGuire">
// Copyright © 2017 - 2026 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.BackUp.Library;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using Microsoft.Extensions.Logging;

/// <summary>
/// Account Service class.
/// </summary>
public abstract class BaseService(
		Account account, ILogger<BackUpService> logger = null)
{
	private readonly Account account = account;
	private readonly ILogger<BackUpService> logger = logger;

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
	/// Should process file method.
	/// </summary>
	/// <param name="excludes">The list of excludes.</param>
	/// <param name="path">The path to process.</param>
	/// <returns>A value indicating whether to process the file
	/// or not.</returns>
	[System.Diagnostics.CodeAnalysis.SuppressMessage(
		"Design",
		"CA1062:Validate arguments of public methods",
		Justification = "File.Exists also covers the null argument case.")]
	internal static bool ShouldProcessFile(
		ICollection<Exclude> excludes, string path)
	{
		bool processFile = true;

		bool exists = File.Exists(path);

		if (exists == false)
		{
			processFile = false;
		}
		else if (excludes != null)
		{
			IReadOnlySet<ExcludeType> allowedTypes = new HashSet<ExcludeType>
			{
				ExcludeType.File,
				ExcludeType.FileIgnore
			};

			foreach (Exclude exclude in excludes)
			{
				bool isMatch = TraversalContext.IsExcludeMatch(
					path, exclude, allowedTypes);

				if (isMatch == true)
				{
					processFile = false;
					break;
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
	[System.Diagnostics.CodeAnalysis.SuppressMessage(
		"Design",
		"CA1062:Validate arguments of public methods",
		Justification = "File.Exists also covers the null argument case.")]
	internal static bool ShouldProcessFiles(
		ICollection<Exclude> excludes, string path)
	{
		bool processFiles = true;

		bool exists = Path.Exists(path);

		if (exists == false)
		{
			processFiles = false;
		}
		else if (excludes != null)
		{
			IReadOnlySet<ExcludeType> allowedTypes =
				new HashSet<ExcludeType> { ExcludeType.OnlyRoot };

			foreach (Exclude exclude in excludes)
			{
				bool isMatch = TraversalContext.IsExcludeMatch(
					path, exclude, allowedTypes);

				if (isMatch == true)
				{
					processFiles = false;
					break;
				}
			}
		}

		return processFiles;
	}

	/// <summary>
	/// Determines whether a folder should be processed during backup.
	/// </summary>
	/// <remarks>This method evaluates a single folder in isolation. Prevention
	/// of processing of an excluded folder's subtree is the responsibility
	/// of the caller.</remarks>
	/// <param name="excludes">The collection of excludes to check
	/// against. A null or empty collection always permits
	/// processing.</param>
	/// <param name="path">The path to process.</param>
	/// <returns>True if the folder should be processed;
	/// false if it is explicitly excluded.</returns>
	internal static bool ShouldProcessFolder(
		ICollection<Exclude> excludes, string path)
	{
		bool processSubFolders = true;

		if (excludes != null)
		{
			IReadOnlySet<ExcludeType> allowedTypes = new HashSet<ExcludeType>
			{
				ExcludeType.Global,
				ExcludeType.SubDirectory
			};

			foreach (Exclude exclude in excludes)
			{
				if (exclude == null || exclude.Path == null)
				{
					continue;
				}

				{
					bool isMatch = TraversalContext.IsExcludeMatch(
						path, exclude, allowedTypes);

					if (isMatch == true)
					{
						processSubFolders = false;
						break;
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
	[System.Diagnostics.CodeAnalysis.SuppressMessage(
		"Design",
		"CA1062:Validate arguments of public methods",
		Justification = "File.Exists also covers the null argument case.")]
	internal static bool ShouldRemoveFile(
		ICollection<Exclude> excludes, string path)
	{
		bool removeFile = true;

		if (excludes != null)
		{
			IReadOnlySet<ExcludeType> allowedTypes =
				new HashSet<ExcludeType> { ExcludeType.FileIgnore };

			foreach (Exclude exclude in excludes)
			{
				bool isMatch = TraversalContext.IsExcludeMatch(
					path, exclude, allowedTypes);

				if (isMatch == true)
				{
					removeFile = false;
					break;
				}
			}
		}

		return removeFile;
	}

	/// <summary>
	/// Should skip this directory method.
	/// </summary>
	/// <param name="parentPath">The parent path.</param>
	/// <param name="excludes">The list of excludes.</param>
	/// <returns>A value indicating whether to process the file
	/// or not.</returns>
	internal static bool ShouldSkipThisDirectory(
		string parentPath, ICollection<Exclude> excludes)
	{
		bool skipThisDirectory = false;

		if (string.IsNullOrWhiteSpace(parentPath))
		{
			skipThisDirectory = true;
		}
		else if (excludes != null)
		{
			IReadOnlySet<ExcludeType> allowedTypes =
				new HashSet<ExcludeType> { ExcludeType.Keep };

			foreach (Exclude exclude in excludes)
			{
				bool isMatch = TraversalContext.IsExcludeMatch(
					parentPath, exclude, allowedTypes);

				if (isMatch == true)
				{
					skipThisDirectory = true;
					break;
				}
			}
		}

		return skipThisDirectory;
	}
}
