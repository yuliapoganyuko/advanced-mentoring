using System.Reflection;
using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CartService.WebApi.OpenApi
{
	public class ConfigureSwaggerGenOptions: IConfigureNamedOptions<SwaggerGenOptions>
	{
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
		}

		public void Configure(string name, SwaggerGenOptions options)
		{
			Configure(options);
		}
	}
}
