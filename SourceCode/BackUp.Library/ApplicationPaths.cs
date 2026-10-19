/////////////////////////////////////////////////////////////////////////////
// <copyright file="ApplicationPaths.cs" company="James John McGuire">
// Copyright © 2017 - 2026 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.BackUp.Library;

using System;
using System.IO;

/// <summary>Resolves application paths without creating directories.</summary>
public static class ApplicationPaths
{
	/// <summary>Gets the platform-specific application data directory.</summary>
	/// <returns>The application data directory.</returns>
	public static string GetDataDirectory()
	{
		string baseDirectory = Environment.GetFolderPath(
			Environment.SpecialFolder.ApplicationData,
			Environment.SpecialFolderOption.DoNotVerify);

		string directory = GetDataDirectory(baseDirectory);

		return directory;
	}

	/// <summary>Gets a file path within the application data directory.
	/// </summary>
	/// <param name="fileName">The application data filename.</param>
	/// <returns>The file path.</returns>
	public static string GetFilePath(string fileName)
	{
		string directory = GetDataDirectory();
		string path = Path.Combine(directory, fileName);

		return path;
	}

	/// <summary>Combines an application data root with the application name.
	/// </summary>
	/// <param name="baseDirectory">The platform's application data root.
	/// </param>
	/// <returns>The application data directory.</returns>
	internal static string GetDataDirectory(string baseDirectory)
	{
		if (string.IsNullOrWhiteSpace(baseDirectory) ||
			!Path.IsPathFullyQualified(baseDirectory))
		{
			string message = "The operating system did not provide an " +
				"absolute application data directory.";

			throw new InvalidOperationException(message);
		}

		string directory = Path.Combine(
			baseDirectory, "DigitalZenWorks", "BackUpManager");

		return directory;
	}
}
