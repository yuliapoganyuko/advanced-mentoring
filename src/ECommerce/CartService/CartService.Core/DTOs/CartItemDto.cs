namespace CartService.Core
{
	public class CartItemDto
	{
		public int Id { get; set; }
		public string Name { get; set; } = string.Empty;
		public string? ImageUrl { get; set; }
		public string? ImageAltText { get; set; }
		public decimal Price { get; set; }
		public uint Quantity {  get; set; }
	}
}
