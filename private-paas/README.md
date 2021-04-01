# Private PaaS - Private networking features in Azure platform services

> 👷🏻‍♀️🚧👷🏻‍♂️ WIP

> ℹ My name is Daniel Larsen. I work for Microsoft. Opinions are my own.

Azure Platform as a Service (PaaS) is a unique offering in the Cloud Services market. Azure PaaS services like App Services, Application Gateway, API Management, Azure SQL DB and so on provide a fully managed platform service where Microsoft is taking responsibility for every layer of the stack except the Application layer. 

## Networking at industrial scale

The public endpoints provisioned for Azure PaaS services, on the so called "Azure Edge", are secure internet gateways (SIG's), in place to defend the Azure Platform from attack and, by proxy, customer workloads. 

The firewalls, DDOS protection and Network infrastructure deployed are at industrial scale; only a handful of vendors globally have the resources to deploy and maintain this level of infrastructure. Microsoft was reported to spend more than a billion US dollars on Cloud infrastructure each month<sup>1</sup>. To put this in perspective for New Zealand readers, that's equivalent to spending the entire budget for the Waterview Tunnel project every month<sup>2</sup>.

Microsoft operates one of the largest global networks on the planet. Once traffic enters our network it stays on it until the last possible hop. Traffic between Azure services, even in different regions, is never routed across the public Internet<sup>3</sup>. By the time HTTP/S traffic arrives at the entry point of your application it as already been scrubbed, DDOS'ed, and encrypted in transit (at multiple layers).

## Azure networking is different

Microsoft has built an incredible global networking service by reinventing networking. We have embraced software defined networking, zero trust and defense in depth, and developed entirely new technologies to achieve this at phenomenal scale. Sometimes the biggest challenge for customers coming to Azure from other Clouds, or from their own networking infrastructure, is to grasp just how different networking is in the Azure Cloud. Our traditional mental models of through devices, physical networking infrastructure, point to point connections - even IP addressing will evaporate into thin air the first time you get to experience a walk through an Azure Datacentre, which are different to any others on the planet. 

These differences are also reflected in our approach to networking features in Azure PaaS services, which is what the remainder of this article is about. One way to think of private networking features in PaaS is that they are an abstraction; we present an API that looks and behaves like layer 4 networking to help users understand the features and match them to their requirements. However the story "under the hood" is much different.

## Requirements, requirements, requirements

If I was to sum up the Azure approcah to Cloud Architecture in one statement it would be "Business requirements first". We recommend architects and engineers start with business requirements first which can be easier said than done. Also as architects and engineers we are prone to "solutioning" without really consulting the business. Or to reaching for the most gold-plated solution in the assumption it will be the best for the business. These behaviours, that may have gone un-noticed in the on-premises IT world of upfront capital expenditure and sunk cost can be problematic in the modern cloud world. In other words, architects that learn to truly understand business requirement and then simply map them to platform features in Azure are the most successful in my experience<sup>*</sup>.

With that said, let's look at the networking features in Azure and how they may meet your business's requirements.

> <sup>*</sup> This left-to-right mapping of requirements works well for several classes of features in Azure including Networking, Security, High availability, Disaster recovery, Cost-optimisation & Performance (and more). John Downs and I talk about this at length in the [FastTrack for Azure Live] series on Cloud Architecture.

## Networking features in Azure PaaS

Let's look at some of the key networking features of Azure PaaS services; pros and cons, current feature gaps and tricky integration points.

### App Service Plan

1. In an App Service Plan, "private" traffic that originates in a Web App will take a different route (Regional VNet Integration) to inbound private traffic (Private Endpoint)
1. App Service Plan Regional VNet Integration DNS resolution is magic. DNS server is configured using an App Setting. `System.Net.Dns.GetHostAddresses()` will resolve Azure public IPs no matter what. Only `nameresolver.exe` will resolve private IPs of a lookup (to Azure Private DNS Zone for example).
1. Azure Blob Storage Private Endpoint does not work with Regional VNet Integration. A Service Endpoint must be used. Service Endpoints are magic.
1. App Service Plan Regional VNet Integration requires an [empty and exclusive /26 subnet](https://docs.microsoft.com/en-us/azure/app-service/web-sites-integrate-with-vnet#azure-dns-private-zones#:~:text=A%20%2F26%20with%2064%20addresses%20accommodates%20a%20Premium%20plan%20with%2030%20instances) (for a Premium ASP to scale to 30 instances).

### Azure Application Gateway

1. Azure App Gateway v2 (AppGWv2) requires an exclusive subnet (multiple AppGWv2's can share the subnet). An IP is required for every instance. A single AppGWv2 can scale to 125 instances. [In this case a /24 subnet would be required](https://docs.microsoft.com/en-us/azure/application-gateway/configuration-infrastructure#size-of-the-subnet#:~:text=subnet%20size%20of%20%2F24%20is%20recommended).

### Azure API Management

1. APIM requires an exclusive subnet (can be shared by other APIM instances)
1. APIM requires [2 IPs per instance (Premium SKU)](https://docs.microsoft.com/en-us/azure/api-management/api-management-using-with-vnet#--subnet-size-requirement). Max of 10 instances would require 20 + 5 IPs or a /27.
1. APIM requires outbound traffic access to multiple PaaS services (fair enough) including Storage, SQL, Event Hubs. [There are several other requirements](https://docs.microsoft.com/en-us/azure/api-management/api-management-using-with-vnet#-common-network-configuration-issues).

### Private endpoints

1. Private endpoints require split-horizon DNS: reconstruction of public DNS records in a private zone for connections to work. 
1. In App Services, a web app with a private endpoint configured is still accessible via its public IP (it responds with 403).

<!--
## You will need

1. Your own DNS Server and management processes
1. To place as much trust in Azure SDN as you do when you trust our public network infrastructure
-->

## Getting started

To understand how features work and how services work together I have created a couple of resources to get your started. You can run these scripts in your own Azure Subscription so that you can make your own observations.

You will need:

* Azure Subscription
* AZ CLI installed
* PowerShell Core installed

```powershell
# Deploy an App Service with VNet Integration, 
# VNetRouteAll enabled, an NSG and a Test App for 
# testing routing behaviour
./deploy-vnet-route-all.ps1
```

The test app can be configured via App Settings to make various network connections. See [DanielLarsenNZ/HelloAspDotNetCore](https://github.com/DanielLarsenNZ/HelloAspDotNetCore).

## Links and references

<sup>1</sup> <!-- TODO LINK -->

<sup>2</sup> The Waterview Tunnel project was New Zealand's largest Infrastructure project to be completed. It took about 10 years and cost around 1.4 Billion New Zealand dollars. <!-- TODO LINK -->

<sup>3</sup> Any traffic between data centres, within Microsoft Azure or between Microsoft services such as Virtual Machines, Microsoft 365, XBox, SQL DBs, Storage, and virtual networks are routed within our global network and never over the public Internet. See [Microsoft global network](https://docs.microsoft.com/en-us/azure/networking/microsoft-global-network#:~:text=any%20traffic%20between%20data%20centers%2C%20within%20Microsoft%20Azure%20or%20between%20Microsoft%20services%20such%20as%20Virtual%20Machines%2C%20Microsoft%20365%2C%20XBox%2C%20SQL%20DBs%2C%20Storage%2C%20and%20virtual%20networks%20are%20routed%20within%20our%20global%20network%20and%20never%20over%20the%20public%20Internet).

> <http://helloprivate-aue.australiaeast.cloudapp.azure.com/>

> <https://helloprivate-aue.azurewebsites.net/>

* <https://docs.microsoft.com/en-us/azure/service-bus-messaging/network-security#private-endpoints>
* Reza's Private Function <https://github.com/RezaMahmood/privatefunction>
* <https://docs.microsoft.com/en-us/azure/app-service/web-sites-integrate-with-vnet>
* <https://docs.microsoft.com/en-us/azure/storage/common/storage-network-security#change-the-default-network-access-rule>
* <https://docs.microsoft.com/en-us/azure/private-link/private-endpoint-dns>
* <https://docs.microsoft.com/en-us/azure/virtual-network/virtual-networks-name-resolution-for-vms-and-role-instances>
* <https://docs.microsoft.com/en-us/azure/storage/common/storage-private-endpoints>
* <https://docs.microsoft.com/en-us/azure/app-service/scripts/cli-deploy-privateendpoint>
* <https://docs.microsoft.com/en-us/azure/service-bus-messaging/service-bus-service-endpoints>
* <https://docs.microsoft.com/en-us/azure/virtual-network/virtual-network-service-endpoints-overview>
* <https://docs.microsoft.com/en-us/azure/service-bus-messaging/network-security#private-endpoints>
* <https://docs.microsoft.com/en-us/azure/api-management/api-management-using-with-vnet>

<!-- link refs -->
[FastTrack for Azure Live]:https://aka.ms/ftalive