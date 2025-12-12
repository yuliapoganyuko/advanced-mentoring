using System.Security.Claims;
using Asp.Versioning;
using AutoMapper;
using CartService.WebApi.BackgroundServices;
using CartService.WebApi.OpenApi;
using Messaging.Abstractions;
using Messaging.Client;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace CartService.WebApi
{
	public class Program
	{
		public static void Main(string[] args)
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
			builder.Services.AddLogging(builder => builder.AddConsole());

			builder.Services.AddApiVersioning(options =>
			{
				options.DefaultApiVersion = new ApiVersion(2);
				options.ApiVersionReader = new UrlSegmentApiVersionReader();
			})
				.AddMvc()
				.AddApiExplorer(options =>
				{
					options.GroupNameFormat = "'v'V";
					options.SubstituteApiVersionInUrl = true;
					options.AssumeDefaultVersionWhenUnspecified = true;
				});

			builder.AddCartServices();

			builder.Services.AddSingleton<IMessageListener, AzureServiceBusListener>(sp =>
			{
				var configuration = sp.GetRequiredService<IConfiguration>();
				var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(AzureServiceBusListener));
				var connectionString = configuration.GetConnectionString("AzureServiceBusConnectionString") ?? throw new InvalidOperationException("Azure Service Bus connection string is not configured.");
				return new AzureServiceBusListener(connectionString, logger);
			});

			builder.Services.AddHostedService<AzureServiceBusListenerService>(sp =>
			{
				var listener = sp.GetRequiredService<IMessageListener>();
				var queue = builder.Configuration["ServiceBus:Queue"] ?? throw new InvalidOperationException("Missing ServiceBus:Queue in configuration.");
				return new AzureServiceBusListenerService(listener, sp, queue);
			});

			builder.Services.AddSwaggerGen();
			builder.Services.ConfigureOptions<ConfigureSwaggerGenOptions>();

			var app = builder.Build();

			app.UseAuthentication();
			app.UseMiddleware<TokenLoggingMiddleware>();
			app.UseAuthorization();

			app.MapControllers();

			// Configure the HTTP request pipeline.
			if (app.Environment.IsDevelopment())
			{
				app.UseSwagger();
				app.UseSwaggerUI(options =>
				{
					var descriptions = app.DescribeApiVersions();
					foreach (var description in descriptions)
					{
						options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json",
							description.GroupName.ToUpperInvariant());
					}
				});
			}

			app.UseHttpsRedirection();

			app.Run();
		}
	}
}
