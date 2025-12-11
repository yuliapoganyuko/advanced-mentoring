using CatalogService.Infrastructure;
using Messaging.Abstractions;
using Messaging.Client;

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
			builder.Services.AddSingleton<IMessagePublisher, AzureServiceBusPublisher>(sp =>
			{
				var configuration = sp.GetRequiredService<IConfiguration>();
				var connectionString = configuration.GetConnectionString("AzureServiceBusConnectionString") ?? throw new InvalidOperationException("Azure Service Bus connection string is not configured.");
				return new AzureServiceBusPublisher(connectionString);
			});

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
