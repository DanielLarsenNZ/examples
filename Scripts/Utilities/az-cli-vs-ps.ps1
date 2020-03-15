# Timing differences in execution time between az CLI and AZ PowerShell

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
