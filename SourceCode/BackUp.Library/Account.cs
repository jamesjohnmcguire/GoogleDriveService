/////////////////////////////////////////////////////////////////////////////
// <copyright file="Account.cs" company="James John McGuire">
// Copyright © 2017 - 2026 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.BackUp.Library;

using System.Collections.Generic;

/// <summary>
/// Account class.
/// </summary>
public class Account
{
	private readonly IList<DriveMapping> driveMappings = [];

	private List<string> driveMappingPaths;

	/// <summary>
	/// Gets or sets service account property.
	/// </summary>
	/// <value>Service account property.</value>
	public string AccountIdentifier { get; set; }

	/// <summary>
	/// Gets or sets the account type.
	/// </summary>
	/// <value>The account type.</value>
	public AccountType AccountType { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether [check drive mappings].
	/// </summary>
	/// <value>
	///   <c>true</c> if [check drive mappings]; otherwise, <c>false</c>.
	/// </value>
	public bool CheckDriveMappings { get; set; }

	/// <summary>
	/// Gets driveMappings property.
	/// </summary>
	/// <value>DriveMappings property.</value>
	public IList<string> DriveMappingPaths
	{
		get
		{
			if (driveMappingPaths == null)
			{
				driveMappingPaths = [];

				foreach (DriveMapping mapping in driveMappings)
				{
					driveMappingPaths.Add(mapping.LocalPath);
				}
			}

			return driveMappingPaths;
		}
	}

	/// <summary>
	/// Gets driveMappings property.
	/// </summary>
	/// <value>DriveMappings property.</value>
	public IList<DriveMapping> DriveMappings
	{
		get { return driveMappings; }
	}

	/// <summary>
	/// Adds the specified global exclusion strings to all existing drive
	/// mappings.
	/// </summary>
	/// <remarks>This method iterates through all drive mappings and applies
	/// the provided global exclusions to each one. Ensure that the exclusion
	/// strings are valid and appropriate for the context of the drive
	/// mappings.</remarks>
	/// <param name="globalExcludes">A read-only list of exclusion strings to
	/// be applied to each drive mapping. This parameter cannot be null or
	/// empty.</param>
	public void AddGlobalExcludes(IReadOnlyCollection<string> globalExcludes)
	{
		foreach (DriveMapping mapping in driveMappings)
		{
			mapping.AddGlobalExcludesTemplates(globalExcludes);
		}
	}
}
