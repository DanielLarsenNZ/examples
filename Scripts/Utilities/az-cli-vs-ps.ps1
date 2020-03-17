# Timing differences in execution time between az CLI and AZ PowerShell

[DateTime]::UtcNow.ToString('R')

& az --version

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

Tue, 17 Mar 2020 20:44:50 GMT
azure-cli                          2.2.0

command-modules-nspkg              2.0.3
core                               2.2.0
nspkg                              3.0.4
telemetry                          1.0.4

Extensions:
application-insights               0.1.1
front-door                         0.1.8
interactive                        0.4.3
storage-preview                    0.2.7

Python location 'C:\Program Files (x86)\Microsoft SDKs\Azure\CLI2\python.exe'
Extensions directory 'C:\Users\dalars\.azure\cliextensions'

Python (Windows) 3.6.6 (v3.6.6:4cf1f54eb7, Jun 27 2018, 02:47:15) [MSC v.1900 32 bit (Intel)]

Legal docs and information: aka.ms/AzureCliLegal



Your CLI is up-to-date.

Please let us know how we are doing: https://aka.ms/clihats

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

az group create

Ticks             : 82722758
Days              : 0
Hours             : 0
Milliseconds      : 272
Minutes           : 0
Seconds           : 8
TotalDays         : 9.57439328703704E-05
TotalHours        : 0.00229785438888889
TotalMilliseconds : 8272.2758
TotalMinutes      : 0.137871263333333
TotalSeconds      : 8.2722758

New-AzResourceGroup
VERBOSE: Performing the operation "Replacing resource group ..." on target "test2-rg".
VERBOSE: 9:45:05 am - Created resource group 'test2-rg' in location 'australiaeast'

Ticks             : 15474831
Days              : 0
Hours             : 0
Milliseconds      : 547
Minutes           : 0
Seconds           : 1
TotalDays         : 1.79106840277778E-05
TotalHours        : 0.000429856416666667
TotalMilliseconds : 1547.4831
TotalMinutes      : 0.025791385
TotalSeconds      : 1.5474831

az group show

Ticks             : 29305181
Days              : 0
Hours             : 0
Milliseconds      : 930
Minutes           : 0
Seconds           : 2
TotalDays         : 3.39180335648148E-05
TotalHours        : 0.000814032805555556
TotalMilliseconds : 2930.5181
TotalMinutes      : 0.0488419683333333
TotalSeconds      : 2.9305181

Get-AzResourceGroup

Ticks             : 7408608
Days              : 0
Hours             : 0
Milliseconds      : 740
Minutes           : 0
Seconds           : 0
TotalDays         : 8.57477777777778E-06
TotalHours        : 0.000205794666666667
TotalMilliseconds : 740.8608
TotalMinutes      : 0.01234768
TotalSeconds      : 0.7408608

az group delete

Ticks             : 495528816
Days              : 0
Hours             : 0
Milliseconds      : 552
Minutes           : 0
Seconds           : 49
TotalDays         : 0.000573528722222222
TotalHours        : 0.0137646893333333
TotalMilliseconds : 49552.8816
TotalMinutes      : 0.82588136
TotalSeconds      : 49.5528816

Remove-AzResourceGroup
VERBOSE: Performing the operation "Removing resource group ..." on target "test2-rg".

Ticks             : 475125652
Days              : 0
Hours             : 0
Milliseconds      : 512
Minutes           : 0
Seconds           : 47
TotalDays         : 0.000549913949074074
TotalHours        : 0.0131979347777778
TotalMilliseconds : 47512.5652
TotalMinutes      : 0.791876086666667
TotalSeconds      : 47.5125652

#>