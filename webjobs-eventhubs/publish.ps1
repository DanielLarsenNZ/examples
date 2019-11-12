# Load Vars
. ./_vars.ps1

# Package and zip the WebJob
#dotnet publish .\Examples.Pipeline.WebJobs\ --configuration Release -o '../_zip/app_data/Jobs/Continuous/Examples.Pipeline.Webjobs'
#copy ./run.cmd './_zip/app_data/Jobs/Continuous/Examples.Pipeline.Webjobs'
#Compress-Archive -Path ./_zip/* -DestinationPath ./deploy.zip -Force

# Deploy source code
#az webapp deployment source config-zip -g $rg -n $webjobApp --src ./deploy.zip

# Package and zip the Function App
Remove-Item './_functionzip' -Recurse -Force
New-Item './_functionzip' -ItemType Directory
dotnet publish .\Examples.Pipeline.Functions\ --configuration Release -o './_functionzip'
Compress-Archive -Path ./_functionzip/* -DestinationPath ./deployfunction.zip -Force

# Deploy source code
az functionapp deployment source config-zip -g $rg -n $functionApp --src ./deployfunction.zip
