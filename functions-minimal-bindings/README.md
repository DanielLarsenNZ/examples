# Event Sourcing

See: <https://docs.microsoft.com/en-us/azure/architecture/patterns/cqrs#example>

## Links and references

<https://docs.microsoft.com/en-us/azure/architecture/patterns/event-sourcing>

<https://docs.microsoft.com/en-us/previous-versions/msp-n-p/jj591559(v=pandp.10)>

<https://medium.com/@jeffhollan/in-order-event-processing-with-azure-functions-bb661eb55428>

<https://weblogs.asp.net/sfeldman/bend-message-deduplication-on-azure-service-bus-to-your-will>

<http://microsoftintegration.guru/2016/09/20/use-azure-function-to-deduplicate-messages-on-azure-service-bus/>

MOre

<https://azure.microsoft.com/en-au/blog/processing-100-000-events-per-second-on-azure-functions/>

<https://blogs.msdn.microsoft.com/kaevans/2015/02/24/scaling-azure-event-hubs-processing-with-worker-roles/>

## Web jobs

WebJobs host: <https://docs.microsoft.com/en-us/azure/app-service/webjobs-sdk-how-to#webjobs-host>

_The host is a runtime container for functions. It listens for triggers and calls functions. In version 3.x, the host is an implementation of IHost. You create a host instance in your code and write code to customize its behavior._

_This is a key difference between using the WebJobs SDK directly and using it indirectly through Azure Functions. In Azure Functions, the service controls the host, and you can't customize the host by writing code. Azure Functions lets you customize host behavior through settings in the host.json file. Those settings are strings, not code, and this limits the kinds of customizations you can do._

Event Hubs Packages - Functions 2.x: <https://docs.microsoft.com/en-us/azure/azure-functions/functions-bindings-event-hubs#packages---functions-2x>