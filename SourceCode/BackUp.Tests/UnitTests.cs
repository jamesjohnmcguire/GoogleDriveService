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
		public void RemoveAbandanedFoltersTest()
		{
			Account accountData = new ();

			using GoogleServiceAccount account =
				new (accountData);
			account.IgnoreAbandoned = false;

			string path = Directory.GetCurrentDirectory();
			string[] subDirectoriesRaw = Directory.GetDirectories(path);
			IList<string> subDirectories = [.. subDirectoriesRaw];

			account.RemoveAbandonedFolders(
				path,
				subDirectories,
				null,
				null,
				null);

			Assert.Pass();
		}
	}
}
