# Deploy a development instance of APIM
# Development SKU includes most features with no SLA and 500 RPS max throughput: 
#  https://azure.microsoft.com/en-us/pricing/details/api-management/

$ErrorActionPreference = "Stop"

# There is no AZ CLI command for APIM 🙄 Vote it up! https://feedback.azure.com/forums/248703-api-management/suggestions/34482196-on-board-commands-to-azure-cli-2-0

$rg = 'hello-apim-rg'
$location = 'westus'

# Run Connect-AzAccount to login

New-AzResourceGroup -Name $rg -Location $location -Force
New-AzApiManagement -ResourceGroupName $rg -Location $location -Name 'hello-apim' -Organization 'Hello' `
    -AdminEmail 'hello@localtest.me' -Sku 'Developer' -Verbose
