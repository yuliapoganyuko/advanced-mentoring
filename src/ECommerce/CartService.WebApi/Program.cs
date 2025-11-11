using Asp.Versioning;
using CartService.WebApi.OpenApi;

namespace CartService.WebApi
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);

			// Add services to the container.

			builder.Services.AddControllers();

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
				}
				);

			builder.AddCartServices();

			builder.Services.AddSwaggerGen();
			builder.Services.ConfigureOptions<ConfigureSwaggerGenOptions>();

			var app = builder.Build();
			
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
