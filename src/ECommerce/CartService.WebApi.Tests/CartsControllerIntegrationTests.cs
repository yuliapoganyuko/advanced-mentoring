using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using CartService.Core;
using CartService.WebApi.Controllers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace CartService.WebApi.Tests
{
	[TestClass]
	public sealed class CartsControllerIntegrationTests
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
		public async Task GetV1_ReturnsBadRequest_WhenCartIdInvalid()
		{
			// Act
			var resp = await client!.GetAsync("/api/v1/Carts/not-a-guid");

			// Assert
			Assert.AreEqual(HttpStatusCode.BadRequest, resp.StatusCode);
		}

		[TestMethod]
		public async Task GetV1_ReturnsCartDto_WhenServiceReturnsItems()
		{
			// Arrange
			var id = Guid.NewGuid();
			var items = new List<CartItemDto>
			{
				new CartItemDto { Id = 1, Name = "A", Price = 1m, Quantity = 1 },
				new CartItemDto { Id = 2, Name = "B", Price = 2m, Quantity = 2 }
			};
			factory!.CartServiceMock.Setup(s => s.GetCartItemsAsync(id)).ReturnsAsync(items);

			// Act
			var resp = await client!.GetAsync($"/api/v1/Carts/{id}");

			// Assert
			Assert.AreEqual(HttpStatusCode.OK, resp.StatusCode);
			var content = await resp.Content.ReadAsStringAsync();
			var cart = JsonSerializer.Deserialize<CartDto>(content, jsonOptions);
			Assert.IsNotNull(cart);
			Assert.AreEqual(id, cart!.Id);
			Assert.IsNotNull(cart.Items);
			Assert.AreEqual(items.Count, cart.Items.Count, "Item count mismatch");
			for (int i = 0; i < items.Count; i++)
			{
				var expected = items[i];
				var actual = cart.Items.ElementAt(i);
				Assert.AreEqual(expected.Id, actual.Id, $"Item[{i}].Id mismatch");
				Assert.AreEqual(expected.Name, actual.Name, $"Item[{i}].Name mismatch");
				Assert.AreEqual(expected.Price, actual.Price, $"Item[{i}].Price mismatch");
				Assert.AreEqual(expected.Quantity, actual.Quantity, $"Item[{i}].Quantity mismatch");
			}
			factory.CartServiceMock.Verify(s => s.GetCartItemsAsync(id), Times.Once);
		}

		[TestMethod]
		public async Task GetV1_ReturnsEmptyItems_WhenServiceReturnsNull()
		{
			// Arrange
			var id = Guid.NewGuid();
			factory!.CartServiceMock.Setup(s => s.GetCartItemsAsync(id)).ReturnsAsync((IEnumerable<CartItemDto>?)null);

			// Act
			var resp = await client!.GetAsync($"/api/v1/Carts/{id}");

			// Assert
			Assert.AreEqual(HttpStatusCode.OK, resp.StatusCode);
			var content = await resp.Content.ReadAsStringAsync();
			var cart = JsonSerializer.Deserialize<CartDto>(content, jsonOptions);
			Assert.IsNotNull(cart);
			Assert.AreEqual(id, cart!.Id);
			Assert.IsNotNull(cart.Items);
			Assert.AreEqual(0, cart.Items.Count);
			factory.CartServiceMock.Verify(s => s.GetCartItemsAsync(id), Times.Once);
		}

		[TestMethod]
		public async Task GetV2_ReturnsBadRequest_WhenCartIdInvalid()
		{
			// Act
			var resp = await client!.GetAsync("/api/v2/Carts/");

			// The route requires a cartId segment; requesting without it will result in 404.
			// To trigger the controller's GUID validation, supply an invalid segment.
			resp = await client.GetAsync("/api/v2/Carts/invalid-guid");

			// Assert
			Assert.AreEqual(HttpStatusCode.BadRequest, resp.StatusCode);
		}

		[TestMethod]
		public async Task GetV2_ReturnsItemsList_WhenServiceReturnsItems()
		{
			// Arrange
			var id = Guid.NewGuid();
			var items = new List<CartItemDto>
			{
				new CartItemDto { Id = 3, Name = "X", Price = 10m, Quantity = 1 }
			};
			factory!.CartServiceMock.Setup(s => s.GetCartItemsAsync(id)).ReturnsAsync(items);

			// Act
			var resp = await client!.GetAsync($"/api/v2/Carts/{id}");

			// Assert
			Assert.AreEqual(HttpStatusCode.OK, resp.StatusCode);
			factory.CartServiceMock.Verify(s => s.GetCartItemsAsync(id), Times.Once);
		}

		[TestMethod]
		public async Task Post_ReturnsBadRequest_WhenItemIsInvalid()
		{
			// Arrange
			var id = Guid.NewGuid();
			var invalidItem = new CartItemDto { Id = 1, Name = "ValidName", Price = 0m, Quantity = 1 };
			var payload = JsonSerializer.Serialize(invalidItem);
			var content = new StringContent(payload, Encoding.UTF8, "application/json");

			// Act
			var resp = await client!.PostAsync($"/api/v2/Carts/{id}/items", content);

			// Assert
			Assert.AreEqual(HttpStatusCode.BadRequest, resp.StatusCode);
		}

		[TestMethod]
		public async Task Post_CallsServiceAndReturnsOk_WhenInputIsValid()
		{
			// Arrange
			var id = Guid.NewGuid();
			var item = new CartItemDto { Id = 7, Name = "NewItem", Price = 9.99m, Quantity = 2 };
			var payload = JsonSerializer.Serialize(item);
			var content = new StringContent(payload, Encoding.UTF8, "application/json");

			// Act
			var resp = await client!.PostAsync($"/api/v2/Carts/{id}/items", content);

			// Assert
			Assert.AreEqual(HttpStatusCode.OK, resp.StatusCode);
			factory!.CartServiceMock.Verify(s => s.AddCartItemAsync(id, It.Is<CartItemDto>(ci => ci.Id == item.Id && ci.Name == item.Name)), Times.Once);
		}

		[TestMethod]
		public async Task Delete_ReturnsBadRequest_WhenCartIdInvalid()
		{
			// Act
			var resp = await client!.DeleteAsync("/api/v2/Carts/bad-guid/items/1");

			// Assert
			Assert.AreEqual(HttpStatusCode.BadRequest, resp.StatusCode);
		}

		[TestMethod]
		public async Task Delete_ReturnsOkWithResult_WhenValid()
		{
			// Arrange
			var id = Guid.NewGuid();
			var itemId = 21;
			factory!.CartServiceMock.Setup(s => s.RemoveCartItemAsync(id, itemId)).ReturnsAsync(true);

			// Act
			var resp = await client!.DeleteAsync($"/api/v2/Carts/{id}/items/{itemId}");

			// Assert
			Assert.AreEqual(HttpStatusCode.OK, resp.StatusCode);
			var content = await resp.Content.ReadAsStringAsync();
			var value = JsonSerializer.Deserialize<bool>(content, jsonOptions);
			Assert.IsTrue(value);
			factory.CartServiceMock.Verify(s => s.RemoveCartItemAsync(id, itemId), Times.Once);
		}

		/// <summary>
		/// A small WebApplicationFactory that replaces the ICartService registration with a Mock instance
		/// so tests can run as integration tests while controlling the service behavior.
		/// </summary>
		private sealed class TestWebApplicationFactory : WebApplicationFactory<Program>
		{
			public Mock<ICartService> CartServiceMock { get; }

			public TestWebApplicationFactory()
			{
				CartServiceMock = new Mock<ICartService>();
			}

			protected override void ConfigureWebHost(IWebHostBuilder builder)
			{
				builder.ConfigureServices(services =>
				{
					// Remove existing ICartService registration (if any) and replace with our mock.
					var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(ICartService));
					if (descriptor != null)
						services.Remove(descriptor);

					services.AddSingleton<ICartService>(sp => CartServiceMock.Object);
				});
			}
		}
	}
}