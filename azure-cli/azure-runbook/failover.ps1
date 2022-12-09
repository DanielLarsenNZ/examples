<#

    # Failover Runbook

    > WIP! This script has not been tested. 

    This runbook fails over by making `$NewPrimaryLocation` the new primary replicas for SQL and Cosmos.
    Then Traffic Manager profile priority is updated to direct traffic to the `$NewPrimaryLocation`.

    1. Failover SQL current secondary -> new primary
    2. Failover Cosmos DB current secondary -> new primary
    3. Failover TM current secondary -> new primary
    4. Update Application Configuration

    ## Usage

    ```powershell
    Invoke-Failover -Environment Prod -NewPrimaryLocation australiaeast
    ```

#>

function Invoke-Failover {
    [CmdletBinding()]
    param (
        [ValidateSet('Dev', 'Staging', 'Prod')] [string] $Environment,
        [ValidateSet('australiaeast', 'australiasoutheast')] [string] $NewPrimaryLocation
    )

    $ErrorActionPreference = 'Stop'

    $primaryLoc = $NewPrimaryLocation -eq 'australiaeast' ? 'aue' : 'ase'
    $secondaryLoc = $NewPrimaryLocation -eq 'australiaeast' ? 'ase' : 'aue'

    $envn = $Environment.ToLower()
    
    $rg = "hellofailover-$envn-rg"
    $primarySql = "hellofailover-$envn-$primaryLoc-sql"
    $secondarySql = "hellofailover-$envn-$secondaryLoc-sql"
    $sqlDB = "hellofailover-db"
    $primaryRG = "hellofailover-$envn-$primaryLoc-rg"
    $secondaryRG = "hellofailover-$envn-$secondaryLoc-rg"
    $cosmos = "hellofailover-$envn-cosmos"
    $profile = "hellofailover-$envn-profile"


    # ############
    # FAILOVER SQL
    # ############

    # https://docs.microsoft.com/en-us/azure/sql-database/scripts/sql-database-setup-geodr-and-failover-database-powershell#sample-scripts
    
    # Initiate failover
    $database = Get-AzSqlDatabase -DatabaseName $sqlDB -ResourceGroupName $primaryRG -ServerName $primarySql
    $database | Set-AzSqlDatabaseSecondary -PartnerResourceGroupName $secondaryRG -Failover

    # Monitor Geo-Replication config and health after failover
    $database = Get-AzSqlDatabase -DatabaseName $sqlDB -ResourceGroupName $primaryRG -ServerName $primarySql
    $database | Get-AzSqlDatabaseReplicationLink -PartnerResourceGroupName $secondaryRG -PartnerServerName $secondarySql

    
    # ##################
    # FAILOVER COSMOS DB
    # ##################

    # https://docs.microsoft.com/en-us/azure/cosmos-db/scripts/powershell/common/ps-account-failover-priority-update#sample-script
    
    # Regions ordered by UPDATED failover priority
    $locations = @($primaryLoc, $secondaryLoc)  

    # Get existing Cosmos DB account
    $account = Get-AzCosmosDBAccount -ResourceGroupName $rg -Name $cosmos

    # Update account failover priority
    Update-AzCosmosDBAccountFailoverPriority -InputObject $account -FailoverPolicy $locations    
    
    # ########################
    # FAILOVER TRAFFIC MANAGER
    # ########################

    # https://docs.microsoft.com/en-us/azure/traffic-manager/traffic-manager-powershell-arm#example-1-updating-endpoints-using-get-aztrafficmanagerprofile-and-set-aztrafficmanagerprofile

    $tmProfile = Get-AzTrafficManagerProfile -Name $profile -ResourceGroupName $rg
    $tmProfile.Endpoints[0].Priority = $primaryLoc -eq 'aue' ? 1 : 2
    $tmProfile.Endpoints[1].Priority = $primaryLoc -eq 'aue' ? 2 : 1
    Set-AzTrafficManagerProfile -TrafficManagerProfile $tmProfile
    
    # #################
    # UPDATE APP CONFIG
    # #################

    # https://docs.microsoft.com/en-us/azure/azure-app-configuration/scripts/cli-work-with-keys

    $primaryAppConfig = "hellorunbook-$envn-$primaryLoc-config"
    $secondaryAppConfig = "hellorunbook-$envn-$secondaryLoc-config"

    # Change Application Configuration
    az appconfig kv set --name $primaryAppConfig --key 'Primary_Location' --value $primaryLoc
    az appconfig kv set --name $secondaryAppConfig --key 'Primary_Location' --value $primaryLoc

}
