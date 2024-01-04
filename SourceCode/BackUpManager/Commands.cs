/////////////////////////////////////////////////////////////////////////////
// <copyright file="Commands.cs" company="James John McGuire">
// Copyright © 2017 - 2024 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

using DigitalZenWorks.CommandLine.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackUpManager
{
	internal class Commands
	{
		public static IList<Command> GetCommands()
		{
			IList<Command> commands = new List<Command>();

			Command help = new ("help");
			help.Description = "Show this information";
			commands.Add(help);

			IList<CommandOption> options = new List<CommandOption>();

			CommandOption ignoreAbandoned = new ("i", "ignore-abandoned", false);
			options.Add(ignoreAbandoned);

			Command backup = new ("backup", options, 0, "Back up files");
			commands.Add(backup);

			return commands;
		}
	}
}
