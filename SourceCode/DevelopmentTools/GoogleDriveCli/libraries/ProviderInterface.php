<?php

interface ProviderInterface
{
	//public function Videos($countryCode, $date, $track);
	public function Videos($startDate, $endDate);
}
