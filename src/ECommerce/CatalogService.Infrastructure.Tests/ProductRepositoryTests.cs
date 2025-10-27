using CatalogService.Infrastructure.Repositories;
using CatalogService.Core;
using Microsoft.EntityFrameworkCore;

namespace CatalogService.Infrastructure.Tests
{
	[TestClass]
	public class ProductRepositoryTests
	{
		private CatalogContext? catalogContext;
		private ProductRepository? productRepository;

		[TestInitialize]
		public void Initialize()
		{
			var options = new DbContextOptionsBuilder<CatalogContext>()
				.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
				.Options;

			catalogContext = new CatalogContext(options);
			productRepository = new ProductRepository(catalogContext);
		}

		[TestCleanup]
		public void Cleanup()
		{
			catalogContext?.Dispose();
		}

		[TestMethod]
		public async Task AddAsync_AddsProductToDatabase()
		{
			var category = new Category { Name = "CatForAdd" };
			catalogContext!.Categories.Add(category);
			await catalogContext.SaveChangesAsync();

			var product = new Product
			{
				Name = "New Product",
				CategoryId = category.Id,
				Price = 9.99m,
				Amount = 5,
				Description = "Desc"
			};

			await productRepository!.AddAsync(product, CancellationToken.None);

			var saved = await catalogContext!.Products.FirstOrDefaultAsync(p => p.Name == "New Product", CancellationToken.None);
			Assert.IsNotNull(saved);
			Assert.AreEqual("New Product", saved!.Name);
			Assert.AreEqual(category.Id, saved.CategoryId);
			Assert.AreEqual(9.99m, saved.Price);
			Assert.AreEqual((uint)5, saved.Amount);
		}

		[TestMethod]
		public async Task GetAsync_ReturnsProduct_WhenExists()
		{
			var category = new Category { Name = "CatForGet" };
			catalogContext!.Categories.Add(category);
			await catalogContext.SaveChangesAsync();

			var product = new Product
			{
				Name = "GetMe",
				CategoryId = category.Id,
				Price = 1.23m,
				Amount = 2
			};
			catalogContext.Products.Add(product);
			await catalogContext.SaveChangesAsync();

			var result = await productRepository!.GetAsync(product.Id, CancellationToken.None);

			Assert.IsNotNull(result);
			Assert.AreEqual(product.Id, result!.Id);
			Assert.AreEqual("GetMe", result.Name);
			Assert.AreEqual(category.Id, result.CategoryId);
		}

		[TestMethod]
		public async Task GetAsync_ReturnsNull_WhenNotExists()
		{
			var result = await productRepository!.GetAsync(9999, CancellationToken.None);

			Assert.IsNull(result);
		}

		[TestMethod]
		public async Task ListAsync_ReturnsAllProducts()
		{
			var category = new Category { Name = "CatForList" };
			catalogContext!.Categories.Add(category);
			await catalogContext.SaveChangesAsync();

			var list = new[]
			{
				new Product { Name = "A", CategoryId = category.Id, Price = 1m, Amount = 1 },
				new Product { Name = "B", CategoryId = category.Id, Price = 2m, Amount = 2 }
			};
			catalogContext!.Products.AddRange(list);
			await catalogContext.SaveChangesAsync();

			var result = await productRepository!.ListAsync(CancellationToken.None);

			CollectionAssert.AreEquivalent(list.Select(p => p.Name).ToList(), result.Select(p => p.Name).ToList());
		}

		[TestMethod]
		public async Task UpdateAsync_PersistsChanges()
		{
			var category = new Category { Name = "CatForUpdate" };
			catalogContext!.Categories.Add(category);
			await catalogContext.SaveChangesAsync();

			var product = new Product { Name = "Before", CategoryId = category.Id, Price = 5m, Amount = 3 };
			catalogContext!.Products.Add(product);
			await catalogContext.SaveChangesAsync();

			product.Name = "After";
			product.Price = 7.5m;
			await productRepository!.UpdateAsync(product, CancellationToken.None);

			var saved = await catalogContext.Products.FindAsync(new object[] { product.Id }, CancellationToken.None);
			Assert.IsNotNull(saved);
			Assert.AreEqual("After", saved!.Name);
			Assert.AreEqual(7.5m, saved.Price);
		}

		[TestMethod]
		public async Task DeleteAsync_RemovesExistingProduct_ReturnsTrue()
		{
			var category = new Category { Name = "CatForDelete" };
			catalogContext!.Categories.Add(category);
			await catalogContext.SaveChangesAsync();

			var product = new Product { Name = "ToDelete", CategoryId = category.Id, Price = 3m, Amount = 1 };
			catalogContext!.Products.Add(product);
			await catalogContext.SaveChangesAsync();

			var result = await productRepository!.DeleteAsync(product.Id, CancellationToken.None);

			Assert.IsTrue(result);
			var exists = await catalogContext.Products.FindAsync(new object[] { product.Id }, CancellationToken.None);
			Assert.IsNull(exists);
		}

		[TestMethod]
		public async Task DeleteAsync_NonExistingProduct_ReturnsFalse()
		{
			var result = await productRepository!.DeleteAsync(123456, CancellationToken.None);

			Assert.IsFalse(result);
		}
	}
}