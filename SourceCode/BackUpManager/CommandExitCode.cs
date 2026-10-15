/////////////////////////////////////////////////////////////////////////////
// <copyright file="CommandExitCode.cs" company="James John McGuire">
// Copyright © 2017 - 2026 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace BackUpManager;

/// <summary>Defines process exit codes for command execution.</summary>
internal enum CommandExitCode
{
	/// <summary>The command completed without errors.</summary>
	Success = 0,

	/// <summary>The reviewed configuration contains errors.</summary>
	ConfigurationError = 1,

	/// <summary>The command arguments are invalid.</summary>
	InvalidArguments = 2,
}
