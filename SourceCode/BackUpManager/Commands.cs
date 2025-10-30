/////////////////////////////////////////////////////////////////////////////
// <copyright file="Commands.cs" company="James John McGuire">
// Copyright © 2017 - 2025 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace BackUpManager
{
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Globalization;
	using System.Linq;
	using DigitalZenWorks.CommandLine.Commands;
	using DigitalZenWorks.Common.VersionUtilities;
	using Serilog;

	/// <summary>
	/// Commands class.
	/// </summary>
	internal static class Commands
	{
		/// <summary>
		/// Gets the list of commands.
		/// </summary>
		/// <returns>The list of commands.</returns>
		public static IList<Command> GetCommands()
		{
			List<Command> commands = [];

			Command help = new ("help");
			help.Description = "Show this information";
			commands.Add(help);

			List<CommandOption> options = [];

			CommandOption configFile = new ("c", "config", true);
			options.Add(configFile);

			CommandOption ignoreAbandoned =
				new ("i", "ignore-abandoned", false);
			options.Add(ignoreAbandoned);

			Command backup = new ("backup", options, 0, "Back up files");
			commands.Add(backup);

			return commands;
		}

		/// <summary>
		/// Show help.
		/// </summary>
		/// <param name="additionalMessage">An additional message,
		/// if any.</param>
		public static void ShowHelp(string additionalMessage = null)
		{
			FileVersionInfo fileVersionInfo =
				VersionSupport.GetAssemblyInformation();

			if (fileVersionInfo != null)
			{
				string assemblyVersion = fileVersionInfo.FileVersion;
				string companyName = fileVersionInfo.CompanyName;
				string copyright = fileVersionInfo.LegalCopyright;
				string name = fileVersionInfo.FileName;

				string header = string.Format(
					CultureInfo.CurrentCulture,
					"{0} {1} {2} {3}",
					name,
					assemblyVersion,
					copyright,
					companyName);
				Log.Logger.Information(header);
			}

			if (!string.IsNullOrWhiteSpace(additionalMessage))
			{
				Log.Logger.Information(additionalMessage);
			}
		}

		/// <summary>
		/// Update arguments.
		/// </summary>
		/// <param name="commands">The command.</param>
		/// <param name="arguments">The arguments.</param>
		/// <returns>The updated command arguments.</returns>
		public static string[] UpdateArguments(
			IList<Command> commands, string[] arguments)
		{
			if (arguments == null || arguments.Length == 0)
			{
				arguments = new string[1];
				arguments[0] = "backup";
			}
			else
			{
				string requestedCommand = arguments[0];
				bool exists = commands.Any(c => c.Name == requestedCommand);

				if (exists == false)
				{
					int length = arguments.Length + 1;
					string[] newArguments = new string[length];
					newArguments[0] = "backup";

					for (int index = 0; index < arguments.Length; index++)
					{
						newArguments[index + 1] = arguments[index];
					}

					arguments = newArguments;
				}
			}

			return arguments;
		}
	}
}
