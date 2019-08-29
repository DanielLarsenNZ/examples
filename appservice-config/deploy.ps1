# Deploy App Service Plan, App Service and Key Vault

$location = 'australiaeast'
$loc = 'aue'
$rg = 'appserviceconfig-rg'
$tags = 'project=appservice-config'
$plan = "appserviceconfig-$loc-plan"
$app = "appserviceconfig-$loc"
$insights = 'appserviceconfig-insights'
$planSku = 'B1'         
$planInstances = 1
$keyvault = "appserviceconfig-$loc-kv"

# RESOURCE GROUP
az group create -n $rg --location $location --tags $tags


# APPLICATION INSIGHTS
#  https://docs.microsoft.com/en-us/cli/azure/ext/application-insights/monitor/app-insights/component?view=azure-cli-latest
az extension add -n application-insights

$instrumentationKey = ( az monitor app-insights component create --app $insights --location $location -g $rg --tags $tags | ConvertFrom-Json ).instrumentationKey


# APP SERVICES
# Create App Service Plan
az appservice plan create -n $plan -g $rg --location $location --sku $planSku --number-of-workers $planInstances --tags $tags

# Create webapp
az webapp create -n $app --plan $plan -g $rg --tags $tags

# Configure always on
az webapp config set -n $app -g $rg --always-on true

# Package and zip the app
dotnet publish .\Examples.AppServiceConfig\ --configuration Release -o '../_zip'
Compress-Archive -Path ./_zip/* -DestinationPath ./deploy.zip -Force

# Deploy source code
az webapp deployment source config-zip -g $rg -n $app --src ./deploy.zip

# Managed Identity
$principalId = ( az webapp identity assign -g $rg -n $app | ConvertFrom-Json ).principalId


# KEY VAULT
az keyvault create --location $location --name $keyvault -g $rg
az keyvault secret set --vault-name $keyvault --name "AppServiceConfig--Secret1" --value New-Guid

# Grant the webapp managed identity access
az keyvault set-policy --name $keyvault --object-id $principalId --secret-permissions get


# APP SETTINGS
az webapp config appsettings set -n $app -g $rg --settings "APPINSIGHTS_INSTRUMENTATIONKEY=$instrumentationKey" "AzureKeyVaultUrl=https://$keyvault.vault.azure.net/"



# Tear down
# az group delete -n $rg --yes
