# WARNING! THIS SCRIPT WILL DELETE ALL RESOURCE GROUPS AND ALL RESOURCES IN A SUBSCRIPTION
[CmdletBinding()]
param (
    [Parameter(Mandatory = $true)] [string] $SubscriptionId,
    [switch] $ImSureIWantToDeleteEverythingInThisSubscription
)

$ErrorActionPreference = 'Stop'
$groups = Get-AzResourceGroup
foreach ($group in $groups) {
    $groupSubscriptionId = $group.ResourceId.Split('/')[2]

    if ($groupSubscriptionId -ne $SubscriptionId) {
        Write-Host "$SubscriptionId/$($group.ResourceGroupName) is being skipped because wrong Subscription ($groupSubscriptionId)"
        continue
    }

    if ( 
        # Put exclusion rules here
        $group.ResourceGroupName -like '*shared*' `
        -or $group.ResourceGroupName -like '*default*' `
        -or $group.ResourceGroupName -like '*azurelunch*' `
        -or $group.ResourceGroupName -like '*cloud-shell-storage*' `
        -or $group.ResourceGroupName -like '*dashboards*' `
        -or $group.ResourceGroupName -like '*fscale*' `
        -or $group.ResourceGroupName -like '*hazr*' `
    ) 
    { 
        Write-Host "$SubscriptionId/$($group.ResourceGroupName) is being skipped because of an exclusion rule"
        continue
    }

    if ($ImSureIWantToDeleteEverythingInThisSubscription) {
        Write-Host "$SubscriptionId/$($group.ResourceGroupName) is being deleted" -ForegroundColor Red
        $nul = Remove-AzResourceGroup -Name $group.ResourceGroupName -Force -AsJob
    } else {
        Write-Host "$SubscriptionId/$($group.ResourceGroupName) will be deleted if -ImSureIWantToDeleteEverythingInThisSubscription" -ForegroundColor DarkYellow
    }
}
