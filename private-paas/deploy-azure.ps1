$ErrorActionPreference = 'Stop'

$location = 'Australia East'
$loc = 'aue'
$rg = 'helloprivate-rg'
$tags = 'project=private-paas'
$vnet = "helloprivate-$loc-vnet"
$subnet = "helloprivate"
$planSubnet = "asp"
$appgwSubnet = "appgws"

$app = "helloprivate-$loc"
$plan = "helloprivate-$loc-plan"
$planEndpoint = "$plan-endpoint"
$planConnection = "$plan-connection"
$planZone = "$app.azurewebsites.net"
$planDnsLink = "$plan-dnslink"
$planZoneGroup = "$plan-zonegroup"

$pip = "helloprivate-$loc-pip"
$appgw = "helloprivate-$loc-appgw"

$storage = "helloprivate$loc"
$storageEndpoint = "$storage-endpoint"
$storageConnection = "$storage-connection"
$storageDnsLink = "$storage-dnslink"
$storageZoneGroup = "$storage-zonegroup"

$apim = "helloprivate-$loc-apim"
$apimSubnet = 'apim'

## ADDRESS SCHEMA
# VNet  10.0.0.0/16
#   privatepaas     10.0.0.0/24
#   asp             10.0.1.0/26
#   apim            10.0.1.64/26
#   appgws          10.0.2.0/24


if ($false) {
    # RESOURCE GROUP
    az group create -n $rg --location $location --tags $tags


    # VNET
    az network vnet create -n $vnet -g $rg --location $location --subnet-name $subnet --tags $tags
    az network vnet subnet update -g $rg --vnet-name $vnet -n $subnet --disable-private-endpoint-network-policies $true
    
    # App Service Plan VNet Integration, /26 is recommended if scaling up to 30 instances
    #   https://docs.microsoft.com/en-us/azure/app-service/web-sites-integrate-with-vnet#azure-dns-private-zones#:~:text=A%20%2F26%20with%2064%20addresses%20accommodates%20a%20Premium%20plan%20with%2030%20instances
    az network vnet subnet create -g $rg --vnet-name $vnet -n $planSubnet --address-prefixes 10.0.1.0/26

    # APPGW V2 SUBNET 
    #   Requires separate subnet. Subnet can be shared by multiple gateways
    #   https://docs.microsoft.com/en-us/azure/application-gateway/application-gateway-faq#is-application-gateway-always-deployed-in-a-virtual-network
    #   /24 recommended (for max scale out)
    #   https://docs.microsoft.com/en-us/azure/application-gateway/configuration-infrastructure#size-of-the-subnet#:~:text=subnet%20size%20of%20%2F24%20is%20recommended
    az network vnet subnet create -g $rg --vnet-name $vnet -n $appgwSubnet --address-prefixes 10.0.2.0/24


    # APP SERVICE
    az appservice plan create -n $plan -g $rg --location $location --sku 'P1v3' --tags $tags
    $appId = (az webapp create -n $app --plan $plan -g $rg --tags $tags `
        --deployment-source-url 'https://github.com/DanielLarsenNZ/HelloAspDotNetCore' | ConvertFrom-Json ).id


    # APP SERVICE PRIVATE ENDPOINT
    az network private-endpoint create -n $planEndpoint -g $rg --vnet-name $vnet --subnet $subnet --connection-name $planConnection `
        --private-connection-resource-id $appId --group-id 'sites'
    #az network private-dns zone create -n $planZone -g $rg
    #az network private-dns link vnet create --name $planDnsLink -g $rg --registration-enabled $true --virtual-network $vnet --zone-name $planZone
    #az network private-endpoint dns-zone-group create -n $planZoneGroup -g $rg --endpoint-name $planEndpoint --private-dns-zone $planZone --zone-name $planZone



    # APP GATEWAY
    $planEndpointConfig = (az network private-endpoint show -n $planEndpoint -g $rg | ConvertFrom-Json)
    az network public-ip create -g $rg -n $pip --allocation-method 'Static' --sku 'Standard' --dns-name $app
    az network application-gateway create -n $appgw --location $location -g $rg --capacity 1 --sku Standard_v2 `
        --vnet-name $vnet --subnet $appgwSubnet --servers $planEndpointConfig.customDnsConfigs[0].ipAddresses[0] `
        --public-ip-address $pip
    az network application-gateway http-settings update -g $rg --gateway-name $appgw -n 'appGatewayBackendHttpSettings' `
        --host-name $planZone


    # STORAGE ACCOUNT
    $storageConfig = (az storage account create -n $storage -g $rg --location $location --sku  'Standard_LRS' --https-only `
        --kind StorageV2 --tags $tags | ConvertFrom-Json)

    # STORAGE PRIVATE ENDPOINT
    $storageEndpointConfig = (az network private-endpoint create -n $storageEndpoint -g $rg --vnet-name $vnet --subnet $subnet `
        --connection-name $storageConnection `
        --private-connection-resource-id $storageConfig.id --group-id 'blob' | ConvertFrom-Json)

    $storageZoneName = 'privatelink.blob.core.windows.net' #$storageEndpointConfig.customDnsConfigs[0].fqdn

    az network private-dns zone create -n $storageZoneName -g $rg
    az network private-dns link vnet create --name $storageDnsLink -g $rg --registration-enabled $true --virtual-network $vnet --zone-name $storageZoneName
    az network private-endpoint dns-zone-group create -n $storageZoneGroup -g $rg --endpoint-name $storageEndpoint --private-dns-zone $storageZoneName --zone-name 'storage'


    # APP SERVICE PLAN Regional VNET Integration
    az webapp vnet-integration add --name $app -g $rg --subnet $planSubnet --vnet $vnet
    # https://docs.microsoft.com/en-us/azure/app-service/web-sites-integrate-with-vnet#azure-dns-private-zones
    #az webapp config appsettings set -n $app -g $rg --settings "WEBSITE_DNS_SERVER=168.63.129.16" "WEBSITE_VNET_ROUTE_ALL=1"
}

# APIM
$apimSubnetId = ( az network vnet subnet create -g $rg --vnet-name $vnet -n $apimSubnet --address-prefixes '10.0.1.64/26' | ConvertFrom-Json ).id
$apimVNet = New-AzApiManagementVirtualNetwork -SubnetResourceId $apimSubnetId
New-AzApiManagement -ResourceGroupName $rg -Location $location -Name $apim -Organization "DanielLarsenNZ" -AdminEmail "dalars@microsoft.com" -VirtualNetwork $apimVNet -VpnType "Internal" -Sku "Developer"
