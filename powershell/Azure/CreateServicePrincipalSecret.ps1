<#
.SUMMARY
    Creates a Client Secret for an Azure AD Service Principal (SPN)
.PARAMETER Search
    Search string to find the SPN. Provide the full name of the SPN.  
#>
param(
    [string] $Search 
)

$ErrorActionPreference = 'Stop'

$spn = @(Get-AzureRMADServicePrincipal -Search $Search)

if ($spn.Length -eq 0) {
    throw "No Service Principal matching ""$Search"" found"
}

if ($spn.Length -gt 1) {
    throw "Found more than one Service Principal matching ""$Search"""
}

$secret = ([Guid]::NewGuid()).ToString('N')

New-AzureRmADSpCredential -ObjectId $spn.Id -Password $secret 

Write-Output "Client Id = $($spn.ApplicationId)"
Write-Output "Client Secret = $secret"
