/////////////////////////////////////////////////////////////////////////////
// <copyright file="GoogleDriveBackUpServiceData.cs" company="James John McGuire">
// Copyright © 2017 - 2025 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.BackUp.Library
{
	/// <summary>
	/// Google Drive back up service data.
	/// </summary>
	/// <remarks>
	/// Initializes a new instance of the <see
	/// cref="GoogleDriveBackUpServiceData"/> class.
	/// </remarks>
	/// <param name="serviceAccountJsonFile">The service account
	/// json file.</param>
	/// <param name="parentId">The parent folder id.</param>
	public class GoogleDriveBackUpServiceData(
		string serviceAccountJsonFile, string parentId) : IBackUpServiceData
	{
		private readonly string parentId = parentId;
		private readonly string serviceAccountJsonFile =
			serviceAccountJsonFile;

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
