namespace CartService.Infrastructure
{
	public interface ICartRepository
	{
		Task<IEnumerable<CartItem>?> GetCartItemsAsync(Guid cartId);
		Task AddCartItemAsync(Guid cartId, CartItem item);
		Task UpdateItemsOnProductChangedAsync(int productId, string productName, string? productImageUrl, decimal productPrice);
		Task<bool> RemoveCartItemAsync(Guid cartId, int itemId);
	}
}