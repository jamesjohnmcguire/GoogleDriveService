/////////////////////////////////////////////////////////////////////////////
// <copyright file="Account.cs" company="James John McGuire">
// Copyright © 2017 - 2025 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

namespace DigitalZenWorks.BackUp.Library
{
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
						driveMappingPaths.Add(mapping.Path);
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
	}
}