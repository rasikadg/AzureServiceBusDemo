using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace FunctionApp1
{
    public class Function1
    {
        [FunctionName("Function1")]
        public void Run([ServiceBusTrigger("newcrmorgs", Connection = "QueueConnectionString")]string myQueueItem, ILogger log)
        {
            if(myQueueItem.Contains("Error"))
            {
                throw new Exception();
            }

            log.LogInformation($"C# ServiceBus queue trigger function processed message: {myQueueItem}");
        }
    }
}
