# Deploy App Service Plan and slots for test in prod

$location = 'australiaeast'
$loc = 'aue'
$rg = 'testinprod-rg'
$plan = "testinprodfn-$loc-plan"
$tags = 'project=functions-test-in-prod'
$app = "testinprodfn-$loc"
$insights = 'testinprod-insights'
$loaderioKey = 'loaderio-f59cd4c712e8ef80df0c056f2dea0a2d'   # loader.io load testing validation key
$storage = "testinprodfn$loc"
$repo = 'https://github.com/DanielLarsenNZ/HelloFunctionsDotNetCore.git'

# Create resource group
az group create -n $rg --location $location --tags $tags

# Create an Application Insights instance and set the Instrumentation Key
az extension add -n application-insights

#  https://docs.microsoft.com/en-us/cli/azure/ext/application-insights/monitor/app-insights/component?view=azure-cli-latest
$instrumentationKey = ( az monitor app-insights component create --app $insights --location $location -g $rg --tags $tags | ConvertFrom-Json ).instrumentationKey

# Create App Service Plan
#az appservice plan create -n $plan -g $rg --location $location --sku S1 --number-of-workers 1 --tags $tags

# Create an azure storage account
az storage account create -n $storage -g $rg --tags $tags --location $location --sku 'Standard_LRS'

# Create a Premium plan
az functionapp plan create -n $plan -g $rg --location $location --sku EP1

# Create a Function App
az functionapp create -n $app -g $rg --tags $tags --storage-account $storage --plan $plan --functions-version 3 `
    --deployment-source-url $repo `
    --app-insights $insights --app-insights-key $instrumentationKey

az functionapp config appsettings set -n $app -g $rg --settings "APPINSIGHTS_INSTRUMENTATIONKEY=$instrumentationKey" `
    "loader.io=$loaderioKey" --slot-settings "Colour=Green"

# Create a blue slot
az functionapp deployment slot create -n $app -g $rg --slot 'blue'
az functionapp config appsettings set -n $app -g $rg --slot 'blue' --settings "APPINSIGHTS_INSTRUMENTATIONKEY=$instrumentationKey" "loader.io=$loaderioKey" `
    --slot-settings "Colour=Blue"
az functionapp deployment source config -n $app -g $rg --slot 'blue' --repo-url $repo

# 80/20 traffic routing
az webapp traffic-routing set -n $app -g $rg --distribution blue=20

# Open the apps in the browser
start "https://$app.azurewebsites.net/api/GetHealth"
start "https://$app-blue.azurewebsites.net/api/GetHealth"


# Tear down
# az group delete -n $rg --yes