# App Service Config

This example demonstrates loading configuration from Azure KeyVault at Startup. The configuration store
can be read during an API request without incuring a synchrounous call to Azure KeyVault.

This example also demonstrates Health checks on Azure KeyVault. And a sequential number service (WIP).

## Getting started

1. Install PowerShell
1. Install `az` CLI
1. Install `dotnet` SDK
1. `git clone` this repo
1. `cd` to this folder in a PowerShell terminal and type:

```powershell
> ./deploy.ps1
```

Deploys a Resource Group into your Azure Subscription with the following resources:

* Applications Insights
* App Service Plan and Web App
* Managed Identity
* Azure Key vault

The script also packages and publishes the `Examples.AppServiceConfig` app and configures App Settings.

## Links and references

Use Managed identities for Azure resources: <https://docs.microsoft.com/en-us/aspnet/core/security/key-vault-configuration?view=aspnetcore-2.2#use-managed-identities-for-azure-resources>

Health checks in ASP.NET Core: <https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/health-checks?view=aspnetcore-2.2>

AspNetCore.Diagnostics.HealthChecks - `IHealthCheck` implementations for a wide range of resources: <https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks>
