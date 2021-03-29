using namespace System.Collections.Generic

# Timing differences in execution time between az CLI and AZ PowerShell

[DateTime]::UtcNow.ToString('R')

& az --version

$PSVersionTable

Write-Host ''

$commands = @( 
    @{ Cli = { az --version }; PS = { $PSVersionTable } },
    @{ Cli = { az group create -n 'test1-rg' --location 'australiaeast' }; PS = { New-AzResourceGroup -Name 'test2-rg' -Location 'Australia East' -Force } },
    @{ Cli = { az group show -n 'test1-rg' }; PS = { Get-AzResourceGroup -Name 'test2-rg' } },
    @{ Cli = { az group delete -n 'test1-rg' --yes }; PS = { Remove-AzResourceGroup -Name 'test2-rg' -Force } }
)

$results = @()

foreach ($command in $commands)
{
    $cliSeconds = (Measure-Command {
        Invoke-Command -ScriptBlock $command.Cli
    }).TotalSeconds
    
    $psSeconds = (Measure-Command {
        Invoke-Command -ScriptBlock $command.PS
    }).TotalSeconds
    
    $result = New-Object -TypeName PSObject
    $result | Add-Member -Name CliCommand -MemberType NoteProperty -Value $command.Cli 
    $result | Add-Member -Name CliSeconds -MemberType NoteProperty -Value $cliSeconds
    $result | Add-Member -Name PSCommand -MemberType NoteProperty -Value $command.PS
    $result | Add-Member -Name PSSeconds -MemberType NoteProperty -Value $psSeconds
    
    $result | Format-Table
    $results += $result
}

$results | Format-Table
