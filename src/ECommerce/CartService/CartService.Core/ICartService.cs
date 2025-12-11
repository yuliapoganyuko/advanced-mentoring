using CartService.CartService.Core.DTOs;

namespace CartService.Core
{
	public interface ICartService
	{
		Task<IEnumerable<CartItemDto>?> GetCartItemsAsync(Guid cartId);
		Task AddCartItemAsync(Guid cartId, CartItemDto item);
		Task UpdateItemsOnProductChangedAsync(ProductChangedDto productChanged);
		Task<bool> RemoveCartItemAsync(Guid cartId, int itemId);
	}
}
