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

namespace BackupManagerLibrary
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
							new (excludeName, ExcludeType.AllSubDirectories);
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
		/// Expand excludes method.
		/// </summary>
		/// <returns>A list of expanded excludes.</returns>
		public IList<Exclude> ExpandExcludes()
		{
			foreach (Exclude exclude in excludes)
			{
				exclude.Path = Environment.ExpandEnvironmentVariables(
					exclude.Path);
				exclude.Path = System.IO.Path.GetFullPath(exclude.Path);

				if (exclude.ExcludeType == ExcludeType.File &&
					exclude.Path.Contains(
						'*', StringComparison.OrdinalIgnoreCase))
				{
					IList<Exclude> newExcludes = ExpandWildCard(exclude.Path);

					if (newExcludes.Count > 0)
					{
						excludes.Remove(exclude);
						excludes = excludes.Concat(newExcludes).ToList();
					}
				}
			}

			return excludes;
		}

		/// <summary>
		/// Excludes contains method.
		/// </summary>
		/// <param name="path">The path to check.</param>
		/// <returns>Indicates whether the path is in the
		/// exludes list.</returns>
		public bool ExcludesContains(string path)
		{
			bool contains = false;

			Exclude exclude = GetExclude(path);

			if (exclude != null)
			{
				contains = true;
			}

			return contains;
		}

		/// <summary>
		/// Get exclude method.
		/// </summary>
		/// <param name="path">The path to check.</param>
		/// <returns>The exclude of the path.</returns>
		public Exclude GetExclude(string path)
		{
			Exclude foundExclude = null;

			foreach (Exclude exclude in excludes)
			{
				bool matched = CheckExclude(exclude, path);

				if (matched == true)
				{
					foundExclude = exclude;
					break;
				}
			}

			return foundExclude;
		}

		private static bool CheckExclude(Exclude exclude, string path)
		{
			bool matched = false;

			string checkPath = System.IO.Path.GetFullPath(path);

			DirectoryInfo directoryInfo =
				System.IO.Directory.GetParent(path);

			string excludeCheckPath = System.IO.Path.GetFullPath(
				exclude.Path, directoryInfo.FullName);

			if (checkPath.Equals(
				excludeCheckPath, StringComparison.OrdinalIgnoreCase))
			{
				matched = true;
			}
			else
			{
				bool isWildCardExclude =
					CheckWildCards(exclude, path, checkPath);

				if (isWildCardExclude == true)
				{
					matched = true;
				}
			}

			return matched;
		}

		private static bool CheckWildCards(
			Exclude exclude, string path, string checkPath)
		{
			bool matched = false;

			if (exclude.Path.Contains(
				'*', StringComparison.OrdinalIgnoreCase))
			{
				int index = exclude.Path.IndexOf(
					'*', StringComparison.OrdinalIgnoreCase);
				string pattern = exclude.Path.Substring(index);

				Matcher matcher = new ();
				matcher.AddInclude(pattern);

				string directory = System.IO.Path.GetDirectoryName(path);
				IEnumerable<string> matchingFiles =
					matcher.GetResultsInFullPath(directory);

				if (matchingFiles.Contains(checkPath))
				{
					matched = true;
				}
			}

			return matched;
		}

		private static IList<Exclude> ExpandWildCard(string path)
		{
			IList<Exclude> newExcludes = new List<Exclude>();

			if (path.Contains(
				'*', StringComparison.OrdinalIgnoreCase))
			{
				int index = path.IndexOf(
					'*', StringComparison.OrdinalIgnoreCase);
				string pattern = path.Substring(index);

				Matcher matcher = new ();
				matcher.AddInclude(pattern);

				string directory = System.IO.Path.GetDirectoryName(path);
				IEnumerable<string> matchingFiles =
					matcher.GetResultsInFullPath(directory);

				foreach (string match in matchingFiles)
				{
					Exclude exclude = new Exclude(match, ExcludeType.File);
					newExcludes.Add(exclude);
				}
			}

			return newExcludes;
		}
	}
}
