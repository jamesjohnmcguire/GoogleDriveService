/////////////////////////////////////////////////////////////////////////////
// <copyright file="IBackUpService.cs" company="James John McGuire">
// Copyright © 2017 - 2024 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

using System.Threading.Tasks;

namespace DigitalZenWorks.BackUp.Library
{
	/// <summary>
	/// The back up service interface.
	/// </summary>
	public interface IBackUpService
	{
		/// <summary>
		/// Run method.
		/// </summary>
		/// <param name="configurationFile">The configuration file.</param>
		/// <returns>A task indicating completion.</returns>
		public Task BackUp(string configurationFile);

		/// <summary>
		/// Back up method.
		/// </summary>
		/// <param name="path">The path to back up.</param>
		/// <param name="serviceDestinationId">A service specific
		/// identifier for the destination.</param>
		public void BackUp(string path, string serviceDestinationId);
	}
}
