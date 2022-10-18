[CmdletBinding()]
param (
    [Parameter()] [string] $TargetDirectory = '.',
    [Parameter()] [string[]] $Modules = '*'
)

<#
    TODO:
    -Version "v0.6.0"
    -NoVersionFolders
    -ReplaceExistingFiles
    -Sub-module filters
#>

$ErrorActionPreference = 'Stop'

$body = Invoke-RestMethod -Method Get -Uri https://api.github.com/repos/Azure/ResourceModules/releases/latest -Headers @{ Accept = 'application/vnd.github+json' }
$poshFolder = "$env:USERPROFILE/posh-bicep"

New-Item -Path $poshFolder -Force -ItemType Directory | Out-Null

$archivePath = "$poshFolder/$($body.id)"
New-Item -Path $archivePath -Force -ItemType Directory | Out-Null
$archivePath = Resolve-Path $archivePath    # forward slash fix

$zipFilePath = "$poshFolder/$($body.id).zip"

if ((Test-Path "$archivePath/**") -eq $false) {
    if ((Test-Path $zipFilePath) -eq $false) {
        Invoke-WebRequest -Uri $body.zipball_url -OutFile $zipFilePath
    }

    Expand-Archive -Path "$poshFolder/$($body.id).zip" -DestinationPath $archivePath
}

# COPY MODULES

$target = "$TargetDirectory\$($body.tag_name)"

Write-Host 'Copying modules...'

# Get each Module folder, one at a time
foreach ($module in $Modules) {
    if ((Test-Path "$archivePath/*/arm/$module") -eq $false) { Write-Error "Module $module not found." }
    
    Get-ChildItem "$archivePath/*/arm/*" -Filter $module -Directory | Foreach-Object `
    {        
        Write-Host "  $($_.Name) " -NoNewline
    
        Get-ChildItem $_ -Recurse -Include "*.bicep" -File | Foreach-Object `
        {        
            $destDir = Split-Path ($_.FullName -Replace [regex]::Escape((Resolve-Path "$archivePath/*/arm")), $target)
            $destFile = "$destDir/$(Split-Path $_.FullName -Leaf)"
    
            if ((Test-Path $destFile) -eq $true) {
                Write-Verbose "Skipping $destFile as it already exists"
            } else {
                if (!(Test-Path $destDir))
                {
                    Write-Verbose "Creating directory $destDir"
                    New-Item -ItemType directory $destDir | Out-Null
                }
    
                Write-Verbose "Copying module file to $destFile"
                Write-Host '.' -NoNewline
                Copy-Item $_ -Destination $destDir
            }
        }

        Write-Host ''
    }    
}
