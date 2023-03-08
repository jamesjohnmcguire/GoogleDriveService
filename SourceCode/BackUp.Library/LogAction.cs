/////////////////////////////////////////////////////////////////////////////
// <copyright file="LogAction.cs" company="James John McGuire">
// Copyright © 2017 - 2023 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

using Microsoft.Extensions.Logging;
using System;

namespace DigitalZenWorks.BackUp.Library
{
	internal class LogAction
	{
		private static Action<ILogger, string, Exception>
			logError = LoggerMessage.Define<string>(
				LogLevel.Error,
				1,
				"{Message}");

		private static Action<ILogger, string, Exception>
			logInformation = LoggerMessage.Define<string>(
				LogLevel.Information,
				1,
				"{Message}");

		private static Action<ILogger, string, Exception>
			logWarning = LoggerMessage.Define<string>(
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
			logError(logger, message, exception);
		}

		/// <summary>
		/// Log an informational message.
		/// </summary>
		/// <param name="logger">The ILogger interface.</param>
		/// <param name="message">The message.</param>
		public static void Information(ILogger logger, string message)
		{
			logInformation(logger, message, null);
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
			logWarning(logger, message, exception);
		}
	}
}
