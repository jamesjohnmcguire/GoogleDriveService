<?php
require_once "Informer.php";
require_once "common/common.php";
require_once "common/debug.php";
require_once "common/FileTools.php";
require_once BASE_PATH . "/vendor/fabpot/goutte/Goutte/Client.php";

use Goutte\Client;
// use GuzzleHttp\Client as GuzzleClient;
use Symfony\Component\DomCrawler\Crawler;
use Symfony\Component\HttpClient\HttpClient;

class Scraper extends Informer
{
	protected $agent = null;
	protected $client;
	protected $crawler;
	protected $debug = null;
	protected $lastSet = null;
	public $name = null;

	public function __construct($debug)
	{
		$this->debug = $debug;

		$this->agent = 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) '.
			'AppleWebKit/537.36 (KHTML, like Gecko) '.
			'Chrome/65.0.3325.181 Safari/537.36';

		$this->client = new Client(HttpClient::create(['timeout' => 60]));
		$this->client->setServerParameter('HTTP_USER_AGENT', $this->agent);
	}

	protected static function GetBeginningMarker($columns, $marker, $start,
		$additional)
	{
		$begin = null;
		$index = 0;
		$found = false;
		$count = count($columns);

		for($index = $start; $index < $count; $index++)
		{
			$test = $columns->eq($index)->extract('_text');
			$test = trim($test[0]);
			//echo "$index: $test<br />\r\n";

			$position = strpos($test, $marker);

			if ($position !== false)
			{
				$found = true;
				//echo "marker found<br />\r\n";
				break;
			}
		}

		if (true == $found)
		{
			$begin = $index + $additional;
		}

		return $begin;
	}

	protected function GetPage($method, $url, $options = array())
	{
		$this->debug->Show(Debug::INFO, "checking: $url");

		// get page
		$this->crawler = $this->client->request($method, $url, $options);
		$response = $this->client->getResponse();
		$status = $response->getStatusCode();

		return $status;
	}

	protected function GetPageContent($method, $url, $postData = array())
	{
		$contents = null;
		$status = $this->GetPage($method, $url, $postData);

		if (200 == $status)
		{
			$response = $this->client->getResponse();
			$contents = $response->getContent();
		}

		return $contents;
	}

	protected function GetPageElements($method, $url, $element,
		$postData = array())
	{
		$contents = null;
		$status = $this->GetPage($method, $url, $postData);

		if (200 == $status)
		{
			$contents = $this->crawler->filter($element);
		}

		return $contents;
	}

	protected static function GetPayoutNumber($contents, $index,
		$divide = true)
	{
		$item = $contents->eq($index)->extract('_text');
		$payout = $item[0];
		$payout = str_replace('円', '', $payout);
		$payout = str_replace(',', '', $payout);
		$payout = str_replace(',', '', $payout);

		if (true == $divide)
		{
			$payout = (float)$payout / 100;
		}

		return $payout;
	}

	protected static function GetTrackName($trackNumber, $trackName = null)
	{
		$track = null;

		if (empty($trackNumber))
		{
			if (!empty($trackName))
			{
				//5回中山8日
				$nameJapanese = mb_substr($trackName, 2, 2, 'UTF-8');

				$tracks = array(
					'札幌' => "sapporo",
					'函館' => "hakodate",
					'福島' => "fukushima",
					'新潟' => "niigata",
					'東京' => "tokyo",
					'中山' => "nakayama",
					'中京' => "chukyo",
					'京都' => "kyoto",
					'阪神' => "hanshin",
					'小倉' => "kokura",
				);

				if (array_key_exists($nameJapanese, $tracks))
				{
					$track = $tracks[$nameJapanese];
				}
			}
		}
		else
		{
			$tracks = array(3 => 'Obihiro', 10 => 'Morioka', 11 => 'Mizusawa',
				18 => 'Urawa', 19 => 'Funabashi', 20 => 'Oi', 21 => 'Kawasaki',
				22 => 'Kanazawa',23 => 'Kasamatsu', 24 => 'Nagoya',
				27 => 'Sonoda', 31 => 'Kochi', 32 => 'Saga', 36 => 'Monbetsu');
			//NA => 'Chukyo'
			//NA ==>Himeji'
			//NA => 'Sapporo'
			$track = $tracks[$trackNumber];
		}

		return $track;
	}

	protected function IsWeekend($date)
	{
		$time = strtotime($date);
		$checkDate = date('l', $time);
		$isWeekend = in_array($checkDate, ["Saturday", "Sunday"]);

		return $isWeekend;
	}

	protected function ShowPageContents()
	{
		$response = $this->client->getResponse();
		$contents = $response->getContent();
		$contents = "<pre><code>$contents</code></pre>";
		var_dump($contents);
	}
}
