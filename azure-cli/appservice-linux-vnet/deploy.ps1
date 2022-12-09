# Deploy Linux App Service Plan, Storage Account and VNET Integration
$location = 'australiaeast'
$loc = 'aue'
$rg = 'linuxwebapp-rg'
$plan = "linuxwebapp-$loc-plan"
$tags = 'project=appservice-linux-vnet', 'repo=DanielLarsenNZ/examples'
$app = "linuxwebapp-$loc"
$insights = 'linuxwebapp-insights'
$storage = "linuxwebapp$loc"
$container = "content"
$loaderioKey = 'loaderio-'   # loader.io load testing validation key

# RESOURCE GROUP
az group create -n $rg --location $location --tags $tags

# APPLICATION INSIGHTS
#az extension add -n application-insights
$instrumentationKey = ( az monitor app-insights component create --app $insights --location $location -g $rg --tags $tags | ConvertFrom-Json ).instrumentationKey

# APP SERVICE PLAN
az appservice plan create -n $plan -g $rg --location $location --tags $tags `
    --sku S1 --number-of-workers 1 --is-linux

# WEB APP
# https://docs.microsoft.com/en-us/azure/app-service/containers/app-service-linux-cli
az webapp create -n $app --plan $plan -g $rg --tags $tags -i 'daniellarsennz/helloaspdotnetcore'
az webapp deployment container config -n $app -g $rg --enable-cd true
az webapp update -n $app -g $rg --client-affinity-enabled false

# STORAGE ACCOUNT
az storage account create -n $storage -g $rg --location $location --tags $tags `
    --sku  'Standard_LRS' --https-only --kind StorageV2
az storage container create -n $container --account-name $storage --public-access off
$storageConnection = ( az storage account show-connection-string -g $rg -n $storage | ConvertFrom-Json ).connectionString


# APP SETTINGS
az webapp config appsettings set -n $app -g $rg `
    --settings `
        "APPINSIGHTS_INSTRUMENTATIONKEY=$instrumentationKey" `
        "loader.io=$loaderioKey" `
        "Blob.StorageConnectionString=$storageConnection" `
    --slot-settings "Colour=Green"

# Open the apps in the browser
start "https://$app.azurewebsites.net"


# Tear down
# az group delete -n $rg --yes