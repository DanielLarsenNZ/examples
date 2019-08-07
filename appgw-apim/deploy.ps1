# Deploy an Azure Application Gateway v2 + WAF (Web Application Firewall) with an Azure API Manager as the backend

$location = 'westus'
$loc = 'wus'
$rg = 'hello-apim-rg'
$tags = 'project=appgw-apim'
$insights = 'hello-apim-insights'
$vnet = "hello-apim-$loc-vnet"
$frontendSubnet = 'hello-apim-frontend-subnet'
$backendSubnet = 'hello-apim-backend-subnet'
$pip = "hello-apim-$loc-pip"
$appGW = "hello-apim-$loc-gw"


# Create resource group
az group create -n $rg --location $location --tags $tags

az network vnet create --name $vnet --resource-group $rg --location $location --tags $tags --address-prefix 10.0.0.0/16 --subnet-name $frontendSubnet --subnet-prefix 10.0.1.0/24 

az network vnet subnet create --name $backendSubnet -g $rg --vnet-name $vnet --address-prefix 10.0.2.0/24

az network public-ip create -g $rg --name $pip --tags $tags --allocation-method Static --sku Standard

az network application-gateway create -n $appGW -g $rg --location $location --tags $tags --public-ip-address $pip --frontend-port 80 --http-settings-port 80 --http-settings-protocol Http --routing-rule-type Basic --servers appserviceplanscale1-eus.azurewebsites.net appserviceplanscale2-eus.azurewebsites.net --sku WAF_v2 --subnet $frontendSubnet --vnet-name $vnet --verbose
