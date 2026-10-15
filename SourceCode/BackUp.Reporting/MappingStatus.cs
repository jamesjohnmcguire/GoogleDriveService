/////////////////////////////////////////////////////////////////////////////
// <copyright file="MappingStatus.cs" company="James John McGuire">
// Copyright © 2017 - 2026 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.BackUp.Reporting;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

/// <summary>Describes the outcome of explaining a source mapping.</summary>
[JsonConverter(typeof(StringEnumConverter))]
public enum MappingStatus
{
	/// <summary>The mapping has not been evaluated.</summary>
	Unknown,

	/// <summary>The provider or source mapping is unsupported.</summary>
	Unsupported,

	/// <summary>Local traversal selects the path for processing.</summary>
	Selected,

	/// <summary>An exclusion prevents traversal of the path.</summary>
	Excluded,

	/// <summary>The filesystem could not be inspected.</summary>
	Unavailable,

	/// <summary>The mapping's paths or exclusion rules are invalid.</summary>
	Invalid,

	/// <summary>The path or an ancestor does not exist.</summary>
	Missing,

	/// <summary>A symbolic link or reparse point prevents traversal.</summary>
	SkippedLink,
}
