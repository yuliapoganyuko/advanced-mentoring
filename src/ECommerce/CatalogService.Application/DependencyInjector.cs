using AutoMapper;
using CatalogService.Core;
using CatalogService.Core.Interfaces;
using CatalogService.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CatalogService.Infrastructure
{
	public static class DependencyInjector
	{
		public static void AddCoreServices(this IHostApplicationBuilder builder)
		{
			builder.Services.AddTransient<ICategoryService, CategoryService>();
			builder.Services.AddTransient<IProductService, ProductService>();

			builder.Services.AddLogging(builder => builder.AddConsole());

			var loggerFactory = builder.Services.BuildServiceProvider().GetService<ILoggerFactory>();
			var mapperConfiguration = new MapperConfiguration(cfg =>
			{
				cfg.CreateMap<CategoryDto, Category>().ReverseMap();
				cfg.CreateMap<ProductDto, Product>().ReverseMap();
			}, loggerFactory);
			builder.Services.AddSingleton<IMapper>(m => mapperConfiguration.CreateMapper());
		}
	}
}
