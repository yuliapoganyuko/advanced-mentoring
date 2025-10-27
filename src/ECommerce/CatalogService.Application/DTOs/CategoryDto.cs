namespace CatalogService.Core
{
	public class CategoryDto
	{
		public int Id { get; set; }
		public string Name { get; set; } = string.Empty;
		public string? Image { get; set; }
		public int? ParentCategoryId { get; set; }
	}
}
