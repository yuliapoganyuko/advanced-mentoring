using CatalogService.Infrastructure.Repositories;
using CatalogService.Core;
using Microsoft.EntityFrameworkCore;

namespace CatalogService.Infrastructure.Tests
{
	[TestClass]
	public class CategoryRepositoryTests
	{
		private CatalogContext? catalogContext;
		private CategoryRepository? categoryRepository;

		[TestInitialize]
		public void Initialize()
		{
			var options = new DbContextOptionsBuilder<CatalogContext>()
				.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
				.Options;

			catalogContext = new CatalogContext(options);
			categoryRepository = new CategoryRepository(catalogContext);
		}

		[TestCleanup]
		public void Cleanup()
		{
			catalogContext?.Dispose();
		}

		[TestMethod]
		public async Task AddAsync_AddsCategoryToDatabase()
		{
			var category = new Category { Name = "New Category" };

			await categoryRepository!.AddAsync(category, CancellationToken.None);

			var saved = await catalogContext!.Categories.FirstOrDefaultAsync(c => c.Name == "New Category", CancellationToken.None);
			Assert.IsNotNull(saved);
			Assert.AreEqual("New Category", saved!.Name);
		}

		[TestMethod]
		public async Task GetAsync_ReturnsCategory_WhenExists()
		{
			var category = new Category { Name = "GetMe" };
			catalogContext!.Categories.Add(category);
			await catalogContext.SaveChangesAsync();

			var result = await categoryRepository!.GetAsync(category.Id, CancellationToken.None);

			Assert.IsNotNull(result);
			Assert.AreEqual(category.Id, result!.Id);
			Assert.AreEqual("GetMe", result.Name);
		}

		[TestMethod]
		public async Task GetAsync_ReturnsNull_WhenNotExists()
		{
			var result = await categoryRepository!.GetAsync(9999, CancellationToken.None);

			Assert.IsNull(result);
		}

		[TestMethod]
		public async Task ListAsync_ReturnsAllCategories()
		{
			var list = new[]
			{
				new Category { Name = "A" },
				new Category { Name = "B" }
			};
			catalogContext!.Categories.AddRange(list);
			await catalogContext.SaveChangesAsync();

			var result = await categoryRepository!.ListAsync(CancellationToken.None);

			CollectionAssert.AreEquivalent(list.Select(c => c.Name).ToList(), result.Select(c => c.Name).ToList());
		}

		[TestMethod]
		public async Task UpdateAsync_PersistsChanges()
		{
			var category = new Category { Name = "Before" };
			catalogContext!.Categories.Add(category);
			await catalogContext.SaveChangesAsync();

			category.Name = "After";
			await categoryRepository!.UpdateAsync(category, CancellationToken.None);

			var saved = await catalogContext.Categories.FindAsync(new object[] { category.Id }, CancellationToken.None);
			Assert.IsNotNull(saved);
			Assert.AreEqual("After", saved!.Name);
		}

		[TestMethod]
		public async Task DeleteAsync_RemovesExistingCategory_ReturnsTrue()
		{
			var category = new Category { Name = "ToDelete" };
			catalogContext!.Categories.Add(category);
			await catalogContext.SaveChangesAsync();

			var result = await categoryRepository!.DeleteAsync(category.Id, CancellationToken.None);

			Assert.IsTrue(result);
			var exists = await catalogContext.Categories.FindAsync(new object[] { category.Id }, CancellationToken.None);
			Assert.IsNull(exists);
		}

		[TestMethod]
		public async Task DeleteAsync_NonExistingCategory_ReturnsFalse()
		{
			var result = await categoryRepository!.DeleteAsync(123456, CancellationToken.None);

			Assert.IsFalse(result);
		}
	}
}