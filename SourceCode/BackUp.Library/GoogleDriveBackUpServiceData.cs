/////////////////////////////////////////////////////////////////////////////
// <copyright file="GoogleDriveBackUpServiceData.cs" company="James John McGuire">
// Copyright © 2017 - 2024 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.BackUp.Library
{
	/// <summary>
	/// Google Drive back up service data.
	/// </summary>
	public class GoogleDriveBackUpServiceData : IBackUpServiceData
	{
		private readonly string parentId;
		private readonly string serviceAccountJsonFile;

		/// <summary>
		/// Initializes a new instance of the <see
		/// cref="GoogleDriveBackUpServiceData"/> class.
		/// </summary>
		/// <param name="serviceAccountJsonFile">The service account
		/// json file.</param>
		/// <param name="parentId">The parent folder id.</param>
		public GoogleDriveBackUpServiceData(
			string serviceAccountJsonFile, string parentId)
		{
			this.serviceAccountJsonFile = serviceAccountJsonFile;
			this.parentId = parentId;
		}

		/// <summary>
		/// Gets or sets the Name of service.
		/// </summary>
		/// <value>The Name of service.</value>
		public string Name { get; set; }

		/// <summary>
		/// Gets service account property.
		/// </summary>
		/// <value>Service account property.</value>
		public string ServiceAccount { get => serviceAccountJsonFile; }
	}
}
