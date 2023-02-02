Get-ChildItem -Path . -Directory | ForEach-Object -Process {
    Write-Host $_
    cd $_
    $status = git status -s
    if ($status -ne $null) 
    {
        Write-Host $status
        git diff
        break 
    }
}

