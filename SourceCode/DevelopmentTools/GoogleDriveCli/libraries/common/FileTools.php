<?php
require_once "WebData.php";

class FileTools
{
	public static function AreFileContentsSame($previous, $new, $debug = null)
	{
		$same = true;

		if ((null != $previous) || (null != $new))
		{
			if ((null == $previous) && (null != $new))
			{
				self::Show("compare condition 1 is false", $debug);
				$same = false;
			}
			elseif ((null != $previous) && (null == $new))
			{
				self::Show("compare condition 2 is false", $debug);
				self::Show("safe failure, so ignoring", $debug);
				//$same = false;
			}
			elseif ((file_exists($previous)) || (file_exists($new)))
			{
				if ((!file_exists($previous)) && (file_exists($new)))
				{
					self::Show("compare condition 3 is false", $debug);
					self::Show("previous: $previous", $debug);
					self::Show("new: $new", $debug);
					$same = false;
				}
				elseif ((file_exists($previous)) && (!file_exists($new)))
				{
					self::Show("compare condition 4 is false", $debug);
					self::Show("safe failure, so ignoring", $debug);
					//$same = false;
				}
				else
				{
					$remoteSize = filesize($new);
					$oldSize = filesize($previous);

					// Check if filesize is different
					if($oldSize !== $remoteSize)
					{
						self::Show("compare condition 5 is false", $debug);
						self::Show("previous size: $oldSize", $debug);
						self::Show("new size: $remoteSize", $debug);
						$same = false;
					}
					else
					{
						// Check if content is different
						$previousHandle = fopen($previous, 'rb');
						$newHandle = fopen($new, 'rb');

						if (($previousHandle !== false) &&
							($newHandle !== false))
						{
							while(!feof($previousHandle))
							{
								$oldData = fread($previousHandle, 8192);
								$newData = fread($newHandle, 8192);

								if($oldData != $newData)
								{
									self::Show("compare condition 6 is false",
										$debug);
									$same = false;
									break;
								}
							}
						}

						fclose($previousHandle);
						fclose($newHandle);
					}
				}
			}
		}

		return $same;
	}

	public static function CreateDirectory($basePath, $directory,
		$debug = null)
	{
		$path = $basePath .'/'.$directory;

		if ((!file_exists($path)) && (!is_dir($path)))
		{
			$result = mkdir($path, 0755);

			if (false == $result)
			{
				self::Show("could not create: $path", $debug);
			}
		}

		return $path;
	}

	public static function DoesRemoteFileExist($file)
	{
		$exists = false;

		$headers = @get_headers($file, 1);

		$status = self::GetRemoteFileStatus($headers);

		if ($status < 400)
		{
			$exists = true;
		}

		return $exists;
	}

	public static function DownloadFile($source, $destination,
		$overwrite = false, $debug = null)
	{
		$exists = false;

		$agent = 'Mozilla/5.0 (Windows NT 10.0; WOW64; rv:53.0) Gecko/20100101 Firefox/53.0';

		if (!FileTools::DoesRemoteFileExist($source))
		{
			self::Show("file doesn't exist: $source", $debug);
		}
		else
		{
			self::Show("getting contents of: $source", $debug);
			$contents = file_get_contents($source);

			if (false === $contents)
			{
				self::Show("failed getting contents of: $source .. try again", $debug);
				sleep(1);
				//$source = str_replace('http:', 'https:', $source);
				self::Show("trying again: $source", $debug);
				$contents = WebData::GetWebPage($source, false, null, true,
					true, false, $agent);
				if (!empty($contents))
				{
					self::Show("contents NOT empty", $debug);
				}
			}

			if (false === $contents)
			{
				self::Show("failed getting contents of: $source", $debug);
			}
			else
			{
				if ((false == $overwrite) && (file_exists($destination)))
				{
					self::Show("file already exists", $debug);
					$exists = true;
				}
				else
				{
					$result = file_put_contents($destination, $contents);

					if (false === $result)
					{
						if (null != $debug)
						{
							self::Show("not saved: $destination", $debug);
							$errorCode = error_get_last();
							$debug->Dump($errorCode);
							$debug->Log($errorCode);
						}
					}

					self::Show("success saved file: $destination", $debug);
					$exists = true;
				}
			}
		}

		return $exists;
	}

