# This script deploys an App Service Plan, Storage Accounts, Application Insights, an Event Hubs Namespace
# and Hub and a Service Bus namespace and queues.
# It also deploys a WebJob App and a Function App.

# Load Vars
. ./_vars.ps1


$instrumentationKey = ( az monitor app-insights component show --app $insights -g $rg | ConvertFrom-Json ).instrumentationKey


# FUNCTION APP
az functionapp create -n $functionApp --plan $plan -g $rg --tags $tags -s $webjobsStorage --app-insights $insights --app-insights-key $instrumentationKey

# Configure always on
az functionapp config set -n $functionApp -g $rg --always-on true


# SERVICE BUS
# https://docs.microsoft.com/en-us/cli/azure/servicebus/namespace?view=azure-cli-latest#az-servicebus-namespace-create
foreach ($queue in $queues) {
    az servicebus queue create -g $rg --namespace-name $servicebusNamespace --name $queue --default-message-time-to-live 'P14D'
}

# Get connection strings
$servicebusConnectionString = ( az servicebus namespace authorization-rule keys list -g $rg --namespace-name $servicebusNamespace --name $servicebusAuthRule | ConvertFrom-Json ).primaryConnectionString
$webjobsStorageConnection = ( az storage account show-connection-string -g $rg -n $webjobsStorage | ConvertFrom-Json ).connectionString

# APP SETTINGS
az functionapp config appsettings set -n $functionApp -g $rg --settings "APPINSIGHTS_INSTRUMENTATIONKEY=$instrumentationKey" "AzureWebJobsStorage=$webjobsStorageConnection" "ServiceBusConnectionString=$servicebusConnectionString"


# Tear down
# az group delete -n $rg --yes
