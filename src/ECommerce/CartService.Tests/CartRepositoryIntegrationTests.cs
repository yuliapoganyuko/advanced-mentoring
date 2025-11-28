using System;
using System.Linq;
using System.Threading.Tasks;
using CartService.Infrastructure;
using Microsoft.Azure.Cosmos;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CartService.Tests
{
	[TestClass]
	public sealed class CartRepositoryIntegrationTests
	{
		private CartRepository CreateRepository(CosmosClient client, string databaseId, string containerId)
		{
			return new CartRepository(client, databaseId, containerId);
		}

		private string CreateDatabaseName() => "CartTestsDb_" + Guid.NewGuid().ToString("N");
		private string CreateContainerName() => "Carts_" + Guid.NewGuid().ToString("N");

		[TestMethod]
		public async Task AddCartItem_PersistsItemToDatabase()
		{
			var connectionString = Environment.GetEnvironmentVariable("COSMOS_CONNECTION_STRING")
				?? throw new InvalidOperationException("Set COSMOS_CONNECTION_STRING environment variable for integration tests.");
			using var client = new CosmosClient(connectionString);

			var databaseId = CreateDatabaseName();
			var containerId = CreateContainerName();

			try
			{
				await client.CreateDatabaseIfNotExistsAsync(databaseId);
				await client.GetDatabase(databaseId).CreateContainerIfNotExistsAsync(new ContainerProperties(containerId, "/id"));

				var container = client.GetContainer(databaseId, containerId);

				var cartId = Guid.NewGuid();
				var cart = new Cart { Id = cartId, Items = new System.Collections.Generic.List<CartItem>() };
				await container.CreateItemAsync(cart, new PartitionKey(cart.IdString));

				var repo = CreateRepository(client, databaseId, containerId);
				var item = new CartItem { Id = 100, Name = "IntegrationItem", Price = 9.99m, Quantity = 1 };

				await repo.AddCartItemAsync(cartId, item);

				// Read back via query to ensure persistence
				var query = new QueryDefinition("SELECT * FROM c WHERE c.id = @id").WithParameter("@id", cartId);
				var iterator = container.GetItemQueryIterator<Cart>(query);
				var feed = await iterator.ReadNextAsync();
				var storedCart = feed.Resource.FirstOrDefault();

				Assert.IsNotNull(storedCart, "Cart should exist in the database after AddCartItem.");
				Assert.IsTrue(storedCart.Items.Any(i => i.Id == 100 && i.Name == "IntegrationItem"),
					"Added item should be persisted in the cart stored in DB.");
			}
			finally
			{
				// cleanup database
				await client.GetDatabase(databaseId).DeleteAsync();
			}
		}

		[TestMethod]
		public async Task GetCartItems_ReturnsItemsFromDatabase()
		{
			var connectionString = Environment.GetEnvironmentVariable("COSMOS_CONNECTION_STRING")
				?? throw new InvalidOperationException("Set COSMOS_CONNECTION_STRING environment variable for integration tests.");
			using var client = new CosmosClient(connectionString);

			var databaseId = CreateDatabaseName();
			var containerId = CreateContainerName();

			try
			{
				await client.CreateDatabaseIfNotExistsAsync(databaseId);
				await client.GetDatabase(databaseId).CreateContainerIfNotExistsAsync(new ContainerProperties(containerId, "/id"));

				var container = client.GetContainer(databaseId, containerId);

				var cartId = Guid.NewGuid();
				var items = new System.Collections.Generic.List<CartItem>
				{
					new CartItem { Id = 1, Name = "A", Price = 1m, Quantity = 1 },
					new CartItem { Id = 2, Name = "B", Price = 2m, Quantity = 2 }
				};
				var cart = new Cart { Id = cartId, Items = items };
				await container.CreateItemAsync(cart, new PartitionKey(cartId.ToString()));

				var repo = CreateRepository(client, databaseId, containerId);
				var result = (await repo.GetCartItemsAsync(cartId))?.ToList();

				Assert.IsNotNull(result);
				Assert.AreEqual(2, result.Count);
				Assert.IsTrue(result.Any(i => i.Id == 1 && i.Name == "A"));
				Assert.IsTrue(result.Any(i => i.Id == 2 && i.Name == "B"));
			}
			finally
			{
				await client.GetDatabase(databaseId).DeleteAsync();
			}
		}

		[TestMethod]
		public async Task RemoveCartItem_RemovesItemFromDatabase()
		{
			var connectionString = Environment.GetEnvironmentVariable("COSMOS_CONNECTION_STRING")
				?? throw new InvalidOperationException("Set COSMOS_CONNECTION_STRING environment variable for integration tests.");
			using var client = new CosmosClient(connectionString);

			var databaseId = CreateDatabaseName();
			var containerId = CreateContainerName();

			try
			{
				await client.CreateDatabaseIfNotExistsAsync(databaseId);
				await client.GetDatabase(databaseId).CreateContainerIfNotExistsAsync(new ContainerProperties(containerId, "/id"));

				var container = client.GetContainer(databaseId, containerId);

				var cartId = Guid.NewGuid();
				var item = new CartItem { Id = 42, Name = "ToRemove", Price = 5m, Quantity = 1 };
				var cart = new Cart { Id = cartId, Items = new System.Collections.Generic.List<CartItem> { item } };
				await container.CreateItemAsync(cart, new PartitionKey(cartId.ToString()));

				var repo = CreateRepository(client, databaseId, containerId);
				await repo.RemoveCartItemAsync(cartId, 42);

				var query = new QueryDefinition("SELECT * FROM c WHERE c.id = @id").WithParameter("@id", cartId);
				var iterator = container.GetItemQueryIterator<Cart>(query);
				var feed = await iterator.ReadNextAsync();
				var storedCart = feed.Resource.FirstOrDefault();

				Assert.IsNotNull(storedCart, "Cart should still exist after RemoveCartItem.");
				Assert.IsFalse(storedCart.Items.Any(i => i.Id == 42), "Item should be removed and persisted.");
			}
			finally
			{
				await client.GetDatabase(databaseId).DeleteAsync();
			}
		}

		[TestMethod]
		public async Task AddCartItem_WhenCartDoesNotExist_CreatesCart()
		{
			var connectionString = Environment.GetEnvironmentVariable("COSMOS_CONNECTION_STRING")
				?? throw new InvalidOperationException("Set COSMOS_CONNECTION_STRING environment variable for integration tests.");
			using var client = new CosmosClient(connectionString);

			var databaseId = CreateDatabaseName();
			var containerId = CreateContainerName();

			try
			{
				await client.CreateDatabaseIfNotExistsAsync(databaseId);
				await client.GetDatabase(databaseId).CreateContainerIfNotExistsAsync(new ContainerProperties(containerId, "/id"));

				var cartId = Guid.NewGuid();
				var repo = CreateRepository(client, databaseId, containerId);
				var item = new CartItem { Id = 11, Name = "NoCart", Price = 2m, Quantity = 1 };

				await repo.AddCartItemAsync(cartId, item);

				var container = client.GetContainer(databaseId, containerId);
				var query = new QueryDefinition("SELECT * FROM c WHERE c.id = @id").WithParameter("@id", cartId);
				var iterator = container.GetItemQueryIterator<Cart>(query);
				var feed = await iterator.ReadNextAsync();
				var stored = feed.Resource.FirstOrDefault();

				Assert.IsNotNull(stored, "Cart should be created when AddCartItem is called for a missing cart.");
			}
			finally
			{
				await client.GetDatabase(databaseId).DeleteAsync();
			}
		}

		[TestMethod]
		public async Task GetCartItems_WhenCartDoesNotExist_ReturnsNull()
		{
			var connectionString = Environment.GetEnvironmentVariable("COSMOS_CONNECTION_STRING")
				?? throw new InvalidOperationException("Set COSMOS_CONNECTION_STRING environment variable for integration tests.");
			using var client = new CosmosClient(connectionString);

			var databaseId = CreateDatabaseName();
			var containerId = CreateContainerName();

			try
			{
				await client.CreateDatabaseIfNotExistsAsync(databaseId);
				await client.GetDatabase(databaseId).CreateContainerIfNotExistsAsync(new ContainerProperties(containerId, "/Id"));

				var repo = CreateRepository(client, databaseId, containerId);
				var items = await repo.GetCartItemsAsync(Guid.NewGuid());
				Assert.IsNull(items);
			}
			finally
			{
				await client.GetDatabase(databaseId).DeleteAsync();
			}
		}

		[TestMethod]
		public async Task RemoveCartItem_WhenCartDoesNotExist_NoOp()
		{
			var connectionString = Environment.GetEnvironmentVariable("COSMOS_CONNECTION_STRING")
				?? throw new InvalidOperationException("Set COSMOS_CONNECTION_STRING environment variable for integration tests.");
			using var client = new CosmosClient(connectionString);

			var databaseId = CreateDatabaseName();
			var containerId = CreateContainerName();

			try
			{
				await client.CreateDatabaseIfNotExistsAsync(databaseId);
				await client.GetDatabase(databaseId).CreateContainerIfNotExistsAsync(new ContainerProperties(containerId, "/id"));

				var repo = CreateRepository(client, databaseId, containerId);
				await repo.RemoveCartItemAsync(Guid.NewGuid(), 1);

				var container = client.GetContainer(databaseId, containerId);
				var iterator = container.GetItemQueryIterator<Cart>(new QueryDefinition("SELECT * FROM c"));
				var feed = await iterator.ReadNextAsync();
				Assert.IsFalse(feed.Resource.Any(), "No cart should exist or be created when RemoveCartItem is called for a missing cart.");
			}
			finally
			{
				await client.GetDatabase(databaseId).DeleteAsync();
			}
		}
	}
}