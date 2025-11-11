using AutoMapper;
using CartService.Core;
using CartService.Infrastructure;
using LiteDB;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;

namespace CartService
{
	public static class DependencyInjector
	{
		public static void AddCartServices(this IHostApplicationBuilder builder)
		{
			builder.Services.AddSingleton<ILiteDatabase, LiteDatabase>(sp => new LiteDatabase(builder.Configuration.GetConnectionString("LiteDbConnectionString")));

			builder.Services.AddTransient<ICartRepository, CartRepository>();
			builder.Services.AddTransient<ICartService, CartService.Core.CartService>();

			builder.Services.AddLogging(builder => builder.AddConsole());

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