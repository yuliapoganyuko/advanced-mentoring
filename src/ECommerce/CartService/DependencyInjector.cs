using AutoMapper;
using CartService.Core;
using CartService.Infrastructure;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CartService
{
	public static class DependencyInjector
	{
		public static void AddCartServices(this IHostApplicationBuilder builder)
		{
			builder.Services.AddSingleton<CosmosClient>(sp => new CosmosClient(builder.Configuration.GetConnectionString("CosmosConnectionString")));

			builder.Services.AddTransient<ICartRepository, CartRepository>(sp =>
			{
				var client = sp.GetRequiredService<CosmosClient>();
				var databaseId = builder.Configuration["Cosmos:DatabaseId"] ?? throw new InvalidOperationException("Missing Cosmos:DatabaseId in configuration.");
				var containerId = builder.Configuration["Cosmos:ContainerId"] ?? throw new InvalidOperationException("Missing Cosmos:ContainerId in configuration.");
				return new CartRepository(client, databaseId, containerId);
			});

			builder.Services.AddTransient<ICartService, Core.CartService>();

			var loggerFactory = builder.Services.BuildServiceProvider().GetService<ILoggerFactory>();
			var mapperConfiguration = new MapperConfiguration(cfg =>
			{
				cfg.CreateMap<CartDto, Cart>();
				cfg.CreateMap<CartItemDto, CartItem>().ReverseMap();
			}, loggerFactory);
			builder.Services.AddSingleton<IMapper>(m => mapperConfiguration.CreateMapper());
		}
	}
}