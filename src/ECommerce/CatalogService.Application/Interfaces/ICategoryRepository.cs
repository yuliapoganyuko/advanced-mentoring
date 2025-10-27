namespace CatalogService.Core.Interfaces
{
	public interface ICategoryRepository
	{
		Task<Category?> GetAsync(int categoryId, CancellationToken cancellationToken = default);
		Task<IEnumerable<Category>> ListAsync(CancellationToken cancellationToken = default);
		Task AddAsync(Category category, CancellationToken cancellationToken = default);
		Task UpdateAsync(Category category, CancellationToken cancellationToken = default);
		Task<bool> DeleteAsync(int categoryId, CancellationToken cancellationToken = default);
	}
}