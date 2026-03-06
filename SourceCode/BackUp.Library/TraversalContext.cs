/////////////////////////////////////////////////////////////////////////////
// <copyright file="TraversalContext.cs" company="James John McGuire">
// Copyright © 2017 - 2026 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.BackUp.Library;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;

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
	///     <paramref name="allowedTypes"/> are not null.
	/// </description></item>
	/// </list>
	/// This method is concerned only with matching — it makes no policy
	/// decisions about how to handle a match or non-match.
	/// </remarks>
	/// <param name="path">The fully qualified, existent path to
	/// evaluate.</param>
	/// <param name="exclude">An Exclude object that defines the exclusion
	/// criteria, including the path and exclusion type. Cannot be null.</param>
	/// <param name="allowedTypes">A collection of ExcludeType values
	/// specifying which types of exclusions are permitted for the evaluation.
	/// Cannot be null.</param>
	/// <returns>true if the path matches the exclusion criteria and the
	/// exclusion type is allowed; otherwise, false.</returns>
	internal static bool IsExcludeMatch(
		[NotNull] string path,
		[NotNull] Exclude exclude,
		[NotNull] IReadOnlySet<ExcludeType> allowedTypes)
	{
		bool isMatch = false;
		bool isTypeAllowed = allowedTypes.Contains(exclude.ExcludeType);

		if (isTypeAllowed == true)
		{
			if (path.Equals(exclude.Path, PathComparison))
			{
				isMatch = true;
			}
		}

		return isMatch;
	}
}
