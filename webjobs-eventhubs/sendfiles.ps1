$i = 0
while ($true)
{
    $i = $i + 1
    Write-Host "transactions$i.csv" -ForegroundColor Yellow
    az storage blob upload --account-name hellomessagingdataaue -f ./transactions.csv -c data -n "transactions$i.csv"
    Start-Sleep 60
}