# Fun with App Settings in a WebJobs Host on .NET Core

This advice is for users of WebJobs 3 on .NET Core 2.2. If you get an error message like the one below
you may need to carefully check the app settings.

    InvalidOperationException: Storage account connection string 'Storage' does not exist. Make sure that it is a defined App Setting.

## TL;DR

> This advice is specific to a **WebJobs host**. Functions is different (see below). 

In development, check:

* You have `appsettings.json` file in the csproj directory. _Environment specific files like "appsettings.Development.json"
  do not work. You don't need to set this up using a Configuration Builder in Program.cs. The WebJobs Host builds
  its own configuration._
* **Copy to Output** is set to **Copy always**
* The app settings file contains a setting `IsEncrypted` with value set to `false`.
* There is a setting or connection string in the app settings file named `AzureWebJobsStorage`. _The 
  value should contain a connection string for the storage account that stores the WebJobs and Functions
  meta-data._
* The Trigger binding Connection exists in app settings. _For example, `[BlobTrigger]` attribute
  has a `Connection` property for the connection string of the Storage account that is to trigger the
  Function. A setting or connection string must exist in the appsettings file, named for the value of
  this property. Check this for every binding._

In production, check you have a **App Setting** or **Configuration Setting** in App Services, or an **Environment Variable** for:

* `AzureWebJobsStorage`
* Each Trigger binding Connection

## Examples

> The settings file must be named `appsettings.json`, must exist in the csproj folder, and must be set to
> Copy Always. 

This example is for a WebJob Host using a Blob Trigger with a Connection named "BlobStorageConnectionString".

If you prefer App Settings:

```json
{
  "IsEncrypted": false,
  "AzureWebJobsStorage": "(Connection string for WebJob meta-data storage)",
  "BlobStorageConnectionString": "(Connection string for Blob storage)",
}
```

If you prefer Connection Strings:

```json
{
  "IsEncrypted": false,
  "ConnectionStrings": {
    "AzureWebJobsStorage": "(Connection string for WebJob meta-data)",
    "BlobStorageConnectionString": "(Connection string for Blob storage)"
  }
}
```

> It doesn't matter whether you use App Settings or Connection Strings. WebJobs will check them both.

## More info

The documentation is not clear (to me) as to where WebJobs looks for Connection Strings in .NET Core.
Adding to the confusion is that Functions (which runs on top of WebJobs) looks in a different place.
Several hours of debugging and looking through source code has led me to the following observations.
Specifically I am refering to **WebJobs 3** and **Functions 2** on **.NET Core 2.2**. See the full version
info below as these APIs seem to be shifting a lot.

The other gotcha is that, for some triggers, WebJobs requires a Storage Connection string named "Storage"
or "AzureWebJobsStorage" to store its meta-data. So far I have not been able to figure out how to change
the App Setting name.

## What about Functions?

Functions works the same, but the file is called `local.settings.json`.