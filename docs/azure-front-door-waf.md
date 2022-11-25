# Azure Front Door WAF

Notes on Azure Front Door (AFD) WAF (Web Application Firewall).

* Custom rules (processed first)
* Managed rule sets

## Approach

* Enable the WAF in Detection mode to ensure that the WAF doesn't block requests while you are working through this process.
* Follow our [guidance for tuning the WAF](https://learn.microsoft.com/en-us/azure/web-application-firewall/afds/waf-front-door-tuning?pivots=front-door-standard-premium). This process requires that you enable diagnostic logging, review the logs regularly, and add rule exclusions and other mitigations.
* Repeat this whole process, checking the logs regularly, until you're satisfied that no legitimate traffic is being blocked. The whole process may take several weeks. Ideally you should see fewer false positive detections after each tuning change you make.
* Finally, enable the WAF in Prevention mode.
* Even once you're running the WAF in production, you should keep monitoring the logs to identify any other false-positive detections. Regularly reviewing the logs will also help you to identify any real attack attempts that have been blocked.

## Rules

* A RULE = match condition + priority + action
* ACTION TYPES = Allow, Block, Log, Redirect
* Once such a match is processed, rules with lower priorities aren't processed further.
* 1 WAF policy per Front Door endpoint
* WAF policies live and are executed on the edge.
* Detection mode:     Logs the request and its (first) matched rule to WAF logs.
* Prevention mode:    Takes the specified action if request matches the rule. Once matched, no further rules are evaluated. 
  * Matched requests are also logged in WAF logs.
* Allow action: - if matched, no further lower priority rules can block the request.
* Log action: Request is logged in WAG logs and WAF _continues evaluating_ lower rules.
* Anomaly score action: Default for DRS, not applicable to Bot manager ruleset.

## Custom rules

There are two type of match variables in IP address match, **RemoteAddr** and **SocketAddr**. RemoteAddr is the original client IP that is usually sent via X-Forwarded-For request header. SocketAddr is the source IP address WAF sees. If your user is behind a proxy, SocketAddr is often the proxy server address.

<https://learn.microsoft.com/en-us/azure/web-application-firewall/afds/waf-front-door-configure-ip-restriction>

## Azure managed rule set categories

> Custom rules are always applied first.

* Cross-site scripting
* Java attacks
* Local file inclusion
* PHP injection attacks
* Remote command execution
* Remote file inclusion
* Session fixation
* SQL injection protection
* Protocol attackers

> Use latest ruleset (e.g. DRS 2.1)

[DRS is fully documented here](https://learn.microsoft.com/en-us/azure/web-application-firewall/afds/waf-front-door-drs?tabs=drs21).

* Use exclusion rules to exclude parts of a request from certain rules.

## Bot protection rule set

Malicious IP addresses are sourced from the Microsoft Threat Intelligence feed and updated every hour.

## Rate limit

Rate limit custom WAF rule must have a match condition. To match all requests, use host length > 0. [(Reference)](https://learn.microsoft.com/en-us/azure/web-application-firewall/afds/waf-front-door-rate-limit#configure-a-rate-limit-policy)

## Deployment

WAF security policy updates take up-to 20 minutes.

## Monitoring

* Turn on **FrontDoorAccessLog** and **FrontDoorWebApplicationFirewallLog** diagnostic logs. 
* The **FrontDoorAccessLog** includes all requests that go through Front Door.

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


# Waf log and access log joined for WAF and DDoS analysis

let span = 24h;
let bin_size = 30m;
let WWW = AzureDiagnostics
| where ResourceProvider == "MICROSOFT.CDN" and Category == "FrontDoorAccessLog" and TimeGenerated > now(-span);
let WAF = AzureDiagnostics
| where ResourceProvider == "MICROSOFT.CDN" and Category == "FrontDoorWebApplicationFirewallLog" and TimeGenerated > now(-span);
WWW
| join kind=leftouter WAF on $left.trackingReference_s == $right.trackingReference_s 
| extend 
  logged_b=action_s1 =~ 'Log' or action_s1 =~ 'logandscore',
  blocked_b=action_s1 =~ 'Block'
| project TrackingReference=trackingReference_s, TimeGenerated, RuleSet=ruleName_s1, HttpStatus=httpStatusCode_s, Action=action_s1, PolicyMode=policyMode_s1, Country=clientCountry_s, POP=pop_s, CacheStatus=cacheStatus_s,
  RateLimited=httpStatusCode_s == '429' and blocked_b, 
  Blocked=blocked_b,
  Logged=logged_b,
  Cached=cacheStatus_s in~ ('HIT', 'PARTIAL_HIT', 'REMOTE_HIT'),
  Bot_Blocked=ruleName_s1 startswith "Microsoft_BotManagerRuleSet" and blocked_b,
  Bot_Logged=ruleName_s1 startswith "Microsoft_BotManagerRuleSet" and logged_b,
  DRS_Blocked=ruleName_s1 startswith "Microsoft_DefaultRuleSet" and blocked_b,
  DRS_Logged=ruleName_s1 startswith "Microsoft_DefaultRuleSet" and logged_b,
  Custom_Blocked=ruleName_s1 !startswith "Microsoft_DefaultRuleSet" and ruleName_s1 !startswith "Microsoft_BotManagerRuleSet" and blocked_b
| make-series 
  Requests=dcount(TrackingReference),
  Cached=dcountif(TrackingReference, Cached),
  RateLimited=dcountif(TrackingReference, RateLimited),
  Logged=dcountif(TrackingReference, Logged), 
  Blocked=dcountif(TrackingReference, Blocked), 
  BotBlocked=dcountif(TrackingReference, Bot_Blocked),
  BotLogged=dcountif(TrackingReference, Bot_Logged),
  DRS_Blocked=dcountif(TrackingReference, DRS_Blocked),
  DRS_Logged=dcountif(TrackingReference, DRS_Logged),
  Custom_Blocked=dcountif(TrackingReference, Custom_Blocked)
  on TimeGenerated in range(ago(span), now(), bin_size) 
| mvexpand TimeGenerated, Requests, Cached, RateLimited,  Logged, Blocked, BotBlocked, BotLogged, DRS_Blocked, DRS_Logged, Custom_Blocked
| project todatetime(TimeGenerated), toint(Requests), toint(Cached), toint(RateLimited), toint(Logged), toint(Blocked), toint(BotBlocked), toint(BotLogged), toint(DRS_Blocked), toint(DRS_Logged), toint(Custom_Blocked)
| render timechart
```

## Feedback

* Health probe latency max = 1000, not 65535

## Links

* [Azure POP location abbreviations](https://learn.microsoft.com/en-us/azure/cdn/microsoft-pop-abbreviations)
* [AFD WAF FAQ](https://learn.microsoft.com/en-us/azure/web-application-firewall/afds/waf-faq?source=recommendations)
* [AFD diagnostic Log reference](https://learn.microsoft.com/en-us/azure/frontdoor/standard-premium/how-to-logs)
* [AFD WAF logging and monitoring](https://learn.microsoft.com/en-us/azure/web-application-firewall/afds/waf-front-door-monitor)
* [AFD reports reference](https://learn.microsoft.com/en-us/azure/frontdoor/standard-premium/how-to-reports)
* [Tuning the AFD WAF](https://learn.microsoft.com/en-us/azure/web-application-firewall/afds/waf-front-door-tuning?pivots=front-door-standard-premium)

## Notes

https://hellowaf-arcwdegwhhgpadbs.z01.azurefd.net