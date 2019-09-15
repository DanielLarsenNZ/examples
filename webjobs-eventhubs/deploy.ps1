# Deploy environment and code to demonstrate running Event (Hubs) Processor Host in a WebJob Host.
# This script deploys an App Service Plan, Storage Accounts, Application Insights, an Event Hubs Namespace
# and Hub and a Service Bus namespace and queues.
# It also deploys a WebJob App and a Function App.

param (
    [Switch] $SkipDeploy
)

$location = 'australiaeast'
$loc = 'aue'
$rg = 'hellomessaging-rg'
$tags = 'project=hello-messaging'
$plan = "hellomessaging-$loc-plan"
$webjobApp = "hellomessaging-$loc-app"
$webjobsStorage = "hellomessaging$loc"
$functionApp = "hellomessaging-$loc-fn"
$dataStorage = "hellomessagingdata$loc"
$container = 'data'
$insights = 'hellomessaging-insights'
$eventhubNamespace = 'hellomessaging-hub'
$eventhubs = 'transactions1', 'transactions2'
$eventhubAuthRule = 'SenderListener1'
$servicebusNamespace = 'pipeline-bus'
$queues = 'test1', 'test2'
$servicebusAuthRule = 'SenderReceiver1'

# Consider these settings for scale
$planSku = 'B1'         # Scale up
$planInstances = 1      # Scale out
$eventhubsSku = 'Basic'
$eventhubsRetentionDays = 1
$eventhubsPartitions = 2    # 2 - 32. Cannot be changed after deployment. Good discussion here: https://medium.com/@iizotov/azure-functions-and-event-hubs-optimising-for-throughput-549c7acd2b75
$servicebusSku = 'Basic'


if (!$SkipDeploy) {
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

    # Package and zip the WebJob
    dotnet publish .\Examples.Pipeline.WebJobs\ --configuration Release -o '../_zip/app_data/Jobs/Continuous/Examples.Pipeline.Webjobs'
    copy ./run.cmd './_zip/app_data/Jobs/Continuous/Examples.Pipeline.Webjobs'
    Compress-Archive -Path ./_zip/* -DestinationPath ./deploy.zip -Force

    # Deploy source code
    az webapp deployment source config-zip -g $rg -n $webjobApp --src ./deploy.zip

    # FUNCTION APP
    az functionapp create -n $functionApp --plan $plan -g $rg --tags $tags -s $webjobsStorage --app-insights $insights --app-insights-key $instrumentationKey

    # Configure always on
    az functionapp config set -n $functionApp -g $rg --always-on true

    # Package and zip the Function App
    dotnet publish .\Examples.Pipeline.Functions\ --configuration Release -o '../_functionzip'
    Compress-Archive -Path ./_functionzip/* -DestinationPath ./deployfunction.zip -Force

    # Deploy source code
    az functionapp deployment source config-zip -g $rg -n $functionApp --src ./deployfunction.zip
}

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
az storage container create -n 'data' --account-name $dataStorage --public-access off
az storage blob upload --account-name $datastorage -f ./transactions.csv -c $container -n 'transactions.csv'

#>

# Tear down
# az group delete -n $rg --yes
