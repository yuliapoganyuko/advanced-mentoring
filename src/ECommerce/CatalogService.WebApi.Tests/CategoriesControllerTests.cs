using CatalogService.Core;
using CatalogService.Core.Interfaces;
using CatalogService.WebApi.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace CatalogService.WebApi.Tests
{
	[TestClass]
	public sealed class CategoriesControllerTests
	{
		[TestMethod]
		public async Task List_ReturnsOkWithCategories()
		{
			// Arrange
			var categories = new[]
			{
				new CategoryDto { Id = 1, Name = "Cat1" },
				new CategoryDto { Id = 2, Name = "Cat2" }
			}.AsEnumerable();

			var categoryServiceMock = new Mock<ICategoryService>();
			categoryServiceMock.Setup(s => s.ListAsync(default))
				.ReturnsAsync(categories);

			var controller = new CategoriesController(categoryServiceMock.Object);

			// Act
			var actionResult = await controller.List();

			// Assert
			Assert.IsNotNull(actionResult);
			Assert.IsInstanceOfType(actionResult.Result, typeof(OkObjectResult));
			var ok = (OkObjectResult)actionResult.Result;
			Assert.AreSame(categories, ok.Value);
		}

		[TestMethod]
		public async Task Get_IdInvalid_ReturnsBadRequest()
		{
			// Arrange
			var categoryServiceMock = new Mock<ICategoryService>();
			var controller = new CategoriesController(categoryServiceMock.Object);

			// Act
			var resultZero = await controller.Get(0);
			var resultNegative = await controller.Get(-5);

			// Assert
			Assert.IsInstanceOfType(resultZero.Result, typeof(BadRequestResult));
			Assert.IsInstanceOfType(resultNegative.Result, typeof(BadRequestResult));
			categoryServiceMock.Verify(s => s.GetAsync(It.IsAny<int>(), default), Times.Never);
		}

		[TestMethod]
		public async Task Get_ValidId_ReturnsOkWithCategory()
		{
			// Arrange
			var expected = new CategoryDto { Id = 5, Name = "MyCategory" };
			var categoryServiceMock = new Mock<ICategoryService>();
			categoryServiceMock.Setup(s => s.GetAsync(expected.Id, default))
				.ReturnsAsync(expected);

			var controller = new CategoriesController(categoryServiceMock.Object);

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
			var categoryServiceMock = new Mock<ICategoryService>();
			var controller = new CategoriesController(categoryServiceMock.Object);

			CategoryDto nullCategory = null!;
			var invalidId = new CategoryDto { Id = 1, Name = "X" };
			var invalidName = new CategoryDto { Id = 0, Name = "" };

			// Act & Assert
			var r1 = await controller.Post(nullCategory);
			Assert.IsInstanceOfType(r1, typeof(BadRequestResult));

			var r2 = await controller.Post(invalidId);
			Assert.IsInstanceOfType(r2, typeof(BadRequestResult));

			var r3 = await controller.Post(invalidName);
			Assert.IsInstanceOfType(r3, typeof(BadRequestResult));

			categoryServiceMock.Verify(s => s.AddAsync(It.IsAny<CategoryDto>(), default), Times.Never);
		}

		[TestMethod]
		public async Task Post_ValidModel_CallsAddAndReturnsCreated()
		{
			// Arrange
			var input = new CategoryDto { Name = "NewCat" }; // Id left as default (0)
			var added = new CategoryDto { Id = 10, Name = "NewCat" };

			var categoryServiceMock = new Mock<ICategoryService>();
			categoryServiceMock.Setup(s => s.AddAsync(It.Is<CategoryDto>(c => c.Name == input.Name && c.Id == 0), It.IsAny<System.Threading.CancellationToken>()))
				.ReturnsAsync(added)
				.Verifiable();

			var controller = new CategoriesController(categoryServiceMock.Object);

			// Act
			var result = await controller.Post(input);

			// Assert
			Assert.IsInstanceOfType(result, typeof(CreatedAtActionResult));
			var created = (CreatedAtActionResult)result;
			Assert.AreEqual(nameof(CategoriesController.Get), created.ActionName);
			Assert.IsNotNull(created.RouteValues);
			Assert.IsTrue(created.RouteValues.ContainsKey("id"));
			Assert.AreEqual(added.Id, created.RouteValues["id"]);
			Assert.AreSame(added, created.Value);

			categoryServiceMock.Verify(s => s.AddAsync(It.IsAny<CategoryDto>(), It.IsAny<System.Threading.CancellationToken>()), Times.Once);
		}

		[TestMethod]
		public async Task Put_InvalidModelOrId_ReturnsBadRequest()
		{
			// Arrange
			var categoryServiceMock = new Mock<ICategoryService>();
			var controller = new CategoriesController(categoryServiceMock.Object);

			CategoryDto nullCategory = null!;
			var mismatch = new CategoryDto { Id = 2, Name = "Name" }; // id param will be different
			var invalidId = new CategoryDto { Id = 1, Name = "Name" };
			var invalidName = new CategoryDto { Id = 3, Name = "" };

			// Act & Assert
			var r1 = await controller.Put(1, nullCategory);
			Assert.IsInstanceOfType(r1, typeof(BadRequestResult));

			var r2 = await controller.Put(5, mismatch); // id mismatch
			Assert.IsInstanceOfType(r2, typeof(BadRequestResult));

			var r3 = await controller.Put(0, invalidId); // invalid id param
			Assert.IsInstanceOfType(r3, typeof(BadRequestResult));

			var r4 = await controller.Put(3, invalidName); // invalid name
			Assert.IsInstanceOfType(r4, typeof(BadRequestResult));

			categoryServiceMock.Verify(s => s.UpdateAsync(It.IsAny<CategoryDto>(), default), Times.Never);
		}

		[TestMethod]
		public async Task Put_ValidModel_CallsUpdateAndReturnsOk()
		{
			// Arrange
			var category = new CategoryDto { Id = 7, Name = "Updated" };

			var categoryServiceMock = new Mock<ICategoryService>();
			categoryServiceMock.Setup(s => s.UpdateAsync(category, default))
				.Returns(Task.CompletedTask)
				.Verifiable();

			var controller = new CategoriesController(categoryServiceMock.Object);

			// Act
			var result = await controller.Put(category.Id, category);

			// Assert
			Assert.IsInstanceOfType(result, typeof(OkResult));
			categoryServiceMock.Verify(s => s.UpdateAsync(category, default), Times.Once);
		}

		[TestMethod]
		public async Task Delete_InvalidId_ReturnsBadRequestAndDoesNotCallServices()
		{
			// Arrange
			var categoryServiceMock = new Mock<ICategoryService>();
			var productServiceMock = new Mock<IProductService>();

			var controller = new CategoriesController(categoryServiceMock.Object);

			// Act
			var result = await controller.Delete(0, productServiceMock.Object);

			// Assert
			Assert.IsInstanceOfType(result, typeof(BadRequestResult));
			categoryServiceMock.Verify(s => s.DeleteAsync(It.IsAny<int>(), default), Times.Never);
			productServiceMock.Verify(s => s.ListAsync(It.IsAny<int?>(), default), Times.Never);
		}

		[TestMethod]
		public async Task Delete_ValidId_DeletesProductsAndCategoryAndReturnsNoContent()
		{
			// Arrange
			var categoryId = 42;
			var products = new[]
			{
				new ProductDto { Id = 100, Name = "P1", CategoryId = categoryId, Price = 1m, Amount = 1 },
				new ProductDto { Id = 101, Name = "P2", CategoryId = categoryId, Price = 2m, Amount = 2 }
			}.AsEnumerable();

			var categoryServiceMock = new Mock<ICategoryService>();
			categoryServiceMock.Setup(s => s.DeleteAsync(categoryId, default))
				.ReturnsAsync(true)
				.Verifiable();

			var productServiceMock = new Mock<IProductService>();
			productServiceMock.Setup(s => s.ListAsync(categoryId, default))
				.ReturnsAsync(products);

			productServiceMock.Setup(s => s.DeleteAsync(It.IsAny<int>(), default))
				.ReturnsAsync(true)
				.Verifiable();

			var controller = new CategoriesController(categoryServiceMock.Object);

			// Act
			var result = await controller.Delete(categoryId, productServiceMock.Object);

			// Assert
			Assert.IsInstanceOfType(result, typeof(NoContentResult));
			foreach (var p in products)
			{
				productServiceMock.Verify(s => s.DeleteAsync(p.Id, default), Times.Once);
			}
			categoryServiceMock.Verify(s => s.DeleteAsync(categoryId, default), Times.Once);
		}
	}
}