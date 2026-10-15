/////////////////////////////////////////////////////////////////////////////
// <copyright file="PathSelection.cs" company="James John McGuire">
// Copyright © 2017 - 2026 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

#nullable enable

namespace DigitalZenWorks.BackUp.Library;

using System;
using System.Collections.Generic;
using System.IO;

/// <summary>Shares the backup traversal's item selection rules.</summary>
internal static class PathSelection
{
	/// <summary>Evaluates one item at a traversal boundary.</summary>
	/// <param name="path">The fully qualified item path.</param>
	/// <param name="excludes">The rules active at this boundary.</param>
	/// <param name="rule">The first matching exclusion.</param>
	/// <returns>The selection status.</returns>
	internal static TraversalItemResponse Evaluate(
		string path, ICollection<Exclude>? excludes, out Exclude? rule)
	{
		TraversalItemResponse result = TraversalItemResponse.Unknown;
		rule = null;

		ArgumentNullException.ThrowIfNull(path);

		if (path != null)
		{
			bool exists = File.Exists(path) || Directory.Exists(path);

			if (exists == false)
			{
				result = TraversalItemResponse.Missing;
			}
			else
			{
				bool isReparsePoint = IsReparsePoint(path);

				if (isReparsePoint == true)
				{
					result = TraversalItemResponse.SkippedLink;
				}
				else
				{
					if (excludes != null)
					{
						foreach (Exclude candidate in excludes)
						{
							bool match = TraversalContext.IsExcludeMatch(
								path, candidate);

							if (match == true)
							{
								rule = candidate;
								result = TraversalItemResponse.Excluded;
								break;
							}
						}
					}

					if (rule == null)
					{
						result = TraversalItemResponse.Selected;
					}
				}
			}
		}

		return result;
	}

	/// <summary>
	/// Determines whether the specified path is a symbolic link or reparse point.
	/// </summary>
	/// <param name="path">The path to check.</param>
	/// <returns>A boolean value indicating whether the path is a symbolic link
	/// or reparse point.</returns>
	/// <remarks>Remove this method once the
	/// DigialZenWorks.Common.Utilities.FileUtils class contains this method.
	/// </remarks>
	internal static bool IsReparsePoint(string path)
	{
		bool isSymLink = false;
		DirectoryInfo directoryInfo = new(path);

		bool hasReparsePoint =
			directoryInfo.Attributes.HasFlag(FileAttributes.ReparsePoint);

		if (hasReparsePoint == true || directoryInfo.LinkTarget != null)
		{
			isSymLink = true;
		}

		return isSymLink;
	}
}
