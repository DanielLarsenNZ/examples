# Private endpoint (private link) trouble shooting guide for developers

For those long lonely nights trying to get your App Service to talk to Redis cache on its private endpoint...

> ℹ️ This guide is for deploying **Linux** App Service and **Linux** Function Apps using **Bicep**. Windows and Portal deployment scenarios are not covered here.

In the words of the great @cbellee...

    I usually create the following.
    1. Private Endpoint
    2. Private DNS zone
    3. DNS Zone Group
    4. VNet Link

The private endpoint will also create a NIC.

## Common faults

### App Service can't resolve the DNS name of the service. 

The Private DNS Zone _Group_ is essential for the automatic creation of A records for the private addresses of the NICs associated with the private links. The private DNS zone also needs to be VNet linked for this to work.

### Linux Function App won't boot - Application Error (503)

The following app setting settings must be set correctly. For more information, [see this guide](https://docs.microsoft.com/en-us/azure/azure-functions/functions-infrastructure-as-code?tabs=bicep#linux-3).

    APPINSIGHTS_INSTRUMENTATIONKEY
    AzureWebJobsStorage
    WEBSITE_CONTENTAZUREFILECONNECTIONSTRING
    WEBSITE_CONTENTSHARE
    FUNCTIONS_EXTENSION_VERSION
    FUNCTIONS_WORKER_RUNTIME

* If the Function's Storage account is private, you need **two private endpoints**; one for **blob** and one for **files**. Some guides say to deploy private endpoints for queue and table as well, although I haven't proven that yet.
* When deploying with Bicep, you must create a storage fileshare named the same as the value of `WEBSITE_CONTENTSHARE`. If you don't do this, nothing will work, SCM won't boot, logs won't work.

## Diagnosing faults

In the Portal, run **Diagnose and solve problems** > **Configuration and Management**. This will automatically diagnose problems that prevent the Function App from booting.

### Test container
On App Service Linux, deploy a "test" container (alongside your VNet integrated App) using the built-in .NET 6 container which has SSH enabled. 

```bicep
// Using CARML modules: https://aka.ms/carml
module webApp2Module 'modules/v0.6.0/Microsoft.Web/sites/deploy.bicep' = {
  name: 'webApp2Module'
  params: {
    kind: 'app'
    name: app2
    serverFarmResourceId: appServicePlanModule.outputs.resourceId
    virtualNetworkSubnetId: vnetModule.outputs.subnetResourceIds[3]
    siteConfig: {
      vnetRouteAllEnabled: true
      linuxFxVersion: 'DOTNETCORE|6.0'
    }
  }
}
```

SSH into the container (via Kudu) to diagnose. The following bash commands are available in this environment:

```bash
host        # resolve a hostname all the way to the IP address
nslookup    # lookup a DNS name
dig         # query a DNS server
curl        # Send an HTTP request
```

## Helpful guides

Tested and working Azure Sample (includes Azure Redis Cache, Storage and Key Vault): https://github.com/Azure-Samples/highly-available-zone-redundant-webapp

Another good sample: [Create Function App and private endpoint-secured Storage](https://github.com/Azure/azure-quickstart-templates/blob/master/quickstarts/microsoft.web/function-app-storage-private-endpoints/main.bicep).

Virtual Network (VNet) Integration has several routes to be configured. See [Integrate your app with an Azure virtual network - Routes](https://docs.microsoft.com/en-us/azure/app-service/overview-vnet-integration#routes) for more information.

Ddeploying Azure Functions with bicep: [Automate resource deployment for your function app in Azure Functions](https://docs.microsoft.com/en-us/azure/azure-functions/functions-infrastructure-as-code?tabs=bicep).
