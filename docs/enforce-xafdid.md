# How to access control Azure Front Door IPs and Headers

Here is how to enforce IP restrictions and `X-Azure-FDID` header check without deploying .NET code. This is to lock down Azure Front Door (AFD) to specific Web App(s).

<https://docs.microsoft.com/en-us/azure/frontdoor/front-door-faq#how-do-i-lock-down-the-access-to-my-backend-to-only-azure-front-door>

In `Web.config` (for each site), add an ARR rule under `<system.webServer>` for `X-Azure-FDID` like so. This example will respond with a `401 Unauthorized` status if the `X-Azure-FDID` does not match my Front Door ID, which is `2571f220-9437-4981-9220-xxxx`.

```xml
<system.webServer>
<rewrite>
    <rules>
    <rule name="XFH">
        <conditions>
        <add input="{HTTP_X_AZURE_FDID}" pattern="2571f220-9437-4981-9220-xxxx" negate="true"/>
        </conditions>
        <action statusCode="401" type="CustomResponse" statusReason="Unauthorized"/>
    </rule>
    </rules>
</rewrite>
```

Combine this with an `<ipSecurity>` element for the AFD IP ranges, example here: <https://docs.microsoft.com/en-us/iis/configuration/system.webserver/security/ipsecurity/#configuration-sample>

Example:

```xml
<security>
         <ipSecurity>
            <add ipAddress="147.243.0.0" subnetMask="255.255.0.0" />
         </ipSecurity>
</security>
```

Unfortunately, we don’t just have `147.243.0.0/16` anymore. You now need to add all 50 odd ranges in [ServiceTags_Public.json](https://www.microsoft.com/en-us/download/details.aspx?id=56519) 😐 This is explained in the FAQ linked above. If you have an Azure device in front of your backend, you can use the `AzureFrontDoor.Backend` service tag in a Network Security Group (NSG) instead.
