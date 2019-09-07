# This script deploys dual (east-west) Service Bus Premium namespaces, and pairs them
param (
    [Switch] $Teardown
)

$primaryLocation = 'australiaeast'
$priLoc = 'aue'
$secondaryLocation = 'australiasoutheast'
$secLoc = 'ase'
$rg = 'servicebusha-rg'
$tags = 'project=servicebus-ha'
$primaryNamespace = "servicebusha-$priLoc"
$secondaryNamespace = "servicebusha-$secLoc"
$alias = 'servicebusha'

if ($teardown) {
    # TEAR DOWN
    
    # Break pairing
    az servicebus georecovery-alias break-pair -g $rg --namespace-name $primaryNamespace --alias $alias
    
    # Delete RG
    az group delete -n $rg --yes

} else {
    # DEPLOY

    # Create Resource Group
    az group create -n $rg --location $primaryLocation --tags $tags

    # SERVICE BUS
    # https://docs.microsoft.com/en-us/azure/service-bus-messaging/service-bus-geo-dr#setup
    # https://docs.microsoft.com/en-us/cli/azure/servicebus/namespace?view=azure-cli-latest#az-servicebus-namespace-create

    # Primary
    az servicebus namespace create -g $rg -n $primaryNamespace --location $primaryLocation --tags $tags --sku Premium --capacity 1

    # Secondary
    az servicebus namespace create -g $rg -n $secondaryNamespace --location $secondaryLocation --tags $tags --sku Premium --capacity 1

    # Set Alias and create Pair
    az servicebus georecovery-alias set -g $rg --namespace-name $primaryNamespace --alias $alias --partner-namespace $secondaryNamespace

}
