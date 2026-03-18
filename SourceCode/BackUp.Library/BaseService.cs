/////////////////////////////////////////////////////////////////////////////
// <copyright file="BaseService.cs" company="James John McGuire">
// Copyright © 2017 - 2026 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.BackUp.Library;

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Logging;

/// <summary>
/// Account Service class.
/// </summary>
public abstract class BaseService(
		Account account,
		Settings settings,
		ILogger<BackUpService> logger = null)
{
	private readonly Account account = account;
	private readonly ILogger<BackUpService> logger = logger;
	private Settings settings = settings;
	private TraversalContext traversalContext;

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
	/// Gets the application settings.
	/// </summary>
	public Settings Settings
	{
		get => settings;
	}

	/// <summary>
	/// Gets or sets the traversal context, which provides information about
	/// the traversal state during backup operations. This context may include
	/// details such as the current directory being processed, the depth of
	/// traversal, and any relevant metadata needed for decision-making during
	/// the backup process.
	/// </summary>
	protected TraversalContext TraversalContext
	{
		get => traversalContext;
		set => traversalContext = value;
	}

	/// <summary>
	/// Determines whether an item, either a directory or file, should be
	/// processed during backup.
	/// </summary>
	/// <remarks>At the point of this method being called, path should be an
	/// existing valid, fully qualified path.</remarks>
	/// <param name="path">The path of the item to process.</param>
	/// <param name="excludes">The collection of excludes to check
	/// against. This should not be null.</param>
	/// <returns>True if the item should be processed;
	/// false if it is explicitly excluded.</returns>
	internal static bool ShouldProcessItem(
		string path, ICollection<Exclude> excludes)
	{
		bool processItem = true;

		ArgumentNullException.ThrowIfNull(path);

		if (excludes != null)
		{
			foreach (Exclude exclude in excludes)
			{
				bool isMatch = TraversalContext.IsExcludeMatch(path, exclude);

				if (isMatch == true)
				{
					processItem = false;
					break;
				}
			}
		}

		return processItem;
	}

	/// <summary>
	/// Should remove item method.
	/// </summary>
	/// <remarks>This method assumes the item has already been excluded
	/// from uploading.</remarks>
	/// <param name="path">The path to remove.</param>
	/// <param name="excludes">The collection of excludes.</param>
	/// <returns>A value indicating whether to remove the item
	/// or not.</returns>
	internal static bool ShouldRemoveItem(
		string path, ICollection<Exclude> excludes)
	{
		bool removeItem = false;

		Exclude? exclude = GetExclude(path, excludes);

		ArgumentNullException.ThrowIfNull(exclude);

		if (exclude != null)
		{
			if (exclude.KeepOnRemote == false)
			{
				removeItem = true;
			}
		}

		return removeItem;
	}

	private static Exclude? GetExclude(
		string path,
		ICollection<Exclude> excludes)
	{
		Exclude? exclude = null;

		if (excludes != null)
		{
			foreach (Exclude checkExclude in excludes)
			{
				bool isMatch =
					TraversalContext.IsExcludeMatch(path, checkExclude);

				if (isMatch == true)
				{
					exclude = checkExclude;
					break;
				}
			}
		}

		return exclude;
	}
}
