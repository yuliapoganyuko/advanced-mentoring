namespace CatalogService.Infrastructure
{
	public class CatalogContextInitializer
	{
		private readonly CatalogContext context;

		public CatalogContextInitializer(CatalogContext context)
		{
			this.context = context;
		}

		public async Task InitialiseAsync()
		{
			await context.Database.EnsureCreatedAsync();
		}
	}
}
