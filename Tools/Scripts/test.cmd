SET utils=C:\Users\JamesMc\Data\Commands

cd ..\..\SourceCode\FlickrStorage
msbuild /t:Build /p:Configuration=Release FlickrStorage.csproj

cd bin\release
COPY /y *.dll %utils%
COPY /y *.exe %utils%
COPY /y *.config %utils%

cd ..\..\..\..

del /Q EdVyoUSsrRxnf4Py2heOSA==.
del /Q lPg7wmAA64wCTygslgSKcg==.png
copy /Y "%USERPROFILE%\sqlfiles.txt" .
FlickrStorage encode sqlfiles.txt password=testtest
FlickrStorage decode sqlfiles.txt password=testtest
fc EdVyoUSsrRxnf4Py2heOSA==. sqlfiles.txt
