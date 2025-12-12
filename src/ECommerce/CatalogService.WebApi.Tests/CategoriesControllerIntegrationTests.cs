using System.Net;
using System.Text;
using System.Text.Json;
using CatalogService.Core;
using CatalogService.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace CatalogService.WebApi.Tests
{
	[TestClass]
	public sealed class CategoriesControllerIntegrationTests
	{
		private TestWebApplicationFactory? factory;
		private HttpClient? client;
		private JsonSerializerOptions jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

		[TestInitialize]
		public void TestInitialize()
		{
			factory = new TestWebApplicationFactory();
			client = factory.CreateClient();
		}

		[TestCleanup]
		public void TestCleanup()
		{
			client?.Dispose();
			factory?.Dispose();
		}

		[TestMethod]
		public async Task List_ReturnsOkWithCategories()
		{
			// Arrange
			var categories = new[]
			{
				new CategoryDto { Id = 1, Name = "Cat1" },
				new CategoryDto { Id = 2, Name = "Cat2" }
			}.AsEnumerable();
			factory!.CategoryServiceMock.Setup(s => s.ListAsync(default)).ReturnsAsync(categories);

			// Act
			var resp = await client!.GetAsync("/api/Categories");

			// Assert
			Assert.AreEqual(HttpStatusCode.OK, resp.StatusCode);
			var content = await resp.Content.ReadAsStringAsync();
			var returned = JsonSerializer.Deserialize<IEnumerable<CategoryDto>>(content, jsonOptions)?.ToArray();
			Assert.IsNotNull(returned);
			Assert.AreEqual(2, returned!.Length);
			Assert.AreEqual(1, returned[0].Id);
			Assert.AreEqual("Cat1", returned[0].Name);
			factory.CategoryServiceMock.Verify(s => s.ListAsync(default), Times.Once);
		}

		[TestMethod]
		public async Task Get_InvalidId_ReturnsBadRequest()
		{
			// Act
			var respZero = await client!.GetAsync("/api/Categories/0");
			var respNegative = await client.GetAsync("/api/Categories/-5");

			// Assert
			Assert.AreEqual(HttpStatusCode.BadRequest, respZero.StatusCode);
			Assert.AreEqual(HttpStatusCode.BadRequest, respNegative.StatusCode);
		}

		[TestMethod]
		public async Task Get_ValidId_ReturnsCategory()
		{
			// Arrange
			var id = 5;
			var expected = new CategoryDto { Id = id, Name = "MyCategory" };
			factory!.CategoryServiceMock.Setup(s => s.GetAsync(id, default)).ReturnsAsync(expected);

			// Act
			var resp = await client!.GetAsync($"/api/Categories/{id}");

			// Assert
			Assert.AreEqual(HttpStatusCode.OK, resp.StatusCode);
			var content = await resp.Content.ReadAsStringAsync();
			var returned = JsonSerializer.Deserialize<CategoryDto>(content, jsonOptions);
			Assert.IsNotNull(returned);
			Assert.AreEqual(expected.Id, returned!.Id);
			Assert.AreEqual(expected.Name, returned.Name);
			factory.CategoryServiceMock.Verify(s => s.GetAsync(id, default), Times.Once);
		}

		[TestMethod]
		public async Task Post_InvalidModel_ReturnsBadRequest()
		{
			// Arrange: invalid because Id > 0 and/or Name empty
			var invalid = new CategoryDto { Id = 1, Name = "" };
			var payload = JsonSerializer.Serialize(invalid);
			var content = new StringContent(payload, Encoding.UTF8, "application/json");

			// Act
			var resp = await client!.PostAsync("/api/Categories", content);

			// Assert
			Assert.AreEqual(HttpStatusCode.BadRequest, resp.StatusCode);
		}

		[TestMethod]
		public async Task Post_ValidModel_CallsServiceAndReturnsCreated()
		{
			// Arrange
			var input = new CategoryDto { Name = "NewCat" };
			var added = new CategoryDto { Id = 10, Name = "NewCat" };
			factory!.CategoryServiceMock.Setup(s => s.AddAsync(It.Is<CategoryDto>(c => c.Name == input.Name && c.Id == 0), It.IsAny<System.Threading.CancellationToken>()))
				.ReturnsAsync(added);

			var payload = JsonSerializer.Serialize(input);
			var content = new StringContent(payload, Encoding.UTF8, "application/json");

			// Act
			var resp = await client!.PostAsync("/api/Categories", content);

			// Assert
			Assert.AreEqual(HttpStatusCode.Created, resp.StatusCode);
			var responseBody = await resp.Content.ReadAsStringAsync();
			var returned = JsonSerializer.Deserialize<CategoryDto>(responseBody, jsonOptions);
			Assert.IsNotNull(returned);
			Assert.AreEqual(added.Id, returned!.Id);
			Assert.AreEqual(added.Name, returned.Name);
			factory.CategoryServiceMock.Verify(s => s.AddAsync(It.IsAny<CategoryDto>(), It.IsAny<System.Threading.CancellationToken>()), Times.Once);
		}

		[TestMethod]
		public async Task Delete_InvalidId_ReturnsBadRequest()
		{
			// Act
			var resp = await client!.DeleteAsync("/api/Categories/0");

			// Assert
			Assert.AreEqual(HttpStatusCode.BadRequest, resp.StatusCode);
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

			factory!.ProductServiceMock.Setup(s => s.ListAsync(categoryId, default)).ReturnsAsync(products);
			factory.ProductServiceMock.Setup(s => s.DeleteAsync(It.IsAny<int>(), default)).ReturnsAsync(true).Verifiable();
			factory.CategoryServiceMock.Setup(s => s.DeleteAsync(categoryId, default)).ReturnsAsync(true).Verifiable();

			// Act
			var resp = await client!.DeleteAsync($"/api/Categories/{categoryId}");

			// Assert
			Assert.AreEqual(HttpStatusCode.NoContent, resp.StatusCode);
			foreach (var p in products)
			{
				factory.ProductServiceMock.Verify(s => s.DeleteAsync(p.Id, default), Times.Once);
			}
			factory.CategoryServiceMock.Verify(s => s.DeleteAsync(categoryId, default), Times.Once);
		}

		/// <summary>
		/// WebApplicationFactory that replaces ICategoryService and IProductService registrations with mocks.
		/// </summary>
		private sealed class TestWebApplicationFactory : WebApplicationFactory<Program>
		{
			public Mock<ICategoryService> CategoryServiceMock { get; }
			public Mock<IProductService> ProductServiceMock { get; }

			public TestWebApplicationFactory()
			{
				CategoryServiceMock = new Mock<ICategoryService>();
				ProductServiceMock = new Mock<IProductService>();
			}

			protected override void ConfigureWebHost(IWebHostBuilder builder)
			{
				builder.ConfigureServices(services =>
				{
					var catDesc = services.SingleOrDefault(d => d.ServiceType == typeof(ICategoryService));
					if (catDesc != null)
						services.Remove(catDesc);

					var prodDesc = services.SingleOrDefault(d => d.ServiceType == typeof(IProductService));
					if (prodDesc != null)
						services.Remove(prodDesc);

					services.AddSingleton<ICategoryService>(sp => CategoryServiceMock.Object);
					services.AddSingleton<IProductService>(sp => ProductServiceMock.Object);
					
					// Make all Authorize checks succeed in tests
					services.AddSingleton<IAuthorizationMiddlewareResultHandler, TestAuthorizationMiddlewareResultHandler>();
				});
			}
		}
		
		private class TestAuthorizationMiddlewareResultHandler : IAuthorizationMiddlewareResultHandler
		{
			private readonly AuthorizationMiddlewareResultHandler defaultHandler = new();

			public async Task HandleAsync(RequestDelegate next, HttpContext context, AuthorizationPolicy policy, PolicyAuthorizationResult authorizeResult)
			{
				// If authorization succeeded, behave normally.
				if (authorizeResult.Succeeded)
				{
					await defaultHandler.HandleAsync(next, context, policy, authorizeResult);
					return;
				}

				// For tests: ignore failures (both Forbid and Challenge) and continue the pipeline.
				// This makes [Authorize] effectively a no-op for test requests.
				await next(context);
			}
		}
	}
}