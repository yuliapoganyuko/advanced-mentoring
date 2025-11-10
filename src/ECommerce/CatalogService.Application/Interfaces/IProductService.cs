namespace CatalogService.Core.Interfaces
{
	public interface IProductService
	{
		Task<ProductDto?> GetAsync(int productId, CancellationToken cancellationToken = default);
		Task<IEnumerable<ProductDto>> ListAsync(int? categoryId = null, CancellationToken cancellationToken = default);
		Task<ProductDto?> AddAsync(ProductDto product, CancellationToken cancellationToken = default);
		Task UpdateAsync(ProductDto product, CancellationToken cancellationToken = default);
		Task<bool> DeleteAsync(int productId, CancellationToken cancellationToken = default);
	}
}