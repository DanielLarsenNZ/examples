$rg = 'eshopmodernizing-rg'
$location = 'westus2'
$loc = 'wus2'
$sql = "eshopmodernizing-sql"
$sqlFqn = "$sql.database.windows.net"
$sqlDb = 'MyDb'
$localPublicIp = 'X.X.X.X'
$tags = @{
    owner = 'dalars';
    project = 'DanielLarsenNZ/examples/azuresql-spn'
}

$ErrorActionPreference = 'Stop'

#New-AzResourceGroup -Name $rg -Location $location -Tag $tags -Force -Verbose

$User = "XXXX"
$PWord = ConvertTo-SecureString -String "XXXX" -AsPlainText -Force
$creds = New-Object -TypeName System.Management.Automation.PSCredential -ArgumentList $User, $PWord

#$sqlServer = New-AzSqlServer -ServerName $sql -ResourceGroupName $rg -Location $location -SqlAdministratorCredentials $creds -Verbose

New-AzSqlDatabase -ResourceGroupName $rg -ServerName $sql -DatabaseName $sqlDb -Verbose

New-AzSqlServerFirewallRule -ResourceGroupName $rg -ServerName $sql -FirewallRuleName "local" -StartIpAddress $localPublicIp -EndIpAddress $localPublicIp -Verbose

#Install-Module -Name SqlServer

#Invoke-SqlCmd -ServerInstance @sqlServer -Credential = (Get-Credential) -Query 'SELECT @@VERSION'

