/////////////////////////////////////////////////////////////////////////////
// <copyright file="IBackUpService.cs" company="James John McGuire">
// Copyright © 2017 - 2023 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.BackUp.Library
{
	/// <summary>
	/// The back up service interface.
	/// </summary>
	public interface IBackUpService
	{
		/// <summary>
		/// Back up method.
		/// </summary>
		/// <param name="path">The path to back up.</param>
		/// <param name="serviceDestinationId">A service specific
		/// identifier for the destination.</param>
		public void BackUp(string path, string serviceDestinationId);
	}
}
