/////////////////////////////////////////////////////////////////////////////
// <copyright file="IBackUpServiceData.cs" company="James John McGuire">
// Copyright © 2017 - 2023 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace BackupManagerLibrary
{
	/// <summary>
	/// The back up service data interface.
	/// </summary>
	public interface IBackUpServiceData
	{
		/// <summary>
		/// Gets or sets the Name of service.
		/// </summary>
		/// <value>The Name of service.</value>
		public string Name { get; set; }
	}
}
