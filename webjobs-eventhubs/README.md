# Event Hubs on WebJobs

This example shows an _Event Processor Host_ running on an _WebJobs Host_. This is an alternative to
using the _Event Hubs Trigger_ for Functions when more configuration and monitoring of the underlying
host is desired.

This example is a simple pipeline:

    Transactions file uploaded -> File parsed -> Commands serialized -> Events published to Event Hub

    Events received -> Commands deserialized -> Commands processed -> Transaction and Account tables updated

## Getting started

1. Install PowerShell
1. Install `az` CLI
1. Install `dotnet` SDK
1. `git clone` this repo
1. `cd` to this folder in a PowerShell terminal and type:

```powershell
> ./deploy.ps1
```

The script will deploy a Resource Group into your Azure Subscription with the following resources:

* WebJobs Storage Account
* Data Storage Account
* Applications Insights
* App Service Plan and Web App
* Event Hubs namespace and Event Hub

The script will package and publish the `Examples.Pipeline.WebJobs` app and configure App Settings.
Finally the script will upload a test file to kick off the pipeline processing.

## Things to Note

* `Examples.Pipeline.WebJobs` is a .NET Core 3 Console app (packaged as a DLL). It incorporates the
  WebJobs Host SDK which provides runtime host functionality.
* The line `host.RunAsync();` in `Program.cs` will run continuously in an App Service instance. 
* Each instance of the App Service will run an instance of the Host, enabling scale-out.
* The WebJobs Host binds the Functions, Triggers and Bindings that it discovers in the library and creates
  triggers to respond to events (just like Azure Functions does, but completely self-contained).
* Functions can be defined as Singletons.
* _Event Processor Host_ is an intelligent client that registers clients for each partition, automatically
  scales in/out, manages checkpoints and recovers from failures.
* The Event Hubs client library is used for sending Events for better configuration, performance and
  monitoring. 

## Background

Most Azure users will be familiar with _[Azure Functions]_ an event driven, server-less experience on
Azure. Functions is also an SDK and Host which can run anywhere (in a container, on your desktop, in
your cloud). Functions is powered by the [Azure WebJobs SDK], the "core" of the Azure Functions runtime 
and many bindings.

Some scenarios call for more control and customisation over the Host than Functions can offer. For example,
Azure Event Hubs requires a stateful _Event Processor Host_ (for receiving messages) and it can be difficult
to configure, monitor and debug this Host when it is abstracted away by Functions. From the [WebJobs
Host docs]: 

_The host is a runtime container for functions. It listens for triggers and calls functions. In version 3.x, the host is an implementation of IHost. You create a host instance in your code and write code to customize its behavior._

_This is a key difference between using the WebJobs SDK directly and using it indirectly through Azure Functions. In Azure Functions, the service controls the host, and you can't customize the host by writing code. Azure Functions lets you customize host behavior through settings in the host.json file. Those settings are strings, not code, and this limits the kinds of customizations you can do._

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


[WebJobs Host docs]:(https://docs.microsoft.com/en-us/azure/app-service/webjobs-sdk-how-to#webjobs-host)