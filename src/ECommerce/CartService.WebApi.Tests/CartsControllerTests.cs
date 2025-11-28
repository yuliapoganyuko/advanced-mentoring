using CartService.Core;
using CartService.WebApi.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace CartService.WebApi.Tests
{
	[TestClass]
	public sealed class CartsControllerTests
	{
		private Mock<ICartService> cartServiceMock = null!;
		private CartsController controller = null!;

		[TestInitialize]
		public void Init()
		{
			cartServiceMock = new Mock<ICartService>();
			controller = new CartsController(cartServiceMock.Object);
		}

		[TestMethod]
		public async Task Get_ReturnsBadRequest_WhenCartIdIsInvalid()
		{
			// Act
			var result = await controller.Get("not-a-guid");

			// Assert
			Assert.IsNotNull(result.Result);
			var badReq = result.Result as BadRequestResult;
			Assert.IsNotNull(badReq);
		}

		[TestMethod]
		public async Task Get_ReturnsOk_WithCartDto_WhenServiceReturnsItems()
		{
			// Arrange
			var id = Guid.NewGuid();
			var items = new List<CartItemDto>
			{
				new CartItemDto { Id = 1, Name = "A", Price = 1m, Quantity = 1 },
				new CartItemDto { Id = 2, Name = "B", Price = 2m, Quantity = 2 }
			};
			cartServiceMock.Setup(s => s.GetCartItemsAsync(id)).ReturnsAsync(items);

			// Act
			var action = await controller.Get(id.ToString());

			// Assert
			var ok = action.Result as OkObjectResult;
			Assert.IsNotNull(ok);
			var cart = ok!.Value as CartDto;
			Assert.IsNotNull(cart);
			Assert.AreEqual(id, cart!.Id);
			CollectionAssert.AreEqual(items.ToList(), cart.Items.ToList());
			cartServiceMock.Verify(s => s.GetCartItemsAsync(id), Times.Once);
		}

		[TestMethod]
		public async Task Get_ReturnsOk_WithEmptyItems_WhenServiceReturnsNull()
		{
			// Arrange
			var id = Guid.NewGuid();
			cartServiceMock.Setup(s => s.GetCartItemsAsync(id)).ReturnsAsync((IEnumerable<CartItemDto>?)null);

			// Act
			var action = await controller.Get(id.ToString());

			// Assert
			var ok = action.Result as OkObjectResult;
			Assert.IsNotNull(ok);
			var cart = ok!.Value as CartDto;
			Assert.IsNotNull(cart);
			Assert.AreEqual(id, cart!.Id);
			Assert.IsNotNull(cart.Items);
			Assert.AreEqual(0, cart.Items.Count);
			cartServiceMock.Verify(s => s.GetCartItemsAsync(id), Times.Once);
		}

		[TestMethod]
		public async Task GetV2_ReturnsBadRequest_WhenCartIdIsInvalid()
		{
			// Act
			var result = await controller.GetV2(string.Empty);

			// Assert
			Assert.IsNotNull(result.Result);
			var badReq = result.Result as BadRequestResult;
			Assert.IsNotNull(badReq);
		}

		[TestMethod]
		public async Task GetV2_ReturnsOk_WithItems_WhenServiceReturnsItems()
		{
			// Arrange
			var id = Guid.NewGuid();
			var items = new List<CartItemDto>
			{
				new CartItemDto { Id = 3, Name = "X", Price = 10m, Quantity = 1 }
			};
			cartServiceMock.Setup(s => s.GetCartItemsAsync(id)).ReturnsAsync(items);

			// Act
			var action = await controller.GetV2(id.ToString());

			// Assert
			var ok = action.Result as OkObjectResult;
			Assert.IsNotNull(ok);
			var returned = ok!.Value as IEnumerable<CartItemDto>;
			Assert.IsNotNull(returned);
			CollectionAssert.AreEqual(items.ToList(), returned!.ToList());
			cartServiceMock.Verify(s => s.GetCartItemsAsync(id), Times.Once);
		}

		[TestMethod]
		public async Task Post_ReturnsBadRequest_WhenItemIsInvalid()
		{
			// Arrange
			var id = Guid.NewGuid().ToString();
			var invalidItem = new CartItemDto
			{
				Id = 1,
				Name = "ValidName",
				Price = 0m, // invalid: price must be > 0
				Quantity = 1
			};

			// Act
			var result = await controller.Post(id, invalidItem);

			// Assert
			var badReq = result as BadRequestResult;
			Assert.IsNotNull(badReq);
		}

		[TestMethod]
		public async Task Post_ReturnsBadRequest_WhenCartIdIsInvalid()
		{
			// Arrange
			var invalidCartId = "bad-guid";
			var item = new CartItemDto
			{
				Id = 5,
				Name = "Item",
				Price = 5m,
				Quantity = 1
			};

			// Act
			var result = await controller.Post(invalidCartId, item);

			// Assert
			var badReq = result as BadRequestResult;
			Assert.IsNotNull(badReq);
		}

		[TestMethod]
		public async Task Post_CallsServiceAndReturnsOk_WhenInputIsValid()
		{
			// Arrange
			var id = Guid.NewGuid();
			var item = new CartItemDto
			{
				Id = 7,
				Name = "NewItem",
				Price = 9.99m,
				Quantity = 2
			};
			cartServiceMock.Setup(s => s.AddCartItemAsync(id, It.Is<CartItemDto>(ci => ci.Id == item.Id && ci.Name == item.Name)));

			// Act
			var result = await controller.Post(id.ToString(), item);

			// Assert
			Assert.IsInstanceOfType(result, typeof(OkResult));
			cartServiceMock.Verify(s => s.AddCartItemAsync(id, It.Is<CartItemDto>(ci => ci.Id == item.Id && ci.Name == item.Name)), Times.Once);
		}

		[TestMethod]
		public async Task Delete_ReturnsBadRequest_WhenCartIdInvalid()
		{
			// Act
			var result = await controller.Delete("", 1);

			// Assert
			var badReq = result as BadRequestResult;
			Assert.IsNotNull(badReq);
		}

		[TestMethod]
		public async Task Delete_ReturnsOk_WithServiceResult_WhenValid()
		{
			// Arrange
			var id = Guid.NewGuid();
			var itemId = 21;
			cartServiceMock.Setup(s => s.RemoveCartItemAsync(id, itemId)).ReturnsAsync(true);

			// Act
			var action = await controller.Delete(id.ToString(), itemId);

			// Assert
			var ok = action as OkObjectResult;
			Assert.IsNotNull(ok);
			Assert.AreEqual(true, ok!.Value);
			cartServiceMock.Verify(s => s.RemoveCartItemAsync(id, itemId), Times.Once);
		}
	}
}