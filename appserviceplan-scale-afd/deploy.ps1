# Deploy multiple App Service Plans and loadbalance across them using Azure Front Door

$location = 'eastus'
$loc = 'eus'
$rg = 'appserviceplanscale-rg'
$plan1 = "appserviceplanscale1-$loc-plan"
$plan2 = "appserviceplanscale2-$loc-plan"
$tags = 'project=appserviceplan-scale-afd'
$app1 = "appserviceplanscale1-$loc"
$app2 = "appserviceplanscale2-$loc"
$frontDoor = 'appserviceplanscale'

# Create resource group
az group create -n $rg --location $location --tags $tags

# Create App Service Plans
az appservice plan create -n $plan1 -g $rg --location $location --sku P2V2 --number-of-workers 2 --tags $tags
az appservice plan create -n $plan2 -g $rg --location $location --sku P2V2 --number-of-workers 2 --tags $tags

# Create Apps, deploy from Github
az webapp create -n $app1 --plan $plan1 -g $rg --tags $tags `
    --deployment-source-url 'https://github.com/DanielLarsenNZ/HelloAspDotNetCore'
az webapp create -n $app2 --plan $plan2 -g $rg --tags $tags `
    --deployment-source-url 'https://github.com/DanielLarsenNZ/HelloAspDotNetCore'

# Turn off ARR
az webapp update -n $app1 -g $rg --client-affinity-enabled false
az webapp update -n $app2 -g $rg --client-affinity-enabled false

# Create Front Door
az extension add --name front-door
az network front-door create -n $frontDoor -g $rg --tags $tags `
    --backend-address "$app1.azurewebsites.net" `
    --accepted-protocols Http Https

# Add second Backend
az network front-door backend-pool backend add -g $rg `
    --front-door-name $frontDoor `
    --address "$app2.azurewebsites.net" `
    --pool-name 'DefaultBackendPool'

#TODO: https://docs.microsoft.com/en-us/azure/frontdoor/front-door-custom-domain

# Open the apps in the browser
start "https://$app1.azurewebsites.net"
start "https://$app2.azurewebsites.net"
start "https://$frontDoor.azurefd.net"

# Tear down
# az group delete -n $rg --yes