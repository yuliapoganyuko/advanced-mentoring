using LiteDB;

namespace CartService.Infrastructure
{
	public class CartRepository : ICartRepository
	{
		ILiteDatabase liteDatabase;
		ILiteCollection<Cart> cartCollection;

		public CartRepository(ILiteDatabase liteDatabase)
		{
			this.liteDatabase = liteDatabase;
			this.cartCollection = liteDatabase.GetCollection<Cart>();
		}

		public void AddCartItem(Guid cartId, CartItem item)
		{
			Cart cart = cartCollection.FindById(cartId);
			if (cart == null)
			{
				cartCollection.Insert(new Cart
				{
					Id = cartId,
					Items = new List<CartItem> { item }
				});
			}
			else
			{
				cart.Items.Add(item);
				cartCollection.Update(cart);
			}
			liteDatabase.Commit();
		}

		public IEnumerable<CartItem>? GetCartItems(Guid cartId)
		{
			Cart cart = cartCollection.FindById(cartId);
			return cart?.Items;
		}

		public bool RemoveCartItem(Guid cartId, int itemId)
		{
			bool result = false;
			Cart cart = cartCollection.FindById(cartId);
			CartItem? item = cart?.Items.FirstOrDefault(i => i.Id == itemId);
			if (item != null && cart != null)
			{
				cart.Items.Remove(item);
				cartCollection.Update(cart);
				result = liteDatabase.Commit();
			}
			return result;
		}
	}
}
