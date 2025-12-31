/////////////////////////////////////////////////////////////////////////////
// <copyright file="UnitTests.cs" company="James John McGuire">
// Copyright © 2017 - 2025 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace BackUp.Tests
{
	using System.Collections.Generic;
	using System.IO;
	using DigitalZenWorks.BackUp.Library;
	using NUnit.Framework;
	using NUnit.Framework.Internal;

	/// <summary>
	/// Contains unit tests for verifying the behavior of the
	/// GoogleServiceAccount class and related functionality.
	/// </summary>
	/// <remarks>This class uses the NUnit testing framework. Each test method
	/// is marked with the [Test] attribute and is executed independently by
	/// the test runner. The [SetUp] method is called before each test to
	/// perform any necessary initialization.</remarks>
	public class UnitTests
	{
		/// <summary>
		/// Initializes resources or state before each test is run.
		/// </summary>
		/// <remarks>This method is called automatically by the test framework
		/// before each test method in the class executes. Override or
		/// implement this method to set up any required test data or
		/// environment.</remarks>
		[SetUp]
		public void Setup()
		{
		}

		/// <summary>
		/// Verifies that the test framework is functioning correctly by
		/// passing unconditionally.
		/// </summary>
		[Test]
		public void Test1()
		{
			Assert.Pass();
		}

		/// <summary>
		/// Verifies that no folders are removed when there are no abandoned
		/// folders present.
		/// </summary>
		/// <remarks>This test ensures that the RemoveAbandonedFolders method
		/// returns 0 when the IgnoreAbandoned property is set to false and no
		/// abandoned folders exist in the specified directory.</remarks>
		[Test]
		public void RemoveAbandanedFoltersNone()
		{
			Account accountData = new();

			using GoogleServiceAccount account = new(accountData);
			account.IgnoreAbandoned = false;

			string path = Directory.GetCurrentDirectory();
			string[] subDirectoriesRaw = Directory.GetDirectories(path);
			IList<string> subDirectories = [.. subDirectoriesRaw];

			int filesRemoved = account.RemoveAbandonedFolders(
				path,
				subDirectories,
				null,
				null,
				null);

			Assert.That(filesRemoved, Is.Zero);
		}
	}
}
