# Fun with Functions Proxies

Functions proxies are awesome!

## Getting started

1. Create a `proxies.json` file in the root of your Functions project using one of the examples below.
1. If using a `.csproj` project file (C# Function), ensure that `proxies.json` is set to copy to output directory.
1. Deploy or redeploy your Function

## Verify Azure Functions host in Loader.io

[Loader.io](https://loader.io/) is a nice simple Cloud-based load-testing service. I use it often for load-testing APIs. Before you load test a host you must verify it; you can upload a static file or create a DNS entry. Hosting static files in Azure Functions is tricky and changing the DNS for a Function (without a custom domain) is impossible. But there is a feature called **Azure Functions Proxies** that can help out.

> 📖 <https://docs.microsoft.com/en-us/azure/azure-functions/functions-proxies>

Azure Functions proxies are pretty handy - you can rewrite request URLs and even rewrite requests and responses. One way to do this is create a `proxies.json` file and upload it with your Functions App code. Example: 

```json
{
    "$schema": "http://json.schemastore.org/proxies",
    "proxies": {
        "loader": {
            "matchCondition": {
                "methods": [ "GET" ],
                "route": "/loaderio-f13b281951404283979f7a0050f04c00"
            },
            "responseOverrides": {
                "response.body": "loaderio-f13b281951404283979f7a0050f04c00",
                "response.headers.Content-Type": "text/plain"
            }
        }
    }
}
```

This proxy, named `loader`, is configured for any GET request to `/loaderio-f13b281951404283979f7a0050f04c00`. A _response override_ is configured to respond with a plain-text response of "loaderio-f13b281951404283979f7a0050f04c00". In this scenario I don't even need to back it with a Function. This is all Loader.io needs to verify the Functions host.

I guess I could also host other small static assets like favico in this manner. Pretty sweet!

Read about Azure Function Proxies, response overrides and more here: <https://docs.microsoft.com/en-us/azure/azure-functions/functions-proxies#responseOverrides>

## Return a blank response from /

This will change the default Homepage to return a blank 200 response instead of the welcome page. Ensure that  the [AzureWebJobsDisableHomepage](https://docs.microsoft.com/en-us/azure/azure-functions/functions-app-settings#azurewebjobsdisablehomepage) App Setting has not been set.

```json
{
    "$schema": "http://json.schemastore.org/proxies",
    "proxies": {
        "loader": {
            "matchCondition": {
                "methods": [ "GET" ],
                "route": "/"
            },
            "responseOverrides": {
                "response.body": "",
                "response.headers.Content-Type": "text/plain"
            }
        }
    }
}
```