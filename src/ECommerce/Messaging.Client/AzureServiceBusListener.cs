using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Messaging.Abstractions;
using Microsoft.Extensions.Logging;

namespace Messaging.Client
{
	public class AzureServiceBusListener: IMessageListener
	{
		private readonly ServiceBusClient client;
		private readonly ILogger logger;

		public AzureServiceBusListener(string connectionString, ILogger logger)
		{
			client = new ServiceBusClient(connectionString);
			this.logger = logger;
		}

		public async Task ListenAsync<T>(string queue, Func<T, Task> messageHandler)
		{
			ServiceBusProcessor processor = client.CreateProcessor(queue);
			processor.ProcessMessageAsync += async args =>
			{
				T message = JsonSerializer.Deserialize<T>(args.Message.Body.ToString())!;
				await messageHandler(message);
				await args.CompleteMessageAsync(args.Message);
			};

			processor.ProcessErrorAsync += async args =>
			{
				logger.LogError(args.Exception, "Error processing message: {ErrorSource}", args.ErrorSource);
				await Task.CompletedTask;
			};

			await processor.StartProcessingAsync();
		}
	}
}
