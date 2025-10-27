using System.Reflection;
using CartService.Infrastructure;
using LiteDB;

namespace CartService.Tests
{
	[TestClass]
	public sealed class CartRepositoryIntegrationTests
	{
		private ConstructorInfo GetNonPublicCtor()
		{
			var ctor = typeof(CartRepository).GetConstructor(
				BindingFlags.Instance | BindingFlags.NonPublic,
				null,
				new Type[] { typeof(ILiteDatabase) },
				null);

			Assert.IsNotNull(ctor, "Could not find non-public constructor for CartRepository.");
			return ctor!;
		}

		private CartRepository CreateRepository(ILiteDatabase db)
		{
			var ctor = GetNonPublicCtor();
			return (CartRepository)ctor.Invoke(new object[] { db });
		}

		private string CreateTempDatabasePath() =>
			Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".db");

		[TestMethod]
		public void AddCartItem_PersistsItemToDatabase()
		{
			var dbPath = CreateTempDatabasePath();
			try
			{
				using (var db = new LiteDatabase(dbPath))
				{
					var col = db.GetCollection<Cart>();
					var cartId = Guid.NewGuid();
					var cart = new Cart { Id = cartId, Items = new System.Collections.Generic.List<CartItem>() };
					col.Insert(cart);

					var repo = CreateRepository(db);
					var item = new CartItem { Id = 100, Name = "IntegrationItem", Price = 9.99m, Quantity = 1 };

					repo.AddCartItem(cartId, item);
				}

				// Open a fresh DB instance to ensure persistence
				using (var verifyDb = new LiteDatabase(dbPath))
				{
					var verifyCol = verifyDb.GetCollection<Cart>();
					var stored = verifyCol.FindById(new BsonValue(Guid.Empty)); // default call to avoid overload ambiguity
					// above line just to ensure BsonValue type available; we will fetch by Guid directly below

					// Find the single cart we inserted
					var cart = verifyCol.FindAll().FirstOrDefault();
					Assert.IsNotNull(cart, "Cart should exist in the database after AddCartItem.");
					Assert.IsTrue(cart.Items.Any(i => i.Id == 100 && i.Name == "IntegrationItem"),
						"Added item should be persisted in the cart stored in DB.");
				}
			}
			finally
			{
				if (File.Exists(dbPath)) File.Delete(dbPath);
			}
		}

		[TestMethod]
		public void GetCartItems_ReturnsItemsFromDatabase()
		{
			var dbPath = CreateTempDatabasePath();
			try
			{
				var cartId = Guid.NewGuid();
				using (var db = new LiteDatabase(dbPath))
				{
					var col = db.GetCollection<Cart>();
					var items = new System.Collections.Generic.List<CartItem>
					{
						new CartItem { Id = 1, Name = "A", Price = 1m, Quantity = 1 },
						new CartItem { Id = 2, Name = "B", Price = 2m, Quantity = 2 }
					};
					var cart = new Cart { Id = cartId, Items = items };
					col.Insert(cart);

					var repo = CreateRepository(db);
					var result = repo.GetCartItems(cartId)?.ToList();
					
					Assert.IsNotNull(result);
					Assert.AreEqual(2, result.Count);
					Assert.IsTrue(result.Any(i => i.Id == 1 && i.Name == "A"));
					Assert.IsTrue(result.Any(i => i.Id == 2 && i.Name == "B"));
				}
			}
			finally
			{
				if (File.Exists(dbPath)) File.Delete(dbPath);
			}
		}

		[TestMethod]
		public void RemoveCartItem_RemovesItemFromDatabase()
		{
			var dbPath = CreateTempDatabasePath();
			try
			{
				var cartId = Guid.NewGuid();
				using (var db = new LiteDatabase(dbPath))
				{
					var col = db.GetCollection<Cart>();
					var item = new CartItem { Id = 42, Name = "ToRemove", Price = 5m, Quantity = 1 };
					var cart = new Cart { Id = cartId, Items = new System.Collections.Generic.List<CartItem> { item } };
					col.Insert(cart);

					var repo = CreateRepository(db);
					repo.RemoveCartItem(cartId, 42);
				}

				using (var verifyDb = new LiteDatabase(dbPath))
				{
					var verifyCol = verifyDb.GetCollection<Cart>();
					var storedCart = verifyCol.FindAll().FirstOrDefault();
					Assert.IsNotNull(storedCart, "Cart should still exist after RemoveCartItem.");
					Assert.IsFalse(storedCart.Items.Any(i => i.Id == 42), "Item should be removed and persisted.");
				}
			}
			finally
			{
				if (File.Exists(dbPath)) File.Delete(dbPath);
			}
		}

		[TestMethod]
		public void AddCartItem_WhenCartDoesNotExist_DoesNotCreateCart()
		{
			var dbPath = CreateTempDatabasePath();
			try
			{
				var nonExistingCartId = Guid.NewGuid();
				using (var db = new LiteDatabase(dbPath))
				{
					var repo = CreateRepository(db);
					var item = new CartItem { Id = 11, Name = "NoCart", Price = 2m, Quantity = 1 };

					repo.AddCartItem(nonExistingCartId, item);

					var col = db.GetCollection<Cart>();
					Assert.IsFalse(col.FindAll().Any(), "No cart should be created when AddCartItem is called for a missing cart.");
				}
			}
			finally
			{
				if (File.Exists(dbPath)) File.Delete(dbPath);
			}
		}

		[TestMethod]
		public void GetCartItems_WhenCartDoesNotExist_ReturnsNull()
		{
			var dbPath = CreateTempDatabasePath();
			try
			{
				using (var db = new LiteDatabase(dbPath))
				{
					var repo = CreateRepository(db);
					var items = repo.GetCartItems(Guid.NewGuid());
					Assert.IsNull(items);
				}
			}
			finally
			{
				if (File.Exists(dbPath)) File.Delete(dbPath);
			}
		}

		[TestMethod]
		public void RemoveCartItem_WhenCartDoesNotExist_NoOp()
		{
			var dbPath = CreateTempDatabasePath();
			try
			{
				using (var db = new LiteDatabase(dbPath))
				{
					var repo = CreateRepository(db);
					repo.RemoveCartItem(Guid.NewGuid(), 1);

					var col = db.GetCollection<Cart>();
					Assert.IsFalse(col.FindAll().Any(), "No cart should exist or be created when RemoveCartItem is called for a missing cart.");
				}
			}
			finally
			{
				if (File.Exists(dbPath)) File.Delete(dbPath);
			}
		}
	}
}