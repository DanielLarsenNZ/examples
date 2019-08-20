# Deploy environment and code to demonstrate running Event (Hubs) Processor Host in a WebJob Host.
# This script deploys an App Service Plan, Storage Accounts, Application Insights and an Event Hub.

$location = 'australiaeast'
$loc = 'aue'
$rg = 'webjobevents-rg'
$tags = 'project=webjobs-eventhubs'
$plan = "webjobevents-$loc-plan"
$app = "webjobevents-$loc"
$webjobsStorage = "webjobevents$loc"
$dataStorage = "webjobeventsdata$loc"
$container = 'data'
$insights = 'webjobevents-insights'
$eventhubNamespace = 'webjobevents-hub'

# Consider these settings for scale
$planSku = 'B1'         # Scale up
$planInstances = 2      # Scale out
$eventhubsSku = 'Basic'
$eventhubsRetentionDays = 1
$eventhubsPartitions = 2    # 2 - 32. Cannot be changed after deployment. Good discussion here: https://medium.com/@iizotov/azure-functions-and-event-hubs-optimising-for-throughput-549c7acd2b75


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
az webapp create -n $app --plan $plan -g $rg --tags $tags

# Configure always on
az webapp config set -n $app -g $rg --always-on true

# Package and zip the WebJob
dotnet publish .\Examples.Pipeline.WebJobs\ --configuration Release -o '../_zip/app_data/Jobs/Continuous/Examples.Pipeline.Webjobs'
copy ./run.cmd './_zip/app_data/Jobs/Continuous/Examples.Pipeline.Webjobs'
Compress-Archive -Path ./_zip/* -DestinationPath ./deploy.zip -Force

# Deploy source code
az webapp deployment source config-zip -g $rg -n $app --src ./deploy.zip


# EVENT HUBS
# https://docs.microsoft.com/en-us/cli/azure/eventhubs?view=azure-cli-latest
# Create Event Hub and namespace and get the key
az eventhubs namespace create -g $rg --name $eventhubNamespace --location $location --tags $tags --sku $eventhubsSku
az eventhubs eventhub create -g $rg --namespace-name $eventhubNamespace --name 'transactions' --message-retention $eventhubsRetentionDays --partition-count $eventhubsPartitions

# Don't use RootManageSharedAccessKey in Production
$eventHubConnectionString = ( az eventhubs namespace authorization-rule keys list -g $rg --namespace-name $eventhubNamespace --name 'RootManageSharedAccessKey' | ConvertFrom-Json ).primaryConnectionString


# APP SETTINGS
az webapp config appsettings set -n $app -g $rg --settings "APPINSIGHTS_INSTRUMENTATIONKEY=$instrumentationKey" "AzureWebJobsStorage=$webjobsStorageConnection" "DataStorageConnectionString=$dataStorageConnection" "EventHubConnectionString=$eventHubConnectionString"

# Count to 10
Start-Sleep 10

# Upload a test file
# https://mockaroo.com/
az storage container create -n 'data' --account-name $dataStorage --public-access off
az storage blob upload --account-name $datastorage -f ./transactions.csv -c $container -n 'transactions.csv'

# Tear down
# az group delete -n $rg --yes
