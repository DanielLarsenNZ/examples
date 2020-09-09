# Deploy multiple App Service Plans and load-balance across them using Azure Front Door

$location = 'eastus'
$loc = 'eus'
$rg = 'appserviceplanscale-rg'
$plan1 = "appserviceplanscale1-$loc-plan"
$plan2 = "appserviceplanscale2-$loc-plan"
$tags = 'project=appserviceplan-scale-afd'
$app1 = "appserviceplanscale1-$loc"
$app2 = "appserviceplanscale2-$loc"
$frontDoor = 'appserviceplanscale'
$insights = 'appserviceplanscale-insights'

# Scale settings
$sku = 'B1'
$numberWorkers = 2

# If you are Load testing using loader.io enter a validation key here
$loaderioKey = ''   # loader.io load testing validation key

# Create resource group
Write-Host "az group create -n $rg" -ForegroundColor Yellow
az group create -n $rg --location $location --tags $tags

# Create App Service Plans
Write-Host "az appservice plan create -n $plan1" -ForegroundColor Yellow
az appservice plan create -n $plan1 -g $rg --location $location --sku $sku --number-of-workers $numberWorkers --tags $tags
Write-Host "az appservice plan create -n $plan2" -ForegroundColor Yellow
az appservice plan create -n $plan2 -g $rg --location $location --sku $sku --number-of-workers @numberWorkers --tags $tags

# Create Apps, deploy from Github
Write-Host "az webapp create -n $app1" -ForegroundColor Yellow
az webapp create -n $app1 --plan $plan1 -g $rg --tags $tags 
Write-Host "az webapp deployment source config -n $app1" -ForegroundColor Yellow
az webapp deployment source config -n $app1 -g $rg --repo-url 'https://github.com/DanielLarsenNZ/HelloAspDotNetCore'

Write-Host "az webapp create -n $app2" -ForegroundColor Yellow
az webapp create -n $app2 --plan $plan2 -g $rg --tags $tags 
Write-Host "az webapp deployment source config -n $app2" -ForegroundColor Yellow
az webapp deployment source config -n $app2 -g $rg --repo-url 'https://github.com/DanielLarsenNZ/HelloAspDotNetCore'

# Disable ARR Affinity
Write-Host "az webapp update -n $app1 --client-affinity-enabled false" -ForegroundColor Yellow
az webapp update -n $app1 -g $rg --client-affinity-enabled false
Write-Host "az webapp update -n $app2 --client-affinity-enabled false" -ForegroundColor Yellow
az webapp update -n $app2 -g $rg --client-affinity-enabled false

# Create an Application Insights instance and set the Instrumentation Key on both plans
#az extension add -n application-insights

#  https://docs.microsoft.com/en-us/cli/azure/ext/application-insights/monitor/app-insights/component?view=azure-cli-latest
Write-Host "az monitor app-insights component create --app $insights" -ForegroundColor Yellow
$instrumentationKey = ( az monitor app-insights component create --app $insights --location $location -g $rg --tags $tags | ConvertFrom-Json ).instrumentationKey
#  https://docs.microsoft.com/en-us/cli/azure/webapp/config/appsettings?view=azure-cli-latest#az-webapp-config-appsettings-set
Write-Host "az webapp config appsettings set -n $app1" -ForegroundColor Yellow
az webapp config appsettings set -n $app1 -g $rg --settings "APPINSIGHTS_INSTRUMENTATIONKEY=$instrumentationKey" "loader.io=$loaderioKey"
Write-Host "az webapp config appsettings set -n $app2" -ForegroundColor Yellow
az webapp config appsettings set -n $app2 -g $rg --settings "APPINSIGHTS_INSTRUMENTATIONKEY=$instrumentationKey" "loader.io=$loaderioKey"

# Create Front Door
#az extension add -n front-door

#  https://docs.microsoft.com/en-us/cli/azure/ext/front-door/network/front-door?view=azure-cli-latest#ext-front-door-az-network-front-door-create
Write-Host "az network front-door create -n $frontDoor" -ForegroundColor Yellow
az network front-door create -n $frontDoor -g $rg --tags $tags `
    --backend-address "$app1.azurewebsites.net" `
    --accepted-protocols Http Https `
    --protocol Http

# az network front-door backend-pool backend add -g $rg -f $frontDoor `
#     --address "$app1.azurewebsites.net" `
#     --pool-name 'DefaultBackendPool'


# Add second Backend
#  https://docs.microsoft.com/en-us/cli/azure/ext/front-door/network/front-door/backend-pool?view=azure-cli-latest#ext-front-door-az-network-front-door-backend-pool-create
Write-Host "az network front-door backend-pool backend add --address $app2.azurewebsites.net" -ForegroundColor Yellow
az network front-door backend-pool backend add -g $rg `
    --front-door-name $frontDoor `
    --address "$app2.azurewebsites.net" `
    --pool-name 'DefaultBackendPool'


#TODO: Set latency sensitivity to high value (1000ms) to round-robin across two ASPs in same region. 
#   https://docs.microsoft.com/en-us/azure/frontdoor/front-door-backend-pool
#   https://docs.microsoft.com/en-us/azure/frontdoor/front-door-routing-methods#weighted-traffic-routing-method

# Open the apps in the browser
start "https://$app1.azurewebsites.net"
start "https://$app2.azurewebsites.net"
start "https://$frontDoor.azurefd.net"

# Tear down
# az group delete -n $rg --yes