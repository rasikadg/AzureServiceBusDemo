using Azure.Messaging.ServiceBus;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace AzureServiceBusDemo.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CrmOrganisationController : ControllerBase
    {

        private readonly ILogger<CrmOrganisationController> _logger;
        private readonly ServiceBusClient _client;
        public CrmOrganisationController(ILogger<CrmOrganisationController> logger,
            ServiceBusClient client
            )
        {
            _logger = logger;
            _client = client;
        }

        [HttpGet(Name = "GetCRMOrganisations")]
        public async Task Get()
        {
            var queueConnectionString = "Endpoint=sb://servicebustestrg.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=VIJlllMRywcMrfYk5NusTwIj8S4WyDS9pGKFUQ+rMls=";
            

            var clientOptions = new ServiceBusClientOptions()
            {
                TransportType = ServiceBusTransportType.AmqpWebSockets
            };

            var options = new ServiceBusProcessorOptions
            {
                AutoCompleteMessages = false
            };

            var sbClient = new ServiceBusClient(queueConnectionString, clientOptions);

            // create a processor that we can use to process the messages
            ServiceBusProcessor processor = sbClient.CreateProcessor("newcrmorgs", options);

            try
            {
                // configure the message and error handler to use
                processor.ProcessMessageAsync += MessageHandler;
                processor.ProcessErrorAsync += ErrorHandler;

                // start processing
                await processor.StartProcessingAsync();
            }
             catch(Exception ex)
            {
                throw;
            }
            finally
            {
                await processor.DisposeAsync();
                await sbClient.DisposeAsync();
            }
        }

        [HttpPost]
        public async Task Post(CrmOrganisation org)
        {

            var sender = _client.CreateSender("newcrmorgs");

            var body = JsonSerializer.Serialize(org);

            var message = new ServiceBusMessage(body);

            if(body.Contains("ttl"))
            {
                message.TimeToLive = TimeSpan.FromSeconds(5);
            }

            await sender.SendMessageAsync(message);
        }

        private async Task MessageHandler(ProcessMessageEventArgs args)
        {
            string body = args.Message.Body.ToString();
            System.Diagnostics.Trace.WriteLine(body);
            await args.CompleteMessageAsync(args.Message);
        }

        private Task ErrorHandler(ProcessErrorEventArgs args)
        {
            System.Diagnostics.Trace.WriteLine(args.ErrorSource);
            System.Diagnostics.Trace.WriteLine(args.FullyQualifiedNamespace);

            System.Diagnostics.Trace.WriteLine(args.EntityPath);
            System.Diagnostics.Trace.WriteLine(args.Exception.ToString());

            _logger.LogError(args.Exception.ToString());

            return Task.CompletedTask;
        }
    }
}