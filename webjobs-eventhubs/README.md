# Event Sourcing

## Gotchas

Read [/docs/fun-with-appsettings.md](/docs/fun-with-appsettings.md)

Make sure `BatchCheckpointFrequency` is set to default of 1, otherwise you may wonder why some of your
checkpoints are not being committed! (like I did for about 3 hours)

## Links and references

<https://docs.microsoft.com/en-us/previous-versions/msp-n-p/jj591559(v=pandp.10)>

<https://medium.com/@jeffhollan/in-order-event-processing-with-azure-functions-bb661eb55428>

<https://weblogs.asp.net/sfeldman/bend-message-deduplication-on-azure-service-bus-to-your-will>

<http://microsoftintegration.guru/2016/09/20/use-azure-function-to-deduplicate-messages-on-azure-service-bus/>

### Event Sourcing

Command and Query Responsibility Segregation (CQRS) pattern example: <https://docs.microsoft.com/en-us/azure/architecture/patterns/cqrs#example>

Event Sourcing pattern example: <https://docs.microsoft.com/en-us/azure/architecture/patterns/event-sourcing#example>

<https://azure.microsoft.com/en-au/blog/processing-100-000-events-per-second-on-azure-functions/>

<https://blogs.msdn.microsoft.com/kaevans/2015/02/24/scaling-azure-event-hubs-processing-with-worker-roles/>

### Event Processor Host

Event processor host: <https://docs.microsoft.com/en-us/azure/event-hubs/event-hubs-event-processor-host>

Programming guide for Azure Event Hubs: <https://docs.microsoft.com/en-us/azure/event-hubs/event-hubs-programming-guide>

Implement the IEventProcessor interface: <https://docs.microsoft.com/en-us/azure/event-hubs/event-hubs-dotnet-standard-getstarted-send#implement-the-ieventprocessor-interface>

Checkpointing: <https://docs.microsoft.com/en-us/azure/event-hubs/event-hubs-event-processor-host#checkpointing>

Azure Functions and Event Hubs: Optimising for Throughput: <https://medium.com/@iizotov/azure-functions-and-event-hubs-optimising-for-throughput-549c7acd2b75>

Reliable Event Processing in Azure Functions: <https://hackernoon.com/reliable-event-processing-in-azure-functions-37054dc2d0fc>

Provide a configurable retry policy for event hub triggered functions: <https://github.com/Azure/azure-webjobs-sdk/issues/1597>

### WebJobs host

Get started with the Azure WebJobs SDK for event-driven background processing: <https://docs.microsoft.com/en-us/azure/app-service/webjobs-sdk-get-started>

WebJobs host: <https://docs.microsoft.com/en-us/azure/app-service/webjobs-sdk-how-to#webjobs-host>

_The host is a runtime container for functions. It listens for triggers and calls functions. In version 3.x, the host is an implementation of IHost. You create a host instance in your code and write code to customize its behavior._

_This is a key difference between using the WebJobs SDK directly and using it indirectly through Azure Functions. In Azure Functions, the service controls the host, and you can't customize the host by writing code. Azure Functions lets you customize host behavior through settings in the host.json file. Those settings are strings, not code, and this limits the kinds of customizations you can do._

Azure Event Hubs bindings for Azure Functions: <https://docs.microsoft.com/en-us/azure/azure-functions/functions-bindings-event-hubs#packages---functions-2x>

Azure Webjobs SDK repo: <https://github.com/Azure/azure-webjobs-sdk/wiki>

Azure WebJobs SDK Extensions repo: <https://github.com/Azure/azure-webjobs-sdk-extensions>

Singleton Attribute: <https://github.com/Azure/azure-webjobs-sdk/blob/b798412ad74ba97cf2d85487ae8479f277bdd85c/src/Microsoft.Azure.WebJobs/SingletonAttribute.cs>

### Performance settings

<https://github.com/projectkudu/kudu/wiki/WebJobs#configuration-settings>

### WebJobs Kudu

Reference: <https://github.com/projectkudu/kudu/wiki/WebJobs>

### AddConsole() issue

<https://github.com/Azure/azure-webjobs-sdk/issues>

Try compile this: <https://github.com/Azure/azure-webjobs-sdk/blob/00686a5ae3b31ca1c70b477c1ca828e4aa754340/sample/SampleHost/Program.cs>