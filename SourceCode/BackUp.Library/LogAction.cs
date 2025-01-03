/////////////////////////////////////////////////////////////////////////////
// <copyright file="LogAction.cs" company="James John McGuire">
// Copyright © 2017 - 2025 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

using Microsoft.Extensions.Logging;
using System;
using System.Runtime.CompilerServices;

namespace DigitalZenWorks.BackUp.Library
{
	internal static class LogAction
	{
		private static readonly Action<ILogger, string, Exception>
			LogError = LoggerMessage.Define<string>(
				LogLevel.Error,
				1,
				"{Message}");

		private static readonly Action<ILogger, string, Exception>
			LogInformation = LoggerMessage.Define<string>(
				LogLevel.Information,
				1,
				"{Message}");

		private static readonly Action<ILogger, string, Exception>
			LogWarning = LoggerMessage.Define<string>(
				LogLevel.Warning,
				1,
				"{Message}");

		/// <summary>
		/// Log an error.
		/// </summary>
		/// <param name="logger">The ILogger interface.</param>
		/// <param name="message">The message.</param>
		/// <param name="exception">The exception.</param>
		public static void Error(
			ILogger logger, string message, Exception exception)
		{
			LogError(logger, message, exception);
		}

		/// <summary>
		/// Log Exception.
		/// </summary>
		/// <param name="logger">The ILogger interface.</param>
		/// <param name="exception">The exception.</param>
		/// <param name="caller">The caller.</param>
		/// <param name="lineNumber">The line number.</param>
		public static void Exception(
			ILogger logger,
			Exception exception,
			[CallerMemberName] string caller = null,
			[CallerLineNumber] int lineNumber = 0)
		{
			string message = $"Exception at: {caller}: Line: {lineNumber}";
			LogAction.Error(logger, message, exception);
		}

		/// <summary>
		/// Log an informational message.
		/// </summary>
		/// <param name="logger">The ILogger interface.</param>
		/// <param name="message">The message.</param>
		public static void Information(ILogger logger, string message)
		{
			LogInformation(logger, message, null);
		}

		/// <summary>
		/// Log an warning.
		/// </summary>
		/// <param name="logger">The ILogger interface.</param>
		/// <param name="message">The message.</param>
		/// <param name="exception">The exception.</param>
		public static void Warning(
			ILogger logger, string message, Exception exception)
		{
			LogWarning(logger, message, exception);
		}
	}
}
