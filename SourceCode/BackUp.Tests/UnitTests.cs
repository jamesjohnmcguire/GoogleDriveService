using DigitalZenWorks.BackUp.Library;
using NUnit.Framework.Internal;

namespace BackUp.Tests
{
	public class Tests
	{
		[SetUp]
		public void Setup()
		{
		}

		[Test]
		public void Test1()
		{
			Assert.Pass();
		}

		[Test]
		public void RemoveAbandanedFoltersNone()
		{
			Account accountData = new ();

			using GoogleServiceAccount account =
				new (accountData);
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

			Assert.That(filesRemoved, Is.EqualTo(0));
		}
	}
}
