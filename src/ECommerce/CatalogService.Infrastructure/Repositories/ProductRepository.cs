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

		public async Task<Product?> AddAsync(Product product, CancellationToken cancellationToken = default)
		{
			var added = await context.Products.AddAsync(product, cancellationToken);
			await context.SaveChangesAsync(cancellationToken);
			return added.Entity;
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
			var local = context.Set<Product>()
				.Local
				.FirstOrDefault(e => e.Id == product.Id);
	
			if (local != null)
			{
				context.Entry(local).State = EntityState.Detached; // Detach the tracked entity
			}

			context.Products.Update(product);
			await context.SaveChangesAsync(cancellationToken);
		}
	}
}
