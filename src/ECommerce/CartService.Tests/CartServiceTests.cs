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
		public async Task AddCartItem_AddsItem_WhenItemIsValid()
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
			await cartService.AddCartItemAsync(cartId, cartItem);

			// Assert
			mapperMock.Verify(m => m.Map<CartItem>(cartItem), Times.Once());
			cartRepositoryMock.Verify(m => m.AddCartItemAsync(cartId, It.Is<CartItem>(ci => ci.Id == cartItem.Id && ci.Name == cartItem.Name)), Times.Once());
		}

		[TestMethod]
		public void AddCartItem_ThrowsArgumentException_WhenCartIdIsEmpty()
		{
			// Arrange
			var emptyId = Guid.Empty;

			// Act & Assert
			Assert.ThrowsExceptionAsync<ArgumentException>(() => cartService.AddCartItemAsync(emptyId, cartItem));
		}

		[TestMethod]
		public void AddCartItem_ThrowsArgumentNullException_WhenItemIsNull()
		{
			// Arrange
			CartItemDto nullItem = null!;

			// Act & Assert
			Assert.ThrowsExceptionAsync<ArgumentNullException>(() => cartService.AddCartItemAsync(cartId, nullItem));
		}

		[TestMethod]
		public async Task GetCartItems_ReturnsMappedItems_WhenCartIdValid()
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

			cartRepositoryMock.Setup(r => r.GetCartItemsAsync(cartId)).ReturnsAsync(itemsFromRepo);
			mapperMock.Setup(m => m.Map<IEnumerable<CartItemDto>>(itemsFromRepo)).Returns(mappedDtos);

			// Act
			var result = await cartService.GetCartItemsAsync(cartId);

			// Assert
			cartRepositoryMock.Verify(r => r.GetCartItemsAsync(cartId), Times.Once());
			mapperMock.Verify(m => m.Map<IEnumerable<CartItemDto>>(itemsFromRepo), Times.Once());
			CollectionAssert.AreEqual((System.Collections.ICollection)mappedDtos, (System.Collections.ICollection)result);
		}

		[TestMethod]
		public void GetCartItems_ThrowsArgumentException_WhenCartIdIsEmpty()
		{
			// Arrange
			var emptyId = Guid.Empty;

			// Act & Assert
			Assert.ThrowsExceptionAsync<ArgumentException>(() => cartService.GetCartItemsAsync(emptyId));
		}

		[TestMethod]
		public async Task RemoveCartItem_CallsRepositoryRemove_WhenInputIsValid()
		{
			// Act
			await cartService.RemoveCartItemAsync(cartId, cartItem.Id);

			// Assert
			cartRepositoryMock.Verify(m => m.RemoveCartItemAsync(cartId, cartItem.Id), Times.Once());
		}

		[TestMethod]
		public void RemoveCartItem_ThrowsArgumentException_WhenCartIdIsEmpty()
		{
			// Arrange
			var emptyId = Guid.Empty;

			// Act & Assert
			Assert.ThrowsExceptionAsync<ArgumentException>(() => cartService.RemoveCartItemAsync(emptyId, cartItem.Id));
		}

		[TestMethod]
		public void RemoveCartItem_ThrowsArgumentOutOfRange_WhenItemIdIsNegative()
		{
			// Arrange
			int negativeId = -1;

			// Act & Assert
			Assert.ThrowsExceptionAsync<ArgumentOutOfRangeException>(() => cartService.RemoveCartItemAsync(cartId, negativeId));
		}
	}
}