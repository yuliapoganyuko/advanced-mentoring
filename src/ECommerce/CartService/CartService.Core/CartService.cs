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

		public void AddCartItem(Guid cartId, CartItemDto item)
		{
			if (cartId == Guid.Empty)
				throw new ArgumentException(nameof(cartId));
			if (item is null)
				throw new ArgumentNullException(nameof(item));
			cartRepository.AddCartItem(cartId, mapper.Map<CartItem>(item));
			return;
		}

		public IEnumerable<CartItemDto>? GetCartItems(Guid cartId)
		{
			if (cartId == Guid.Empty)
				throw new ArgumentException(nameof(cartId));
			IEnumerable<CartItem>? cartItems = cartRepository.GetCartItems(cartId);
			if (cartItems is null)
				return null;
			return mapper.Map<IEnumerable<CartItemDto>>(cartItems);
		}

		public bool RemoveCartItem(Guid cartId, int itemId)
		{
			if (cartId == Guid.Empty)
				throw new ArgumentException(nameof(cartId));
			if (itemId <= 0)
				throw new ArgumentOutOfRangeException(nameof(itemId));
			return cartRepository.RemoveCartItem(cartId, itemId);
		}
	}
}
