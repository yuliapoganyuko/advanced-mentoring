namespace CartService.Core
{
	public interface ICartService
	{
		Task<IEnumerable<CartItemDto>?> GetCartItemsAsync(Guid cartId);
		Task AddCartItemAsync(Guid cartId, CartItemDto item);
		Task<bool> RemoveCartItemAsync(Guid cartId, int itemId);
	}
}
