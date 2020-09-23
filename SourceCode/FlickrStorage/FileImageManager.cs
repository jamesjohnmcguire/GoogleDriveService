using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace FlickrBackup
{
	public static class FileImageManager
	{
		private static string Inputkey =
			"AA38C3DA-4868-4A8D-B90A-1BB4615D6A59";

		public static void Decode(string fileName, string password)
		{
			if (string.IsNullOrWhiteSpace(password))
			{
				password = GetPassword();
			}

			string hashName = GetFileNameHash(fileName) + ".png";

			if (!File.Exists(hashName))
			{
				Console.WriteLine("Can't decode. File doesn't exist");
			}
			else
			{
				Bitmap bitmap = (Bitmap)Image.FromFile(hashName);

				int width = bitmap.Width;
				int height = bitmap.Height;
				BitmapData data = bitmap.LockBits
					(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly,
					bitmap.PixelFormat);

				try
				{
					ExtractFileFromImage(hashName, bitmap, data, true, password);
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.Message);
				}
				finally
				{
					bitmap.UnlockBits(data);
				}
			}
		}

		public static void Encode(string fileName, string password)
		{
			// this may be a command line option some day
			bool encrypt = true;

			string activeFile = fileName;

			if (true == encrypt)
			{
				if (string.IsNullOrWhiteSpace(password))
				{
					password = GetPassword();
				}

				EncryptFile(fileName, password);

				activeFile = fileName + ".enc";
			}

			int[] dimensions = GetBitmapDimensions(activeFile);
			SaveFileAsImage(fileName, true, dimensions[0], dimensions[1],
				password);
		}

		public static string GetFileNameHash(string fileName)
		{
			byte[] bytes = Encoding.UTF8.GetBytes(fileName);
			byte[] hash = null;

			using (MD5 md5Hash = MD5.Create())
			{
				hash = md5Hash.ComputeHash(bytes);
			}

			string filenameHash = Convert.ToBase64String(hash);
			string newFilename = filenameHash.Replace("/", "_");

			return newFilename;
		}

		public static string GetPassword()
		{
			string password = string.Empty;
			bool verified = false;

			Console.WriteLine("Enter a password for file encryption");
			Console.WriteLine("If you ever need to restore these files, you will need the same password");

			while (false == verified)
			{
				Console.WriteLine("password:");

				password = Console.ReadLine();
				Console.WriteLine("password again:");

				string passwordVerifier = Console.ReadLine();

				if (passwordVerifier.Equals(password))
				{
					Console.WriteLine("Password matched");

					verified = true;
				}
				else
				{
					Console.WriteLine("Password did NOT match");
				}
			}

			return password;
		}

		private static void EncryptFile(string filename, string password)
		{
			try
			{
				string outFile = filename + ".enc";

				using (FileStream outFileStream = new FileStream(outFile, FileMode.Create))
				{
					using (RijndaelManaged rijndaelManaged =
						GetEncrypterObject(password))
					{
						ICryptoTransform transform =
							rijndaelManaged.CreateEncryptor();

						// Now write the cipher text using
						// a CryptoStream for encrypting.
						using (CryptoStream outStreamEncrypted =
							new CryptoStream(outFileStream, transform,
								CryptoStreamMode.Write))
						{
							// encrypt by chunks to accommodate large files
							int count = 0;

							// 4096 is NTFS friendly
							int blockSizeBytes = 4096;
							byte[] data = new byte[blockSizeBytes];

							using (FileStream inFs =
								new FileStream(filename, FileMode.Open))
							{
								do
								{
									count = inFs.Read(data, 0, blockSizeBytes);
									outStreamEncrypted.Write(data, 0, count);
								}
								while (count > 0);
								inFs.Close();
							}
							outStreamEncrypted.FlushFinalBlock();
							outStreamEncrypted.Close();
						}
					}
					outFileStream.Close();
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}
		}

		private static string EncryptFileName(string text, string password)
		{
			string encryptedFileName = string.Empty;

			try
			{
				using (MemoryStream stream = new MemoryStream())
				{
					using (RijndaelManaged rijndaelManaged =
						GetEncrypterObject(password))
					{
						ICryptoTransform transform =
							rijndaelManaged.CreateEncryptor();

						using (CryptoStream EncryptedStream = new CryptoStream(
							stream, transform, CryptoStreamMode.Write))
						{
							using (StreamWriter writer =
								new StreamWriter(EncryptedStream))
							{
								writer.Write(text);
							}
						}
					}
					encryptedFileName =
						Convert.ToBase64String(stream.ToArray());
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}

			return encryptedFileName;
		}

		private static string DecryptFileName(string text, string password)
		{
			string fileName = string.Empty;

			try
			{
				byte[] cipher = Convert.FromBase64String(text);

				using (MemoryStream stream = new MemoryStream(cipher))
				{
					using (RijndaelManaged rijndaelManaged =
						GetEncrypterObject(password))
					{
						ICryptoTransform transform =
							rijndaelManaged.CreateDecryptor();
						{
							using (CryptoStream decryptStream =
								new CryptoStream(stream, transform,
									CryptoStreamMode.Read))
							{
								using (var srDecrypt =
									new StreamReader(decryptStream))
								{
									fileName = srDecrypt.ReadToEnd();
								}
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}

			return fileName;
		}

		private static string ExtractFileFromImage(string fileName, Bitmap bitmap,
			BitmapData bitmapData, bool encrypt, string password)
		{
			string extractedFile = string.Empty;
			string actualFilename = string.Empty;
			string outFile = Path.GetFileNameWithoutExtension(fileName);

			using (FileStream outFileStream = new FileStream(outFile,
				FileMode.Create))
			{
				if (false == encrypt)
				{
					outFile = Path.GetFileNameWithoutExtension(fileName);
					using (BinaryWriter writer =
						new BinaryWriter(outFileStream))
					{
						WriteImageBitsToStream(bitmap, bitmapData,
							outFileStream, password);
						outFileStream.Flush();
						outFileStream.Close();
					}
				}
				else
				{
					using (RijndaelManaged rijndaelManaged =
						GetEncrypterObject(password))
					{
						ICryptoTransform transform =
							rijndaelManaged.CreateDecryptor();

						using (CryptoStream outStreamDecrypted =
							new CryptoStream(outFileStream, transform,
								CryptoStreamMode.Write))
						{
							WriteImageBitsToStream(bitmap, bitmapData,
								outStreamDecrypted, password);
							outStreamDecrypted.FlushFinalBlock();
							outStreamDecrypted.Close();
						}
					}
				}

				outFileStream.Close();

				File.Move(outFile, actualFilename);
			}

			return extractedFile;
		}

		private static int[] GetBitmapDimensions(string fileName)
		{
			int[] dimensions = new int[2];

			byte[] filenameBytes = new byte[0];
			filenameBytes = GetEncodedFilename(fileName);

			FileInfo fileInfo = new FileInfo(fileName);
			// In addition to the file itself, we are storing the file size,
			// filename size and the file name
			long totalSize = filenameBytes.Length + fileInfo.Length +
				sizeof(long) + sizeof(int);

			// by casting to int, we've trimmed a bit, increase by 1 to
			// compensate
			int width = (int)Math.Sqrt(totalSize);

			int height = width + 1;

			if (totalSize < 225)
			{
				// too much of a hassle to have a width under 15
				width = 16;
				height = (int)(totalSize / width) + 1;
			}

			// if a file is, for example, 11 bytes, 3 * 4 can hold it
			// if a file is, 15 bytes, 4 * 4 is needed
			if (fileInfo.Length > (height * width))
			{
				width++;
			}

			dimensions[0] = width;
			dimensions[1] = height;

			return dimensions;
		}

		private static byte[] GetEncodedFilename(string filename)
		{
			byte[] bytesFilename =
				System.Text.Encoding.UTF8.GetBytes(filename);
			//string encodedFilename =
			//	System.Convert.ToBase64String(bytesFilename);

			return bytesFilename;
		}

		private static RijndaelManaged GetEncrypterObject(string password)
		{
			byte[] passwordBytes = Encoding.ASCII.GetBytes(password);

			// Create Rijndael instance for symmetric encryption of the data.
			RijndaelManaged rijndaelManaged = new RijndaelManaged();
			rijndaelManaged.KeySize = 256;
			rijndaelManaged.BlockSize = 256;
			rijndaelManaged.Mode = CipherMode.CBC;

			using (Rfc2898DeriveBytes key =
				new Rfc2898DeriveBytes(Inputkey, passwordBytes))
			{
				rijndaelManaged.Key =
					key.GetBytes(rijndaelManaged.KeySize / 8);
				rijndaelManaged.IV =
					key.GetBytes(rijndaelManaged.BlockSize / 8);
			}

			return rijndaelManaged;
		}

		private static byte[] GetFileLengthBytes(string filename)
		{
			byte[] size = null;
			FileInfo fileInfo = new FileInfo(filename);

			size = BitConverter.GetBytes(fileInfo.Length);

			return size;
		}

		private static byte[] GetFilenameLength(byte[] encodedFileName)
		{
			byte[] size = BitConverter.GetBytes(encodedFileName.Length);

			return size;
		}

		private static int GetFilenameSize(BitmapData bitmapData)
		{
			int fileNameSize = 0;

			IntPtr source = bitmapData.Scan0 + 1 + sizeof(long);

			// get the stored actual file size
			int bufferSize = sizeof(int);
			byte[] fileNameSizeBytes = new byte[bufferSize];

			Marshal.Copy(source, fileNameSizeBytes, 0, bufferSize);
			fileNameSize = BitConverter.ToInt32(fileNameSizeBytes, 0);

			return fileNameSize;
		}

		private static long GetFileSize(BitmapData bitmapData)
		{
			long fileSize = 0;

			IntPtr source = bitmapData.Scan0 + 1;

			// get the stored actual file size
			int bufferSize = sizeof(long);
			byte[] fileSizeBytes = new byte[bufferSize];

			Marshal.Copy(source, fileSizeBytes, 0, bufferSize);
			fileSize = BitConverter.ToInt64(fileSizeBytes, 0);

			return fileSize;
		}

		private static void SaveFileAsImage(string fileName, bool encrypt,
			int width, int height, string password)
		{
			string activeFile = fileName;
			if (true == encrypt)
			{
				activeFile = fileName + ".enc";
			}

			using (Bitmap bitmap = new Bitmap(width, height,
				System.Drawing.Imaging.PixelFormat.Format8bppIndexed))
			{
				BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0,
					bitmap.Width, bitmap.Height), ImageLockMode.WriteOnly,
					bitmap.PixelFormat);

				try
				{
					using (FileStream stream = new FileStream(activeFile,
						FileMode.Open, FileAccess.Read))
					{
						using (BinaryReader reader = new BinaryReader(stream))
						{
							WriteStreamToImage(bitmap, bitmapData, fileName,
								reader, encrypt, password);
						}
					}

					if (true == encrypt)
					{
						File.Delete(activeFile);
					}
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.Message);
				}
				finally
				{
					bitmap.UnlockBits(bitmapData);
				}

				string newFilename = GetFileNameHash(fileName);
				bitmap.Save(newFilename + ".png", ImageFormat.Bmp);
				bitmap.Save(newFilename + "3.png", ImageFormat.Png);
			}
		}

		private static void SaveFilename(byte[] fileName,
			BitmapData bitmapData)
		{
			IntPtr destination = bitmapData.Scan0 + 1 + sizeof(long) +
				sizeof(int);

			Marshal.Copy(fileName, 0, destination, fileName.Length);
		}

		private static void SaveFilenameSize(byte[] fileName,
			BitmapData bitmapData)
		{
			IntPtr destination = bitmapData.Scan0 + 1 + sizeof(long);

			byte[] filenameSize = GetFilenameLength(fileName);

			Marshal.Copy(filenameSize, 0, destination, sizeof(int));
		}

		private static void SaveFileSize(string fileName,
			BitmapData bitmapData)
		{
			IntPtr destination = bitmapData.Scan0 + 1;
			byte[] fileSize = GetFileLengthBytes(fileName);

			Marshal.Copy(fileSize, 0, destination, fileSize.Length);
		}

		private static int WriteFileInformation(string fileName,
			BitmapData bitmapData, bool encrypt, string password)
		{
			IntPtr destination = bitmapData.Scan0;

			string activeFile = fileName;
			if (true == encrypt)
			{
				activeFile = fileName + ".enc";
				fileName = EncryptFileName(fileName, password);
			}

			// in the future, the format might change, but still need
			// to handle older formats
			byte[] fileFormatVersion = new byte[1];
			fileFormatVersion[0] = 1;
			Marshal.Copy(fileFormatVersion, 0, destination, 1);

			// need to save the file size for decoding
			SaveFileSize(activeFile, bitmapData);

			// need to save the file name size
			byte[] encodedFileName = GetEncodedFilename(fileName);

			SaveFilenameSize(encodedFileName, bitmapData);

			// copy the actual filename
			SaveFilename(encodedFileName, bitmapData);

			return encodedFileName.Length;
		}

		private static string WriteImageBitsToStream(Bitmap bitmap,
			BitmapData data, Stream stream, string password)
		{
			string actualFileName = string.Empty;
			byte[] bytes = new byte[bitmap.Width];
			long fileSize = 0;
			long bytesWritten = 0;
			int filenameSize = 0;
			byte[] filenameBytes = null;

			for (int rowIndex = 0; rowIndex < data.Height; ++rowIndex)
			{
				int fileRowLength = bitmap.Width;

				int offset = rowIndex * data.Stride;
				IntPtr source = data.Scan0 + offset;

				// some things to do for the first row
				if (0 == rowIndex)
				{
					// first byte is fileFormatVersion, not used yet

					// get the stored actual file size
					fileSize = GetFileSize(data);

					// get the file name size
					filenameSize = GetFilenameSize(data);

					// get the actual file name
					filenameBytes = new byte[filenameSize];

					offset = 1 + sizeof(long) + sizeof(int);
					source = data.Scan0 + offset;
					Marshal.Copy(source, filenameBytes, 0, filenameSize);
					string encryptedFileName =
						Encoding.UTF8.GetString(filenameBytes);
					actualFileName = DecryptFileName(encryptedFileName,
						password);

					// adjust copied bytes length for the stream
					offset = 1 + sizeof(long) + sizeof(int) +
						filenameBytes.Length;
					source = data.Scan0 + offset;
					fileRowLength = bitmap.Width - offset;
				}

				if (rowIndex == data.Height - 1)
				{
					// need only to copy to the actual end of file
					// inside the image, it may contain extra 'padding'
					offset = 1 + sizeof(long) + sizeof(int) +
						filenameBytes.Length;
					fileRowLength =
						(int)fileSize + offset - (rowIndex * data.Width);
				}

				Marshal.Copy(source, bytes, 0, fileRowLength);
				stream.Write(bytes, 0, fileRowLength);
				bytesWritten += fileRowLength;
			}

			return actualFileName;
		}

		private static void WriteStreamToImage(Bitmap bitmap,
			BitmapData bitmapData, string fileName, BinaryReader reader,
			bool encrypt, string password)
		{
			int fileNameLength =
				WriteFileInformation(fileName, bitmapData, encrypt, password);

			// Copy to bitmapData row by row (to account for
			// the case where bitmapData.Stride != bitmap.Width)
			for (int rowIndex = 0; rowIndex < bitmapData.Height; rowIndex++)
			{
				int fileRowLength = bitmap.Width;

				IntPtr destination = bitmapData.Scan0 + (rowIndex *
					bitmapData.Stride);

				// some things to do for the first row
				if (0 == rowIndex)
				{
					// need to adjust the amount of bytes to copy from the file
					int offset = 1 + sizeof(long) + sizeof(int) +
						fileNameLength;
					destination = bitmapData.Scan0 + offset;
					fileRowLength = bitmap.Width - offset;
				}

				byte[] fileBytes = reader.ReadBytes(fileRowLength);

				if (fileBytes.Length > 0)
				{
					Marshal.Copy(fileBytes, 0, destination, fileBytes.Length);
				}
			}
		}
	}
}