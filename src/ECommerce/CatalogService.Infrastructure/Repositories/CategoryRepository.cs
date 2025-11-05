using CatalogService.Core;
using CatalogService.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CatalogService.Infrastructure.Repositories
{
	public class CategoryRepository : ICategoryRepository
	{
		private CatalogContext context;

		public CategoryRepository(CatalogContext context)
		{
			this.context = context;
		}

		public async Task AddAsync(Category category, CancellationToken cancellationToken = default)
		{
			await context.Categories.AddAsync(category, cancellationToken);
			await context.SaveChangesAsync(cancellationToken);
		}

		public async Task<bool> DeleteAsync(int categoryId, CancellationToken cancellationToken = default)
		{
			var category = await context.Categories
				.FirstOrDefaultAsync(c => c.Id == categoryId, cancellationToken);

			if (category is null)
				return false;

			context.Categories.Remove(category);
			await context.SaveChangesAsync(cancellationToken);
			return true;
		}

		public async Task<Category?> GetAsync(int categoryId, CancellationToken cancellationToken = default)
		{
			return await context.Categories.FindAsync(categoryId, cancellationToken);
		}

		public async Task<IEnumerable<Category>> ListAsync(CancellationToken cancellationToken = default)
		{
			return await context.Categories.ToListAsync(cancellationToken);
		}

		public async Task UpdateAsync(Category category, CancellationToken cancellationToken = default)
		{
			context.Categories.Update(category);
			await context.SaveChangesAsync(cancellationToken);
		}
	}
}
