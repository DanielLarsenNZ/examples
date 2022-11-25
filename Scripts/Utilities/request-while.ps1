$url = 'https://hellowaf-arcwdegwhhgpadbs.z01.azurefd.net/'
$sleepSeconds = 0

While ($true) {
    $response = $null
    Write-Host $url
    $response=Invoke-WebRequest -Uri $url -ErrorAction SilentlyContinue -Verbose -UseBasicParsing
    Write-Host "$($response.StatusCode) $($response.StatusDescription)"
    Start-Sleep -Seconds $sleepSeconds
}