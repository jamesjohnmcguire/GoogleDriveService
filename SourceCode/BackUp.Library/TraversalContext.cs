/////////////////////////////////////////////////////////////////////////////
// <copyright file="TraversalContext.cs" company="James John McGuire">
// Copyright © 2017 - 2026 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.BackUp.Library;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;

#nullable enable

/// <summary>
/// Provides contextual information and utility methods for traversing file
/// system paths, including evaluation of exclusion criteria during traversal
/// operations.
/// </summary>
/// <remarks>TraversalContext is intended for use in scenarios where controlled
/// traversal of file system paths is required, such as backup or
/// synchronization processes. It offers methods to determine whether specific
/// paths should be excluded based on defined criteria. This class assumes that
/// callers handle path validation and symlink or reparse point resolution
/// prior to invoking its methods.</remarks>
public static class TraversalContext
{
	private static readonly StringComparison PathComparison =
		OperatingSystem.IsWindows()
			? StringComparison.OrdinalIgnoreCase
			: StringComparison.Ordinal;

	/// <summary>
	/// Determines whether the specified path matches the exclusion criteria
	/// defined by the given Exclude object and is of an allowed exclusion
	/// type.
	/// </summary>
	/// <remarks>
	/// This is a low-level matching primitive intended to be called from
	/// controlled traversal contexts. The following preconditions are
	/// assumed by the caller:
	/// <list type="bullet">
	/// <item><description>
	///     <paramref name="path"/> is not null, not whitespace, is fully
	///     qualified, and exists on the filesystem. May refer to a file,
	///     directory, or reparse point — symlink/reparse point handling
	///     is the caller's responsibility prior to this call.
	/// </description></item>
	/// <item><description>
	///     <paramref name="exclude"/> and
	///     <paramref name="excludeTypes"/> are not null.
	/// </description></item>
	/// </list>
	/// This method is concerned only with matching — it makes no policy
	/// decisions about how to handle a match or non-match.
	/// </remarks>
	/// <param name="path">The fully qualified, existent path to
	/// evaluate.</param>
	/// <param name="exclude">An Exclude object that defines the exclusion
	/// criteria, including the path and exclusion type. Cannot be null.</param>
	/// <param name="excludeTypes">A collection of ExcludeType values
	/// specifying which types of exclusions are permitted for the evaluation.
	/// Cannot be null.</param>
	/// <returns>true if the path matches the exclusion criteria and the
	/// exclusion type is allowed; otherwise, false.</returns>
	internal static bool IsExcludeMatch(
		[NotNull] string path,
		[NotNull] Exclude exclude,
		[NotNull] IReadOnlySet<ExcludeType> excludeTypes)
	{
		bool isMatch = false;
		bool isTypeAllowed = excludeTypes.Contains(exclude.ExcludeType);

		if (isTypeAllowed == true)
		{
			if (path.Equals(exclude.Path, PathComparison))
			{
				isMatch = true;
			}
		}

		return isMatch;
	}

	/// <summary>
	/// Normalizes the specified file or directory path and returns a fully
	/// qualified path if the path exists.
	/// </summary>
	/// <remarks>If the provided path is not fully qualified, it is converted
	/// to an absolute path using the current working directory. The method
	/// returns null if the path does not exist or if the input is null, empty,
	/// or consists only of whitespace.</remarks>
	/// <param name="path">The path to normalize. This can be a relative or
	/// absolute path. The value must not be null, empty, or whitespace.
	/// </param>
	/// <param name="rootPath">The root path to use for resolving relative
	/// paths.</param>
	/// <returns>A fully qualified path as a string if the specified path
	/// exists; otherwise, null.</returns>
	internal static string? NormalizePath(
		string? path, string? rootPath = null)
	{
		string? normalizedPath = null;

		if (!string.IsNullOrWhiteSpace(path))
		{
			path = Environment.ExpandEnvironmentVariables(path);

			bool exists = File.Exists(path) || Directory.Exists(path);

			if (exists == true)
			{
				normalizedPath = path;

				bool isFullyQualified = Path.IsPathFullyQualified(path);

				if (isFullyQualified == false &&
					!string.IsNullOrWhiteSpace(rootPath))
				{
					// When logging becomes static, add a warning here.
					normalizedPath = Path.Combine(rootPath, path);
				}

				normalizedPath = Path.GetFullPath(normalizedPath);
			}
		}

		return normalizedPath;
	}
}
