namespace CartService.Core
{
	public class CartDto
	{
		public Guid Id { get; set; }
		public ICollection<CartItemDto> Items { get; set; } = new List<CartItemDto>();
	}
}
