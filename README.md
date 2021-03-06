# Examples

Quick examples for Azure, PowerShell and .NET.

## docs

* [powershell-cheatsheet.md](./docs/powershell-cheatsheet.md) - A PowerShell cheatsheet.
* [Fun with Functions Proxies](./docs/fun-with-functions-proxies.md)
* [Fun with App Settings in a WebJobs Host on .NET Core](./docs/fun-with-appsettings.md)

## Solutions

* [nginx-lb-appservices](./nginx-lb-appservices) - Load balance two (or more) **App Services** with **nginx**
  running in a **Linux App Service** container
* [appserviceplan-scale-afd](./appserviceplan-scale-afd) - Achieve massive scale-out by deploying **Azure Front Door**
  in front of multiple **App Service Plans**.
* [webjobs-eventhubs](https://github.com/DanielLarsenNZ/messaging) - Host **Functions** in a **WebJobs Host** for better configuration
  and monitoring.
* [appservice-config](./appservice-config) - Loading **Azure Key vault** secrets into an **App Service** at
  Startup using a **Managed Identity**. Also **Health checks**.

## Scripts

* [CreateServicePrincipalSecret.ps1](/Scripts/Azure/CreateServicePrincipalSecret.ps1) -
  Create a secret for an Azure Active Directory (AAD) Service Principal.
* [InvokeHttpObservatoryScan.ps1](/Scripts/Test/InvokeHttpObservatoryScan.ps1) -
  Invoke a Mozilla HTTP Observatory scan and wait for the scan to complete. Throw an error if grade is lower than a "B".
* [InvokeKuduNpmInstall.ps1](/Scripts/Deploy/InvokeKuduNpmInstall.ps1) - Invokes
  `npm install --production` on an Azure App Service via the Kudu API.
* [RewriteConfig.ps1](/Scripts/Deploy/RewriteConfig.ps1) - Rewrite XML config
  settings and save as a new file.
* [deploy-config-apim.ps1](Scripts\Azure\deploy-config-apim.ps1) - Deploy Azure API Management

## Talks

> Talks have moved to <https://github.com/DanielLarsenNZ/talks>

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](/LICENSE)
