<#
    .SYNOPSIS
    Invoke a Mozilla HTTP Observatory scan and wait for the scan to complete

    .PARAMETER Hostname
    The Hostname of the website to scan, e.g. 'www.mozilla.org'

    .PARAMETER Rescan
    Set this switch to force a Rescan

    .DESCRIPTION
    See https://github.com/mozilla/http-observatory/blob/master/httpobs/docs/api.md for more information. 
    By default, the HTTP Observatory will return a cached site result if the site has been scanned anytime 
    in the previous 24 hours. Regardless of the value of rescan, a site can not be scanned at a frequency 
    greater than every three minutes.
#>
[CmdletBinding()]
param (
    [string] $Hostname,
    [switch] $Rescan
)

$ErrorActionPreference = 'Stop'

$Endpoint = 'https://http-observatory.security.mozilla.org/api/v1'

if ($Rescan){
    $rescanParam = 'true'
} else {
    $rescanParam = 'false'
}

$params = @{ hidden = 'true'; rescan = $rescanParam }
$url = "$Endpoint/analyze?host=$Hostname"

Write-Output "POST $url"
$scan = Invoke-RestMethod -Method Post -Uri $url -Body $params

Write-Output $scan

if ($scan.error -ne $null) {
    throw $scan.error
}

while ($scan.state -ne 'FINISHED') {
    Write-Output "Waiting 5 seconds..."
    Start-Sleep -Seconds 5
    Write-Output "GET $url"
    $scan = Invoke-RestMethod -Method Get -Uri $url

    Write-Output $scan
}

# Anything lower than a 'B' is a fail
if ($scan.grade -notin ('B', 'B+', 'A-', 'A', 'A+')) {
    throw "HTTP Observatory Test failed. Grade = $($scan.grade)"
}
