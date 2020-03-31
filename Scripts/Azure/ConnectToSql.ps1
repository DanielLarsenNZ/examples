[CmdletBinding()]
param (
    [string] $TenantId = $env:AadTenantId,
    [string] $ClientId = $env:AadAppClientId,
    [string] $ClientSecret = $env:AadAppClientSecret,
    [string] $SqlServerFqn = $env:SqlServerFqn,
    [string] $SqlDb = $env:SqlDb
)

$resourceAppIdURI = 'https://database.windows.net/'

$tokenResponse = Invoke-RestMethod -Method Post -UseBasicParsing `
    -Uri "https://login.windows.net/$($TenantID)/oauth2/token" `
    -Body @{
        resource=$resourceAppIdURI
        client_id=$ClientId
        grant_type='client_credentials'
        client_secret=$ClientSecret
    } -ContentType 'application/x-www-form-urlencoded'

if ($tokenResponse) {
    Write-debug "Access token type is $($tokenResponse.token_type), expires $($tokenResponse.expires_on)"
    $Token = $tokenResponse.access_token
}

$conn = new-object System.Data.SqlClient.SqlConnection
$conn.ConnectionString = "Server=tcp:$($SqlServerFqn);Initial Catalog=$($SqlDb);Persist Security Info=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;" 
$conn.AccessToken = $token
$conn
Write-Verbose "Connect to database and execute SQL script"
$conn.Open() 
$query = 'select @@version'
$command = New-Object -TypeName System.Data.SqlClient.SqlCommand($query, $conn) 	
$Result = $command.ExecuteScalar()
$Result
$conn.Close() 
