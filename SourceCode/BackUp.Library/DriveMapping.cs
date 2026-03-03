/////////////////////////////////////////////////////////////////////////////
// <copyright file="DriveMapping.cs" company="James John McGuire">
// Copyright © 2017 - 2026 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.BackUp.Library;

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.FileSystemGlobbing;

/// <summary>
/// DriveMapping custom class.
/// </summary>
public class DriveMapping
{
	private List<Exclude> excludes = [];

	/// <summary>
	/// Initializes a new instance of the <see cref="DriveMapping"/> class
	/// using the specified settings.
	/// </summary>
	/// <remarks>Global excludes defined in the provided settings are
	/// processed to determine which items should be excluded from the drive
	/// mapping.</remarks>
	/// <param name="settings">The settings used to configure the drive
	/// mapping. If null, default settings are applied.</param>
	public DriveMapping(Settings settings = null)
	{
		if (settings != null)
		{
			excludes = AddGlobalExcludes(settings.GlobalExcludes);
		}
	}

	/// <summary>
	/// Gets or sets path property.
	/// </summary>
	/// <value>Path property.</value>
	public string Path { get; set; }

	/// <summary>
	/// Gets or sets the core shared parent folder id property.
	/// </summary>
	/// <value>The core shared parent folder id property.</value>
	public string DriveParentFolderId { get; set; }

	/// <summary>
	/// Gets excludes property.
	/// </summary>
	/// <value>Excludes property.</value>
	public IList<Exclude> Excludes
	{
		get { return excludes; }
	}

	/// <summary>
	/// Gets or sets the global excludes templates.
	/// </summary>
	/// <value>The global excludes templates.</value>
#pragma warning disable CA2227 // Collection properties should be read only
	public ICollection<string> GlobalExcludesTemplates { get; set; }
#pragma warning restore CA2227 // Collection properties should be read only

	/// <summary>
	/// Expand global excludes method.
	/// </summary>
	/// <param name="rootPath">The root path.</param>
	/// <param name="excludes">The current set of includes.</param>
	/// <returns>A list of expanded excludes.</returns>
	public static IList<Exclude> ExpandGlobalExcludes(
		string rootPath, IList<Exclude> excludes)
	{
		List<Exclude> expandedExcludes = null;

		if (excludes != null)
		{
			expandedExcludes = [];

			foreach (Exclude temporaryExclude in excludes)
			{
				Exclude exclude = temporaryExclude;

				if (exclude.ExcludeType == ExcludeType.Global)
				{
					exclude = ExpandExclude(rootPath, exclude);
				}

				if (exclude.ExcludeType == ExcludeType.File &&
					exclude.Path.Contains(
						'*', StringComparison.OrdinalIgnoreCase))
				{
					List<Exclude> newExcludes = ExpandWildCard(exclude.Path);

					if (newExcludes.Count > 0)
					{
						expandedExcludes.AddRange(newExcludes);
					}
				}
				else
				{
					expandedExcludes.Add(exclude);
				}
			}
		}

		return expandedExcludes;
	}

	/// <summary>
	/// Adds the specified global exclusion names to the collection, ensuring
	/// that only unique exclusions are included.
	/// </summary>
	/// <remarks>If the provided list is null, no exclusions are added.
	/// Duplicate exclusion names are ignored, and only unique names are
	/// stored.</remarks>
	/// <param name="globalExcludes">A read-only list of exclusion names to
	/// add as global exclusions. Each name must be unique and cannot be null.
	/// </param>
	/// <returns>A list of Exclude objects representing all global exclusions,
	/// including any newly added exclusions.</returns>
	public List<Exclude> AddGlobalExcludes(
		IReadOnlyList<string> globalExcludes)
	{
		if (globalExcludes != null)
		{
			foreach (string excludeName in globalExcludes)
			{
				bool exists = excludes.Any(e =>
					e.Path.Equals(
						excludeName, StringComparison.OrdinalIgnoreCase));

				if (exists == false)
				{
					Exclude exclude = new(excludeName, ExcludeType.Global);
					excludes.Add(exclude);
				}
			}
		}

		return excludes;
	}

	/// <summary>
	/// Expand excludes method.
	/// </summary>
	/// <returns>A list of expanded excludes.</returns>
	public IList<Exclude> ExpandExcludes()
	{
		if (excludes != null)
		{
			List<Exclude> expandedExcludes = [];

			foreach (Exclude temporaryExclude in excludes)
			{
				Exclude exclude = temporaryExclude;

				if (exclude.ExcludeType != ExcludeType.Global)
				{
					exclude = ExpandExclude(Path, exclude);
				}

				if (exclude.ExcludeType == ExcludeType.File &&
					exclude.Path.Contains(
						'*', StringComparison.OrdinalIgnoreCase))
				{
					List<Exclude> newExcludes = ExpandWildCard(exclude.Path);

					if (newExcludes.Count > 0)
					{
						expandedExcludes.AddRange(newExcludes);
					}
				}
				else
				{
					expandedExcludes.Add(exclude);
				}
			}

			excludes = expandedExcludes;
		}

		return excludes;
	}

	private static List<Exclude> ExpandWildCard(string path)
	{
		List<Exclude> newExcludes = [];

		if (path.Contains(
			'*', StringComparison.OrdinalIgnoreCase))
		{
			int index = path.IndexOf('*', StringComparison.OrdinalIgnoreCase);
			string pattern = path[index..];

			Matcher matcher = new();
			matcher.AddInclude(pattern);

			string directory = System.IO.Path.GetDirectoryName(path);
			IEnumerable<string> matchingFiles =
				matcher.GetResultsInFullPath(directory);

			foreach (string match in matchingFiles)
			{
				Exclude exclude = new(match, ExcludeType.File);
				newExcludes.Add(exclude);
			}
		}

		return newExcludes;
	}

	private static Exclude ExpandExclude(string rootPath, Exclude exclude)
	{
		exclude.Path = Environment.ExpandEnvironmentVariables(exclude.Path);

		bool isQualified = System.IO.Path.IsPathFullyQualified(exclude.Path);

		if (isQualified == false)
		{
			exclude.Path = System.IO.Path.Combine(rootPath, exclude.Path);
		}

		exclude.Path = System.IO.Path.GetFullPath(exclude.Path);

		return exclude;
	}
}
