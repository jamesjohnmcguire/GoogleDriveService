@IF "%1"=="pull" GOTO pull
@IF "%1"=="push" GOTO push
goto end

:push
copy /Y CLI.php	..\..\JraDataMiner\libraries
copy /Y ConsumerInterface.php	..\..\JraDataMiner\libraries
copy /Y datawherehouse.php	..\..\JraDataMiner\libraries
copy /Y Informer.php	..\..\JraDataMiner\libraries
copy /Y MSSQL.php	..\..\JraDataMiner\libraries
copy /Y ProviderInterface.php	..\..\JraDataMiner\libraries
copy /Y Scraper.php	..\..\JraDataMiner\libraries
copy /Y xanalyst.php	..\..\JraDataMiner\libraries

copy /Y CLI.php	..\..\UkIreMiner\libraries
copy /Y ConsumerInterface.php	..\..\UkIreMiner\libraries
copy /Y datawherehouse.php	..\..\UkIreMiner\libraries
copy /Y Informer.php	..\..\UkIreMiner\libraries
copy /Y MSSQL.php	..\..\UkIreMiner\libraries
copy /Y ProviderInterface.php	..\..\UkIreMiner\libraries
copy /Y Scraper.php	..\..\UkIreMiner\libraries
copy /Y xanalyst.php	..\..\UkIreMiner\libraries
goto end

:pull
copy /Y ..\..\JraDataMiner\libraries\CLI.php .
copy /Y ..\..\JraDataMiner\libraries\ConsumerInterface.php .
copy /Y ..\..\JraDataMiner\libraries\datawherehouse.php .
copy /Y ..\..\JraDataMiner\libraries\Informer.php .
copy /Y ..\..\JraDataMiner\libraries\MSSQL.php .
copy /Y ..\..\JraDataMiner\libraries\ProviderInterface.php .
copy /Y ..\..\JraDataMiner\libraries\Scraper.php .
copy /Y ..\..\JraDataMiner\libraries\xanalyst.php .
goto end

:end
