cd "\Users\James Mcguire\Data\Projects\DigitalZenWorks\Backup\FlickrStorage\bin\Debug"
del /Q "lPg7wmAA64wCTygslgSKcg==."
del /Q "lPg7wmAA64wCTygslgSKcg==.png"
copy /Y "\Users\James Mcguire\Data\Inbox.txt" .
FlickrStorage encode Inbox.txt password=testtest
FlickrStorage decode Inbox.txt password=testtest
fc lPg7wmAA64wCTygslgSKcg==. Inbox.txt
