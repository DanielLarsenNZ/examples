# Azure Front Door WAF

Notes on Azure Front Door (AFD) WAF (Web Application Firewall).

* Custom rules (processed first)
* Managed rule sets

## Approach

* Enable the WAF in Detection mode to ensure that the WAF doesn't block requests while you are working through this process.
* Follow our [guidance for tuning the WAF](https://learn.microsoft.com/en-us/azure/web-application-firewall/afds/waf-front-door-tuning?pivots=front-door-standard-premium). The whole process may take several weeks. Ideally you should see fewer false positive detections after each tuning change you make.
  * Enable diagnostic logging
  * Review the logs
  * Add rule exclusions and other mitigations.
  * Repeat this whole process, checking the logs regularly, until you're satisfied that no legitimate traffic is being blocked. 
* Finally, enable the WAF in Prevention mode.
* Monitoring the logs to identify any other false-positive detections. Regularly reviewing the logs will also help you to identify any real attack attempts that have been blocked.

## Monitoring

* Turn on **FrontDoorAccessLog** and **FrontDoorWebApplicationFirewallLog** diagnostic logs. 
* The **FrontDoorAccessLog** includes all requests that go through Front Door.

## Rules

* A rule = match condition + priority + action
* Action types = Allow, Block, Log, Redirect
* Once such a match is processed, rules with lower priorities aren't processed further.
* 1 WAF policy per Front Door endpoint
* WAF policies live and are executed on the edge.
* Detection mode: Logs the request and its (first) matched rule to WAF logs.
* Prevention mode: Takes the specified action if request matches the rule. Once matched, no further rules are evaluated. 
  * Matched requests are also logged in WAF logs.
* Allow action: if matched, no further lower priority rules can block the request.
* Log action: Request is logged in WAF logs and WAF _continues evaluating_ lower rules.
* Anomaly score action: Default for DRS, not applicable to Bot manager ruleset.

## Custom rules

There are two type of match variables in IP address match, **RemoteAddr** and **SocketAddr**. RemoteAddr is the original client IP that is usually sent via X-Forwarded-For request header. SocketAddr is the source IP address WAF sees. If your user is behind a proxy, SocketAddr is often the proxy server address.

📖 [Configure an IP restriction rule with a Web Application Firewall for Azure Front Door](https://learn.microsoft.com/en-us/azure/web-application-firewall/afds/waf-front-door-configure-ip-restriction).

## Azure managed rule sets 

> Custom rules are always applied first.

**Categories:**

* Cross-site scripting
* Java attacks
* Local file inclusion
* PHP injection attacks
* Remote command execution
* Remote file inclusion
* Session fixation
* SQL injection protection
* Protocol attackers

> Always use latest ruleset (e.g. DRS 2.1)

📖 [DRS is fully documented here](https://learn.microsoft.com/en-us/azure/web-application-firewall/afds/waf-front-door-drs?tabs=drs21).

* Use exclusion rules to exclude parts of a request from certain rules.

## Bot protection rule set

Malicious IP addresses are sourced from the Microsoft Threat Intelligence feed and updated every hour.

## Rate limit

* Only per-IP rate limits are currently supported.
* Rate limit custom WAF rule must have a match condition. To match all requests, use host length > 0.  
* Don't bother with small rate-limits as rate limits are calculated per server (PoP node). In my experience, rate limits in the magnitude of tens-of-thousands are good.

📖 [What is rate limiting for Azure Front Door Service?](https://learn.microsoft.com/en-us/azure/web-application-firewall/afds/waf-front-door-rate-limit#configure-a-rate-limit-policy)

## Deployment

WAF security policy updates can take up-to 20 minutes. But usually you will see the update within seconds or minutes.

### Example queries

```kql
# All 429 responses (Access log)
AzureDiagnostics
| where ResourceProvider == "MICROSOFT.CDN" and Category == "FrontDoorAccessLog"
| where httpStatusCode_s == '429'

# All blocked requests (WAF log)
AzureDiagnostics
| where ResourceProvider == "MICROSOFT.CDN" and Category == "FrontDoorWebApplicationFirewallLog"
| where action_s == "Block"
```

Kusto query for DDOS attack analysis: [/kusto/afd-waf-ddos.kql](/kusto/afd-waf-ddos.kql)

## Links

* [Azure POP location abbreviations](https://learn.microsoft.com/en-us/azure/cdn/microsoft-pop-abbreviations)
* [AFD WAF FAQ](https://learn.microsoft.com/en-us/azure/web-application-firewall/afds/waf-faq?source=recommendations)
* [AFD diagnostic Log reference](https://learn.microsoft.com/en-us/azure/frontdoor/standard-premium/how-to-logs)
* [AFD WAF logging and monitoring](https://learn.microsoft.com/en-us/azure/web-application-firewall/afds/waf-front-door-monitor)
* [AFD reports reference](https://learn.microsoft.com/en-us/azure/frontdoor/standard-premium/how-to-reports)
* [Tuning the AFD WAF](https://learn.microsoft.com/en-us/azure/web-application-firewall/afds/waf-front-door-tuning?pivots=front-door-standard-premium)

<!-- 

## Feedback

* Health probe latency max = 1000, not 65535
## Notes

https://hellowaf-arcwdegwhhgpadbs.z01.azurefd.net

-->