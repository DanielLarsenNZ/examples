# Private PaaS - Private networking features in Azure platform services

> 👷🏻‍♀️🚧👷🏻‍♂️ WIP

> ℹ Hi! My name is Daniel Larsen. I work for Microsoft. Opinions are my own.

Azure Platform as a Service (PaaS) is a unique offering in the Cloud Services market. Azure PaaS services like App Services, Application Gateway, API Management, Azure SQL DB and so on provide a fully managed platform service where Microsoft is taking responsibility for every layer of the stack except the Application layer. 

## Networking at industrial scale

The public endpoints provisioned for Azure PaaS services, on the so called "Azure Edge", are secure internet gateways (SIG's), in place to defend the Azure Platform from attack and, by proxy, customer workloads. 

The firewalls, DDOS protection and Network infrastructure deployed are at industrial scale; only a handful of vendors globally have the resources to deploy and maintain this level of infrastructure. Microsoft was reported to spend more than a billion US dollars on Cloud infrastructure each month<sup>1</sup>. To put this in perspective for New Zealand readers, that's equivalent to spending the entire budget for the Waterview Tunnel project every month<sup>2</sup>.

Microsoft operates one of the largest global networks on the planet. Once traffic enters our network it stays on it until the last possible hop. Traffic between Azure services, even in different regions, is never routed across the public Internet<sup>3</sup>. By the time HTTP/S traffic arrives at the entry point of your application it as already been scrubbed, DDOS'ed, and encrypted in transit (at multiple layers).

### Azure networking is different

Microsoft has built an incredible global networking service by reinventing networking. We have embraced software defined networking, zero trust and defense in depth, and developed entirely new technologies to achieve this at phenomenal scale. Sometimes the biggest challenge for customers coming to Azure from other Clouds, or from their own networking infrastructure, is to grasp just how different networking is in the Azure Cloud. Our traditional mental models of through devices, physical networking infrastructure, point to point connections - even IP addressing will evaporate into thin air the first time you get to experience a walk through an Azure Datacentre, which are different to any others on the planet. 

These differences are also reflected in our approach to networking features in Azure PaaS services, which is what the remainder of this article is about. One way to think of private networking features in PaaS is that they are an abstraction; we present an API that looks and behaves like layer 4 networking to help users understand the features and match them to their requirements. However the story "under the hood" is much different.

### Requirements, requirements, requirements

If I was to sum up the Azure approcah to Cloud Architecture in one statement it would be "Business requirements first". We recommend architects and engineers start with business requirements first which can be easier said than done. Also as architects and engineers we are prone to "solutioning" without really consulting the business. Or to reaching for the most gold-plated solution in the assumption it will be the best for the business. These behaviours, that may have gone un-noticed in the on-premises IT world of upfront capital expenditure and sunk cost can be problematic in the modern cloud world. In other words, architects that learn to truly understand business requirement and then simply map them to platform features in Azure are the most successful in my experience<sup>*</sup>.

> <sup>*</sup> This left-to-right mapping of requirements works well for several classes of features in Azure including Networking, Security, High availability, Disaster recovery, Cost-optimisation & Performance (and more). John Downs and I talk about this at length in the [FastTrack for Azure Live] series on Cloud Architecture.

### Premium features

Azure PaaS services are deployed at scale with features that satisfy most (but not all) customers. Most customers do not require private networking in Cloud. This is why private networking features are often only available in Premium plans, which offer more features for the fewer customers who need them and are priced accordingly.

### Multi-tenancy

Almost all PaaS services are multi-tenanted and will share a multi-tenanted network. In the shared-responsibility model, Microsoft is taking responsibility for security and isolation within the PaaS service. Multi-tenancy unlocks cost savings at scale, which is why PaaS services are cheaper than services built on IaaS on a TCO basis.

## Security and Compliance

Azure PaaS Services were born in Cloud, embracing modern thinking around security and networking including zero-trust environments, the cloud is the datacentre, Identity is the perimeter, Defense in depth and so on. Azure PaaS Services are designed to run on the internet. Every public IP is protected by layers of security infrastructure provided by the platform. In our world cyber attacks are BAU and almost all are mitigated automatically, without human intervention or any customer being notified.

This is why the default configuration for Azure PaaS services is to route traffic in the public address space and this is how most customers configure them. Some older regulations and compliance standards can take an out-dated stance when it comes to security in Cloud, often relying on layer 4 controls and not properly protecting identities (which is the most common attack vector today<sup>4</sup>).

Compliance does not equal security. However for many businesses, not being able to satisfy compliance requirements can be a blocker to doing business, which is why we provide private networking features in PaaS (and are adding more all the time).

### Defense in depth

Defense in depth is a security strategy that is often misunderstood. IP restrictions on their own is not defence in depth. Providing defense against attack at multiple layers; identity, application, protocol, transport and physical is a good example of defense in depth. 

For example: 

1. AAD for user Identities 
1. Enable MFA for all users
1. Priveleged access workstations for Administrators
1. Managed identities for service to service authorization
1. Deploying Defender for Identity
1. Deploying Azure Sentinel
1. Deploying Azure Security Centre 
1. Enabling Firewall and Access control features on each service

Combine these defences with the certified physical controls provided Azure data centres (more than any other cloud<sup>5</sup>) and you have an excellent defence in depth strategy.


## Networking features in Azure PaaS

Let's look at some of the key networking features of Azure PaaS services; pros and cons, current feature gaps and tricky integration points. 

### App Service Plan

> ℹ This section is about the multi-tenanted App Service Plan SKUs from Basic to Premium. There is also an Isolated SKU (App Service Environment) which is deployed as a single tenant cluster with a single-tenant network. Isolated App Service Environments offer more fine-grained network controls and are well suited to environments with a high number of compliance and regulatory requirements. See [Introduction to the App Service Environments].

> 📖 [App Service networking features]

App Service networking features can be divided into two groups; features for inbound traffic and features for outbound traffic.

Notes:

1. In an App Service Plan, "private" traffic that originates in a Web App will take a different route (VNet Integration) to inbound private traffic (Private Endpoint)
1. App Service Plan VNet Integration DNS resolution is magic. DNS server is configured using an App Setting. `System.Net.Dns.GetHostAddresses()` will resolve Azure public IPs no matter what. Only `nameresolver.exe` will resolve private IPs of a lookup (to Azure Private DNS Zone for example).
1. Azure Blob Storage Private Endpoint does not work with VNet Integration. A Service Endpoint must be used.
1. App Service Plan VNet Integration requires an [empty and exclusive /26 subnet](https://docs.microsoft.com/en-us/azure/app-service/web-sites-integrate-with-vnet#azure-dns-private-zones#:~:text=A%20%2F26%20with%2064%20addresses%20accommodates%20a%20Premium%20plan%20with%2030%20instances) (for a Premium ASP to scale to 30 instances).

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

## Private endpoint behaviour by service

> 👷🏻‍♀️🚧👷🏻‍♂️ WIP

Azure Service | Access controls without private endpoint? | Public endpoint remains enabled? | Service endpoint available? | Managed identity support?
--- | --- | --- | ---
APIM | ? | ? | ? 
App GW v2 | ? | ? | ?
App Services | Yes | No, returns HTTP status 403 | ?
Cosmos DB | ? | ? | ?
Key Vault | ? | ? | ?
Redis Service | ? | ? | ?
SQL DB | ? | ? | ?

## Reference architectures

Generally Private PaaS requirements fall into one of three scenarios:

### 1. Restrict any access from public internet

Disable or restrict any public endpoint (that isn't required to be public) so that it cannot be accessed from the public internet. For each service, only allow traffic from a list of services (instances), subnets or IPs.

Combined with defenses at each layer (as described above in Defence in Depth) this is a pragmatic approach to reducing the attack surface and defence in depth.

<!-- TODO: Diagram and Sample -->

### 2. Route all traffic through VNET

Force all traffic that flows between Azure Services through a VNet in an attempt to keep all traffic isolated.

Except in a few special cases this is a technically flawed strategy, but the only choice due to compliance requirements. Azure PaaS services' networks are almost always multi-tenanted; Traffic isolation is the responsibility of the platform. However customers that trust PaaS don't neccesarily trust networking between Azure services; traffic between Azure services will never transit the public internet, however we still label any traffic that transits between virtual networks as Internet. The distinction between Internet and Public internet is difficult to explain, and is often too ambiguous for outdated standards and legislation.

<!-- TODO: Diagram and Sample -->

### 3. Hybrid cloud

Allow traffic to flow to and from on-premises networks where Azure is seen as a logical extension of the customer's datacentre. Azure and Azure PaaS has always had excellent features for hybrid cloud and hybrid networking and this is a common and well understood strategy.

<!-- TODO: Links to reference architecture and Sample -->

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
[Introduction to the App Service Environments]:(https://docs.microsoft.com/en-us/azure/app-service/environment/intro)
[App Service networking features]:https://docs.microsoft.com/en-us/azure/app-service/networking-features