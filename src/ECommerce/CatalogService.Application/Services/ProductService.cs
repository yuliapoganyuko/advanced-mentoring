using AutoMapper;
using CatalogService.Core.DTOs;
using CatalogService.Core.Interfaces;
using Messaging.Abstractions;

namespace CatalogService.Core.Services
{
	public class ProductService : IProductService
	{
		IProductRepository productRepository;
		IMapper mapper;
		IMessagePublisher messagePublisher;
		private readonly string queue;

		public ProductService(IProductRepository productRepository, IMapper mapper, IMessagePublisher messagePublisher, string queue)
		{
			this.productRepository = productRepository;
			this.messagePublisher = messagePublisher;
			this.mapper = mapper;
			this.queue = queue;
		}

		public async Task<ProductDto?> AddAsync(ProductDto product, CancellationToken cancellationToken = default)
		{
			if (product == null)
				throw new ArgumentNullException(nameof(product));
			var added = await productRepository.AddAsync(mapper.Map<Product>(product), cancellationToken);
			return added == null ? null : mapper.Map<ProductDto>(added);
		}

		public async Task<bool> DeleteAsync(int productId, CancellationToken cancellationToken = default)
		{
			if (productId <= 0)
				throw new ArgumentOutOfRangeException(nameof(productId));
			return await productRepository.DeleteAsync(productId, cancellationToken);
		}

		public async Task<ProductDto?> GetAsync(int productId, CancellationToken cancellationToken = default)
		{
			var result = await productRepository.GetAsync(productId, cancellationToken);
			return result == null ? null : mapper.Map<ProductDto>(result);
		}

		public async Task<IEnumerable<ProductDto>> ListAsync(int? categoryId = null, CancellationToken cancellationToken = default)
		{
			var products = await productRepository.ListAsync(cancellationToken);
			if (categoryId.HasValue)
			{
				products = products.Where(p => p.CategoryId == categoryId.Value);
			}

			return mapper.Map<IEnumerable<ProductDto>>(products);
		}

		public async Task UpdateAsync(ProductDto product, CancellationToken cancellationToken = default)
		{
			if (product == null)
				throw new ArgumentNullException(nameof(product));

			var existing = await productRepository.GetAsync(product.Id, cancellationToken);
			await productRepository.UpdateAsync(mapper.Map<Product>(product), cancellationToken);

			if (existing != null && 
				(existing.Price != product.Price || existing.Name != product.Name || existing.Image != product.Image))
			{
				var productChanged = new ProductChangedDto
				{
					Id = product.Id,
					Name = product.Name,
					ImageUrl = product.Image,
					Price = product.Price
				};
				await messagePublisher.PublishAsync(queue, productChanged);
			}
		}
	}
}
