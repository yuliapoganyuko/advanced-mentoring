using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Messaging.Abstractions;

namespace Messaging.Client
{
	public class AzureServiceBusPublisher : IMessagePublisher
	{
		private readonly ServiceBusClient client;

		public AzureServiceBusPublisher(string connectionString)
		{
			client = new ServiceBusClient(connectionString);
		}

		public async Task PublishAsync<T>(string queue, T message)
		{
			ServiceBusSender sender = client.CreateSender(queue);
			ServiceBusMessage busMessage = new ServiceBusMessage(JsonSerializer.Serialize(message));
			await sender.SendMessageAsync(busMessage);
		}
	}
}
