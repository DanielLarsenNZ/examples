<#
.SYNOPSIS
Invokes 'npm install --production' on an Azure App Service via the Kudu API.

.PARAMETER Username
The App Service username. Download the Publish Profile from the Overview
blade (in the Azure Portal) of the App Service. Copy the 'userName' attribute value
from the 'MSDeploy' 'publishProfile' element.

.PARAMETER Password
The App Service password. Download the Publish Profile from the Overview
blade (in the Azure Portal) of the App Service. Copy the 'userPWD' attribute value
from the 'MSDeploy' 'publishProfile' element.

.PARAMETER AppServiceName
The name of the App Service, e.g. 'my-appservice'.

.DESCRIPTION
Based on the sample in https://github.com/projectkudu/kudu/wiki/REST-API
#>
param (
    [Parameter(Mandatory = $true)] [string] $Username,
    [Parameter(Mandatory = $true)] [string] $Password,
    [Parameter(Mandatory = $true)] [string] $AppServiceName
)

$auth = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(("{0}:{1}" -f $username,$password)))
$body = '{ "command": "npm install --production", "dir": "site/wwwroot" }'
$apiUrl = "https://$AppServiceName.scm.azurewebsites.net/api/command"

Write-Output "Invoking 'npm install --production' on $AppServiceName ..."

$response = Invoke-RestMethod -Uri $apiUrl -Headers @{Authorization=("Basic {0}" -f $auth)} -Method Post `
            -ContentType "application/json" -Body $body -TimeoutSec 600

Write-Output $response
Write-Output $response.Output
