namespace CatalogService.Core.DTOs
{
	public class ProductChangedDto
	{
		public int Id { get; set; }
		public string Name { get; set; } = string.Empty;
		public string? ImageUrl { get; set; }
		public decimal Price { get; set; }
	}
}