$location = 'westus'
$loc = 'wus'
$rg = 'helloprivate-rg'
$plan = "helloprivate-$loc-plan"
$tags = 'project=hello-private-networking'
$app = "helloprivate-$loc"
$vnet = "helloprivate-$loc-vnet"
$storage = "helloprivate$loc"

# Create resource group
& az group create -n $rg --location $location --tags $tags

# Create App Service Plan
& az appservice plan create -n $plan -g $rg --location $location --sku S1 --tags $tags

# Create App and deploy from Github
& az webapp create -n $app --plan $plan -g $rg --tags $tags `
    --deployment-source-url 'https://github.com/DanielLarsenNZ/HelloAspDotNetCore'

# Create a VNet
& az network vnet create -n $vnet -g $rg --location $location --subnet-name default --tags $tags

# Create a Storage Account
& az storage account create -n $storage -g $rg --location $location --sku  'Standard_LRS' --https-only --kind StorageV2 --tags $tags 



# Open the app in the browser
start "https://$app.azurewebsites.net"

# Tear down
# & az group delete -n $rg --yes