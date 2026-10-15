/////////////////////////////////////////////////////////////////////////////
// <copyright file="LocalGitInspectorTests.cs" company="James John McGuire">
// Copyright © 2017 - 2026 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.BackUp.Library.Tests;

using System;
using System.Diagnostics;
using System.IO;
using DigitalZenWorks.BackUp.Reporting;
using NUnit.Framework;

/// <summary>
/// Checks local Git protection using disposable repositories.
/// </summary>
[TestFixture]
internal sealed class LocalGitInspectorTests
{
	private string root = string.Empty;

	/// <summary>
	/// Creates an isolated repository without configured remotes.
	/// </summary>
	[SetUp]
	public void SetUp()
	{
		Guid identifier = Guid.NewGuid();
		string name = identifier.ToString();

		root = Path.Combine(Path.GetTempPath(), "BackUp.Git.Tests", name);
		Directory.CreateDirectory(root);
		Git(["init", "--initial-branch=main"]);
	}

	/// <summary>Removes only the repository created for this test.</summary>
	[TearDown]
	public void TearDown()
	{
		// Git marks some object files read-only on Windows.
		string[] files = Directory.GetFiles(
			root, "*", SearchOption.AllDirectories);

		foreach (string file in files)
		{
			File.SetAttributes(file, FileAttributes.Normal);
		}

		Directory.Delete(root, recursive: true);
	}

	/// <summary>Reports untracked work and absent remote protection.</summary>
	[Test]
	public void UntrackedFilesAndMissingRemoteAreReported()
	{
		string file = Path.Combine(root, "local.txt");
		File.WriteAllText(file, "local-only data");
		ConfigurationReport report = new();

		LocalGitInspector.Review(report, root, "Rule");

		Assert.That(HasCode(report, "GIT003"), Is.True);
		Assert.That(HasCode(report, "GIT004"), Is.True);
		Assert.That(HasCode(report, "GIT006"), Is.True);
	}

	/// <summary>
	/// Uses cached upstream state without contacting its remote.
	/// </summary>
	[Test]
	public void UnpushedCommitIsReportedWithoutExposingRemoteUrl()
	{
		string file = Path.Combine(root, "tracked.txt");
		File.WriteAllText(file, "first");
		Git(["add", "tracked.txt"]);
		Commit("Initial");
		Git(["remote", "add", "origin", "https://secret.invalid/private"]);
		Git(["update-ref", "refs/remotes/origin/main", "HEAD"]);
		Git(["branch", "--set-upstream-to=origin/main", "main"]);

		File.WriteAllText(file, "second");
		Git(["add", "tracked.txt"]);
		Commit("Unpushed");
		ConfigurationReport report = new();

		LocalGitInspector.Review(report, root, "Rule");

		Assert.That(HasCode(report, "GIT005"), Is.True);
		Assert.That(HasCode(report, "GIT004"), Is.False);
		Assert.That(HasCode(report, "GIT008"), Is.True);

		foreach (ConfigurationDiagnostic diagnostic in report.Diagnostics)
		{
			Assert.That(diagnostic.Message, Does.Not.Contain("secret.invalid"));
		}
	}

	private static bool HasCode(ConfigurationReport report, string code)
	{
		foreach (ConfigurationDiagnostic diagnostic in report.Diagnostics)
		{
			if (diagnostic.Code == code)
			{
				return true;
			}
		}

		return false;
	}

	private void Commit(string message)
	{
		Git(["-c", "user.name=Test", "-c", "user.email=test@example.invalid",
			"-c", "commit.gpgsign=false", "commit", "-m", message]);
	}

	private void Git(string[] arguments)
	{
		ProcessStartInfo start = new("git")
		{
			WorkingDirectory = root,
			UseShellExecute = false,
			CreateNoWindow = true,
			RedirectStandardOutput = true,
			RedirectStandardError = true,
		};

		foreach (string argument in arguments)
		{
			start.ArgumentList.Add(argument);
		}

		using Process process = new();
		process.StartInfo = start;
		process.Start();

		string output = process.StandardOutput.ReadToEnd();
		string error = process.StandardError.ReadToEnd();
		process.WaitForExit();

		Assert.That(process.ExitCode, Is.Zero, output + error);
	}
}
