# Scale-out App Service Plans with Azure Front Door

Customers want the convenience of a fully managed hosting platform at cloud-scale. Scaling App Service 
Plans beyond the Premium SKU limit of 30 instances has been problematic in the past. Azure Traffic Manager 
(a DNS Load Balancer) is unable to distinguish between separate App Service Plans in the same region. 

These problems are resolved with **[Azure Front Door]**, a global HTTP load-balancer
service used by Microsoft first-party services including Xbox, Skype, Bing, Office 365 and Azure DevOps. With Front 
Door a single frontend can scale to 3,000 instances of a P3v2 plan, as well as many additional benefits 
including:

1. Front Door is a global HTTP load balancer. It does not have the restrictions of the DNS based Traffic 
   Manager.
1. Front Door can load-balance across multiple App Service Plans _in the same region_
1. On a Premium Service Plan, a single front-end can theoretically scale in/out from 0 to 3,000 instances
   (100 backends (plans) per pool x 30 instances each). That's up to 12,000 cores of compute available
   to the front end.
1. You can balance more evenly across the plans, due to the speed of HTTP load balancing (vs DNS), reducing
   overprovisioning
1. Custom SSL certs can be installed on the Front Door, allowing traffic to be encrypted and then re-encrypted
   if required using custom or Microsoft managed certs
1. Client connection times and response times are dramatically improved as clients connect to the nearest
   POP Node (130 POP nodes across 82 metros) and then Anycast across Microsoft’s Global Fibre Network
1. Front Door can load-balance and route across any combination of HTTP backend including App Services,
   Cloud Services, AKS and Functions
1. No code or other infrastructure changes are required. This is a drop-in replacement for Traffic Manager. 
   Hostnames, certs and CDN stay the same.

## Deploy Script

To deploy Azure Front Door + App Service Plans using az CLI and PowerShell:

1. Open [./deploy.ps1](./deploy.ps1) and edit the vars as required
1. Run the script:

```
./deploy.ps1
```

## More information and resources

* [How to access control Azure Front Door IPs and Headers](/docs/enforce-xafdid.md)
* Front Door overview: <https://docs.microsoft.com/en-us/azure/frontdoor/front-door-overview>
* Front Door pricing: <https://azure.microsoft.com/en-us/pricing/details/frontdoor/>
* Sample deployment script: [./deploy.ps1](./deploy.ps1)


[Azure Front Door]:https://azure.microsoft.com/en-us/services/frontdoor/