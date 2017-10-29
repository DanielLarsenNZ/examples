# # Deploy Scaffold

[CmdletBinding()]
param()

# ## Resource Groups

# Create SQL Resource Group
Write-Verbose "Creating Resource Group example-sql-prod-aue-rg ..."

New-AzureRMResourceGroup -Name example-sql-prod-aue-rg -Location australiaeast `
    -Tag @{ 'cost-centre' = 'IT'; 'owner-username' = 'dalars@microsoft.com'; 'project-code' = 'SQL-Cluster' } `
    -Force

# Assign sysops as owner
Write-Verbose "Assigning Sysops Owner role..."

$sysopsGroupObjectId = 'ae7a3fdd-e1a7-4cf5-8791-00d310bbeef8'
New-AzureRmRoleAssignment -ObjectId $sysopsGroupObjectId -RoleDefinitionName owner `
                          -ResourceGroupName example-sql-prod-aue-rg -ErrorAction SilentlyContinue

# Assign SQL DBAs as contributors
Write-Verbose "Assigning SQL DBAs Contributor role..."

$sqlDbasGroupObjectId = '7899d7b0-e5ef-487b-b002-9a86ba977f32'
New-AzureRmRoleAssignment -ObjectId $sqlDbasGroupObjectId -RoleDefinitionName contributor `
                          -ResourceGroupName example-sql-prod-aue-rg -ErrorAction SilentlyContinue


# Create Network Resource Group
Write-Verbose "Creating Resource Group networking-prod-rg ..."
New-AzureRMResourceGroup -Name networking-prod-rg -Location australiaeast `
    -Tag @{ 'cost-centre' = 'IT'; 'owner-username' = 'dalars@microsoft.com'; 'project-code' = 'SQL-Cluster' } `
    -Force

# Assign sysops as owner
Write-Verbose "Assigning Sysops Owner role..."
New-AzureRmRoleAssignment -ObjectId $sysopsGroupObjectId -RoleDefinitionName owner `
                          -ResourceGroupName networking-prod-rg -ErrorAction SilentlyContinue


# ## ARM Policies

# Create a Definition

Write-Verbose "Creating ARM Policy Definition..."
$definition = New-AzureRmPolicyDefinition -Name AllowedVNetPolicy -DisplayName 'Allowed VNets' `
                                          -Description 'Only allow scaffold VNets.' `
                                          -Policy .\DenyNonScaffoldVnet.json

# Derive the scope = "/subscriptions/(subscriptionId)/resourceGroups/(resourceGroup)"
$subscription = (Get-AzureRmContext).Subscription

# (PSAzureSubscription objects can vary. Test for the correct property using a conditional expression)
$subId = if ($subscription.SubscriptionId) { $subscription.SubscriptionId } elseif ($subscription.Id) { $subscription.Id }
$scope = "/subscriptions/$subId/resourceGroups/example-sql-prod-aue-rg"

# Assign the Definition to the Resource Group
Write-Verbose "Assigning ARM Policy..."

New-AzureRmPolicyAssignment -Name OnlyConnectVMsToScaffoldVNets -DisplayName 'VMs can only connect to Scaffold VNets' `
                            -PolicyDefinition $definition -Scope $scope


# ## Networking

Write-Verbose "Deploying Networking to networking-prod-rg..."

.\Example.ARM\Deploy-AzureResourceGroup.ps1 `
    -ResourceGroupLocation australiaeast -ResourceGroupName networking-prod-rg `
    -TemplateFile .\simple-networking\simple-networking.json `
    -TemplateParametersFile .\simple-networking\simple-networking-parameters.json `
   









<#
MIT License

Copyright (c) 2017 Daniel Larsen

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
#>