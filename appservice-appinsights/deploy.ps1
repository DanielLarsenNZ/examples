# Deploy App Service Plan, App Service and Application Insights
$location = 'australiaeast'
$loc = 'aue'
$rg = 'appserviceinsights-rg'
$tags = 'project=appservice-appinsights'
$plan = "appserviceinsights-$loc-plan"
$app = "appserviceinsights-$loc"
$insights = 'appserviceinsights-insights'
$planSku = 'B1'
$planInstances = 1

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

# APP SETTINGS
# https://markheath.net/post/automate-app-insights-extension
az webapp config appsettings set -n $app -g $rg --settings `
    "APPINSIGHTS_INSTRUMENTATIONKEY=$instrumentationKey" `
    "ApplicationInsightsAgent_EXTENSION_VERSION=~2" `
    "XDT_MicrosoftApplicationInsights_Mode=recommended" `
    "InstrumentationEngine_EXTENSION_VERSION=~1" `
    "XDT_MicrosoftApplicationInsights_BaseExtensions=~1"


start "https://$app.azurewebsites.net/"


# Tear down
# az group delete -n $rg --yes






<#

MIT License

Copyright (c) 2017 Daniel Larsen

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.

#>