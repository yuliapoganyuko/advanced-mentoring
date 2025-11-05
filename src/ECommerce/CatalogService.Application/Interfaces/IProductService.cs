namespace CatalogService.Core.Interfaces
{
	public interface IProductService
	{
		Task<ProductDto?> GetAsync(int productId, CancellationToken cancellationToken = default);
		Task<IEnumerable<ProductDto>> ListAsync(CancellationToken cancellationToken = default);
		Task AddAsync(ProductDto product, CancellationToken cancellationToken = default);
		Task UpdateAsync(ProductDto product, CancellationToken cancellationToken = default);
		Task<bool> DeleteAsync(int productId, CancellationToken cancellationToken = default);
	}
}