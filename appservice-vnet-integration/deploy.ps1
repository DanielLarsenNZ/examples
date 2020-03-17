$rg = 'hellovnet-rg'
$location = 'australiaeast'
$tags = @{ 
    expires = [DateTime]::UtcNow.AddDays(7); 
    owner = "dalars"; 
    project = "appservice-vnet-integration" 
}

$ErrorActionPreference = 'Stop'

New-AzResourceGroup -Name $rg -Location $location -Tag $tags -Force -Verbose
New-AzResourceGroupDeployment -ResourceGroupName $rg -Mode 'Incremental' -TemplateFile (Join-Path $PSScriptRoot './armdeploy.json') -Verbose

start 'https://hellovnet.azurewebsites.net/'


# TEARDOWN
#Remove-AzResourceGroup -Name $rg -Force