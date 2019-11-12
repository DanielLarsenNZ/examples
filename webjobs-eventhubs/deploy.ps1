# This script deploys an App Service Plan, Storage Accounts, Application Insights, an Event Hubs Namespace
# and Hub and a Service Bus namespace and queues.
# It also deploys a WebJob App and a Function App.

# Load Vars
. ./_vars.ps1

# Create Resource Group
az group create -n $rg --location $location --tags $tags


# STORAGE ACCOUNTS
# https://docs.microsoft.com/en-us/cli/azure/storage/account?view=azure-cli-latest#az-storage-account-create
az storage account create -n $webjobsStorage -g $rg -l $location --tags $tags --sku Standard_LRS
az storage account create -n $dataStorage -g $rg -l $location --tags $tags --sku Standard_LRS

$webjobsStorageConnection = ( az storage account show-connection-string -g $rg -n $webjobsStorage | ConvertFrom-Json ).connectionString
$dataStorageConnection = ( az storage account show-connection-string -g $rg -n $dataStorage | ConvertFrom-Json ).connectionString


# APPLICATION INSIGHTS
#  https://docs.microsoft.com/en-us/cli/azure/ext/application-insights/monitor/app-insights/component?view=azure-cli-latest
az extension add -n application-insights
$instrumentationKey = ( az monitor app-insights component create --app $insights --location $location -g $rg --tags $tags | ConvertFrom-Json ).instrumentationKey


# APP SERVICES
# Create App Service Plan
az appservice plan create -n $plan -g $rg --location $location --sku $planSku --number-of-workers $planInstances --tags $tags

# Create WebJob app
az webapp create -n $webjobApp --plan $plan -g $rg --tags $tags

# Configure always on
az webapp config set -n $webjobApp -g $rg --always-on true


# FUNCTION APP
az functionapp create -n $functionApp --plan $plan -g $rg --tags $tags -s $webjobsStorage --app-insights $insights --app-insights-key $instrumentationKey

# Configure always on
az functionapp config set -n $functionApp -g $rg --always-on true


# EVENT HUBS
# https://docs.microsoft.com/en-us/cli/azure/eventhubs?view=azure-cli-latest

# Create Event Hub, namespace and auth rule
az eventhubs namespace create -g $rg --name $eventhubNamespace --location $location --tags $tags --sku $eventhubsSku

foreach ($eventhub in $eventhubs) {
    az eventhubs eventhub create -g $rg --namespace-name $eventhubNamespace --name $eventhub --message-retention $eventhubsRetentionDays --partition-count $eventhubsPartitions
}

az eventhubs namespace authorization-rule create -g $rg --namespace-name $eventhubNamespace --name $eventhubAuthRule --rights Listen Send

# Get connection string
$eventhubConnectionString = ( az eventhubs namespace authorization-rule keys list --resource-group $rg --namespace-name $eventhubnamespace --name $eventhubauthrule | ConvertFrom-Json ).primaryConnectionString


# SERVICE BUS
# https://docs.microsoft.com/en-us/cli/azure/servicebus/namespace?view=azure-cli-latest#az-servicebus-namespace-create

# Create namespace, queue and auth rule
az servicebus namespace create -g $rg --name $servicebusNamespace --location $location --tags $tags --sku $servicebusSku

foreach ($queue in $queues) {
    az servicebus queue create -g $rg --namespace-name $servicebusNamespace --name $queue --default-message-time-to-live 'P14D'
}

az servicebus namespace authorization-rule create -g $rg --namespace-name $servicebusNamespace --name $servicebusAuthRule --rights Listen Send

# Get connection string
$servicebusConnectionString = ( az servicebus namespace authorization-rule keys list -g $rg --namespace-name $servicebusNamespace --name $servicebusAuthRule | ConvertFrom-Json ).primaryConnectionString


# APP SETTINGS
az webapp config appsettings set -n $webjobApp -g $rg --settings "APPINSIGHTS_INSTRUMENTATIONKEY=$instrumentationKey" "AzureWebJobsStorage=$webjobsStorageConnection" "DataStorageConnectionString=$dataStorageConnection" "EventHubConnectionString=$eventhubConnectionString" "ServiceBusConnectionString=$servicebusConnectionString"

az functionapp config appsettings set -n $functionApp -g $rg --settings "APPINSIGHTS_INSTRUMENTATIONKEY=$instrumentationKey" "AzureWebJobsStorage=$webjobsStorageConnection" "DataStorageConnectionString=$dataStorageConnection" "EventHubConnectionString=$eventhubConnectionString" "ServiceBusConnectionString=$servicebusConnectionString"

<#
# Count to 10
Start-Sleep 10

# Upload a test file
# https://mockaroo.com/
az storage container create -n $container --account-name $dataStorage --public-access off
az storage blob upload --account-name $datastorage -f ./transactions.csv -c $container -n 'transactions.csv'

#>

# Tear down
# az group delete -n $rg --yes
