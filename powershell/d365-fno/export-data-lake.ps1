# Example for stkop: https://docs.microsoft.com/en-us/dynamics365/fin-ops-core/dev-itpro/data-entities/configure-export-data-lake
# See also: https://github.com/DanielLarsenNZ/examples/blob/main/docs/powershell-cheatsheet.md

# These are parameters that are passed into the script like this:
#   .\export-data-lake.ps1 -ApplicationName 'MySuperApp'
param (
    [string] $ApplicationName,
    # Application Id must be alphanumeric, no spaces or non-alphanumeric chars
    [ValidatePattern('^[a-zA-Z0-9]+$')] [string] $ApplicationId,
    # Add the Azure regions where F&O is deployed to the ValidateSet below
    [ValidateSet('australiaeast', 'uswest')] [string] $Location
)

# Constants

# Resource Group name
$rg = 'exportdatalake-rg'
$storage = $ApplicationId.ToLower()

# Login to Azure (you only need to do this once per session)
Connect-AzureAD

# Create Service Principle for Microsoft Dynamics ERP Microservices
try {
    New-AzureADServicePrincipal –AppId '0cdb527f-a8d1-4bf8-9436-b352c68682b2'
}
catch {
    Write-Host "Looks like you aren't an Admin. Contact your AAD Global Admin"
    break
}

# Create an application in Azure Active Directory
New-AzADApplication -DisplayName $ApplicationName -HomePage "http://www.microsoft.com" -IdentifierUris "http://$ApplicationId"

# Create a Resource Group
New-AzResourceGroup -Name $rg -Location $Location

# Create a Data Lake Storage (Gen2) account
New-AzStorageAccount -ResourceGroupName $rg -AccountName $storage -Location $Location `
    -SkuName "Standard_GRS" -Kind StorageV2  -EnableHierarchicalNamespace $true

# etc...