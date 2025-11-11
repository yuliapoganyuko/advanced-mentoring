using AutoMapper;
using CatalogService.Core.Interfaces;
using CatalogService.Core.Services;
using Moq;

namespace CatalogService.Core.Tests
{
	[TestClass]
	public class CategoryServiceTests
	{
		private Mock<ICategoryRepository> repositoryMock = null!;
		private Mock<IMapper> mapperMock = null!;
		private CategoryService categoryService = null!;

		[TestInitialize]
		public void TestInitialize()
		{
			repositoryMock = new Mock<ICategoryRepository>(MockBehavior.Strict);
			mapperMock = new Mock<IMapper>(MockBehavior.Strict);
			categoryService = new CategoryService(repositoryMock.Object, mapperMock.Object);
		}

		[TestCleanup]
		public void TestCleanup()
		{
			repositoryMock.VerifyAll();
			mapperMock.VerifyAll();
		}

		[TestMethod]
		public async Task AddAsync_NullCategory_ThrowsArgumentNullException()
		{
			await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => categoryService.AddAsync(null!, CancellationToken.None));
		}

		[TestMethod]
		public async Task AddAsync_ValidCategory_MapsAndCallsRepository()
		{
			var dto = new CategoryDto { Id = 7, Name = "MyCategory" };
			var entity = new Category { Id = 7, Name = "MyCategory" };

			mapperMock.Setup(m => m.Map<Category>(dto)).Returns(entity);
			repositoryMock.Setup(r => r.AddAsync(entity, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
			mapperMock.Setup(m => m.Map<CategoryDto>(entity)).Returns(dto);

			var result = await categoryService.AddAsync(dto, CancellationToken.None);

			Assert.IsNotNull(result);
			Assert.AreEqual(dto.Id, result!.Id);
			Assert.AreEqual(dto.Name, result.Name);

			mapperMock.Verify(m => m.Map<Category>(dto), Times.Once);
			repositoryMock.Verify(r => r.AddAsync(entity, It.IsAny<CancellationToken>()), Times.Once);
			mapperMock.Verify(m => m.Map<CategoryDto>(entity), Times.Once);
		}

		[TestMethod]
		public async Task UpdateAsync_NullCategory_ThrowsArgumentNullException()
		{
			await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => categoryService.UpdateAsync(null!, CancellationToken.None));
		}

		[TestMethod]
		public async Task UpdateAsync_ValidCategory_MapsAndCallsRepository()
		{
			var dto = new CategoryDto { Id = 3, Name = "Updated" };
			var entity = new Category { Id = 3, Name = "Updated" };

			mapperMock.Setup(m => m.Map<Category>(dto)).Returns(entity);
			repositoryMock.Setup(r => r.UpdateAsync(entity, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

			await categoryService.UpdateAsync(dto, CancellationToken.None);

			mapperMock.Verify(m => m.Map<Category>(dto), Times.Once);
			repositoryMock.Verify(r => r.UpdateAsync(entity, It.IsAny<CancellationToken>()), Times.Once);
		}

		[TestMethod]
		public async Task DeleteAsync_NegativeId_ThrowsArgumentOutOfRangeException()
		{
			await Assert.ThrowsExceptionAsync<ArgumentOutOfRangeException>(() => categoryService.DeleteAsync(-1, CancellationToken.None));
		}

		[TestMethod]
		public async Task DeleteAsync_ValidId_ReturnsRepositoryResult()
		{
			const int id = 5;
			repositoryMock.Setup(r => r.DeleteAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(true);

			var result = await categoryService.DeleteAsync(id, CancellationToken.None);

			Assert.IsTrue(result);
			repositoryMock.Verify(r => r.DeleteAsync(id, It.IsAny<CancellationToken>()), Times.Once);
		}

		[TestMethod]
		public async Task GetAsync_NotFound_ReturnsNull()
		{
			const int id = 11;
			repositoryMock.Setup(r => r.GetAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((Category?)null);

			var result = await categoryService.GetAsync(id, CancellationToken.None);

			Assert.IsNull(result);
			repositoryMock.Verify(r => r.GetAsync(id, It.IsAny<CancellationToken>()), Times.Once);
		}

		[TestMethod]
		public async Task GetAsync_Found_MapsAndReturnsDto()
		{
			const int id = 2;
			var entity = new Category { Id = id, Name = "Found" };
			var dto = new CategoryDto { Id = id, Name = "Found" };

			repositoryMock.Setup(r => r.GetAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
			mapperMock.Setup(m => m.Map<CategoryDto>(entity)).Returns(dto);

			var result = await categoryService.GetAsync(id, CancellationToken.None);

			Assert.IsNotNull(result);
			Assert.AreEqual(dto.Id, result!.Id);
			Assert.AreEqual(dto.Name, result.Name);

			repositoryMock.Verify(r => r.GetAsync(id, It.IsAny<CancellationToken>()), Times.Once);
			mapperMock.Verify(m => m.Map<CategoryDto>(entity), Times.Once);
		}

		[TestMethod]
		public async Task ListAsync_MapsAndReturnsDtoEnumerable()
		{
			var entities = new List<Category>
			{
				new Category { Id = 1, Name = "A" },
				new Category { Id = 2, Name = "B" }
			};

			var dtos = new List<CategoryDto>
			{
				new CategoryDto { Id = 1, Name = "A" },
				new CategoryDto { Id = 2, Name = "B" }
			};

			repositoryMock.Setup(r => r.ListAsync(It.IsAny<CancellationToken>())).ReturnsAsync(entities);
			mapperMock.Setup(m => m.Map<IEnumerable<CategoryDto>>(entities)).Returns(dtos);

			var result = await categoryService.ListAsync(CancellationToken.None);

			Assert.IsNotNull(result);
			CollectionAssert.AreEqual(new List<CategoryDto>(dtos), new List<CategoryDto>(result));
			repositoryMock.Verify(r => r.ListAsync(It.IsAny<CancellationToken>()), Times.Once);
			mapperMock.Verify(m => m.Map<IEnumerable<CategoryDto>>(entities), Times.Once);
		}
	}
}