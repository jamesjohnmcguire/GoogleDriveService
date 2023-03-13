﻿/////////////////////////////////////////////////////////////////////////////
// <copyright file="Account.cs" company="James John McGuire">
// Copyright © 2017 - 2023 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using GoogleDriveFile = Google.Apis.Drive.v3.Data.File;

namespace DigitalZenWorks.BackUp.Library
{
	/// <summary>
	/// Account class.
	/// </summary>
	public class Account
	{
		private readonly IList<DriveMapping> driveMappings =
			new List<DriveMapping>();

		/// <summary>
		/// Gets or sets the account type.
		/// </summary>
		public AccountType AccountType { get; set; }

		/// <summary>
		/// Gets or sets account email property.
		/// </summary>
		/// <value>Account email property.</value>
		public string Email { get; set; }

		/// <summary>
		/// Gets or sets service account property.
		/// </summary>
		/// <value>Service account property.</value>
		public string ServiceAccount { get; set; }

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
