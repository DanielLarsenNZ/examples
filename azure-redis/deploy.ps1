# https://docs.microsoft.com/en-us/azure/azure-cache-for-redis/cache-how-to-premium-clustering

$rg = 'helloredis-rg'
$location = 'australiaeast'
$loc = 'aue'
$redis = "hello-$loc-redis"
$redis2 = "hello2-$loc-redis"
$tags = @{ 
    expires = [DateTime]::UtcNow.AddDays(7); 
    owner = "dalars"; 
    project = "DanielLarsenNZ/examples/azure-redis" 
}

$ErrorActionPreference = 'Stop'

New-AzResourceGroup -Name $rg -Location $location -Tag $tags -Verbose -Force

# https://docs.microsoft.com/en-us/powershell/module/az.rediscache/New-AzRedisCache?view=azps-3.6.1
#New-AzRedisCache -ResourceGroupName $rg -Name $redis -Location $location -Size 'C4' -Sku 'Standard' -Verbose

Get-AzRedisCache -ResourceGroupName $rg -Name $redis

Get-AzRedisCacheKey -ResourceGroupName $rg -Name $redis


#New-AzRedisCache -ResourceGroupName $rg -Name $redis2 -Location $location -Size 'P1' -Sku 'Premium' -Verbose

Get-AzRedisCache -ResourceGroupName $rg -Name $redis2

Get-AzRedisCacheKey -ResourceGroupName $rg -Name $redis2

