using CatalogService.Core;
using CatalogService.Infrastructure;

namespace CatalogService.WebApi
{
	public class Program
	{
		public static async Task Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);

			// Add services to the container.

			builder.Services.AddControllers();

			builder.AddCoreServices();
			builder.AddInfrastructureServices();

			// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
			builder.Services.AddOpenApi();
			WebApplication? app = builder.Build();

			// Configure the HTTP request pipeline.
			if (app.Environment.IsDevelopment())
			{
				using var scope = app.Services.CreateScope();
				var initialiser = scope.ServiceProvider.GetRequiredService<CatalogContextInitializer>();
				await initialiser.InitialiseAsync();
				app.MapOpenApi();
			}

			app.UseHttpsRedirection();

			app.UseAuthorization();


			app.MapControllers();

			app.Run();
		}
	}
}
