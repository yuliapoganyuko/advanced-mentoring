using CatalogService.Core;
using CatalogService.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CatalogService.Infrastructure.Repositories
{
	public class ProductRepository : IProductRepository
	{
		CatalogContext context;

		public ProductRepository(CatalogContext context)
		{
			this.context = context;
		}

		public async Task AddAsync(Product product, CancellationToken cancellationToken = default)
		{
			await context.Products.AddAsync(product, cancellationToken);
			await context.SaveChangesAsync(cancellationToken);
		}

		public async Task<bool> DeleteAsync(int productId, CancellationToken cancellationToken = default)
		{
			var product = await context.Products
				.FirstOrDefaultAsync(c => c.Id == productId, cancellationToken);

			if (product is null)
				return false;

			context.Products.Remove(product);
			await context.SaveChangesAsync(cancellationToken);
			return true;
		}

		public async Task<Product?> GetAsync(int productId, CancellationToken cancellationToken = default)
		{
			return await context.Products.FindAsync(productId, cancellationToken);
		}

		public async Task<IEnumerable<Product>> ListAsync(CancellationToken cancellationToken = default)
		{
			return await context.Products.ToListAsync(cancellationToken);
		}

		public async Task UpdateAsync(Product product, CancellationToken cancellationToken = default)
		{
			context.Products.Update(product);
			await context.SaveChangesAsync(cancellationToken);
		}
	}
}
