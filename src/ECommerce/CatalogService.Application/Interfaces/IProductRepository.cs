namespace CatalogService.Core.Interfaces
{
	public interface IProductRepository
	{
		Task<Product?> GetAsync(int productId, CancellationToken cancellationToken = default);
		Task<IEnumerable<Product>> ListAsync(CancellationToken cancellationToken = default);
		Task<Product?> AddAsync(Product product, CancellationToken cancellationToken = default);
		Task UpdateAsync(Product product, CancellationToken cancellationToken = default);
		Task<bool> DeleteAsync(int productId, CancellationToken cancellationToken = default);
	}
}