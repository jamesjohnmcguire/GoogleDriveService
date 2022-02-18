/////////////////////////////////////////////////////////////////////////////
// <copyright file="Directory.cs" company="James John McGuire">
// Copyright © 2017 - 2022 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BackupManagerLibrary
{
	/// <summary>
	/// Directory custom class.
	/// </summary>
	public class Directory
	{
		private readonly IList<Exclude> excludes = new List<Exclude>();

		/// <summary>
		/// Initializes a new instance of the <see cref="Directory"/> class.
		/// </summary>
		public Directory()
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
				string checkPath = System.IO.Path.GetFullPath(path);

				DirectoryInfo directoryInfo =
					System.IO.Directory.GetParent(path);

				string excludeCheckPath = System.IO.Path.GetFullPath(
					exclude.Path, directoryInfo.FullName);

				if (checkPath.Equals(
					excludeCheckPath, StringComparison.OrdinalIgnoreCase))
				{
					foundExclude = exclude;
					break;
				}
			}

			return foundExclude;
		}
	}
}
