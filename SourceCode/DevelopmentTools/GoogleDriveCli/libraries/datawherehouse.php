<?php
require_once "CLI.php";

require_once "common/debug.php";
require_once "common/common.php";

use Goutte\Client;
use GuzzleHttp\Client as GuzzleClient;
use Symfony\Component\DomCrawler\Crawler;

class DataWherehouse
{
	private $client;
	private $crawler;
	private $logFile = null;

	public function __construct($level, $logFile)
	{
		$this->logFile = $logFile;
		$this->debug = new Debug($level, $logFile);

		$this->client = new Client();
		$guzzleClient = new GuzzleClient(array('timeout' => 60));
		$this->client->setClient($guzzleClient);
	}

	public function GetCompetitors($eventId, $raceNumber)
	{
		$competitorIds = null;
		$jsonData = $this->GetCachedEventsFile($eventId);

		$competitorIds = $this->GetCompetitorsInternal($eventId, $raceNumber,
			$jsonData);

		if (empty($competitorIds))
		{
			// events file may not be up to date
			$jsonData = $this->GetEventsFile($eventId);

			$competitorIds = $this->GetCompetitorsInternal($eventId,
				$raceNumber, $jsonData);
		}

		Common::FlushBuffers();

		return $competitorIds;
	}

	public function GetEventId($trackName, $raceNumber, $date = null)
	{
		$eventId = null;

		$this->debug->Show(Debug::INFO, "track: $trackName");

		if (empty($date))
		{
			$date = date('Y-m-d');
		}

		$jsonData = $this->GetCachedMeetingFile($date);
		$data = json_decode($jsonData, true);
		$meetings = $data["data"]["meetings"];

		if (empty($meetings))
		{
			$this->debug->Show(Debug::ERROR, "no meetings");
		}
		else
		{
			foreach($meetings as $meeting)
			{
				if (strtolower($meeting['name']) == strtolower($trackName))
				{
					$events = $meeting['events'];
					foreach($events as $event)
					{
						if ($event['number'] == $raceNumber)
						{
							// our event
							$eventId = $event['event_id'];
							break;
						}
					}

					if (null != $eventId)
					{
						break;
					}
				}
			}
		}

		return $eventId;
	}

	public function GetMeetingId($trackName, $date = null)
	{
		$meetingId = null;

		$this->debug->Show(Debug::INFO, "track: $trackName");

		if (empty($date))
		{
			$date = date('Y-m-d');
		}

		$jsonData = $this->GetCachedMeetingFile($date);

		if (!empty($jsonData))
		{
			$data = json_decode($jsonData, true);
			$meetings = $data["data"]["meetings"];

			foreach($meetings as $meeting)
			{
				if (strtolower($meeting['name']) == strtolower($trackName))
				{
					$meetingId = $meeting['meeting_id'];
					break;
				}
			}
		}

		return $meetingId;
	}

	public function PrepMarketData($meetingId, $raceNumber, $type, $data,
		$key = 'japan_nar')
	{
		$debug = json_encode($data);
		$this->debug->Show(Debug::INFO, "PrepMarketData: $debug");
		$japanNar = array();
		$japanNar[$type] = $data;

		$marketData = array();
		$marketData[$key] = $japanNar;

		$dataPoints = array();
		$dataPoints['event_number'] = $raceNumber;
		$dataPoints['meeting_id'] = $meetingId;
		$dataPoints['provider_id'] = 27;
		$dataPoints['provider_market_data'] = $marketData;

		$debug = json_encode($dataPoints);
		$this->debug->Show(Debug::INFO, "PrepMarketData dataPoints: $debug");

		return $dataPoints;
	}

	public function Send($method, $api, $meetingId, $eventId, $data,
		$endPoint = null)
	{
		$result = false;
		$jsonData = json_encode($data);

		//$url = "http://dw-staging-elb-1068016683.ap-southeast-2.elb.amazonaws.com/api/$api/$endPoint";
		$url = "http://staging.dw.xtradeiom.com/api/$api/$endPoint";
		//$url = "https://staging.dw.xtradeiom.com/api/$api/$endPoint";
		$time = Common::GetNowMicroTime();
		$this->debug->Show(Debug::DEBUG, "$time: Sending to: $url");
		$this->debug->Show(Debug::DEBUG, "$time: Sending data: $jsonData");

		//$crawler = $client->request('POST', 'http://example.com/post.php',
		//[], [], ['HTTP_CONTENT_TYPE' => 'application/x-www-form-urlencoded'], $content);
		$type = ['HTTP_CONTENT_TYPE' => 'application/json'];

		$this->crawler = $this->client->request($method, $url,
			array(), array(), $type, $jsonData);

		$status = (int)$this->client->getResponse()->getStatus();
		$response = $this->client->getResponse();
		$contents = $response->getContent();
		$json = json_decode($contents, true);
		//var_dump($contents);
		//echo "<br />\r\n";
		$endpoint = "$api/$endPoint";

		if ($status < 200 || $status >= 300)
		{
			echo CLI::red(" ✗  DW  $status :: ".CLI::rpad($json['status']).
				"  [ ".CLI::rpad($method).CLI::rpad($endpoint, 22)." ]".
				CLI::lpad("$time ms", 12).EOL);

			if ($json)
			{
				if ($json['errors'])
				{
					foreach ($json['errors'] as $e => &$error)
					{
						if (is_array($error) && $error['backtrace'])
						{
							foreach ($error['backtrace'] as $b => $backtrace)
							{
								if (strpos($backtrace,
									"/var/www/dw/shared/bundle/ruby/") !==
									false)
								{
									unset($error['backtrace'][$b]);
								}
							}
						}
					}
				}
				print_r($json);
			}

			$result = false;
		}
		else
		{
			 //✔  DW  200 :: OK        [ PUT     /api/events/xxx        ]      565 ms
			echo CLI::green(" ✔  DW  $status :: ".CLI::rpad($json['status'])).
				"  [ " . CLI::rpad($method).CLI::rpad($endpoint, 22)." ]".
				CLI::lpad("$time ms", 12).EOL;
			$this->debug->Show(Debug::DEBUG, "status: $status");
			$result = true;
		}

		return $result;
	}

