using AutoMapper;
using CartService.Infrastructure;

namespace CartService.Core
{
	public class CartService : ICartService
	{
		ICartRepository cartRepository;
		IMapper mapper;

		public CartService(ICartRepository cartRepository, IMapper mapper)
		{
			this.cartRepository = cartRepository;
			this.mapper = mapper;
		}

		public async Task AddCartItemAsync(Guid cartId, CartItemDto item)
		{
			if (cartId == Guid.Empty)
				throw new ArgumentException(nameof(cartId));
			if (item is null)
				throw new ArgumentNullException(nameof(item));
			await cartRepository.AddCartItemAsync(cartId, mapper.Map<CartItem>(item));
		}

		public async Task<IEnumerable<CartItemDto>?> GetCartItemsAsync(Guid cartId)
		{
			if (cartId == Guid.Empty)
				throw new ArgumentException(nameof(cartId));
			IEnumerable<CartItem>? cartItems = await cartRepository.GetCartItemsAsync(cartId);
			if (cartItems is null)
				return null;
			return mapper.Map<IEnumerable<CartItemDto>>(cartItems);
		}

		public async Task<bool> RemoveCartItemAsync(Guid cartId, int itemId)
		{
			if (cartId == Guid.Empty)
				throw new ArgumentException(nameof(cartId));
			if (itemId <= 0)
				throw new ArgumentOutOfRangeException(nameof(itemId));
			return await cartRepository.RemoveCartItemAsync(cartId, itemId);
		}
	}
}
