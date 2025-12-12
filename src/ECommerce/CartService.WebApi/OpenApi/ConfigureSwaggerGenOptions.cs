using System.Reflection;
using Asp.Versioning.ApiExplorer;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CartService.WebApi.OpenApi
{
	public class ConfigureSwaggerGenOptions: IConfigureNamedOptions<SwaggerGenOptions>
	{
		private const string authScheme = JwtBearerDefaults.AuthenticationScheme;
		private readonly IApiVersionDescriptionProvider provider;

		public ConfigureSwaggerGenOptions(IApiVersionDescriptionProvider provider)
		{
			this.provider = provider;
		}

		public void Configure(SwaggerGenOptions options) 
		{
			foreach (ApiVersionDescription description in provider.ApiVersionDescriptions)
			{
				options.SwaggerDoc(description.GroupName, new OpenApiInfo
				{
					Title = $"Cart Service API v{description.ApiVersion}",
					Version = description.ApiVersion.ToString()
				});

				var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
				options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
			}
			
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
		}

		public void Configure(string name, SwaggerGenOptions options)
		{
			Configure(options);
		}
	}
}