	public function SendFinishPlace($meetingId, $eventId, $raceNumber,
		$competitorId, $horse, $position)
	{
		$this->debug->Show(Debug::INFO, "SendFinishPlace begin");

		if (null != $competitorId)
		{
			$this->debug->Show(Debug::INFO, "sending finishing place: ".
				"meetingId: $meetingId - eventId: $eventId");
			$finishPosition = array();
			$finishPosition['finish_position'] = $position;

			$competitor = array();
			$competitor['event_number'] = $raceNumber;
			$competitor['meeting_id'] = $meetingId;
			$competitor['race_data'] = $finishPosition;
			$competitor['runner_number'] = $horse;

			$this->debug->Dump(Debug::INFO, $competitor);

			$result = $this->Send('PATCH', 'event_competitors', $meetingId,
				$eventId, $competitor, $competitorId);

			return $result;
		}
	}

	public function SendStatus($meetingId, $eventId, $status)
	{
		$data = array();
		$data['meeting_id'] = $meetingId;
		$data['event_number'] = $eventId;
		$data['status'] = $status;
		$result = $this->Send('PUT', 'events', $meetingId, $eventId, $data, 'xxx/status');

		return $result;
	}

	private function GetCachedEventsFile($eventId, $date = null)
	{
		$data = null;

		if (empty($date))
		{
			$date = date('Ymd');
		}

		$file = "events/$date.$eventId.json";

		if (!file_exists($file))
		{
			$data = $this->GetEventsFile($eventId);
			$result = file_put_contents($file, $data);
		}
		else
		{
			$data = file_get_contents($file);
		}

		return $data;
	}

	private function GetCachedMeetingFile($date = null)
	{
		$data = null;

		if (empty($date))
		{
			$date = date('Ymd');
		}

		$file = "meetings/$date.json";

		if (!file_exists($file))
		{
			$data = $this->GetMeetingsFile($date);
			$result = file_put_contents($file, $data);
		}
		else
		{
			$data = file_get_contents($file);
		}

		return $data;
	}

	private function GetCompetitorsInternal($eventId, $raceNumber, $jsonData)
	{
		$competitorIds = null;

		$this->debug->Show(Debug::DEBUG, "GetCompetitorsInternal: ".
			"$eventId - $raceNumber");
		$data = json_decode($jsonData, true);

		//var_dump($jsonData);
		//echo "<br />\r\n";
		$events = $data["data"]["events"];
		//var_dump($events);

		if (!empty($events))
		{
			foreach($events as $event)
			{
				if ($event['number'] == $raceNumber)
				{
					$competitors = $event['event_competitors'];
					foreach($competitors as $competitor)
					{
						$index = $competitor['number'];
						$competitorIds[$index] =
							$competitor['event_competitor_id'];
					}

					break;
				}
			}
		}

		return $competitorIds;
	}

	private function GetEventsFile($eventId)
	{
		$data = null;

		$url = "http://dw-staging-elb-1068016683.ap-southeast-2.elb.".
			"amazonaws.com/api/events/$eventId";

		$this->crawler = $this->client->request('GET', $url);
		$status = $this->client->getResponse()->getStatus();

		if (200 == $status)
		{
			$data = $this->client->getResponse()->getContent();
		}
		else
		{
			$this->debug->Show(Debug::ERROR, "GetEventsFile bad status code");
		}

		return $data;
	}

	private function GetMeetingsFile($date = null)
	{
		$data = null;

		if (empty($date))
		{
			$date = date('Ymd');
		}

		$url = "http://dw-staging-elb-1068016683.ap-southeast-2.elb".
			".amazonaws.com/api/meetings?filters[meeting_date]=$date&".
			"filters[countries][]=JPN";

		$this->crawler = $this->client->request('GET', $url);
		$status = $this->client->getResponse()->getStatus();

		if (200 == $status)
		{
			$data = $this->client->getResponse()->getContent();
		}
		else
		{
			$this->debug->Show(Debug::ERROR, "bad status code");
		}
		
		return $data;
	}
}
