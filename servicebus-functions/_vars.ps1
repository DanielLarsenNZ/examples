# Load messaging vars
. ../webjobs-eventhubs/_vars.ps1

$functionApp = "hellomessagingsb-$loc-fn"
$queues = 'queue1', 'queue2', 'queue3'
