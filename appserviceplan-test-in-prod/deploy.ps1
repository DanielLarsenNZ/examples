# Deploy App Service Plan and slots for test in prod

$location = 'australiaeast'
$loc = 'aue'
$rg = 'testinprod-rg'
$plan = "testinprod-$loc-plan"
$tags = 'project=appserviceplan-test-in-prod'
$app = "testinprod-$loc"
$insights = 'testinprod-insights'
$loaderioKey = 'loaderio-f59cd4c712e8ef80df0c056f2dea0a2d'   # loader.io load testing validation key

# Create resource group
az group create -n $rg --location $location --tags $tags

# Create App Service Plan
az appservice plan create -n $plan -g $rg --location $location --sku S1 --number-of-workers 5 --tags $tags

# Create App, deploy from Github
az webapp create -n $app --plan $plan -g $rg --tags $tags `
    --deployment-source-url 'https://github.com/DanielLarsenNZ/HelloAspDotNetCore'

# Turn off ARR
az webapp update -n $app -g $rg --client-affinity-enabled false

# Create an Application Insights instance and set the Instrumentation Key on both plans
az extension add -n application-insights

#  https://docs.microsoft.com/en-us/cli/azure/ext/application-insights/monitor/app-insights/component?view=azure-cli-latest
$instrumentationKey = ( az monitor app-insights component create --app $insights --location $location -g $rg --tags $tags | ConvertFrom-Json ).instrumentationKey
#  https://docs.microsoft.com/en-us/cli/azure/webapp/config/appsettings?view=azure-cli-latest#az-webapp-config-appsettings-set
az webapp config appsettings set -n $app -g $rg --settings "APPINSIGHTS_INSTRUMENTATIONKEY=$instrumentationKey" "loader.io=$loaderioKey" `
    --slot-settings "Colour=Green"

# Create a blue slot
az webapp deployment slot create -n $app -g $rg --slot 'blue'
az webapp config appsettings set -n $app -g $rg --slot 'blue' --settings "APPINSIGHTS_INSTRUMENTATIONKEY=$instrumentationKey" "loader.io=$loaderioKey" `
    --slot-settings "Colour=Blue"
az webapp deployment source config -n $app -g $rg --slot 'blue' --repo-url 'https://github.com/DanielLarsenNZ/HelloAspDotNetCore'

# 80/20 traffic routing
az webapp traffic-routing set -n $app -g $rg --distribution blue=20

# Open the apps in the browser
start "https://$app.azurewebsites.net"
start "https://$app-blue.azurewebsites.net"


# Tear down
# az group delete -n $rg --yes