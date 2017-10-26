[CmdletBinding()]
param(
    [switch] $Teardown,
    [string] $ResourceGroup = 'example-sql-prod-aue-rg',
    [string] $SysopsGroupObjectId = 'ffc5bf9e-ce78-4fd1-9fad-5e9c658c9aa4',
    [string] $SqlDbasGroupObjectId = '8d652dfb-4f91-484a-b239-1c2610e4c05d'
)

if ($Teardown) {
    # Teardown the Resources
    Write-Verbose "Deleting Resource Group $rg ..."
    Remove-AzureRmResourceGroup -Name $rg -Force
    
    Write-Verbose "Deleting ARM Policy definition..."
    Remove-AzureRmPolicyDefinition -Name AllowedVNetPolicy -Force
}


# ################## #
# Resource Group     #
# ################## #

# Create a Resource Group
Write-Verbose "Creating Resource Group $rg ..."
New-AzureRMResourceGroup -Name $rg -Location australiaeast `
    -Tag @{ 'cost-centre' = 'IT'; 'owner-username' = 'dalars@microsoft.com'; 'project-code' = 'SQL-Cluster' } `
    -Force

# Assign sysops as owner
Write-Verbose "Assigning Sysops Owner role..."
New-AzureRmRoleAssignment -ObjectId $sysopsProdId -RoleDefinitionName owner -ResourceGroupName $rg

# Assign SQL DBAs as contributors
Write-Verbose "Assigning SQL DBAs Contributor role..."
New-AzureRmRoleAssignment -ObjectId $sqlDbasProdId -RoleDefinitionName contributor -ResourceGroupName $rg


Read-Host -Prompt 'Press enter to create an ARM Policy Definition'


# ######################## #
#  A R M  P o l i c i e s  #
# ######################## #

# Create a Definition

Write-Verbose "Creating ARM Policy Definition..."
$definition = New-AzureRmPolicyDefinition -Name AllowedVNetPolicy -DisplayName 'Allowed VNets' `
                                          -Description 'Only allow scaffold VNets.' `
                                          -Policy "$PSScriptRoot\DenyNonScaffoldVnet.json"

# Derive the scope = "/subscriptions/(subscriptionId)/resourceGroups/(resourceGroup)"
$subscription = (Get-AzureRmContext).Subscription

# (PSAzureSubscription objects can vary. Test for the correct property using a conditional expression)
$subId = if ($subscription.SubscriptionId) { $subscription.SubscriptionId } elseif ($subscription.Id) { $subscription.Id }
$scope = "/subscriptions/$subId/resourceGroups/$rg"

Read-Host -Prompt 'Press enter to assign the ARM Policy to the resource group'

# Assign the Definition to the Resource Group
Write-Verbose "Assigning ARM Policy to resource group $rg ..." -Verbose
New-AzureRmPolicyAssignment -Name OnlyConnectVMsToScaffoldVNets -DisplayName 'VMs can only connect to Scaffold VNets' `
                            -PolicyDefinition $definition -Scope $scope


