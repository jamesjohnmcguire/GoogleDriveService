/////////////////////////////////////////////////////////////////////////////
// <copyright file="ConfigurationInspector.Explanation.cs" company="James John McGuire">
// Copyright © 2017 - 2026 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.BackUp.Reporting;

using System;
using System.Collections.Generic;
using System.IO;
using DigitalZenWorks.BackUp.Library;

/// <summary>
/// Explains local traversal using the backup selection rules.
/// </summary>
public static partial class ConfigurationInspector
{
	/// <summary>Explains a path using a reviewed configuration.</summary>
	/// <param name="report">The report to extend.</param>
	/// <param name="path">The local path.</param>
	public static void Explain(ConfigurationReport report, string path)
	{
		ArgumentNullException.ThrowIfNull(report);
		ArgumentException.ThrowIfNullOrWhiteSpace(path);

		report.Decisions.Clear();
		string normalized = Normalize(report, path, "Path");

		if (normalized == null)
		{
			return;
		}

		report.ExplainedPath = normalized;
		int selected = 0;

		foreach (ReviewedMapping mapping in report.Mappings)
		{
			if (!ContainsPath(mapping.Source, normalized))
			{
				continue;
			}

			MappingExplanation explanation = ExplainMapping(
				report, mapping, normalized);
			report.Decisions.Add(explanation);

			if (explanation.Status == MappingStatus.Selected)
			{
				selected++;
			}
		}

		CheckSelectionCoverage(report, normalized, selected);
	}

	private static MappingExplanation ExplainMapping(
		ConfigurationReport report, ReviewedMapping reviewed, string path)
	{
		MappingExplanation result = new()
		{
			Account = reviewed.Account,
			Location = reviewed.Location,
			Source = reviewed.Source,
			Destination = reviewed.Mapping.DriveParentFolderId,
			Status = MappingStatus.Unsupported,
			Message = "Provider or individual file mapping is unsupported.",
			RemotePolicy =
				"Remote existence and cleanup targets are unverified.",
		};

		if (!reviewed.Supported)
		{
			return result;
		}

		DriveMapping mapping = CopyMapping(reviewed.Mapping);

		try
		{
			ExplainTraversal(report, reviewed.Source, mapping, path, result);
		}
		catch (IOException)
		{
			result.Status = MappingStatus.Unavailable;
			result.Message = "Cannot inspect local traversal or wildcard " +
				"expansion. Check filesystem availability.";
		}
		catch (UnauthorizedAccessException)
		{
			result.Status = MappingStatus.Unavailable;
			result.Message =
				"Cannot inspect local traversal. Check permissions.";
		}
		catch (ArgumentException)
		{
			result.Status = MappingStatus.Invalid;
			result.Message =
				"Cannot expand exclusion paths; correct the rules.";
		}

		return result;
	}

	private static MappingStatus GetMappingStatus(
		TraversalItemResponse response)
	{
		MappingStatus status = MappingStatus.Unknown;

		switch (response)
		{
			case TraversalItemResponse.Selected:
				status = MappingStatus.Selected;
				break;
			case TraversalItemResponse.Excluded:
				status = MappingStatus.Excluded;
				break;
			case TraversalItemResponse.Missing:
				status = MappingStatus.Missing;
				break;
			case TraversalItemResponse.SkippedLink:
				status = MappingStatus.SkippedLink;
				break;
		}

		return status;
	}

	private static DriveMapping CopyMapping(DriveMapping original)
	{
		DriveMapping copy = new()
		{
			LocalPath = original.LocalPath,
			DriveParentFolderId = original.DriveParentFolderId,
		};

		foreach (Exclude rule in original.Excludes)
		{
			Exclude clone = new(rule.Path, rule.KeepOnRemote)
			{
				Reason = rule.Reason,
				Protection = rule.Protection,
				ConfigurationLocation = rule.ConfigurationLocation,
			};

			copy.Excludes.Add(clone);
		}

		return copy;
	}

	private static List<string> GetTraversalSteps(string root, string path)
	{
		List<string> steps = [root];
		string relative = Path.GetRelativePath(root, path);

		if (relative == ".")
		{
			return steps;
		}

		string[] parts = relative.Split(
			[Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar],
			StringSplitOptions.RemoveEmptyEntries);
		string current = root;

		foreach (string part in parts)
		{
			current = Path.Combine(current, part);
			steps.Add(current);
		}

		return steps;
	}

	private static void DescribeExclusion(
		MappingExplanation result, Exclude rule)
	{
		result.RuleLocation = rule.ConfigurationLocation;
		result.Reason = rule.Reason ?? string.Empty;
		result.Protection = rule.Protection ?? string.Empty;
		result.RemotePolicy = "Matching remote content may be removed by " +
			"exclusion cleanup; actual remote content is unverified.";

		if (rule.KeepOnRemote)
		{
			result.RemotePolicy = "Matching remote content is retained by " +
				"this exclusion's policy; remote existence is unverified.";
		}
	}

	private static void CheckSelectionCoverage(
		ConfigurationReport report, string normalized, int selected)
	{
		if (report.Decisions.Count == 0)
		{
			string cov001Message =
				"No configured mapping contains this path. Add a mapping " +
				"if it should be backed up.";

			report.Add(
				DiagnosticSeverity.Warning,
				"COV001",
				normalized,
				cov001Message);
		}
		else if (selected > 1)
		{
			string cov002Message =
				$"{selected} mappings select this path. Check whether " +
				"multiple destinations are intentional.";

			report.Add(
				DiagnosticSeverity.Warning,
				"COV002",
				normalized,
				cov002Message);
		}
	}

	private static void ExplainTraversal(
		ConfigurationReport report,
		string source,
		DriveMapping mapping,
		string path,
		MappingExplanation result)
	{
		ICollection<Exclude> excludes = mapping.ExpandExcludes();
		TraversalContext context = new(report.GlobalExcludes, excludes);

		List<string> steps = GetTraversalSteps(source, path);

		for (int index = 0; index < steps.Count; index++)
		{
			string step = steps[index];
			TraversalItemResponse status = PathSelection.Evaluate(
				step, excludes, out Exclude rule);

			if (status != TraversalItemResponse.Selected)
			{
				result.Status = GetMappingStatus(status);
				result.Message = $"Traversal stops at {step}.";

				if (rule != null)
				{
					DescribeExclusion(result, rule);
				}

				return;
			}

			if (index + 1 < steps.Count)
			{
				string next = steps[index + 1];

				// BackUpFiles receives this directory's input rules.
				// Only descent into another directory gets new globals.
				if (!File.Exists(next))
				{
					excludes = context.ExpandGlobalExcludes(step);
				}
			}
		}

		result.Status = MappingStatus.Selected;
		result.Message = "Selected by local traversal rules. This does " +
			"not establish a successful upload or remote existence.";
	}
}
