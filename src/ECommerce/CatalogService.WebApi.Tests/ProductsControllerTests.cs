using CatalogService.Core;
using CatalogService.Core.Interfaces;
using CatalogService.WebApi.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace CatalogService.WebApi.Tests
{
	[TestClass]
	public sealed class ProductsControllerTests
	{
		[TestMethod]
		public async Task List_ReturnsOkWithProducts_WhenNoPagination()
		{
			// Arrange
			var products = new[]
			{
				new ProductDto { Id = 1, Name = "P1", CategoryId = 1, Price = 1m, Amount = 1 },
				new ProductDto { Id = 2, Name = "P2", CategoryId = 1, Price = 2m, Amount = 2 }
			}.AsEnumerable();

			var productServiceMock = new Mock<IProductService>();
			productServiceMock.Setup(s => s.ListAsync(It.IsAny<int?>(), default))
				.ReturnsAsync(products);

			var controller = new ProductsController(productServiceMock.Object);

			// Act
			var actionResult = await controller.List();

			// Assert
			Assert.IsNotNull(actionResult);
			Assert.IsInstanceOfType(actionResult.Result, typeof(OkObjectResult));
			var ok = (OkObjectResult)actionResult.Result;
			Assert.AreSame(products, ok.Value);
		}

		[TestMethod]
		public async Task List_ReturnsPagedResults_WhenPaginationProvided()
		{
			// Arrange
			var products = Enumerable.Range(1, 10)
				.Select(i => new ProductDto { Id = i, Name = $"P{i}", CategoryId = 1, Price = i, Amount = (uint)i })
				.ToArray()
				.AsEnumerable();

			var productServiceMock = new Mock<IProductService>();
			productServiceMock.Setup(s => s.ListAsync(It.IsAny<int?>(), default))
				.ReturnsAsync(products);

			var controller = new ProductsController(productServiceMock.Object);

			// Act: pageNumber = 2, pageSize = 3 -> items 4,5,6
			var actionResult = await controller.List(pageNumber: 2, pageSize: 3);

			// Assert
			Assert.IsInstanceOfType(actionResult.Result, typeof(OkObjectResult));
			OkObjectResult? ok = (OkObjectResult)actionResult.Result;
			Assert.IsNotNull(ok.Value);
			var returned = ok != null ? ((IEnumerable<ProductDto>)ok.Value).ToArray() : null;
			Assert.IsNotNull(returned);
			Assert.AreEqual(3, returned.Length);
			Assert.AreEqual(4, returned[0].Id);
			Assert.AreEqual(5, returned[1].Id);
			Assert.AreEqual(6, returned[2].Id);
		}

		[TestMethod]
		public async Task Get_IdInvalid_ReturnsBadRequest()
		{
			// Arrange
			var productServiceMock = new Mock<IProductService>();
			var controller = new ProductsController(productServiceMock.Object);

			// Act
			var resultZero = await controller.Get(0);
			var resultNegative = await controller.Get(-10);

			// Assert
			Assert.IsInstanceOfType(resultZero.Result, typeof(BadRequestResult));
			Assert.IsInstanceOfType(resultNegative.Result, typeof(BadRequestResult));
			productServiceMock.Verify(s => s.GetAsync(It.IsAny<int>(), default), Times.Never);
		}

		[TestMethod]
		public async Task Get_ValidId_ReturnsOkWithProduct()
		{
			// Arrange
			var expected = new ProductDto { Id = 5, Name = "MyProduct", CategoryId = 2, Price = 9.99m, Amount = 3 };
			var productServiceMock = new Mock<IProductService>();
			productServiceMock.Setup(s => s.GetAsync(expected.Id, default))
				.ReturnsAsync(expected);

			var controller = new ProductsController(productServiceMock.Object);

			// Act
			var actionResult = await controller.Get(expected.Id);

			// Assert
			Assert.IsInstanceOfType(actionResult.Result, typeof(OkObjectResult));
			var ok = (OkObjectResult)actionResult.Result;
			Assert.AreSame(expected, ok.Value);
		}

		[TestMethod]
		public async Task Post_InvalidModel_ReturnsBadRequest()
		{
			// Arrange
			var productServiceMock = new Mock<IProductService>();
			var controller = new ProductsController(productServiceMock.Object);

			ProductDto nullProduct = null!;
			var invalidId = new ProductDto { Id = 1, Name = "X", CategoryId = 1, Price = 1m, Amount = 1 };
			var invalidName = new ProductDto { Id = 0, Name = "", CategoryId = 1, Price = 1m, Amount = 1 };
			var invalidCategory = new ProductDto { Id = 0, Name = "N", CategoryId = 0, Price = 1m, Amount = 1 };
			var invalidPrice = new ProductDto { Id = 0, Name = "N", CategoryId = 1, Price = -1m, Amount = 1 };
			var invalidAmount = new ProductDto { Id = 0, Name = "N", CategoryId = 1, Price = 1m, Amount = 0 };

			// Act & Assert
			var r1 = await controller.Post(nullProduct);
			Assert.IsInstanceOfType(r1, typeof(BadRequestResult));

			var r2 = await controller.Post(invalidId);
			Assert.IsInstanceOfType(r2, typeof(BadRequestResult));

			var r3 = await controller.Post(invalidName);
			Assert.IsInstanceOfType(r3, typeof(BadRequestResult));

			var r4 = await controller.Post(invalidCategory);
			Assert.IsInstanceOfType(r4, typeof(BadRequestResult));

			var r5 = await controller.Post(invalidPrice);
			Assert.IsInstanceOfType(r5, typeof(BadRequestResult));

			var r6 = await controller.Post(invalidAmount);
			Assert.IsInstanceOfType(r6, typeof(BadRequestResult));

			productServiceMock.Verify(s => s.AddAsync(It.IsAny<ProductDto>(), default), Times.Never);
		}

		[TestMethod]
		public async Task Post_ValidModel_CallsAddAndReturnsCreated()
		{
			// Arrange
			var input = new ProductDto { Name = "NewProduct", CategoryId = 2, Price = 5.5m, Amount = 2 }; // Id left as default (0)
			var added = new ProductDto { Id = 10, Name = "NewProduct", CategoryId = 2, Price = 5.5m, Amount = 2 };

			var productServiceMock = new Mock<IProductService>();
			productServiceMock.Setup(s => s.AddAsync(It.Is<ProductDto>(p => p.Name == input.Name && p.Id == 0), It.IsAny<CancellationToken>()))
				.ReturnsAsync(added)
				.Verifiable();

			var controller = new ProductsController(productServiceMock.Object);

			// Act
			var result = await controller.Post(input);

			// Assert
			Assert.IsInstanceOfType(result, typeof(CreatedAtActionResult));
			var created = (CreatedAtActionResult)result;
			Assert.AreEqual(nameof(ProductsController.Get), created.ActionName);
			Assert.IsNotNull(created.RouteValues);
			Assert.IsTrue(created.RouteValues.ContainsKey("id"));
			Assert.AreEqual(added.Id, created.RouteValues["id"]);
			Assert.AreSame(added, created.Value);

			productServiceMock.Verify(s => s.AddAsync(It.IsAny<ProductDto>(), It.IsAny<CancellationToken>()), Times.Once);
		}

		[TestMethod]
		public async Task Put_InvalidModelOrId_ReturnsBadRequest()
		{
			// Arrange
			var productServiceMock = new Mock<IProductService>();
			var controller = new ProductsController(productServiceMock.Object);

			ProductDto nullProduct = null!;
			var mismatch = new ProductDto { Id = 2, Name = "Name", CategoryId = 1, Price = 1m, Amount = 1 }; // id param will be different
			var invalidIdParam = new ProductDto { Id = 1, Name = "Name", CategoryId = 1, Price = 1m, Amount = 1 };
			var invalidName = new ProductDto { Id = 3, Name = "", CategoryId = 1, Price = 1m, Amount = 1 };
			var invalidCategory = new ProductDto { Id = 3, Name = "N", CategoryId = 0, Price = 1m, Amount = 1 };
			var invalidPrice = new ProductDto { Id = 3, Name = "N", CategoryId = 1, Price = -1m, Amount = 1 };
			var invalidAmount = new ProductDto { Id = 3, Name = "N", CategoryId = 1, Price = 1m, Amount = 0 };

			// Act & Assert
			var r1 = await controller.Put(1, nullProduct);
			Assert.IsInstanceOfType(r1, typeof(BadRequestResult));

			var r2 = await controller.Put(5, mismatch); // id mismatch
			Assert.IsInstanceOfType(r2, typeof(BadRequestResult));

			var r3 = await controller.Put(0, invalidIdParam); // invalid id param
			Assert.IsInstanceOfType(r3, typeof(BadRequestResult));

			var r4 = await controller.Put(3, invalidName); // invalid name
			Assert.IsInstanceOfType(r4, typeof(BadRequestResult));

			var r5 = await controller.Put(3, invalidCategory); // invalid category
			Assert.IsInstanceOfType(r5, typeof(BadRequestResult));

			var r6 = await controller.Put(3, invalidPrice); // invalid price
			Assert.IsInstanceOfType(r6, typeof(BadRequestResult));

			var r7 = await controller.Put(3, invalidAmount); // invalid amount
			Assert.IsInstanceOfType(r7, typeof(BadRequestResult));

			productServiceMock.Verify(s => s.UpdateAsync(It.IsAny<ProductDto>(), default), Times.Never);
		}

		[TestMethod]
		public async Task Put_ValidModel_CallsUpdateAndReturnsOk()
		{
			// Arrange
			var product = new ProductDto { Id = 7, Name = "Updated", CategoryId = 2, Price = 3.3m, Amount = 5 };

			var productServiceMock = new Mock<IProductService>();
			productServiceMock.Setup(s => s.UpdateAsync(product, default))
				.Returns(Task.CompletedTask)
				.Verifiable();

			var controller = new ProductsController(productServiceMock.Object);

			// Act
			var result = await controller.Put(product.Id, product);

			// Assert
			Assert.IsInstanceOfType(result, typeof(OkResult));
			productServiceMock.Verify(s => s.UpdateAsync(product, default), Times.Once);
		}

		[TestMethod]
		public async Task Delete_InvalidId_ReturnsBadRequest()
		{
			// Arrange
			var productServiceMock = new Mock<IProductService>();
			var controller = new ProductsController(productServiceMock.Object);

			// Act
			var result = await controller.Delete(0);

			// Assert
			Assert.IsInstanceOfType(result, typeof(BadRequestResult));
			productServiceMock.Verify(s => s.DeleteAsync(It.IsAny<int>(), default), Times.Never);
		}

		[TestMethod]
		public async Task Delete_ValidId_CallsDeleteAndReturnsNoContent()
		{
			// Arrange
			var productId = 42;

			var productServiceMock = new Mock<IProductService>();
			productServiceMock.Setup(s => s.DeleteAsync(productId, default))
				.ReturnsAsync(true)
				.Verifiable();

			var controller = new ProductsController(productServiceMock.Object);

			// Act
			var result = await controller.Delete(productId);

			// Assert
			Assert.IsInstanceOfType(result, typeof(NoContentResult));
			productServiceMock.Verify(s => s.DeleteAsync(productId, default), Times.Once);
		}
	}
}