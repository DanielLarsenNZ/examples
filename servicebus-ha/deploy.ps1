# This script deploys dual (east-west) Service Bus Premium namespaces, and pairs them

$primaryLocation = 'australiaeast'
$priLoc = 'aue'
$secondaryLocation = 'australiasoutheast'
$secLoc = 'ase'
$rg = 'servicebusha-rg'
$tags = 'project=servicebus-ha'
$primaryNamespace = "servicebusha-$priLoc"
$secondaryNamespace = "servicebusha-$secLoc"


# Create Resource Group
az group create -n $rg --location $primaryLocation --tags $tags

# SERVICE BUS
# https://docs.microsoft.com/en-us/azure/service-bus-messaging/service-bus-geo-dr#setup
# https://docs.microsoft.com/en-us/cli/azure/servicebus/namespace?view=azure-cli-latest#az-servicebus-namespace-create

# Primary
az servicebus namespace create -g $rg -n $primaryNamespace --location $primaryLocation --tags $tags --sku Premium --capacity 1

# Secondary
az servicebus namespace create -g $rg -n $secondaryNamespace --location $secondaryLocation --tags $tags --sku Premium --capacity 1



# Tear down
# az group delete -n $rg --yes
