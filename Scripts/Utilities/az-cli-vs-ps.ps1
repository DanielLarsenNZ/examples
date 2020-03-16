# Timing differences in execution time between az CLI and AZ PowerShell

$PSVersionTable

Write-Host ''

Measure-Command {
    Write-Host 'az group create'
    az group create -n 'test1-rg' --location 'australiaeast'
}

Measure-Command {
    Write-Host 'New-AzResourceGroup'
    New-AzResourceGroup -Name 'test2-rg' -Location 'Australia East' -Verbose
}

Measure-Command {
    Write-Host 'az group show'
    az group show -n 'test1-rg'
}

Measure-Command {
    Write-Host 'Get-AzResourceGroup'
    Get-AzResourceGroup -Name 'test2-rg' -Verbose
}

Measure-Command {
    Write-Host 'az group delete'
    az group delete -n 'test1-rg' --yes
}

Measure-Command {
    Write-Host 'Remove-AzResourceGroup'
    Remove-AzResourceGroup -Name 'test2-rg' -Force -Verbose
}


<#

Name                           Value
----                           -----
PSVersion                      7.0.0
PSEdition                      Core
GitCommitId                    7.0.0
OS                             Microsoft Windows 10.0.18363
Platform                       Win32NT
PSCompatibleVersions           {1.0, 2.0, 3.0, 4.0…}
PSRemotingProtocolVersion      2.3
SerializationVersion           1.1.0.1
WSManStackVersion              3.0

New-AzResourceGroup
VERBOSE: Performing the operation "Replacing resource group ..." on target "test2-rg".
VERBOSE: 1:51:23 pm - Created resource group 'test2-rg' in location 'australiaeast'
Days              : 0
Hours             : 0
Minutes           : 0
Seconds           : 7
Milliseconds      : 187
Ticks             : 71871073
TotalDays         : 8.31841122685185E-05
TotalHours        : 0.00199641869444444
TotalMinutes      : 0.119785121666667
TotalSeconds      : 7.1871073
TotalMilliseconds : 7187.1073

az group show
Days              : 0
Hours             : 0
Minutes           : 0
Seconds           : 4
Milliseconds      : 76
Ticks             : 40769450
TotalDays         : 4.71868634259259E-05
TotalHours        : 0.00113248472222222
TotalMinutes      : 0.0679490833333333
TotalSeconds      : 4.076945
TotalMilliseconds : 4076.945

Get-AzResourceGroup
Days              : 0
Hours             : 0
Minutes           : 0
Seconds           : 0
Milliseconds      : 685
Ticks             : 6858524
TotalDays         : 7.93810648148148E-06
TotalHours        : 0.000190514555555556
TotalMinutes      : 0.0114308733333333
TotalSeconds      : 0.6858524
TotalMilliseconds : 685.8524

az group delete
Days              : 0
Hours             : 0
Minutes           : 0
Seconds           : 50
Milliseconds      : 397
Ticks             : 503972643
TotalDays         : 0.000583301670138889
TotalHours        : 0.0139992400833333
TotalMinutes      : 0.839954405
TotalSeconds      : 50.3972643
TotalMilliseconds : 50397.2643

Remove-AzResourceGroup
VERBOSE: Performing the operation "Removing resource group ..." on target "test2-rg".
Days              : 0
Hours             : 0
Minutes           : 0
Seconds           : 46
Milliseconds      : 674
Ticks             : 466741518
TotalDays         : 0.000540210090277778
TotalHours        : 0.0129650421666667
TotalMinutes      : 0.77790253
TotalSeconds      : 46.6741518
TotalMilliseconds : 46674.1518

#>