using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace functionapp01
{
    public static class functionapp01
    {
        [FunctionName("functionapp01")]
        public static void Run([QueueTrigger("az-strorage-queue01", Connection = "azstorageaccnt01_connection")]string myQueueItem, ILogger log)
        {
            log.LogInformation($"C# Queue trigger function processed: {myQueueItem}");
        }
    }
}
