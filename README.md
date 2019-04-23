# Examples

Quick examples for Azure, PowerShell and .NET.

## Talks

* **Tech.Hui** talk on Azure Scaffold: Scripts and ARM template examples: [talks/tech-hui](./talks/tech-hui)

## docs

* [powershell-cheatsheet.md](./docs/powershell-cheatsheet.md) - A PowerShell cheatsheet.
* [Verify Azure Functions host in Loader.io](./docs\verify-loader-azure-functions.md)

## Scripts

* [CreateServicePrincipalSecret.ps1](/Scripts/Azure/CreateServicePrincipalSecret.ps1) -
  Create a secret for an Azure Active Directory (AAD) Service Principal.
* [InvokeHttpObservatoryScan.ps1](/Scripts/Test/InvokeHttpObservatoryScan.ps1) -
  Invoke a Mozilla HTTP Observatory scan and wait for the scan to complete. Throw an error if grade is lower than a "B".
* [InvokeKuduNpmInstall.ps1](/Scripts/Deploy/InvokeKuduNpmInstall.ps1) - Invokes
  `npm install --production` on an Azure App Service via the Kudu API.
* [RewriteConfig.ps1](/Scripts/Deploy/RewriteConfig.ps1) - Rewrite XML config
  settings and save as a new file.

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](/LICENSE)
