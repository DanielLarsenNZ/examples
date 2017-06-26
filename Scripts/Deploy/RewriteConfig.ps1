# http://www.powershellmagazine.com/2013/08/19/mastering-everyday-xml-tasks-in-powershell/

param (
    [string] $ConfigFile = '.\example.app.config',
    [string] $Setting1Value = 'New setting value',
    [string] $DefaultConnectionString = 'Data Source=(localdb)\ProjectsV13;'
)

$ErrorActionPreference = 'Stop'

[xml] $xml = Get-Content -Path $ConfigFile

# Read the value of an AppSetting
Write-Output $xml.configuration.appSettings.add | Where-Object -FilterScript { $_.key -eq 'Setting1' }

# Select an appSetting/Add Node and set its value. The appSetting must exist in the file in this case
$appSetting = Select-XML -Xml $xml -XPath '//appSettings/add[@key="Setting1"]'
$appSetting.node.value = $Setting1Value

# Same again for connection string
$connString = Select-XML -Xml $xml -XPath '//connectionStrings/add[@name="Default"]'
$connString.node.connectionString = $DefaultConnectionString

$newPath = "$PSScriptRoot\rewritten.app.config"
$xml.Save($newPath)
