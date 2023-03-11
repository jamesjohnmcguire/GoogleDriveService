/////////////////////////////////////////////////////////////////////////////
// <copyright file="LogAction.cs" company="James John McGuire">
// Copyright © 2017 - 2023 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

using Microsoft.Extensions.Logging;
using System;

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
