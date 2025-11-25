namespace CartService.Infrastructure
{
	public interface ICartRepository
	{
		Task<IEnumerable<CartItem>?> GetCartItemsAsync(Guid cartId);
		Task AddCartItemAsync(Guid cartId, CartItem item);
		Task<bool> RemoveCartItemAsync(Guid cartId, int itemId);
	}
}