# Deploy App Service Plan and Function 
$location = 'australiaeast'
$loc = 'aue'
$rg = 'funruntime-rg'
$plan = "funruntime-$loc-plan"
$tags = 'project=functions-runtime'
$app = "funruntime-$loc"
$consumptionApp = "funruntimeconsumption-$loc"
$insights = 'funruntime-insights'
$storage = "funruntimefn$loc"
$repo = 'https://github.com/DanielLarsenNZ/HelloFunctionsDotNetCore.git'

# Create resource group
Write-Host 'az group create' -ForegroundColor Yellow
az group create -n $rg --location $location --tags $tags

$instrumentationKey = ( az monitor app-insights component create --app $insights --location $location -g $rg --tags $tags | ConvertFrom-Json ).instrumentationKey

# Create an azure storage account
Write-Host 'az storage account create' -ForegroundColor Yellow
az storage account create -n $storage -g $rg --tags $tags --location $location --sku 'Standard_LRS'

# Create a Dedicated plan
Write-Host 'az functionapp plan create' -ForegroundColor Yellow
az functionapp plan create -n $plan -g $rg --location $location --sku B1 --number-of-workers 1


# Create a Function App
Write-Host 'az functionapp create' -ForegroundColor Yellow
az functionapp create -n $app -g $rg --tags $tags --storage-account $storage --plan $plan --functions-version 2 `
    --app-insights $insights --app-insights-key $instrumentationKey
az functionapp deployment source config -n $app -g $rg --repo-url $repo

az functionapp config appsettings set -n $app -g $rg `
    --settings "APPINSIGHTS_INSTRUMENTATIONKEY=$instrumentationKey" "loader.io=$loaderioKey" `
    --slot-settings "Colour=Green"

# Create a Consumption Function App
Write-Host "az functionapp create -n $consumptionApp" -ForegroundColor Yellow
az functionapp create -n $consumptionApp -g $rg --tags $tags --storage-account $storage `
    --consumption-plan-location $location --functions-version 2 `
    --app-insights $insights --app-insights-key $instrumentationKey
az functionapp deployment source config -n $consumptionApp -g $rg --repo-url $repo


# Open the apps in the browser
start "https://$app.azurewebsites.net/api/GetHealth"
start "https://$consumptionApp.azurewebsites.net/api/GetHealth"


# Tear down
# az group delete -n $rg --yes