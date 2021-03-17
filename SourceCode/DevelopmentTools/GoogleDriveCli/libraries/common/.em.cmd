@IF "%1"=="pull" GOTO pull
@IF "%1"=="push" GOTO push
goto end

:push
copy /Y common.php	%PhpToolsPath%
copy /Y debug.php	%PhpToolsPath%
copy /Y FileTools.php	%PhpToolsPath%
copy /Y WebData.php	%PhpToolsPath%

copy /Y common.php	..\..\..\KeibaGoDataMiner\libraries\common
copy /Y debug.php	..\..\..\KeibaGoDataMiner\libraries\common
copy /Y FileTools.php	..\..\..\KeibaGoDataMiner\libraries\common
copy /Y WebData.php	..\..\..\KeibaGoDataMiner\libraries\common
goto end

:pull
copy /Y %PhpToolsPath%\common.php .
copy /Y %PhpToolsPath%\debug.php .
copy /Y %PhpToolsPath%\FileTools.php .
copy /Y %PhpToolsPath%\WebData.php .
copy /Y ..\..\..\KeibaGoDataMiner\libraries\common.php .
copy /Y ..\..\..\KeibaGoDataMiner\libraries\debug.php .
copy /Y ..\..\..\KeibaGoDataMiner\libraries\FileTools.php .
copy /Y ..\..\..\KeibaGoDataMiner\libraries\WebData.php .
goto end

:end
