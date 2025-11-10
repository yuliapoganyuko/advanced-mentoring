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
			cartServiceMock = new Mock<ICartService>(MockBehavior.Strict);
			controller = new CartsController(cartServiceMock.Object);
		}

		[TestMethod]
		public void Get_ReturnsBadRequest_WhenCartIdIsInvalid()
		{
			// Act
			var result = controller.Get("not-a-guid");

			// Assert
			Assert.IsNotNull(result.Result);
			var badReq = result.Result as BadRequestResult;
			Assert.IsNotNull(badReq);
		}

		[TestMethod]
		public void Get_ReturnsOk_WithCartDto_WhenServiceReturnsItems()
		{
			// Arrange
			var id = Guid.NewGuid();
			var items = new List<CartItemDto>
			{
				new CartItemDto { Id = 1, Name = "A", Price = 1m, Quantity = 1 },
				new CartItemDto { Id = 2, Name = "B", Price = 2m, Quantity = 2 }
			};
			cartServiceMock.Setup(s => s.GetCartItems(id)).Returns(items);

			// Act
			var action = controller.Get(id.ToString());

			// Assert
			var ok = action.Result as OkObjectResult;
			Assert.IsNotNull(ok);
			var cart = ok!.Value as CartDto;
			Assert.IsNotNull(cart);
			Assert.AreEqual(id, cart!.Id);
			CollectionAssert.AreEqual(items.ToList(), cart.Items.ToList());
			cartServiceMock.Verify(s => s.GetCartItems(id), Times.Once);
		}

		[TestMethod]
		public void Get_ReturnsOk_WithEmptyItems_WhenServiceReturnsNull()
		{
			// Arrange
			var id = Guid.NewGuid();
			cartServiceMock.Setup(s => s.GetCartItems(id)).Returns((IEnumerable<CartItemDto>?)null);

			// Act
			var action = controller.Get(id.ToString());

			// Assert
			var ok = action.Result as OkObjectResult;
			Assert.IsNotNull(ok);
			var cart = ok!.Value as CartDto;
			Assert.IsNotNull(cart);
			Assert.AreEqual(id, cart!.Id);
			Assert.IsNotNull(cart.Items);
			Assert.AreEqual(0, cart.Items.Count);
			cartServiceMock.Verify(s => s.GetCartItems(id), Times.Once);
		}

		[TestMethod]
		public void GetV2_ReturnsBadRequest_WhenCartIdIsInvalid()
		{
			// Act
			var result = controller.GetV2(string.Empty);

			// Assert
			Assert.IsNotNull(result.Result);
			var badReq = result.Result as BadRequestResult;
			Assert.IsNotNull(badReq);
		}

		[TestMethod]
		public void GetV2_ReturnsOk_WithItems_WhenServiceReturnsItems()
		{
			// Arrange
			var id = Guid.NewGuid();
			var items = new List<CartItemDto>
			{
				new CartItemDto { Id = 3, Name = "X", Price = 10m, Quantity = 1 }
			};
			cartServiceMock.Setup(s => s.GetCartItems(id)).Returns(items);

			// Act
			var action = controller.GetV2(id.ToString());

			// Assert
			var ok = action.Result as OkObjectResult;
			Assert.IsNotNull(ok);
			var returned = ok!.Value as IEnumerable<CartItemDto>;
			Assert.IsNotNull(returned);
			CollectionAssert.AreEqual(items.ToList(), returned!.ToList());
			cartServiceMock.Verify(s => s.GetCartItems(id), Times.Once);
		}

		[TestMethod]
		public void Post_ReturnsBadRequest_WhenItemIsInvalid()
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
			var result = controller.Post(id, invalidItem);

			// Assert
			var badReq = result as BadRequestResult;
			Assert.IsNotNull(badReq);
		}

		[TestMethod]
		public void Post_ReturnsBadRequest_WhenCartIdIsInvalid()
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
			var result = controller.Post(invalidCartId, item);

			// Assert
			var badReq = result as BadRequestResult;
			Assert.IsNotNull(badReq);
		}

		[TestMethod]
		public void Post_CallsServiceAndReturnsOk_WhenInputIsValid()
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
			cartServiceMock.Setup(s => s.AddCartItem(id, It.Is<CartItemDto>(ci => ci.Id == item.Id && ci.Name == item.Name)));

			// Act
			var result = controller.Post(id.ToString(), item);

			// Assert
			Assert.IsInstanceOfType(result, typeof(OkResult));
			cartServiceMock.Verify(s => s.AddCartItem(id, It.Is<CartItemDto>(ci => ci.Id == item.Id && ci.Name == item.Name)), Times.Once);
		}

		[TestMethod]
		public void Delete_ReturnsBadRequest_WhenCartIdInvalid()
		{
			// Act
			var result = controller.Delete("", 1);

			// Assert
			var badReq = result as BadRequestResult;
			Assert.IsNotNull(badReq);
		}

		[TestMethod]
		public void Delete_ReturnsOk_WithServiceResult_WhenValid()
		{
			// Arrange
			var id = Guid.NewGuid();
			var itemId = 21;
			cartServiceMock.Setup(s => s.RemoveCartItem(id, itemId)).Returns(true);

			// Act
			var action = controller.Delete(id.ToString(), itemId);

			// Assert
			var ok = action as OkObjectResult;
			Assert.IsNotNull(ok);
			Assert.AreEqual(true, ok!.Value);
			cartServiceMock.Verify(s => s.RemoveCartItem(id, itemId), Times.Once);
		}
	}
}