	public static function GetClassNameFromFile($fileName, $includeNamespace)
	{
		$contents = file_get_contents($fileName);
 
		$namespace = $class = "";
 
		// Set helper values to know that we have found the namespace/class
		// token and need to collect the string values after them
		$gettingNamespace = $gettingClass = false;
 
		// Go through each token and evaluate it as necessary
		foreach (token_get_all($contents) as $token)
		{
			// If this token is the namespace declaring, then flag that
			// the next tokens will be the namespace name
			if (is_array($token) && $token[0] == T_NAMESPACE)
			{
				$gettingNamespace = true;
			}
 
			// If this token is the class declaring, then flag that the
			// next tokens will be the class name
			if (is_array($token) && $token[0] == T_CLASS)
			{
				$gettingClass = true;
			}
 
			if ($gettingNamespace === true)
			{
				// If the token is a string or the namespace separator...
				if (is_array($token) &&
					in_array($token[0], [T_STRING, T_NS_SEPARATOR]))
				{
					$namespace .= $token[1];
				}
				else if ($token === ';')
				{
 					// If the token is the semicolon, then we're done with
					// the namespace declaration
					$gettingNamespace = false;
				}
			}
 
			if ($gettingClass === true)
			{
				// If the token is a string, it's the name of the class
				if (is_array($token) && $token[0] == T_STRING)
				{
					$class = $token[1];
					break;
				}
			}
		}
 
		// Build the fully-qualified class name and return it
		if (($includeNamespace == false) || (empty($namespace)))
		{
			$fullClass = $class;
		}
		else
		{
			$fullClass = $namespace . '\\' . $class;
		}

		return $fullClass;
	}

	public static function GetRemoteSize($url, $debug = null, $agent = null)
	{
		$size = null;

		$curl = curl_init($url);

		// Issue a HEAD request and follow any redirects.
		curl_setopt($curl, CURLOPT_NOBODY, true);
		curl_setopt($curl, CURLOPT_HEADER, true);
		curl_setopt($curl, CURLOPT_RETURNTRANSFER, true);
		curl_setopt($curl, CURLOPT_FOLLOWLOCATION, true);

		if (!empty($agent))
		{
			curl_setopt($curl, CURLOPT_USERAGENT, $agent);
		}

		$data = curl_exec($curl);

		if ($data)
		{
			if(preg_match("/Content-Length: (\d+)/", $data, $matches))
			{
				$size = (int)$matches[1];
			}
		}
		else
		{
			if (null != $debug)
			{
				$error = curl_error($curl);
				$debug->Dump($error);
			}
		}

		curl_close($curl);

		return $size;
	}

	public static function GetRemoteFileStatus($headers, $url = null,
		$debug = null)
	{
		$status = null;

		if ((empty($headers)) & (!empty($url)))
		{
			$headers = @get_headers($url, 1);

			if (null != $debug)
			{
				$debug->Dump($headers);
			}
		}

		if (!empty($headers))
		{
			$status = (int)substr($headers[0], 9, 3);
		}

		return $status;
	}

	public static function GetRemoteTime($url, $debug = null)
	{
		$time = null;

		$headers = @get_headers($url, 1);

		$status = self::GetRemoteFileStatus($headers);

		if ($status < 400)
		{
			$dateTime = new \DateTime($headers['Last-Modified']);
			$time = $dateTime->format('Y-m-d H:i:s');
		}

		return $time;
	}

	private static function Show($message, $debug)
	{
		if (null != $debug)
		{
			$debug->Show(Debug::INFO, $message);
		}
	}
}
