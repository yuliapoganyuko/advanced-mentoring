using CartService.CartService.Core.DTOs;
using CartService.Core;
using Messaging.Abstractions;

namespace CartService.WebApi.BackgroundServices
{
	public class AzureServiceBusListenerService : BackgroundService
	{
		private IMessageListener messageListener;
		private IServiceProvider serviceProvider;
		private readonly string queue;

		public AzureServiceBusListenerService(IMessageListener messageListener, IServiceProvider serviceProvider, string queue) 
		{
			this.messageListener = messageListener;
			this.serviceProvider = serviceProvider;
			this.queue = queue;
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			await messageListener.ListenAsync<ProductChangedDto>(queue, async (message) => 
			{
				using var scope = serviceProvider.CreateScope();
				var cartService = scope.ServiceProvider.GetRequiredService<ICartService>();
				await cartService.UpdateItemsOnProductChangedAsync(message);
			});

			// Keep running until cancellation
			await Task.Delay(Timeout.Infinite, stoppingToken);
		}
	}
}
