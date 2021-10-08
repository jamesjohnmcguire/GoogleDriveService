<?php
/////////////////////////////////////////////////////////////////////////////
// $Id: $
//
// WebData related functions
//
// NOTES:
//
/////////////////////////////////////////////////////////////////////////////
class WebData
{
	private function __construct() {}

	/************************************************************************
	 * GetWebPage function
	 *
	 * Get a web file (HTML, XHTML, XML, image, etc.) from a URL.  Return an
	 * array containing the HTTP server response header fields and content.
	 * @access	public
	 * @param	string
	 * @param	bool
	 * @param	string
	 * @param	bool
	 * @param	bool
	 * @param	bool
	 * @param	string
	 * @param	int
	 * @param	int
	 ***********************************************************************/
	public static function GetWebPage($url, $usePost = false, $sendData = '',
		$followRedirects = true, $newSession = false, $includeHeader = false,
		$agent = null, $headers = null, $method = null, $timeOut = 0,
		$maxRedirects = -1, $cookieSendData = null)
	{
		$curlObject	= curl_init($url);

		$path = getcwd();
		$cookie_file_path = $path."/cookiejar.txt";

		curl_setopt($curlObject, CURLOPT_RETURNTRANSFER, true);
		curl_setopt($curlObject, CURLOPT_NOBODY, false);

		// handle all encodings
		curl_setopt($curlObject, CURLOPT_ENCODING, "");
		// set referer on redirect
		curl_setopt($curlObject, CURLOPT_AUTOREFERER, true);
		//curl_setopt($curlObject, CURLOPT_VERBOSE, 1);
		curl_setopt($curlObject, CURLOPT_SSL_VERIFYPEER, false);
		curl_setopt($curlObject, CURLOPT_SSL_VERIFYHOST, false);

		if (array_key_exists('REQUEST_URI', $_SERVER))
		{
			curl_setopt(
				$curlObject, CURLOPT_REFERER, $_SERVER['REQUEST_URI']);
		}

		curl_setopt($curlObject, CURLOPT_FOLLOWLOCATION, $followRedirects);

		// identifies the caller (this)
		if (empty($agent))
		{
			$agent = 'Mozilla/5.0 (Windows NT 10.0; WOW64; rv:53.0) '.
				'Gecko/20100101 Firefox/53.0';
		}
		curl_setopt($curlObject, CURLOPT_USERAGENT, $agent);
		curl_setopt($curlObject, CURLOPT_CONNECTTIMEOUT, $timeOut);
		// timeout on response
		curl_setopt($curlObject, CURLOPT_TIMEOUT, $timeOut);
		curl_setopt($curlObject, CURLOPT_MAXREDIRS, $maxRedirects);

		if ($newSession == true)
		{
			curl_setopt($curlObject, CURLOPT_COOKIESESSION, true);
		}

		if (!empty($cookieSendData))
		{
			curl_setopt($curlObject, CURLOPT_COOKIE, $cookieSendData);
		}

		if (true == $includeHeader)
		{
			curl_setopt($curlObject, CURLOPT_HEADER, true);
			curl_setopt($curlObject, CURLINFO_HEADER_OUT, true);

			// Assuming if headers wanted, probably cookies too
			curl_setopt($ch, CURLOPT_COOKIEJAR, $cookie_file_path);
		}

		curl_setopt($curlObject, CURLOPT_POST, $usePost);

		if (TRUE == $usePost)
		{
			curl_setopt($curlObject, CURLOPT_CUSTOMREQUEST, "POST");
			curl_setopt($curlObject, CURLOPT_POSTFIELDS, $sendData);
		}
		else if (!empty($method))
		{
			curl_setopt($curlObject, CURLOPT_CUSTOMREQUEST, $method);
		}

		if (!empty($headers))
		{
			curl_setopt($curlObject, CURLOPT_HTTPHEADER, $headers);
		}

		$content = curl_exec($curlObject);
		$header  = curl_getinfo($curlObject);

		if (true == $includeHeader)
		{
			$header['errorNumber'] = curl_errno($curlObject);
			$header['errorMessage'] = curl_error($curlObject);

			$header['headers'] = $headers;

			$cookies = file_get_contents($cookie_file_path);
			$header['cookies'] = $cookies;

			$header['content'] = $content;
			$result = $header;
		}
		else
		{
			$result = $content;
		}

		curl_close($curlObject);

		return $result;
	}
}

?>
