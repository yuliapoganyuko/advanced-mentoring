using CatalogService.Core.Interfaces;
using CatalogService.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CatalogService.Infrastructure
{
	public static class DependencyInjector
	{
		public static void AddInfrastructureServices(this IHostApplicationBuilder builder)
		{
			builder.Services.AddDbContext<CatalogContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("CatalogDbConnectionString")));

			builder.Services.AddTransient<ICategoryRepository, CategoryRepository>();
			builder.Services.AddTransient<IProductRepository, ProductRepository>();

			builder.Services.AddScoped<CatalogContextInitializer>();
		}
	}
}
