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
$insights = 'appserviceplanscale-insights'
$customDomain = ''  # FQDN, e.g. 'www.foobar.com'
$loaderioKey = ''   # loader.io load testing validation key

# Create resource group
az group create -n $rg --location $location --tags $tags

# Create App Service Plans
az appservice plan create -n $plan1 -g $rg --location $location --sku B1 --number-of-workers 2 --tags $tags
az appservice plan create -n $plan2 -g $rg --location $location --sku B1 --number-of-workers 2 --tags $tags

# Create Apps, deploy from Github
az webapp create -n $app1 --plan $plan1 -g $rg --tags $tags `
    --deployment-source-url 'https://github.com/DanielLarsenNZ/HelloAspDotNetCore'
az webapp create -n $app2 --plan $plan2 -g $rg --tags $tags `
    --deployment-source-url 'https://github.com/DanielLarsenNZ/HelloAspDotNetCore'

# Turn off ARR
az webapp update -n $app1 -g $rg --client-affinity-enabled false
az webapp update -n $app2 -g $rg --client-affinity-enabled false

# Create an Application Insights instance and set the Instrumentation Key on both plans
az extension add -n application-insights

#  https://docs.microsoft.com/en-us/cli/azure/ext/application-insights/monitor/app-insights/component?view=azure-cli-latest
$instrumentationKey = ( az monitor app-insights component create --app $insights --location $location -g $rg --tags $tags | ConvertFrom-Json ).instrumentationKey
#  https://docs.microsoft.com/en-us/cli/azure/webapp/config/appsettings?view=azure-cli-latest#az-webapp-config-appsettings-set
az webapp config appsettings set -n $app1 -g $rg --settings "APPINSIGHTS_INSTRUMENTATIONKEY=$instrumentationKey"
az webapp config appsettings set -n $app2 -g $rg --settings "APPINSIGHTS_INSTRUMENTATIONKEY=$instrumentationKey"

if ($loaderioKey -ne '') {
    # Set Loader.io validation key as App Setting
    az webapp config appsettings set -n $app1 -g $rg --settings "loader.io=$loaderioKey"
    az webapp config appsettings set -n $app2 -g $rg --settings "loader.io=$loaderioKey"
}

# Create Front Door
az extension add -n front-door

#  https://docs.microsoft.com/en-us/cli/azure/ext/front-door/network/front-door?view=azure-cli-latest#ext-front-door-az-network-front-door-create
az network front-door create -n $frontDoor -g $rg --tags $tags `
    --backend-address "$app1.azurewebsites.net" `
    --accepted-protocols Http Https `
    --protocol Http `

# Add second Backend
#  https://docs.microsoft.com/en-us/cli/azure/ext/front-door/network/front-door/backend-pool?view=azure-cli-latest#ext-front-door-az-network-front-door-backend-pool-create
az network front-door backend-pool backend add -g $rg `
    --front-door-name $frontDoor `
    --address "$app2.azurewebsites.net" `
    --pool-name 'DefaultBackendPool'

if ($customDomain -ne '') {
    # Add a Custom Domain front end
    #  https://docs.microsoft.com/en-us/azure/frontdoor/front-door-custom-domain
    #  https://docs.microsoft.com/en-us/cli/azure/ext/front-door/network/front-door/frontend-endpoint?view=azure-cli-latest#ext-front-door-az-network-front-door-frontend-endpoint-create
    az network front-door frontend-endpoint create --front-door-name $frontDoor -g $rg --host-name $customDomain --name 'CustomDomainFrontendEndpoint'

    # Update the routing rule to add the custom domain front-end
    #  https://docs.microsoft.com/en-us/cli/azure/ext/front-door/network/front-door/routing-rule?view=azure-cli-latest#ext-front-door-az-network-front-door-routing-rule-create
    az network front-door routing-rule create --front-door-name $frontDoor -g $rg -n 'DefaultRoutingRule' --frontend-endpoints 'DefaultFrontendEndpoint' 'CustomDomainFrontendEndpoint' --route-type Forward --accepted-protocols Http Https --backend-pool 'DefaultBackendPool' --caching Disabled --disabled false --forwarding-protocol HttpOnly

    # Warning: This command did not work when I tried it (used Portal instead). Was in preview as at 19/7/2019
    # https://docs.microsoft.com/en-us/azure/frontdoor/front-door-custom-domain-https#ssl-certificates
    # https://docs.microsoft.com/en-us/cli/azure/ext/front-door/network/front-door/frontend-endpoint?view=azure-cli-latest#ext-front-door-az-network-front-door-frontend-endpoint-enable-https
    az network front-door frontend-endpoint enable-https --front-door-name $frontDoor -g $rg -n 'DefaultRoutingRule' --certificate-source FrontDoor

    start "https://$customDomain"
}

#TODO: Set latency sensitivity to high value (1000ms) to round-robin across two ASPs in same region. 
#   https://docs.microsoft.com/en-us/azure/frontdoor/front-door-backend-pool
#   https://docs.microsoft.com/en-us/azure/frontdoor/front-door-routing-methods#weighted-traffic-routing-method

# Open the apps in the browser
start "https://$app1.azurewebsites.net"
start "https://$app2.azurewebsites.net"
start "https://$frontDoor.azurefd.net"

# Tear down
# az group delete -n $rg --yes