namespace CartService.Infrastructure
{
	public interface ICartRepository
	{
		IEnumerable<CartItem>? GetCartItems(Guid cartId);
		void AddCartItem(Guid cartId, CartItem item);
		bool RemoveCartItem(Guid cartId, int itemId);
	}
}