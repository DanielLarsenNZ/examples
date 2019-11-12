$location = 'australiaeast'
$loc = 'aue'
$rg = 'hellomessaging-rg'
$tags = 'project=hello-messaging'
$plan = "hellomessaging-$loc-plan"
$webjobApp = "hellomessaging-$loc-app"
$webjobsStorage = "hellomessaging$loc"
$functionApp = "hellomessaging-$loc-fn"
$dataStorage = "hellomessagingdata$loc"
$container = 'data'
$insights = 'hellomessaging-insights'
$eventhubNamespace = 'hellomessaging-hub'
$eventhubs = 'transactions1', 'transactions2', 'numbers', 'numbers-batched'
$eventhubAuthRule = 'SenderListener1'
$servicebusNamespace = 'pipeline-bus'
$queues = 'test1', 'test2'
$servicebusAuthRule = 'SenderReceiver1'

# Consider these settings for scale
$planSku = 'B1'         # Scale up
$planInstances = 1      # Scale out
$eventhubsSku = 'Basic'
$eventhubsRetentionDays = 1
$eventhubsPartitions = 12    # 2 - 32. Cannot be changed after deployment. Good discussion here: https://medium.com/@iizotov/azure-functions-and-event-hubs-optimising-for-throughput-549c7acd2b75
$servicebusSku = 'Basic'
