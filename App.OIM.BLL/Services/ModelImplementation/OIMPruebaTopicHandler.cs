using Microsoft.Azure.ServiceBus;
using Azure.Messaging.ServiceBus;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System;
using System.Threading;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using System.Runtime;
using APP.OIM.Example.Controllers;

namespace MCIM.Example.App.OIM.BLL.Services.ModelImplementation
{

    public class OIMPruebaTopicHandlerImpl : BackgroundService
    {
        private readonly IConfiguration _config;
        static IQueueClient queueClient;
        static ServiceBusClient client;
        private string connectionString = "Endpoint=sb://pruebadeconceptoecomdemo.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=Ijn8kXfS+I2RqcSneEYH9rrDCDiw4nQ3qWzhq6+ALYU=";
        private string queueName = "modificacionespim";
        private readonly ILogger<OIMPruebaTopicHandlerImpl> _logger;


        public OIMPruebaTopicHandlerImpl(IConfiguration config, ILogger<OIMPruebaTopicHandlerImpl> logger)
        {
            _config = config;
            var _logger = logger;
        }

        public async Task Handle(Message message, CancellationToken cancelToken)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            var body = Encoding.UTF8.GetString(message.Body);
            Console.WriteLine(body);
            await queueClient.CompleteAsync(message.SystemProperties.LockToken).ConfigureAwait(false);
        }

        public virtual Task HandleFailureMessage(ExceptionReceivedEventArgs exceptionReceivedEventArgs)
        {
            if (exceptionReceivedEventArgs == null)
                throw new ArgumentNullException(nameof(exceptionReceivedEventArgs));
            return Task.CompletedTask;
        }

        //Esta funcion se ejecuta primero
        protected override async Task<Task> ExecuteAsync(CancellationToken stoppingToken)
        {

        queueClient = new QueueClient(connectionString, queueName);

            var messageHandlerOptions = new MessageHandlerOptions(HandleFailureMessage)
            {
                MaxConcurrentCalls = 1,
                AutoComplete = false,
            };

            queueClient.RegisterMessageHandler(ProcessMessagesAsync, messageHandlerOptions);
            //Console.ReadLine();

            await queueClient.CloseAsync();


            return Task.CompletedTask;
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            Console.WriteLine($"{nameof(OIMPruebaTopicHandlerImpl)} service has stopped.");
            await queueClient.CloseAsync().ConfigureAwait(false);
        }

        private static async Task ProcessMessagesAsync(Message message, CancellationToken token)
        {
            var jsonString = Encoding.UTF8.GetString(message.Body);
            Console.WriteLine($"Message Received: {jsonString}");
            
            await queueClient.CompleteAsync(message.SystemProperties.LockToken);
        }

    }
}



