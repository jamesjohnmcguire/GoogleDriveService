/////////////////////////////////////////////////////////////////////////////
// <copyright file="TraversalItemResponse.cs" company="James John McGuire">
// Copyright © 2017 - 2026 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.BackUp.Library;

/// <summary>Describes the selection outcome for a single traversal item.</summary>
internal enum TraversalItemResponse
{
	/// <summary>
	/// The item selection status is unknown or has not been evaluated.
	/// </summary>
	Unknown,

	/// <summary>
	/// The item is selected for processing and does not match any exclusion
	/// criteria.
	/// </summary>
	Selected,

	/// <summary>
	/// The item is excluded from processing based on the defined exclusion
	/// criteria.
	/// </summary>
	Excluded,

	/// <summary>
	/// The item is a symbolic link or reparse point and is skipped to avoid
	/// infinite loops or other issues.
	/// </summary>
	SkippedLink,

	/// <summary>
	/// The item does not exist in the file system and is therefore considered
	/// missing.
	/// </summary>
	Missing
}
