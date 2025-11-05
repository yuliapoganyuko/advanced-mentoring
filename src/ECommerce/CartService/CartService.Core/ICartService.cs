namespace CartService.Core
{
	public interface ICartService
	{
		IEnumerable<CartItemDto>? GetCartItems(Guid cartId);
		void AddCartItem(Guid cartId, CartItemDto item);
		bool RemoveCartItem(Guid cartId, int itemId);
	}
}
