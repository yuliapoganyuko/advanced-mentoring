using System.Reflection;
using System.Security.Claims;
using CatalogService.Infrastructure;
using Messaging.Abstractions;
using Messaging.Client;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace CatalogService.WebApi
{
	public class Program
	{
		private const string authScheme = JwtBearerDefaults.AuthenticationScheme;

		public static async Task Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);

			// Add services to the container.
			
			builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
				.AddJwtBearer(options =>
				{
					options.Authority = $"https://{builder.Configuration["Auth0:Domain"]}/";
					options.Audience = builder.Configuration["Auth0:Audience"];
					options.TokenValidationParameters = new TokenValidationParameters
					{
						ValidateAudience = false,
						RoleClaimType = ClaimTypes.Role
					};
				});

			builder.Services.AddAuthorization();
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
			builder.Services.AddSwaggerGen(options =>
			{
				options.SwaggerDoc("v1", new OpenApiInfo
				{
					Title = "Catalog Service API",
					Version = "v1"
				});

				var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
				options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
				
				options.AddSecurityDefinition(authScheme, new OpenApiSecurityScheme
				{
					Description = "JWT access token",
					Name = "Authorization",
					In = ParameterLocation.Header,
					Type = SecuritySchemeType.Http,
					Scheme = authScheme,
					BearerFormat = "JWT"
				});

				var bearerRequirement = new OpenApiSecurityRequirement
				{
					{
						new OpenApiSecurityScheme
						{
							Reference = new OpenApiReference
							{
								Type = ReferenceType.SecurityScheme,
								Id = authScheme
							}
						},
						Array.Empty<string>()
					}
				};
				options.AddSecurityRequirement(bearerRequirement);
			});

			WebApplication? app = builder.Build();
			
			app.UseAuthentication();
			app.UseAuthorization();

			// Configure the HTTP request pipeline.
			if (app.Environment.IsDevelopment())
			{
				app.UseSwagger();
				app.UseSwaggerUI();

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
