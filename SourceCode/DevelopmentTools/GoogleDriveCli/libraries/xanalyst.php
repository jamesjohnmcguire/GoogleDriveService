<?php
require_once "CLI.php";
require_once "MSSQL.php";

require_once "common/debug.php";
require_once "common/common.php";

use Goutte\Client;
use GuzzleHttp\Client as GuzzleClient;
use Symfony\Component\DomCrawler\Crawler;

class XAnalystWarehouse
{
	private $client;
	private $crawler;
	private $database;
	private $logFile = null;

	public function __construct($level, $logFile)
	{
		$this->logFile = $logFile;
		$this->debug = new Debug($level, $logFile);

		$this->client = new Client();
		$guzzleClient = new GuzzleClient(array('timeout' => 60));
		$this->client->setClient($guzzleClient);
	}

	public function SendSectionalTimes($date, $venue, $eventNumber, $data)
	{
		$result = false;

		$this->Connect();

		$time = Common::GetNowMicroTime();
		$this->debug->Show(Debug::DEBUG, "$time: Sending data");

		$meetingId = $this->FindMeeting($date, $venue);

		// need to fix up the data
		$count = count($data);

		if (($count % 2) !== 0)
		{
			// add an element with a 0 value, to make even
			array_unshift($data, '0');
		}

		$count = count($data);

		// race is too long, sum up extra parts into first section
		if ($count > 12)
		{
			$item = 0;
			while ($count > 12)
			{
				$item += (float)array_shift($data);
				$count--;
			}

			$final = $item + (float)$data[0];
			$data[0] = (string)$final;
		}

		$times = array();
		$keys = array('ST_6', 'ST_5', 'ST_4', 'ST_3', 'ST_2', 'ST_1');
		for($index = $count - 1; $index >= 0; $index--)
		{
			$key = array_shift($keys);
			// need to combine 2 times into one
			// also, need to fill starting from the back 
			$time = $data[$index] + $data[$index - 1];
			$times[$key] = $time * 100;
			$index--;
		}

		$this->debug->Show(Debug::DEBUG, "date: $date");
		$this->debug->Show(Debug::DEBUG, "venue: $venue");
		$this->debug->Show(Debug::DEBUG, "meetingId: $meetingId");
		$this->debug->Show(Debug::DEBUG, "eventNumber: $eventNumber");
		$this->debug->Dump(Debug::DEBUG, $data);
		$this->debug->Dump(Debug::DEBUG, $times);

		$where = ['MEETING_ID' => $meetingId, 'EVENT_NO' => $eventNumber];
		$result = $this->database->updateRows("EVENT", $times, $where, 1);
		$result = true;

		if (empty($result))
		{
			echo CLI::red(" ✗  DW :: ERROR".EOL);

			$result = false;
		}
		else
		{
			echo CLI::green(" ✔  DW :: OK".EOL);
			$this->debug->Show(Debug::DEBUG, "status: OK");
			$result = true;
		}

		return $result;
	}

	private function Connect()
	{
		if (null == $this->database)
		{
			$this->database = new MSSQL(
				"XDATA", "analyst", "analyst@xGroup#75", "101.0.102.42");
		}
	}

	private function FindMeeting($date, $track)
	{
		$meetingId = null;

		$row = $this->database->fetchSelectorRow("MEETING",
			['MEETING_DATE' => $date, 'COUNTRY' => "JP", 'VENUE' => $track]);

		if (!empty($row))
		{
			$meetingId = $row['MEETING_ID'];
		}

		return $meetingId;
	}

	private function FindRunner($id, $event, $number)
	{
		$row = $this->database->fetchSelectorRow("RUNNER",
			['MEETING_ID' => $id, 'EVENT_NO' => $event,
			'RUNNER_NO' => $number]);
		return $row;
	}
}
