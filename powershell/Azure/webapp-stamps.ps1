# az login
# az account set -s (subscriptionid)

$webapps = ( az webapp list --query "[].{id:id}" | ConvertFrom-Json )

foreach ($id in $webapps) {
    $result = az webapp show --ids $id.id --query "{name:name, ftpPublishingUrl:ftpPublishingUrl}" --output tsv
    $result
    $result | Out-File ./webapp-stamps.txt -Append
}
