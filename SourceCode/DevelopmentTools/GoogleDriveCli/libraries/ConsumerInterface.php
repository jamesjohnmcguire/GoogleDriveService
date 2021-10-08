<?php

interface ConsumerInterface
{
	public function SaveVideo($countryCode, $year, $month, $day, $trackName,
		$raceNumber, $fileType, $sourceFile);
}
