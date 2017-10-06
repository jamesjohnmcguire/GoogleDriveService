using FlickrNet;
using System;
using System.IO;

namespace FlickrBackup
{
	internal class Program
	{

		private bool recurse = true;
		private bool isDirectory = false;
		private FlickrManager flickr = null;
		private string password = string.Empty;
		private string source = string.Empty;

		private static void Main(string[] args)
		{
			Program program = new Program();
			program.Run(args);
		}

		private void Decode(string[] args)
		{
			if (args.Length < 2)
			{
				Console.WriteLine("You need to supply a valid filename.");
			}
			else
			{
				GetPassword(args);
				FileImageManager.Decode(args[1], password);
			}
		}

		private void Encode(string[] args)
		{
			if (args.Length < 2)
			{
				Console.WriteLine("You need to supply a valid filename.");
			}
			else
			{
				GetPassword(args);
				FileImageManager.Encode(args[1], password);
			}
		}

		private void GetPassword(string[] args)
		{
			foreach(string arg in args)
			{
				if (arg.ToLower().StartsWith("password=",
					StringComparison.InvariantCulture))
				{
					int startIndex = arg.IndexOf('=') + 1;
					password = arg.Substring(startIndex);
					break;
				}
			}
		}

		private static void Help(string[] args)
		{
			if (args.Length < 2)
			{
				PrintHelp();
			}
			else
			{
				if ((!string.IsNullOrEmpty(args[1])) &&
					(args[1].ToLower().Equals("details")))
				{
					PrintHelp();
				}
			}
		}

		private void Run(string[] args)
		{
			try
			{
				if (args.Length < 1)
				{
					Console.WriteLine("You need to supply some arguments");
					PrintHelp();
				}
				else
				{
					flickr = new FlickrManager();

					switch (args[0].ToLower())
					{
						case "decode":
						{
							Decode(args);
							break;
						}
						case "encode":
						{
							Encode(args);
							break;
						}
						case "help":
						{
							Help(args);
							break;
						}
						case "init":
						{
							flickr.Init();
							break;
						}
						case "retrieve":
						{
							FlickrManager.Retrieve(args[1], true);
							break;
						}
						case "store":
						{
							Store(args);
							break;
						}
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}
		}

		private static void PrintHelp()
		{
			Console.WriteLine("Usage: FlickrStorage command <options> <source>");
			Console.WriteLine("commands are the following:");
			Console.WriteLine("help: this screen");
			Console.WriteLine("store: encode and upload the file or directory to your flickr account");
			Console.WriteLine("retrieve: download and decode the file or directory to your PC");
			Console.WriteLine("init: initialize the program to use your flickr API keys");
			Console.WriteLine("options are the following:");
			Console.WriteLine("norecurse: if the source is a directory, it will only upload store the files in this directory alone.  By default, it will recurse through all the sub-directories");
			Console.WriteLine("examples:");
			Console.WriteLine("Store a single file");
			Console.WriteLine("FlickrStorage store test.xlsx");
			Console.WriteLine("This will prompt you to authorize this action, fill in the session key, fill in your password to encrypt the file, encrypt the file, encode the file in image format, and upload the file.  In your flickr account, the file name will be a hash of the actual file name with the extension 'png'.  If you ever want to retrieve the file, you will need to use the same password to decrypt it.");
			Console.WriteLine("Store a directory");
			Console.WriteLine("FlickrStorage store .");
			Console.WriteLine("Similar to above, except it will store all the files in the directory and sub-directories.");
			Console.WriteLine("First time usage");
			Console.WriteLine("FlickrStorage init");
			Console.WriteLine("You will need to supply your flickr API keys, in order for the program to communicate with flickr.  You can get these keys at: https://www.flickr.com/services/apps/create/apply.  You will need to supply both your API key and your secret key.");
			
		}

		private void Store(string[] args)
		{
			bool validArgument = false;

			foreach (string arg in args)
			{
				if (File.Exists(arg))
				{
					source = arg;
					isDirectory = false;
					validArgument = true;
				}
				else if (Directory.Exists(arg))
				{
					source = arg;
					isDirectory = true;
					validArgument = true;
				}
				else if (arg.ToLower().Equals("norecurse"))
				{
					recurse = false;
				}
			}

			if (false == validArgument)
			{
				Console.WriteLine("no valid file or directory specified.");
			}

			flickr.Store(source, isDirectory, recurse, true);
		}
	}
}