using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalZenWorks.BackUp.Library
{
	/// <summary>
	/// Account types enumeration.
	/// </summary>
	public enum AccountType
	{
		/// <summary>
		/// Unknown account type.
		/// </summary>
		Unknown,

		/// <summary>
		/// Drop box account.
		/// </summary>
		DropBox,

		/// <summary>
		/// Google drive account.
		/// </summary>
		GoogleDrive,

		/// <summary>
		/// Google service account.
		/// </summary>
		GoogleServiceAccount,

		/// <summary>
		/// One drive account.
		/// </summary>
		OneDrive,

		/// <summary>
		/// Other account type.
		/// </summary>
		Other
	}
}
