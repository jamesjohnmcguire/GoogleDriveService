<?php

if (PHP_SAPI == 'cli')
{
	defined('EOL') OR define('EOL', PHP_EOL);
}
else
{
	defined('EOL') OR define('EOL', '<br />'.PHP_EOL);
}

class Common
{
	public static function AreContentsSame(&$previous, $current)
	{
		$different = false;

		if (empty($current))
		{
			// regardless of state of previous, if current is empty,
			// it's point of action
			$different = true;
			$previous = $current;
		}
		else
		{
			if (empty($previous))
			{
				$previous = $current;
				$different = true;
			}
			else if ($previous !== $current)
			{
				$previous = $current;
				$different = true;
			}
		}

		return $different;
	}

	public static function ConvertJapaneseDate($date)
	{
		$date = str_replace('年', '-', $date);
		$date = str_replace('月', '-', $date);
		$date = str_replace('日', '', $date);

		$time = strtotime($date);
		$date = date('Y-m-d', $time);

		return $date;
	}

	public static function FlushBuffers()
	{
		if (ob_get_level() > 0)
		{
			ob_flush();
		}
		flush();
	}

	public static function GetNowMicroTime()
	{
		$time = 'none';
		$timezone = new DateTimeZone('Asia/Tokyo');
		$now = DateTime::createFromFormat('U.u', microtime(true));

		if (!empty($now))
		{
			$now->setTimeZone($timezone);
			$time = $now->format("Y-m-d H:i:s:u");
		}


		return $time;
	}

	public static function GetNowTime()
	{
		$timezone = new DateTimeZone('Asia/Tokyo');
		$time = '';
		$now = new DateTime();
		if (!empty($now))
		{
			$now->setTimeZone($timezone);
			$time = $now->format("Y-m-d H:i:s:u");
		}

		return $time;
	}

	public static function IsValidTime($time, $beginTime = null,
		$endTime = null)
	{
		$isTime = false;
		$time = trim($time);

		$length = strlen($time);
		if ($length < 6)
		{
			$test = preg_match("/(2[0-3]|[01][0-9]):([0-5][0-9])/", $time);
			if ($test == 1)
			{
				if (($beginTime != null) && ($endTime != null))
				{
					$testTime = DateTime::createFromFormat('H:i', $time);
					$begin = DateTime::createFromFormat('H:i', $beginTime);
					$end = DateTime::createFromFormat('H:i', $endTime);
					if ($testTime > $begin && $testTime < $end)
					{
						$isTime = true;
					}
				}
				else
				{
					$isTime = true;
				}
			}
		}

		return $isTime;
	}

	public static function SetDebugMode()
	{
		error_reporting(E_ALL);
		ini_set("display_errors", 1);
		ini_set('implicit_flush', 1);
		ob_implicit_flush(true);
		set_time_limit(0);

		ini_set('xdebug.var_display_max_depth', -1);
		ini_set('xdebug.var_display_max_children', -1);
		ini_set('xdebug.var_display_max_data', -1);
	}
}

?>