# Create an SPN, associated to a Resource Group, create a Service connection in ADO
$rg = 'myawesome-rg'    # Resource Group Name
$identityName = "$rg-id"
$location = 'australiaeast'
$tags = 'project=my-awesome-project'

# Create an Identity (SPN) scoped to a Resource Group
# https://docs.microsoft.com/en-us/cli/azure/identity?view=azure-cli-latest#az-identity-create
$identity = (az identity create -n $identityName -g $rg --location $location --tags $tags | ConvertFrom-Json )

# AZ DEVOPS
#  https://docs.microsoft.com/en-us/cli/azure/ext/azure-devops/?view=azure-cli-latest
az extension add -n azure-devops

az devops service-endpoint azurerm create --azure-rm-service-principal-id 
                                          --azure-rm-subscription-id
                                          --azure-rm-subscription-name
                                          --azure-rm-tenant-id
                                          --name
                                          [--azure-rm-service-principal-certificate-path]
                                          [--detect {false, true}]
                                          [--org]
                                          [--project]