namespace BackUp.Tests;

using DigitalZenWorks.BackUp.Library;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

[TestFixture]
public class ShouldProcessFolderTests
{
	private string root;
	private string dataPath;
	private string clientsPath;
	private string objPath;
	private string nodeModulesPath;

	[SetUp]
	public void SetUp()
	{
		// Build OS-agnostic paths from the temp directory root
		root = Path.GetTempPath();
		dataPath = Path.Combine(root, "Data");
		clientsPath = Path.Combine(dataPath, "Clients");
		objPath = Path.Combine(dataPath, "obj");
		nodeModulesPath = Path.Combine(dataPath, "node_modules");
	}

	// -------------------------------------------------------------------------
	// Null / empty guards — always permissive
	// -------------------------------------------------------------------------

	[Test]
	public void NullExcludes_ReturnsTrue()
	{
		Assert.That(
			BaseService.ShouldProcessFolder(null, clientsPath),
			Is.True);
	}

	[Test]
	public void EmptyExcludes_ReturnsTrue()
	{
		Assert.That(
			BaseService.ShouldProcessFolder([], clientsPath),
			Is.True);
	}

	[Test]
	public void NullPath_ReturnsTrue()
	{
		ICollection<Exclude> excludes =
			[new Exclude("anything", ExcludeType.SubDirectory)];

		Assert.That(
			BaseService.ShouldProcessFolder(excludes, null),
			Is.True);
	}

	[Test]
	public void EmptyPath_ReturnsTrue()
	{
		ICollection<Exclude> excludes =
			[new Exclude("anything", ExcludeType.SubDirectory)];

		Assert.That(
			BaseService.ShouldProcessFolder(excludes, string.Empty),
			Is.True);
	}

	[Test]
	public void NullExcludeEntry_IsSkipped_ReturnsTrue()
	{
		ICollection<Exclude> excludes = [null];

		Assert.That(
			BaseService.ShouldProcessFolder(excludes, clientsPath),
			Is.True);
	}

	[Test]
	public void ExcludeWithNullPath_IsSkipped_ReturnsTrue()
	{
		ICollection<Exclude> excludes =
			[new Exclude(null, ExcludeType.SubDirectory)];

		Assert.That(
			BaseService.ShouldProcessFolder(excludes, clientsPath),
			Is.True);
	}

	// -------------------------------------------------------------------------
	// Fully qualified exclude vs fully qualified path
	// -------------------------------------------------------------------------

	[Test]
	public void FullyQualifiedExclude_ExactMatch_ReturnsFalse()
	{
		ICollection<Exclude> excludes =
			[new Exclude(clientsPath, ExcludeType.SubDirectory)];

		Assert.That(
			BaseService.ShouldProcessFolder(excludes, clientsPath),
			Is.False);
	}

	[Test]
	public void FullyQualifiedExclude_CaseInsensitiveMatch_ReturnsFalse()
	{
		ICollection<Exclude> excludes =
			[new Exclude(clientsPath.ToLowerInvariant(), ExcludeType.SubDirectory)];

		Assert.That(
			BaseService.ShouldProcessFolder(
				excludes, clientsPath.ToUpperInvariant()),
			Is.False);
	}

	[Test]
	public void FullyQualifiedExclude_DifferentPath_ReturnsTrue()
	{
		ICollection<Exclude> excludes =
			[new Exclude(clientsPath, ExcludeType.SubDirectory)];

		Assert.That(
			BaseService.ShouldProcessFolder(excludes, objPath),
			Is.True);
	}

	// -------------------------------------------------------------------------
	// Relative exclude vs fully qualified path — name-only match
	// Covers the "obj anywhere in the tree" requirement
	// -------------------------------------------------------------------------

	[Test]
	public void RelativeExclude_MatchesFolderNameAtAnyDepth_ReturnsFalse()
	{
		// "obj" should match regardless of where it appears in the tree
		string deepObjPath = Path.Combine(
			dataPath, "Projects", "MyApp", "obj");
		ICollection<Exclude> excludes =
			[new Exclude("obj", ExcludeType.Global)];

		Assert.That(
			BaseService.ShouldProcessFolder(excludes, deepObjPath),
			Is.False);
	}

	[Test]
	public void RelativeExclude_MatchesShallowFolder_ReturnsFalse()
	{
		ICollection<Exclude> excludes =
			[new Exclude("obj", ExcludeType.Global)];

		Assert.That(
			BaseService.ShouldProcessFolder(excludes, objPath),
			Is.False);
	}

	[Test]
	public void RelativeExclude_NoNameMatch_ReturnsTrue()
	{
		ICollection<Exclude> excludes =
			[new Exclude("obj", ExcludeType.Global)];

		Assert.That(
			BaseService.ShouldProcessFolder(excludes, clientsPath),
			Is.True);
	}

	[Test]
	public void RelativeExclude_PartialNameMatch_ReturnsTrue()
	{
		// "obj" should not match "objstore" or "myobj"
		string partialMatch = Path.Combine(dataPath, "objstore");
		ICollection<Exclude> excludes =
			[new Exclude("obj", ExcludeType.Global)];

		Assert.That(
			BaseService.ShouldProcessFolder(excludes, partialMatch),
			Is.True);
	}

