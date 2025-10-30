/////////////////////////////////////////////////////////////////////////////
// <copyright file="LogAction.cs" company="James John McGuire">
// Copyright © 2017 - 2025 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.BackUp.Library
{
	using System;
	using System.Runtime.CompilerServices;
	using Microsoft.Extensions.Logging;

	/// <summary>
	/// The log action class.
	/// </summary>
	internal static class LogAction
	{
		private static readonly Action<ILogger, string, Exception>
			LogError = LoggerMessage.Define<string>(
				LogLevel.Error,
				new EventId(1000, nameof(LogError)),
				"{Message}");

		private static readonly Action<ILogger, string, Exception>
			LogInformation = LoggerMessage.Define<string>(
				LogLevel.Information,
				new EventId(1000, nameof(LogInformation)),
				"{Message}");

		private static readonly Action<ILogger, string, Exception>
			LogWarning = LoggerMessage.Define<string>(
				LogLevel.Warning,
				new EventId(1000, nameof(LogWarning)),
				"{Message}");

		/// <summary>
		/// Log an error.
		/// </summary>
		/// <param name="logger">The ILogger interface.</param>
		/// <param name="message">The message.</param>
		public static void Error(
			ILogger logger, string message)
		{
			LogError(logger, message, null);
		}

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
			string message = $"Unhandled exception in {caller} " +
				$"(line {lineNumber}): {exception.Message}";
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
		public static void Warning(ILogger logger, string message)
		{
			LogWarning(logger, message, null);
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
