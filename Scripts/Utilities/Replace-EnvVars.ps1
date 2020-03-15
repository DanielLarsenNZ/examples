function Replace-EnvVars {
    [CmdletBinding()]
    param (
        [Parameter(Mandatory = $true)]  [string]    $Path,
        [Parameter(Mandatory = $false)] [psobject]  $Variables = @{}
    )
    
    begin {

    }
    
    process {
        $content = Get-Content $Path
        $vars = $content | Select-String -Pattern '\$(\(|\{)[a-zA-Z_]*(\)|})'

        foreach ($var in $vars)
        { 
            foreach ($match in $var.Matches)
            {
                $varname = $match.Value -replace '\$\(', '' -replace '\$\{', '' -replace '\}', '' -replace '\)', ''
                
                if ($null -ne $Variables[$varname])
                {
                    $content = $content.Replace($match.Value, $Variables[$varname])
                    Write-Verbose "Replaced $($match.Value) with variable value '$((Get-Item env:$varname).Value)'"
                }
                else 
                {
                    if (Test-Path env:$varname)
                    {
                        $content = $content.Replace($match.Value, (Get-Item env:$varname).Value)
                        Write-Verbose "Replaced $($match.Value) with env var value '$((Get-Item env:$varname).Value)'"
                    }
                }
            }
        }

        Set-Content $Path $content
    }
    
    end {
        
    }
}

Replace-EnvVars -Path './_test.json' -Variables @{myvar = 'XXXXXX'} -Verbose