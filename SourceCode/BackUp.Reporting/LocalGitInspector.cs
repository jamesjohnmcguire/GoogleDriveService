/////////////////////////////////////////////////////////////////////////////
// <copyright file="LocalGitInspector.cs" company="James John McGuire">
// Copyright © 2017 - 2026 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.BackUp.Reporting;

using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

/// <summary>
/// Inspects declared Git protection without contacting remotes.
/// </summary>
internal static class LocalGitInspector
{
	/// <summary>Reports local repository state for an exclusion.</summary>
	/// <param name="report">The report to extend.</param>
	/// <param name="path">The excluded directory.</param>
	/// <param name="location">The configuration rule location.</param>
	internal static void Review(
		ConfigurationReport report, string path, string location)
	{
		if (!Directory.Exists(path))
		{
			report.Add(
				DiagnosticSeverity.Warning,
				"GIT002",
				location,
				"Git check requires an existing excluded directory.");
			return;
		}

		try
		{
			ReviewRepository(report, path, location);
		}
		catch (System.ComponentModel.Win32Exception)
		{
			report.Add(
				DiagnosticSeverity.Warning,
				"GIT007",
				location,
				"Git is unavailable. Install Git or omit --check-git.");
		}
		catch (TimeoutException)
		{
			report.Add(
				DiagnosticSeverity.Warning,
				"GIT007",
				location,
				"Local Git inspection timed out; protection is unverified.");
		}
	}

	private static string Run(string path, string[] arguments)
	{
		ProcessStartInfo start = CreateStartInfo(path, arguments);

		using Process process = new();
		process.StartInfo = start;
		process.Start();

		Task<string> output = process.StandardOutput.ReadToEndAsync();
		Task<string> error = process.StandardError.ReadToEndAsync();

		if (!process.WaitForExit(10000))
		{
			process.Kill(true);
			throw new TimeoutException();
		}

		Task.WaitAll(output, error);

		if (process.ExitCode != 0)
		{
			return null;
		}

		return output.Result.Trim();
	}

	private static void ReviewRepository(
		ConfigurationReport report, string path, string location)
	{
		string root = Run(path, ["rev-parse", "--show-toplevel"]);

		if (root == null)
		{
			report.Add(
				DiagnosticSeverity.Warning,
				"GIT002",
				location,
				"No local Git working tree found.");
			return;
		}

		ReviewRemotes(report, path, location);

		ReviewWorkingTree(report, path, location);

		ReviewUpstream(report, path, location);

		string git009Message =
			"This checks the containing working tree using cached " +
			"upstream history. Ignored files and remote content are " +
			"not verified; retain separate protection for local-only data.";

		report.Add(
			DiagnosticSeverity.Information,
			"GIT009",
			location,
			git009Message);
	}

	private static void ReviewRemotes(
		ConfigurationReport report, string path, string location)
	{
		string remotes = Run(path, ["remote"]);

		if (string.IsNullOrWhiteSpace(remotes))
		{
			report.Add(
				DiagnosticSeverity.Warning,
				"GIT003",
				location,
				"No Git remote is configured. Review declared protection.");
		}
		else
		{
			string git008Message = "Configured remote names: " +
				remotes.ReplaceLineEndings(", ") +
				". Remote availability and pushed contents are unverified.";

			report.Add(
				DiagnosticSeverity.Information,
				"GIT008",
				location,
				git008Message);
		}
	}

	private static void ReviewWorkingTree(
		ConfigurationReport report, string path, string location)
	{
		string status = Run(
			path,
			["status", "--porcelain=v1", "--untracked-files=normal"]);

		if (status == null)
		{
			report.Add(
				DiagnosticSeverity.Warning,
				"GIT007",
				location,
				"Cannot read Git working tree status.");
		}
		else if (status.Length > 0)
		{
			string git004Message =
				"The repository has changes or untracked files. " +
				"Git remote protection does not cover them.";

			report.Add(
				DiagnosticSeverity.Warning,
				"GIT004",
				location,
				git004Message);
		}
	}

	private static void ReviewUpstream(
		ConfigurationReport report, string path, string location)
	{
		string counts = Run(
			path,
			["rev-list", "--left-right", "--count", "HEAD...@{upstream}"]);

		if (counts == null)
		{
			report.Add(
				DiagnosticSeverity.Warning,
				"GIT006",
				location,
				"No usable upstream history. Check branch tracking.");
		}
		else
		{
			string[] values = counts.Split(
				(char[])null, StringSplitOptions.RemoveEmptyEntries);

			if (values.Length == 2 &&
				int.TryParse(values[0], out int ahead) && ahead > 0)
			{
				string git005Message =
					$"{ahead} commit(s) ahead of the locally recorded " +
					"upstream. No fetch was performed.";

				report.Add(
					DiagnosticSeverity.Warning,
					"GIT005",
					location,
					git005Message);
			}
		}
	}

	private static ProcessStartInfo CreateStartInfo(
		string path, string[] arguments)
	{
		ProcessStartInfo start = new("git")
		{
			WorkingDirectory = path,
			UseShellExecute = false,
			CreateNoWindow = true,
			RedirectStandardOutput = true,
			RedirectStandardError = true,
		};

		start.Environment["GIT_OPTIONAL_LOCKS"] = "0";
		start.Environment["GIT_TERMINAL_PROMPT"] = "0";
		start.ArgumentList.Add("-c");
		start.ArgumentList.Add("core.fsmonitor=false");

		foreach (string argument in arguments)
		{
			start.ArgumentList.Add(argument);
		}

		return start;
	}
}
