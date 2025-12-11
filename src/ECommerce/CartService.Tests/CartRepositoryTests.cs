using System.Net;
using CartService.Infrastructure;
using Microsoft.Azure.Cosmos;
using Moq;

namespace CartService.Tests
{
	[TestClass]
	public sealed class CartRepositoryTests
	{
		private readonly Guid existingCartId = Guid.NewGuid();
		private Mock<CosmosClient> dbClientMock = null!;
		private string dbId = "TestCartDb";
		private string containerId = "Carts";
		private Mock<Container> containerMock = null!;

		[TestInitialize]
		public void Setup()
		{
			dbClientMock = new Mock<CosmosClient>();
			containerMock = new Mock<Container>();
			dbClientMock.Setup(d => d.GetContainer(dbId, containerId)).Returns(containerMock.Object);
		}

		private CartRepository CreateRepository()
		{
			return new CartRepository(dbClientMock.Object, dbId, containerId);
		}

		private static CosmosException CreateNotFoundCosmosException()
		{
			return new CosmosException("Not Found", HttpStatusCode.NotFound, 0, string.Empty, 0);
		}

		[TestMethod]
		public async Task AddCartItemAsync_WhenCartExists_AddsItem_Replaces()
		{
			// Arrange
			var cart = new Cart { Id = existingCartId, Items = new List<CartItem>() };
			containerMock
				.Setup(c => c.ReadItemAsync<Cart>(
					existingCartId.ToString(),
					It.IsAny<PartitionKey>(),
					null,
					It.IsAny<CancellationToken>()))
				.ReturnsAsync(Mock.Of<ItemResponse<Cart>>(r => r.Resource == cart));

			containerMock
				.Setup(c => c.ReplaceItemAsync(
					It.IsAny<Cart>(),
					It.IsAny<string>(),
					It.IsAny<PartitionKey>(),
					null,
					It.IsAny<CancellationToken>()))
				.ReturnsAsync(Mock.Of<ItemResponse<Cart>>());

			var repo = CreateRepository();
			var item = new CartItem { Id = 10, Name = "New", Price = 1m, Quantity = 1 };

			// Act
			await repo.AddCartItemAsync(existingCartId, item);

			// Assert
			containerMock.Verify(c => c.ReplaceItemAsync(
				It.Is<Cart>(ct => ct.Items.Contains(item)),
				existingCartId.ToString(),
				It.IsAny<PartitionKey>(),
				null,
				It.IsAny<CancellationToken>()), Times.Once());

			Assert.IsTrue(cart.Items.Contains(item));
		}

		[TestMethod]
		public async Task AddCartItemAsync_WhenCartDoesNotExist_CreatesNewCart()
		{
			// Arrange - simulate ReadItemAsync throwing 404 (cart not found)
			containerMock
				.Setup(c => c.ReadItemAsync<Cart>(
					It.IsAny<string>(),
					It.IsAny<PartitionKey>(),
					null,
					It.IsAny<CancellationToken>()))
				.ThrowsAsync(CreateNotFoundCosmosException());

			containerMock
				.Setup(c => c.CreateItemAsync(
					It.IsAny<Cart>(),
					It.IsAny<PartitionKey>(),
					null,
					It.IsAny<CancellationToken>()))
				.ReturnsAsync(Mock.Of<ItemResponse<Cart>>());

			var repo = CreateRepository();
			var item = new CartItem { Id = 11, Name = "DoesNotMatter", Price = 2m, Quantity = 1 };

			// Act
			await repo.AddCartItemAsync(Guid.NewGuid(), item);

			// Assert
			containerMock.Verify(c => c.CreateItemAsync(
				It.Is<Cart>(ct => ct.Items.Contains(item)),
				It.IsAny<PartitionKey>(),
				null,
				It.IsAny<CancellationToken>()), Times.Once());
		}

		[TestMethod]
		public async Task GetCartItemsAsync_WhenCartExists_ReturnsItems()
		{
			// Arrange
			var items = new List<CartItem>
			{
				new CartItem { Id = 1, Name = "A", Price = 10m, Quantity = 1 },
				new CartItem { Id = 2, Name = "B", Price = 20m, Quantity = 2 }
			};
			var cart = new Cart { Id = existingCartId, Items = items };

			containerMock
				.Setup(c => c.ReadItemAsync<Cart>(
					existingCartId.ToString(),
					It.IsAny<PartitionKey>(),
					null,
					It.IsAny<CancellationToken>()))
				.ReturnsAsync(Mock.Of<ItemResponse<Cart>>(r => r.Resource == cart));

			var repo = CreateRepository();

			// Act
			var result = (await repo.GetCartItemsAsync(existingCartId))!.ToList();

			// Assert
			CollectionAssert.AreEqual(items, result);
		}

