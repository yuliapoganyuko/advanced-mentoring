using System.Reflection;
using CartService.Infrastructure;
using LiteDB;
using Moq;

namespace CartService.Tests
{
	[TestClass]
	public sealed class CartRepositoryTests
	{
		private readonly Guid existingCartId = Guid.NewGuid();
		private Mock<ILiteDatabase> dbMock = null!;
		private Mock<ILiteCollection<Cart>> collectionMock = null!;

		[TestInitialize]
		public void Setup()
		{
			dbMock = new Mock<ILiteDatabase>();
			collectionMock = new Mock<ILiteCollection<Cart>>();
			dbMock.Setup(d => d.GetCollection<Cart>()).Returns(collectionMock.Object);
		}

		private CartRepository CreateRepository()
		{
			var ctor = typeof(CartRepository).GetConstructor(
				BindingFlags.Instance | BindingFlags.NonPublic,
				null,
				new Type[] { typeof(ILiteDatabase) },
				null);

			Assert.IsNotNull(ctor, "Could not find non-public constructor for CartRepository.");
			return (CartRepository)ctor!.Invoke(new object[] { dbMock.Object });
		}

		[TestMethod]
		public void AddCartItem_WhenCartExists_AddsItem_UpdatesAndCommits()
		{
			// Arrange
			var cart = new Cart { Id = existingCartId, Items = new List<CartItem>() };
			collectionMock.Setup(c => c.FindById(existingCartId)).Returns(cart);

			var repo = CreateRepository();
			var item = new CartItem { Id = 10, Name = "New", Price = 1m, Quantity = 1 };

			// Act
			repo.AddCartItem(existingCartId, item);

			// Assert
			collectionMock.Verify(c => c.Update(It.Is<Cart>(ct => ct.Items.Contains(item))), Times.Once());
			dbMock.Verify(d => d.Commit(), Times.Once());
			Assert.IsTrue(cart.Items.Contains(item));
		}

		[TestMethod]
		public void AddCartItem_WhenCartDoesNotExist_DoesNotUpdateOrCommit()
		{
			// Arrange
			collectionMock.Setup(c => c.FindById(It.IsAny<BsonValue>())).Returns((Cart?)null);
			var repo = CreateRepository();
			var item = new CartItem { Id = 11, Name = "DoesNotMatter", Price = 2m, Quantity = 1 };

			// Act
			repo.AddCartItem(Guid.NewGuid(), item);

			// Assert
			collectionMock.Verify(c => c.Update(It.IsAny<Cart>()), Times.Never());
			dbMock.Verify(d => d.Commit(), Times.Never());
		}

		[TestMethod]
		public void GetCartItems_WhenCartExists_ReturnsItems()
		{
			// Arrange
			var items = new List<CartItem>
			{
				new CartItem { Id = 1, Name = "A", Price = 10m, Quantity = 1 },
				new CartItem { Id = 2, Name = "B", Price = 20m, Quantity = 2 }
			};
			var cart = new Cart { Id = existingCartId, Items = items };
			collectionMock.Setup(c => c.FindById(existingCartId)).Returns(cart);

			var repo = CreateRepository();

			// Act
			var result = repo.GetCartItems(existingCartId).ToList();

			// Assert
			CollectionAssert.AreEqual(items, result);
		}

		[TestMethod]
		public void GetCartItems_WhenCartDoesNotExist_ReturnsNull()
		{
			// Arrange
			collectionMock.Setup(c => c.FindById(It.IsAny<BsonValue>())).Returns((Cart?)null);
			var repo = CreateRepository();

			// Act
			var result = repo.GetCartItems(Guid.NewGuid());

			// Assert
			Assert.IsNull(result);
		}

		[TestMethod]
		public void RemoveCartItem_WhenItemExists_RemovesItem_UpdatesAndCommits()
		{
			// Arrange
			var item = new CartItem { Id = 42, Name = "ToRemove", Price = 5m, Quantity = 1 };
			var cart = new Cart { Id = existingCartId, Items = new List<CartItem> { item } };
			collectionMock.Setup(c => c.FindById(existingCartId)).Returns(cart);
			dbMock.Setup(d => d.Commit()).Returns(true);

			var repo = CreateRepository();

			// Act
			bool result = repo.RemoveCartItem(existingCartId, item.Id);

			// Assert
			Assert.IsTrue(result);
			collectionMock.Verify(c => c.Update(It.Is<Cart>(ct => ct.Items.All(i => i.Id != item.Id))), Times.Once());
			dbMock.Verify(d => d.Commit(), Times.Once());
			Assert.IsFalse(cart.Items.Any(i => i.Id == item.Id));
		}

		[TestMethod]
		public void RemoveCartItem_WhenItemDoesNotExist_DoesNotUpdateOrCommit()
		{
			// Arrange
			var cart = new Cart { Id = existingCartId, Items = new List<CartItem>() };
			collectionMock.Setup(c => c.FindById(existingCartId)).Returns(cart);

			var repo = CreateRepository();

			// Act
			bool result =repo.RemoveCartItem(existingCartId, 999);

			// Assert
			Assert.IsFalse(result);
			collectionMock.Verify(c => c.Update(It.IsAny<Cart>()), Times.Never());
			dbMock.Verify(d => d.Commit(), Times.Never());
		}

		[TestMethod]
		public void RemoveCartItem_WhenCartDoesNotExist_DoesNotUpdateOrCommit()
		{
			// Arrange
			collectionMock.Setup(c => c.FindById(It.IsAny<BsonValue>())).Returns((Cart?)null);
			var repo = CreateRepository();

			// Act
			bool result = repo.RemoveCartItem(Guid.NewGuid(), 1);

			// Assert
			Assert.IsFalse(result);
			collectionMock.Verify(c => c.Update(It.IsAny<Cart>()), Times.Never());
			dbMock.Verify(d => d.Commit(), Times.Never());
		}
	}
}