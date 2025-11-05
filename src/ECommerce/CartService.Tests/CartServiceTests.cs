using AutoMapper;
using CartService.Infrastructure;
using CartService.Core;
using Moq;

namespace CartService.Tests
{
	[TestClass]
	public sealed class CartServiceTests
	{
		private Core.CartService cartService;
		private Mock<ICartRepository> cartRepositoryMock;
		private Mock<IMapper> mapperMock;
		
		private Guid cartId;
		private CartItemDto cartItem;

		public CartServiceTests()
		{
			cartId = Guid.NewGuid();
			cartItem = new CartItemDto()
			{
				Id = 3,
				Name = "Item 3",
				Price = 3000M,
				Quantity = 3
			};

			cartRepositoryMock = new Mock<ICartRepository>();
			mapperMock = new Mock<IMapper>();

			cartService = new Core.CartService(cartRepositoryMock.Object, mapperMock.Object);
		}

		[TestMethod]
		public void AddCartItem_AddsItem_WhenItemIsValid()
		{
			// Arrange
			mapperMock.Setup(m => m.Map<CartItem>(It.IsAny<CartItemDto>()))
				.Returns((CartItemDto dto) => new CartItem
				{
					Id = dto.Id,
					Name = dto.Name,
					ImageUri = dto.ImageUrl,
					ImageAltText = dto.ImageAltText,
					Price = dto.Price,
					Quantity = dto.Quantity
				});

			// Act
			cartService.AddCartItem(cartId, cartItem);

			// Assert
			mapperMock.Verify(m => m.Map<CartItem>(cartItem), Times.Once());
			cartRepositoryMock.Verify(m => m.AddCartItem(cartId, It.Is<CartItem>(ci => ci.Id == cartItem.Id && ci.Name == cartItem.Name)), Times.Once());
		}

		[TestMethod]
		public void AddCartItem_ThrowsArgumentException_WhenCartIdIsEmpty()
		{
			// Arrange
			var emptyId = Guid.Empty;

			// Act & Assert
			Assert.ThrowsException<ArgumentException>(() => cartService.AddCartItem(emptyId, cartItem));
		}

		[TestMethod]
		public void AddCartItem_ThrowsArgumentNullException_WhenItemIsNull()
		{
			// Arrange
			CartItemDto nullItem = null!;

			// Act & Assert
			Assert.ThrowsException<ArgumentNullException>(() => cartService.AddCartItem(cartId, nullItem));
		}

		[TestMethod]
		public void GetCartItems_ReturnsMappedItems_WhenCartIdValid()
		{
			// Arrange
			var itemsFromRepo = new List<CartItem>()
			{
				new CartItem { Id = 1, Name = "Repo Item 1", Price = 100, Quantity = 1 },
				new CartItem { Id = 2, Name = "Repo Item 2", Price = 200, Quantity = 2 }
			};

			var mappedDtos = new List<CartItemDto>()
			{
				new CartItemDto { Id = 1, Name = "Repo Item 1", Price = 100, Quantity = 1 },
				new CartItemDto { Id = 2, Name = "Repo Item 2", Price = 200, Quantity = 2 }
			};

			cartRepositoryMock.Setup(r => r.GetCartItems(cartId)).Returns(itemsFromRepo);
			mapperMock.Setup(m => m.Map<IEnumerable<CartItemDto>>(itemsFromRepo)).Returns(mappedDtos);

			// Act
			var result = cartService.GetCartItems(cartId);

			// Assert
			cartRepositoryMock.Verify(r => r.GetCartItems(cartId), Times.Once());
			mapperMock.Verify(m => m.Map<IEnumerable<CartItemDto>>(itemsFromRepo), Times.Once());
			CollectionAssert.AreEqual((System.Collections.ICollection)mappedDtos, (System.Collections.ICollection)result);
		}

		[TestMethod]
		public void GetCartItems_ThrowsArgumentException_WhenCartIdIsEmpty()
		{
			// Arrange
			var emptyId = Guid.Empty;

			// Act & Assert
			Assert.ThrowsException<ArgumentException>(() => cartService.GetCartItems(emptyId));
		}

		[TestMethod]
		public void RemoveCartItem_CallsRepositoryRemove_WhenInputIsValid()
		{
			// Act
			cartService.RemoveCartItem(cartId, cartItem.Id);

			// Assert
			cartRepositoryMock.Verify(m => m.RemoveCartItem(cartId, cartItem.Id), Times.Once());
		}

		[TestMethod]
		public void RemoveCartItem_ThrowsArgumentException_WhenCartIdIsEmpty()
		{
			// Arrange
			var emptyId = Guid.Empty;

			// Act & Assert
			Assert.ThrowsException<ArgumentException>(() => cartService.RemoveCartItem(emptyId, cartItem.Id));
		}

		[TestMethod]
		public void RemoveCartItem_ThrowsArgumentOutOfRange_WhenItemIdIsNegative()
		{
			// Arrange
			int negativeId = -1;

			// Act & Assert
			Assert.ThrowsException<ArgumentOutOfRangeException>(() => cartService.RemoveCartItem(cartId, negativeId));
		}
	}
}