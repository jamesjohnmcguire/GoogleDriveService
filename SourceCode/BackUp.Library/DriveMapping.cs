/////////////////////////////////////////////////////////////////////////////
// <copyright file="DriveMapping.cs" company="James John McGuire">
// Copyright © 2017 - 2023 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

using Microsoft.Extensions.FileSystemGlobbing;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DigitalZenWorks.BackUp.Library
{
	/// <summary>
	/// DriveMapping custom class.
	/// </summary>
	public class DriveMapping
	{
		private IList<Exclude> excludes = new List<Exclude>();

		/// <summary>
		/// Initializes a new instance of the <see cref="DriveMapping"/> class.
		/// </summary>
		public DriveMapping()
		{
			string baseDataDirectory = Environment.GetFolderPath(
				Environment.SpecialFolder.ApplicationData,
				Environment.SpecialFolderOption.Create);
			string settingsPath = baseDataDirectory +
				"/DigitalZenWorks/BackUpManager/Settings.json";

			if (System.IO.File.Exists(settingsPath))
			{
				string settingsText = File.ReadAllText(settingsPath);

				Settings settings = JsonConvert.DeserializeObject<Settings>(
						settingsText);

				if (settings != null && settings.GlobalExcludes != null)
				{
					foreach (string excludeName in settings.GlobalExcludes)
					{
						Exclude exclude =
							new (excludeName, ExcludeType.Global);
						excludes.Add(exclude);
					}
				}
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
				expandedExcludes = new List<Exclude>();

				for (int index = excludes.Count - 1; index >= 0; index--)
				{
					Exclude temporaryExclude = excludes[index];
					Exclude exclude = new (
						temporaryExclude.Path, temporaryExclude.ExcludeType);

					if (exclude.ExcludeType == ExcludeType.Global)
					{
						exclude = ExpandExclude(rootPath, exclude);
					}

					expandedExcludes.Add(exclude);

					if (exclude.ExcludeType == ExcludeType.File &&
						exclude.Path.Contains(
							'*', StringComparison.OrdinalIgnoreCase))
					{
						IList<Exclude> newExcludes =
							ExpandWildCard(exclude.Path);

						if (newExcludes.Count > 0)
						{
							expandedExcludes.Remove(exclude);
							expandedExcludes =
								expandedExcludes.Concat(newExcludes).ToList();
						}
					}
				}
			}

			return expandedExcludes;
		}

		/// <summary>
		/// Expand excludes method.
		/// </summary>
		/// <returns>A list of expanded excludes.</returns>
		public IList<Exclude> ExpandExcludes()
		{
			if (excludes != null)
			{
				for (int index = excludes.Count - 1; index >= 0; index--)
				{
					Exclude exclude = excludes[index];

					if (exclude.ExcludeType != ExcludeType.Global)
					{
						exclude = ExpandExclude(Path, exclude);
					}

					if (exclude.ExcludeType == ExcludeType.File &&
						exclude.Path.Contains(
							'*', StringComparison.OrdinalIgnoreCase))
					{
						IList<Exclude> newExcludes =
							ExpandWildCard(exclude.Path);

						if (newExcludes.Count > 0)
						{
							excludes.Remove(exclude);
							excludes =
								excludes.Concat(newExcludes).ToList();
						}
					}
				}
			}

			return excludes;
		}

		private static IList<Exclude> ExpandWildCard(string path)
		{
			IList<Exclude> newExcludes = new List<Exclude>();

			if (path.Contains(
				'*', StringComparison.OrdinalIgnoreCase))
			{
				int index = path.IndexOf(
					'*', StringComparison.OrdinalIgnoreCase);
				string pattern = path[index..];

				Matcher matcher = new ();
				matcher.AddInclude(pattern);

				string directory = System.IO.Path.GetDirectoryName(path);
				IEnumerable<string> matchingFiles =
					matcher.GetResultsInFullPath(directory);

				foreach (string match in matchingFiles)
				{
					Exclude exclude = new (match, ExcludeType.File);
					newExcludes.Add(exclude);
				}
			}

			return newExcludes;
		}

		private static Exclude ExpandExclude(string rootPath, Exclude exclude)
		{
			exclude.Path = Environment.ExpandEnvironmentVariables(
				exclude.Path);

			bool isQualified =
				System.IO.Path.IsPathFullyQualified(exclude.Path);

			if (isQualified == false)
			{
				exclude.Path = System.IO.Path.Combine(rootPath, exclude.Path);
			}

			exclude.Path = System.IO.Path.GetFullPath(exclude.Path);

			return exclude;
		}
	}
}
