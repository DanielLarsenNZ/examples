$location = 'Australia East'
$loc = 'aue'
$rg = 'hellovnetrouteall-rg'
$plan = "hellovnetrouteall-$loc-plan"
$tags = 'project=private-paas'
$app = "hellovnetrouteall-$loc"
$vnet = "hellovnetrouteall-$loc-vnet"
$planSubnet = 'asp'
$nsg = 'deny-all-internet-out'

# RESOURCE GROUP
az group create -n $rg --location $location --tags $tags


# VNET
az network vnet create -n $vnet -g $rg --location $location --subnet-name $planSubnet --tags $tags

# NSG: deny all internet outbound
az network nsg create -g $rg -n $nsg --tags $tags
az network nsg rule create -g $rg --nsg-name $nsg -n 'DenyInternetOutbound' --priority 100 `
    --direction 'Outbound' --source-address-prefixes '*' --source-port-ranges '*' `
    --destination-address-prefixes 'Internet' --destination-port-ranges '*' --access Deny `
    --protocol '*' --description "Deny any outbound to internet"

# Assign to subnet
az network vnet subnet update -g $rg -n $planSubnet --vnet-name $vnet --network-security-group $nsg


# APP SERVICE PLAN
az appservice plan create -n $plan -g $rg --location $location --sku S1 --tags $tags

# Create App and deploy test app from Github
az webapp create -n $app --plan $plan -g $rg --tags $tags `
    --deployment-source-url 'https://github.com/DanielLarsenNZ/HelloAspDotNetCore'

# Deploy ARM to set vnetRouteAll (instead of using the WEBSITE_VNET_ROUTE_ALL=1 App Setting)
az deployment group create -n "deployment-$(New-Guid)" -g $rg --template-file ./arm-vnet-route-all-deploy-all-enabled.json --mode Incremental `
    --parameters appName=$app appServicePlanName=$plan location=$location

# VNET Integration
az webapp vnet-integration add --name $app -g $rg --subnet $planSubnet --vnet $vnet

# Configure app to try to connect to Dropbox.com
az webapp config appsettings set -n $app -g $rg --settings "GetUrls=https://www.dropbox.com/"


# Open the app in the browser
start "https://$app.azurewebsites.net"

# Tear down
# & az group delete -n $rg --yes