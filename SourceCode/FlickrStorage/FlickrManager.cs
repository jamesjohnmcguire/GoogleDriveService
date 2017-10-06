using FlickrNet;
using System;
using System.Configuration;
using System.IO;
using System.Reflection;

namespace FlickrBackup
{
	public class FlickrManager
	{
		private string accessToken = string.Empty;
		private string apiKey = string.Empty;
		//private OAuthAccessToken oAuthToken;
		private string password = string.Empty;
		private string secretKey = string.Empty;
		private string tokenSecret = string.Empty;
		private Flickr flickr = null;

		//public OAuthAccessToken OAuthToken
		//{
		//	get { return OAuthToken; }
		//	set { oAuthToken = value; }
		//}

		public FlickrManager()
		{
			apiKey = GetSetting("apiKey");
			secretKey = GetSetting("secretKey");

			accessToken = GetSetting("accessToken");
			tokenSecret = GetSetting("tokenSecret");

			flickr = GetAuthInstance();
		}

		public Flickr GetAuthInstance()
		{
			if ((!string.IsNullOrEmpty(apiKey)) &&
				(!string.IsNullOrEmpty(secretKey)))
			{
				flickr = new Flickr(apiKey, secretKey);

				if ((!string.IsNullOrEmpty(accessToken)) &&
					(!string.IsNullOrEmpty(tokenSecret)))
				{
					flickr.OAuthAccessToken = accessToken;
					flickr.OAuthAccessTokenSecret = tokenSecret;
				}
			}

			return flickr;
		}

		public void Init()
		{
			try
			{
				Console.WriteLine("Paste in your Flickr API key: ");
				apiKey = Console.ReadLine();
				UpdateSetting("apiKey", apiKey);

				Console.WriteLine("Paste in your Flickr secret key: ");
				secretKey = Console.ReadLine();
				UpdateSetting("secretKey", secretKey);

				OAuthRequestToken requestToken;

				Flickr flicker = new Flickr(apiKey, secretKey);

				requestToken = flicker.OAuthGetRequestToken("oob");

				string url = flicker.OAuthCalculateAuthorizationUrl(
					requestToken.Token, AuthLevel.Write);

				System.Diagnostics.Process.Start(url);

				Console.WriteLine("Visit this URL in your browser: " + url);
				Console.WriteLine("Paste the number given after logging in:");

				string verifyCode = Console.ReadLine();

				Console.WriteLine("request token token: " +
					requestToken.Token);
				Console.WriteLine("request token secret: " +
					requestToken.TokenSecret);

				OAuthAccessToken oAuthAccessToken =
					flicker.OAuthGetAccessToken(requestToken, verifyCode);

				accessToken = oAuthAccessToken.Token;
				tokenSecret = oAuthAccessToken.TokenSecret;
				UpdateSetting("accessToken", accessToken);
				UpdateSetting("TokenSecret", tokenSecret);

				Console.WriteLine("Successfully authenticated as " +
				oAuthAccessToken.FullName);
			}
			catch (FlickrApiException ex)
			{
				Console.WriteLine(
					"Failed to get access token. Error message: " +
					ex.Message);
			}
		}

		public static void Retrieve(string source, bool encrypt)
		{
			string password = string.Empty;

			if (true == encrypt)
			{
				password = FileImageManager.GetPassword();
			}

			string newFilename = FileImageManager.GetFileNameHash(source);

			//flickr
			FileImageManager.Decode(newFilename + ".png", password);
		}

		public void Store(string source, bool isDirectory, bool recurse,
			bool encrypt)
		{
			try
			{
				if ((true == encrypt) && (string.IsNullOrEmpty(password)))
				{
					password = FileImageManager.GetPassword();
				}

				if (true == isDirectory)
				{
					Directory.SetCurrentDirectory(source);

					foreach (string file in Directory.GetFiles(source))
					{
						Store(file, false, false, true);
					}

					if (true == recurse)
					{
						foreach (string directory in
							Directory.GetDirectories(source))
						{
							Store(directory, true, true, true);
						}
					}
				}
				else
				{
					FileImageManager.Encode(source, password);

					string newFilename =
						FileImageManager.GetFileNameHash(source);
					flickr.UploadPicture(newFilename + ".png",
						newFilename + ".png", string.Empty, string.Empty,
						false, false, false);
					File.Delete(newFilename + ".png");
					Console.WriteLine("Uploaded " + source + " as " +
						newFilename + ".png");
				}
			}
			catch (System.Exception excpt)
			{
				Console.WriteLine(excpt.Message);
			}
		}

		private static string GetSetting(string key)
		{
			return ConfigurationManager.AppSettings[key];
		}

		private static void UpdateSetting(string key, string value)
		{
			Configuration configuration = ConfigurationManager.
				OpenExeConfiguration(Assembly.GetExecutingAssembly().Location);
			configuration.AppSettings.Settings[key].Value = value;
			configuration.Save();

			ConfigurationManager.RefreshSection("appSettings");
		}
	}
}