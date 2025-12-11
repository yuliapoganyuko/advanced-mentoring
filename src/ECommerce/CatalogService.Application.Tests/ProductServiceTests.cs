using AutoMapper;
using CatalogService.Core.Interfaces;
using CatalogService.Core.Services;
using Messaging.Abstractions;
using Moq;

namespace CatalogService.Core.Tests
{
	[TestClass]
	public sealed class ProductServiceTests
	{
		private Mock<IProductRepository> repositoryMock = null!;
		private Mock<IMapper> mapperMock = null!;
		private Mock<IMessagePublisher> messagePublisherMock = null!;
		private const string queueName = "test-ecommerce";
		private ProductService productService = null!;

		[TestInitialize]
		public void TestInitialize()
		{
			repositoryMock = new Mock<IProductRepository>();
			mapperMock = new Mock<IMapper>();
			messagePublisherMock = new Mock<IMessagePublisher>();
			productService = new ProductService(repositoryMock.Object, mapperMock.Object, messagePublisherMock.Object, queueName);
		}

		[TestCleanup]
		public void TestCleanup()
		{
			repositoryMock.VerifyAll();
			mapperMock.VerifyAll();
			messagePublisherMock.VerifyAll();
		}

		[TestMethod]
		public async Task AddAsync_NullProduct_ThrowsArgumentNullException()
		{
			await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => productService.AddAsync(null!, CancellationToken.None));
		}

		[TestMethod]
		public async Task AddAsync_ValidProduct_MapsAndCallsRepository()
		{
			var dto = new ProductDto { Name = "MyProduct", CategoryId = 1, Price = 9.99m, Amount = 10 }; // default 0
			var entity = new Product { Id = 7, Name = "MyProduct", CategoryId = 1, Price = 9.99m, Amount = 10 };
			var added = new ProductDto { Id = 7, Name = "MyProduct", CategoryId = 1, Price = 9.99m, Amount = 10 }; // default 0

			mapperMock.Setup(m => m.Map<Product>(dto)).Returns(entity);
			repositoryMock.Setup(r => r.AddAsync(entity, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
			mapperMock.Setup(m => m.Map<ProductDto>(entity)).Returns(added);

			var result = await productService.AddAsync(dto, CancellationToken.None);

			Assert.IsNotNull(result);
			Assert.AreNotEqual(dto.Id, result!.Id);
			Assert.AreEqual(dto.Name, result.Name);

			mapperMock.Verify(m => m.Map<Product>(dto), Times.Once);
			repositoryMock.Verify(r => r.AddAsync(entity, It.IsAny<CancellationToken>()), Times.Once);
			repositoryMock.Verify(r => r.AddAsync(entity, It.IsAny<CancellationToken>()), Times.Once);
		}

		[TestMethod]
		public async Task UpdateAsync_NullProduct_ThrowsArgumentNullException()
		{
			await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => productService.UpdateAsync(null!, CancellationToken.None));
		}

		[TestMethod]
		public async Task UpdateAsync_ValidProduct_MapsAndCallsRepository()
		{
			var dto = new ProductDto { Id = 3, Name = "Updated", CategoryId = 2, Price = 19.99m, Amount = 5 };
			var entity = new Product { Id = 3, Name = "Updated", CategoryId = 2, Price = 19.99m, Amount = 5 };

			mapperMock.Setup(m => m.Map<Product>(dto)).Returns(entity);
			repositoryMock.Setup(r => r.UpdateAsync(entity, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

			await productService.UpdateAsync(dto, CancellationToken.None);

			mapperMock.Verify(m => m.Map<Product>(dto), Times.Once);
			repositoryMock.Verify(r => r.UpdateAsync(entity, It.IsAny<CancellationToken>()), Times.Once);
		}

		[TestMethod]
		public async Task DeleteAsync_NegativeId_ThrowsArgumentOutOfRangeException()
		{
			await Assert.ThrowsExceptionAsync<ArgumentOutOfRangeException>(() => productService.DeleteAsync(-1, CancellationToken.None));
		}

		[TestMethod]
		public async Task DeleteAsync_ValidId_ReturnsRepositoryResult()
		{
			const int id = 5;
			repositoryMock.Setup(r => r.DeleteAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(true);

			var result = await productService.DeleteAsync(id, CancellationToken.None);

			Assert.IsTrue(result);
			repositoryMock.Verify(r => r.DeleteAsync(id, It.IsAny<CancellationToken>()), Times.Once);
		}

		[TestMethod]
		public async Task GetAsync_NotFound_ReturnsNull()
		{
			const int id = 11;
			repositoryMock.Setup(r => r.GetAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((Product?)null);

			var result = await productService.GetAsync(id, CancellationToken.None);

			Assert.IsNull(result);
			repositoryMock.Verify(r => r.GetAsync(id, It.IsAny<CancellationToken>()), Times.Once);
		}

		[TestMethod]
		public async Task GetAsync_Found_MapsAndReturnsDto()
		{
			const int id = 2;
			var entity = new Product { Id = id, Name = "Found", CategoryId = 1, Price = 5.5m, Amount = 3 };
			var dto = new ProductDto { Id = id, Name = "Found", CategoryId = 1, Price = 5.5m, Amount = 3 };

			repositoryMock.Setup(r => r.GetAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
			mapperMock.Setup(m => m.Map<ProductDto>(entity)).Returns(dto);

			var result = await productService.GetAsync(id, CancellationToken.None);

			Assert.IsNotNull(result);
			Assert.AreEqual(dto.Id, result!.Id);
			Assert.AreEqual(dto.Name, result.Name);
			Assert.AreEqual(dto.Price, result.Price);
			Assert.AreEqual(dto.Amount, result.Amount);

			repositoryMock.Verify(r => r.GetAsync(id, It.IsAny<CancellationToken>()), Times.Once);
			mapperMock.Verify(m => m.Map<ProductDto>(entity), Times.Once);
		}

		[TestMethod]
		public async Task ListAsync_MapsAndReturnsDtoEnumerable()
		{
			var entities = new List<Product>
			{
				new Product { Id = 1, Name = "A", CategoryId = 1, Price = 1m, Amount = 1 },
				new Product { Id = 2, Name = "B", CategoryId = 1, Price = 2m, Amount = 2 }
			};

			var dtos = new List<ProductDto>
			{
				new ProductDto { Id = 1, Name = "A", CategoryId = 1, Price = 1m, Amount = 1 },
				new ProductDto { Id = 2, Name = "B", CategoryId = 1, Price = 2m, Amount = 2 }
			};

			repositoryMock.Setup(r => r.ListAsync(It.IsAny<CancellationToken>())).ReturnsAsync(entities);
			mapperMock.Setup(m => m.Map<IEnumerable<ProductDto>>(entities)).Returns(dtos);

			var result = await productService.ListAsync(null, CancellationToken.None);

			Assert.IsNotNull(result);
			CollectionAssert.AreEqual(new List<ProductDto>(dtos), new List<ProductDto>(result));
			repositoryMock.Verify(r => r.ListAsync(It.IsAny<CancellationToken>()), Times.Once);
			mapperMock.Verify(m => m.Map<IEnumerable<ProductDto>>(entities), Times.Once);
		}
	}
}