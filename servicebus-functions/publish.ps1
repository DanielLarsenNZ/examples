# Load Vars
. ./_vars.ps1

# Package and zip the Function App
Remove-Item './_functionzip' -Recurse -Force
New-Item './_functionzip' -ItemType Directory
dotnet publish .\Examples.Pipeline.ServiceBusFunctions\ --configuration Release -o './_functionzip'
Compress-Archive -Path ./_functionzip/* -DestinationPath ./deployfunction.zip -Force

# Deploy source code
az functionapp deployment source config-zip -g $rg -n $functionApp --src ./deployfunction.zip

# Log tail
az webapp log tail -n $functionapp -g $rg