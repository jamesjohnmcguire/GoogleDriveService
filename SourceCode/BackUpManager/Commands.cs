/////////////////////////////////////////////////////////////////////////////
// <copyright file="Commands.cs" company="James John McGuire">
// Copyright © 2017 - 2024 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

using DigitalZenWorks.CommandLine.Commands;
using DigitalZenWorks.Common.VersionUtilities;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;

namespace BackUpManager
{
	internal static class Commands
	{
		public static IList<Command> GetCommands()
		{
			IList<Command> commands = new List<Command>();

			Command help = new ("help");
			help.Description = "Show this information";
			commands.Add(help);

			IList<CommandOption> options = new List<CommandOption>();

			CommandOption configFile = new ("c", "config", true);
			options.Add(configFile);

			CommandOption ignoreAbandoned = new ("i", "ignore-abandoned", false);
			options.Add(ignoreAbandoned);

			Command backup = new ("backup", options, 0, "Back up files");
			commands.Add(backup);

			return commands;
		}

		public static void ShowHelp(string additionalMessage)
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

		public static string[] UpdateArguments(string[] arguments)
		{
			if (arguments == null || arguments.Length == 0)
			{
				arguments = new string[1];
				arguments[0] = "backup";
			}
			else if (!arguments[0].Equals(
				"backup", StringComparison.Ordinal))
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

			return arguments;
		}
	}
}
