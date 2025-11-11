namespace CatalogService.Core.Interfaces
{
	public interface ICategoryService
	{
		Task<CategoryDto?> GetAsync(int categoryId, CancellationToken cancellationToken = default);
		Task<IEnumerable<CategoryDto>> ListAsync(CancellationToken cancellationToken = default);
		Task<CategoryDto?> AddAsync(CategoryDto category, CancellationToken cancellationToken = default);
		Task UpdateAsync(CategoryDto category, CancellationToken cancellationToken = default);
		Task<bool> DeleteAsync(int categoryId, CancellationToken cancellationToken = default);
	}
}
