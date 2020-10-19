# Magical "Private" PaaS

## Issues

### App Service Plan
1. In an App Service Plan, "private" traffic that originates in a Web App will take a different route (Regional VNet Integration) to inbound private traffic (Private Endpoint)
1. App Service Plan Regional VNet Integration DNS resolution is magic. DNS server is configured using an App Setting. `System.Net.Dns.GetHostAddresses()` will resolve Azure public IPs no matter what. Only `nameresolver.exe` will resolve private IPs of a lookup (to Azure Private DNS Zone for example).
1. Azure Blob Storage Private Endpoint does not work with Regional VNet Integration. A Service Endpoint must be used. A Service Endpoints are magic.
1. App Service Plan Regional VNet Integration requires an [empty and exclusive /26 subnet](https://docs.microsoft.com/en-us/azure/app-service/web-sites-integrate-with-vnet#azure-dns-private-zones#:~:text=A%20%2F26%20with%2064%20addresses%20accommodates%20a%20Premium%20plan%20with%2030%20instances) (for a Premium ASP to scale to 30 instances).

### Azure Application Gateway

1. Azure App Gateway v2 (AppGWv2) requires an exclusive subnet (multiple AppGWv2's can share the subnet). An IP is required for every instance. A single AppGWv2 can scale to 125 instances. [In this case a /24 subnet would be required](https://docs.microsoft.com/en-us/azure/application-gateway/configuration-infrastructure#size-of-the-subnet#:~:text=subnet%20size%20of%20%2F24%20is%20recommended).

### Azure API Management

1. APIM requires an exclusive subnet (can be shared by other APIM instances)
1. APIM requires [2 IPs per instance (Premium SKU)](https://docs.microsoft.com/en-us/azure/api-management/api-management-using-with-vnet#--subnet-size-requirement). Max of 10 instances would require 20 + 5 IPs or a /27.
1. APIM requires outbound traffic access to multuple PaaS services (fair enough) including Storage, SQL, Event Hubs. [This traffic will be routed in the public address space](https://docs.microsoft.com/en-us/azure/api-management/api-management-using-with-vnet#-common-network-configuration-issues).

### Private endpoints

1. Private endpoints require split-horizon DNS: reconstruction of public DNS records in a private zone for connections to work. 
1. In App Services, a web app with a private endpoint configured is still accessible via its public IP (it responds with 403).

## Recommendations

1. App Service plan


## You will need

1. Your own DNS Server and management processes
1. To place as much trust in Azure SDN as you do when you trust our public network infrastructure

## How a customer expects it to work