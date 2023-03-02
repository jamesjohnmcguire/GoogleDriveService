/////////////////////////////////////////////////////////////////////////////
// <copyright file="IBackUpService.cs" company="James John McGuire">
// Copyright © 2017 - 2023 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace BackupManagerLibrary
{
	/// <summary>
	/// The back up service interface.
	/// </summary>
	public interface IBackUpService
	{
		/// <summary>
		/// Back up method.
		/// </summary>
		/// <param name="serviceData">The service data.</param>
		/// <param name="path">The path to back up.</param>
		public void BackUp(IBackUpServiceData serviceData, string path);
	}
}