	// -------------------------------------------------------------------------
	// Permissive cases
	// -------------------------------------------------------------------------

	[Test]
	public void RelativePath_WithQualifiedExclude_IsPermissive_ReturnsTrue()
	{
		// Cannot definitively match — must allow
		string relativePath = Path.Combine("Data", "Clients");
		ICollection<Exclude> excludes =
			[new Exclude(clientsPath, ExcludeType.SubDirectory)];

		Assert.That(
			BaseService.ShouldProcessFolder(excludes, relativePath),
			Is.True);
	}

	[Test]
	public void RelativePath_WithRelativeExclude_IsPermissive_ReturnsTrue()
	{
		// Both relative — cannot make a definitive match, allow
		string relativePath = Path.Combine("Data", "obj");
		ICollection<Exclude> excludes =
			[new Exclude("obj", ExcludeType.Global)];

		Assert.That(
			BaseService.ShouldProcessFolder(excludes, relativePath),
			Is.True);
	}

	// -------------------------------------------------------------------------
	// ExcludeType filtering
	// -------------------------------------------------------------------------

	[Test]
	public void ExcludeType_File_IsIgnored_ReturnsTrue()
	{
		ICollection<Exclude> excludes =
			[new Exclude(clientsPath, ExcludeType.File)];

		Assert.That(
			BaseService.ShouldProcessFolder(excludes, clientsPath),
			Is.True);
	}

	[Test]
	public void ExcludeType_FileIgnore_IsIgnored_ReturnsTrue()
	{
		ICollection<Exclude> excludes =
			[new Exclude(clientsPath, ExcludeType.FileIgnore)];

		Assert.That(
			BaseService.ShouldProcessFolder(excludes, clientsPath),
			Is.True);
	}

	[Test]
	public void ExcludeType_SubDirectory_IsRespected_ReturnsFalse()
	{
		ICollection<Exclude> excludes =
			[new Exclude(clientsPath, ExcludeType.SubDirectory)];

		Assert.That(
			BaseService.ShouldProcessFolder(excludes, clientsPath),
			Is.False);
	}

	[Test]
	public void ExcludeType_Global_IsRespected_ReturnsFalse()
	{
		ICollection<Exclude> excludes =
			[new Exclude("node_modules", ExcludeType.Global)];

		Assert.That(
			BaseService.ShouldProcessFolder(excludes, nodeModulesPath),
			Is.False);
	}

	[Test]
	public void ExcludeType_Folder_IsRespected_ReturnsFalse()
	{
		ICollection<Exclude> excludes =
			[new Exclude(clientsPath, ExcludeType.SubDirectory)];

		Assert.That(
			BaseService.ShouldProcessFolder(excludes, clientsPath),
			Is.False);
	}

	// -------------------------------------------------------------------------
	// Multiple excludes
	// -------------------------------------------------------------------------

	[Test]
	public void MultipleExcludes_OneMatches_ReturnsFalse()
	{
		ICollection<Exclude> excludes =
		[
			new Exclude("obj", ExcludeType.Global),
		new Exclude("node_modules", ExcludeType.Global),
		new Exclude(".vs", ExcludeType.Global),
	];

		Assert.That(
			BaseService.ShouldProcessFolder(excludes, nodeModulesPath),
			Is.False);
	}

	[Test]
	public void MultipleExcludes_NoneMatch_ReturnsTrue()
	{
		ICollection<Exclude> excludes =
		[
			new Exclude("obj", ExcludeType.Global),
		new Exclude("node_modules", ExcludeType.Global),
		new Exclude(".vs", ExcludeType.Global),
	];

		Assert.That(
			BaseService.ShouldProcessFolder(excludes, clientsPath),
			Is.True);
	}

	[Test]
	public void MultipleExcludes_MixedTypes_OnlyRelevantTypesMatch()
	{
		// File and FileIgnore excludes for clientsPath should not
		// prevent the folder from being processed
		ICollection<Exclude> excludes =
		[
			new Exclude(clientsPath, ExcludeType.File),
		new Exclude(clientsPath, ExcludeType.FileIgnore),
		new Exclude("obj", ExcludeType.Global),
	];

		Assert.That(
			BaseService.ShouldProcessFolder(excludes, clientsPath),
			Is.True);
	}

	// -------------------------------------------------------------------------
	// Subtree exclusion — documents caller responsibility
	// -------------------------------------------------------------------------

	[Test]
	public void RelativeExclude_SubfolderOfExcluded_CallerResponsibility()
	{
		// This test documents that ShouldProcessFolder evaluates each
		// folder in isolation. A subfolder of an excluded folder is NOT
		// automatically excluded — the caller must honour the false result
		// on the parent and not recurse into it.
		string subFolder = Path.Combine(objPath, "Debug", "net8.0");
		ICollection<Exclude> excludes =
			[new Exclude("obj", ExcludeType.Global)];

		// The subfolder itself doesn't match "obj" by name,
		// so this returns true — subtree skipping is the caller's job.
		Assert.That(
			BaseService.ShouldProcessFolder(excludes, subFolder),
			Is.True,
			"Subtree exclusion is the caller's responsibility, " +
			"not this method's.");
	}
}