		[TestMethod]
		public async Task GetCartItemsAsync_WhenCartDoesNotExist_ReturnsNull()
		{
			// Arrange - simulate not found
			containerMock
				.Setup(c => c.ReadItemAsync<Cart>(
					It.IsAny<string>(),
					It.IsAny<PartitionKey>(),
					null,
					It.IsAny<CancellationToken>()))
				.ThrowsAsync(CreateNotFoundCosmosException());

			var repo = CreateRepository();

			// Act
			var result = await repo.GetCartItemsAsync(Guid.NewGuid());

			// Assert
			Assert.IsNull(result);
		}

		[TestMethod]
		public async Task RemoveCartItemAsync_WhenItemExists_ReplacesAndReturnsTrue()
		{
			// Arrange
			var item = new CartItem { Id = 42, Name = "ToRemove", Price = 5m, Quantity = 1 };
			var cart = new Cart { Id = existingCartId, Items = new List<CartItem> { item } };

			containerMock
				.Setup(c => c.ReadItemAsync<Cart>(
					existingCartId.ToString(),
					It.IsAny<PartitionKey>(),
					null,
					It.IsAny<CancellationToken>()))
				.ReturnsAsync(Mock.Of<ItemResponse<Cart>>(r => r.Resource == cart));

			containerMock
				.Setup(c => c.ReplaceItemAsync(
					It.IsAny<Cart>(),
					It.IsAny<string>(),
					It.IsAny<PartitionKey>(),
					null,
					It.IsAny<CancellationToken>()))
				.ReturnsAsync(Mock.Of<ItemResponse<Cart>>(r => r.StatusCode == HttpStatusCode.OK));

			var repo = CreateRepository();

			// Act
			bool result = await repo.RemoveCartItemAsync(existingCartId, item.Id);

			// Assert
			Assert.IsTrue(result);
			containerMock.Verify(c => c.ReplaceItemAsync(
				It.Is<Cart>(ct => ct.Items.All(i => i.Id != item.Id)),
				existingCartId.ToString(),
				It.IsAny<PartitionKey>(),
				null,
				It.IsAny<CancellationToken>()), Times.Once());
			Assert.IsFalse(cart.Items.Any(i => i.Id == item.Id));
		}

		[TestMethod]
		public async Task RemoveCartItemAsync_WhenItemDoesNotExist_DoesNotReplaceAndReturnsFalse()
		{
			// Arrange
			var cart = new Cart { Id = existingCartId, Items = new List<CartItem>() };

			containerMock
				.Setup(c => c.ReadItemAsync<Cart>(
					existingCartId.ToString(),
					It.IsAny<PartitionKey>(),
					null,
					It.IsAny<CancellationToken>()))
				.ReturnsAsync(Mock.Of<ItemResponse<Cart>>(r => r.Resource == cart));

			var repo = CreateRepository();

			// Act
			bool result = await repo.RemoveCartItemAsync(existingCartId, 999);

			// Assert
			Assert.IsFalse(result);
			containerMock.Verify(c => c.ReplaceItemAsync(
				It.IsAny<Cart>(),
				It.IsAny<string>(),
				It.IsAny<PartitionKey>(),
				null,
				It.IsAny<CancellationToken>()), Times.Never());
		}

		[TestMethod]
		public async Task RemoveCartItemAsync_WhenCartDoesNotExist_ReturnsFalse()
		{
			// Arrange - simulate not found
			containerMock
				.Setup(c => c.ReadItemAsync<Cart>(
					It.IsAny<string>(),
					It.IsAny<PartitionKey>(),
					null,
					It.IsAny<CancellationToken>()))
				.ThrowsAsync(CreateNotFoundCosmosException());

			var repo = CreateRepository();

			// Act
			bool result = await repo.RemoveCartItemAsync(Guid.NewGuid(), 1);

			// Assert
			Assert.IsFalse(result);
			containerMock.Verify(c => c.ReplaceItemAsync(
				It.IsAny<Cart>(),
				It.IsAny<string>(),
				It.IsAny<PartitionKey>(),
				null,
				It.IsAny<CancellationToken>()), Times.Never());
		}
	}
}