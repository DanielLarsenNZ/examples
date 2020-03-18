$rg = 'hellomessaging-rg'
$location = 'australiaeast'
$eventhubNamespace = 'hellomessaging-hub'

$ErrorActionPreference = 'Stop'

Get-AzEventHubNamespace -ResourceGroupName $rg -NamespaceName $eventhubNamespace 

# Scale up
Set-AzEventHubNamespace -ResourceGroupName $rg -Location $location -NamespaceName $eventhubNamespace -SkuCapacity 5 -SkuName 'Standard' -Verbose

# Run the Test Script
Write-Host "Running Test Script"
# Your code here
Start-Sleep 10

# Scale down
Set-AzEventHubNamespace -ResourceGroupName $rg -Location $location -NamespaceName $eventhubNamespace -SkuCapacity 1 -SkuName 'Standard' -Verbose
