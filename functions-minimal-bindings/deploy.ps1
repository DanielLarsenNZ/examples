# Deploy environment and code to demonstrate Azure Functions with minimal Bindings.
# This script deploys an App Service Plan, Storage Account and Event Hub.

$location = 'australiaeast'
$loc = 'aue'
$rg = 'functionsminbind-rg'
$tags = 'project=functions-minimal-bindings'
$plan = "functionsminbind-$loc-plan"
$planSku = 'B1'
$function = "functionsminbind-$loc"
$servicesStorage = "funminbindsvcs$loc"
$dataStorage = "funminbinddata$loc"
$container = 'data'
$insights = 'functionsminbind-insights'
$eventhubNamespace = 'functionsminbind-hub'

# Create Resource Group
az group create -n $rg --location $location --tags $tags

# Create App Service Plan
az appservice plan create -n $plan -g $rg --location $location --sku $planSku --number-of-workers 1 --tags $tags

# Create Storage Accounts
# https://docs.microsoft.com/en-us/cli/azure/storage/account?view=azure-cli-latest#az-storage-account-create
az storage account create -n $servicesStorage -g $rg -l $location --tags $tags --sku Standard_LRS
az storage account create -n $dataStorage -g $rg -l $location --tags $tags --sku Standard_LRS

$servicesStorageConnection = ( az storage account show-connection-string -g $rg -n $servicesStorage | ConvertFrom-Json ).connectionString
$dataStorageConnection = ( az storage account show-connection-string -g $rg -n $dataStorage | ConvertFrom-Json ).connectionString


# Create an Application Insights instance and get the instrumentation key
az extension add -n application-insights

#  https://docs.microsoft.com/en-us/cli/azure/ext/application-insights/monitor/app-insights/component?view=azure-cli-latest
#$instrumentationKey = ( az monitor app-insights component create --app $insights --location $location -g $rg --tags $tags | ConvertFrom-Json ).instrumentationKey

# Create Function
# https://docs.microsoft.com/en-us/cli/azure/functionapp?view=azure-cli-latest
#az functionapp create -g $rg --tags $tags -p $plan -n $function -s $servicesStorage -p $plan --os-type Windows --runtime dotnet --app-insights $insights --app-insights-key $instrumentationKey

# Package and zip the app
#dotnet publish .\Examples.Minimal.Functions\ --configuration Release -o ../_zip
#Compress-Archive -Path ./_zip/* -DestinationPath ./deploy.zip -Force

# Deploy source code
#az functionapp deployment source config-zip -g $rg -n $function --src ./deploy.zip

# Create Event Hub and namespace and get the key
# https://docs.microsoft.com/en-us/cli/azure/eventhubs?view=azure-cli-latest
az eventhubs namespace create -g $rg --name $eventhubNamespace --location $location --tags $tags --sku Basic
az eventhubs eventhub create -g $rg --namespace-name $eventhubNamespace --name 'transactions' --message-retention 1 --partition-count 2

# Don't use RootManageSharedAccessKey in Production
$eventHubConnectionString = ( az eventhubs namespace authorization-rule keys list -g $rg --namespace-name $eventhubNamespace --name 'RootManageSharedAccessKey' | ConvertFrom-Json ).primaryConnectionString

# Set connection strings in a Function App Setting
#az functionapp config appsettings set --name $function -g $rg --settings "AzureWebJobsStorage=$servicesStorageConnection"
#az functionapp config appsettings set --name $function -g $rg --settings "DataStorageConnectionString=$dataStorageConnection"
#az functionapp config appsettings set --name $function -g $rg --settings "EventHubConnectionString=$eventHubConnectionString"

# Count to 10
Start-Sleep 10

# Upload a test file
# https://mockaroo.com/
az storage container create -n 'data' --account-name $dataStorage --public-access off
az storage blob upload --account-name $datastorage -f ./transactions.csv -c $container -n 'transactions.csv'

# Tear down
# az group delete -n $rg --yes