using MassTransit;
using MassTransit.AzureServiceBusTransport;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using System;
using System.Configuration;
using System.Threading.Tasks;

namespace ConsoleApp3
{
    class Program
    {
        static void Main(string[] args)
        {
            MainAsync().GetAwaiter().GetResult();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        static async Task MainAsync()
        {
            var bus = Bus.Factory.CreateUsingAzureServiceBus(cfg =>
            {
                var sbName = ConfigurationManager.AppSettings["AzureSbNamespace"];
                var sbKeyName = ConfigurationManager.AppSettings["AzureSbKeyName"];
                var sbSharedAccessKey = ConfigurationManager.AppSettings["AzureSbSharedAccessKey"];

                var serviceUri = new Uri($"sb://{sbName}.servicebus.windows.net/");

                IServiceBusHost host = cfg.Host(serviceUri, h =>
                {
                    h.TransportType = TransportType.NetMessaging;
                    h.OperationTimeout = TimeSpan.FromSeconds(10);
                    h.TokenProvider = TokenProvider.CreateSharedAccessSignatureTokenProvider(sbKeyName, sbSharedAccessKey, TimeSpan.FromDays(1), TokenScope.Namespace);
                });


                cfg.ReceiveEndpoint(host, "testqueue", ec =>
                {
                    ec.Consumer<MyMessageConsumer>();
                });
            });

            bus.Start();

            var i = 10;
            while (i > 0)
            {
                await bus.Publish(new MyMessage() { Text = "Hello World!" });
                i--;
            }
        }
    }

    public class MyMessageConsumer : IConsumer<MyMessage>
    {
        public Task Consume(ConsumeContext<MyMessage> context)
        {
            return Console.Out.WriteLineAsync(context.Message.Text);
        }
    }

    public class MyMessage
    {
        public string Text { get; set; }
    }

}
