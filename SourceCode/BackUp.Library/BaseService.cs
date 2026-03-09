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
	/// <param name="path">The path to process.</param>
	/// <param name="excludes">The list of excludes.</param>
	/// <returns>A value indicating whether to process the file
	/// or not.</returns>
	internal static bool ShouldProcessFile(
		string path, ICollection<Exclude> excludes)
	{
		IReadOnlySet<ExcludeType> excludeTypes = new HashSet<ExcludeType>
			{
				ExcludeType.File,
				ExcludeType.FileIgnore
			};

		bool processFile = ShouldProcessItem(path, excludes, excludeTypes);

		return processFile;
	}

	/// <summary>
	/// Should process files method.
	/// </summary>
	/// <param name="path">The path to process.</param>
	/// <param name="excludes">The list of excludes.</param>
	/// <returns>A value indicating whether to process the file
	/// or not.</returns>
	internal static bool ShouldProcessFiles(
		string path, ICollection<Exclude> excludes)
	{
		IReadOnlySet<ExcludeType> excludeTypes =
			new HashSet<ExcludeType> { ExcludeType.OnlyRoot };

		bool processFiles = ShouldProcessItem(path, excludes, excludeTypes);

		return processFiles;
	}

	/// <summary>
	/// Determines whether a folder should be processed during backup.
	/// </summary>
	/// <remarks>This method evaluates a single folder in isolation. Prevention
	/// of processing of an excluded folder's subtree is the responsibility
	/// of the caller.</remarks>
	/// <param name="path">The path to process.</param>
	/// <param name="excludes">The collection of excludes to check
	/// against. A null or empty collection always permits
	/// processing.</param>
	/// <returns>True if the folder should be processed;
	/// false if it is explicitly excluded.</returns>
	internal static bool ShouldProcessFolder(
		string path, ICollection<Exclude> excludes)
	{
		IReadOnlySet<ExcludeType> excludeTypes = new HashSet<ExcludeType>
			{
				ExcludeType.Global,
				ExcludeType.SubDirectory
			};

		bool processSubFolders =
			ShouldProcessItem(path, excludes, excludeTypes);

		return processSubFolders;
	}

	/// <summary>
	/// Should remove file method.
	/// </summary>
	/// <remarks>This method assumes the file has already been excluded
	/// from uploading.</remarks>
	/// <param name="path">The path to remove.</param>
	/// <param name="excludes">The list of excludes.</param>
	/// <returns>A value indicating whether to remove the file
	/// or not.</returns>
	internal static bool ShouldRemoveFile(
		string path, ICollection<Exclude> excludes)
	{
		IReadOnlySet<ExcludeType> excludeTypes =
			new HashSet<ExcludeType> { ExcludeType.FileIgnore };

		bool removeFile = ShouldProcessItem(path, excludes, excludeTypes);

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
		IReadOnlySet<ExcludeType> excludeTypes =
			new HashSet<ExcludeType> { ExcludeType.Keep };

		bool processitem =
			ShouldProcessItem(parentPath, excludes, excludeTypes);

		bool skipThisDirectory = !processitem;

		return skipThisDirectory;
	}

	private static bool ShouldProcessItem(
		string path,
		ICollection<Exclude> excludes,
		IReadOnlySet<ExcludeType> excludeTypes)
	{
		bool processItem = true;

		if (excludes != null)
		{
			foreach (Exclude exclude in excludes)
			{
				bool isMatch = TraversalContext.IsExcludeMatch(
					path, exclude, excludeTypes);

				if (isMatch == true)
				{
					processItem = false;
					break;
				}
			}
		}

		return processItem;
	}
}
