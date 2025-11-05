using AutoMapper;
using CatalogService.Core.Interfaces;

namespace CatalogService.Core.Services
{
	public class CategoryService : ICategoryService
	{
		ICategoryRepository categoryRepository;
		IMapper mapper;

		public CategoryService(ICategoryRepository categoryRepository, IMapper mapper)
		{
			this.categoryRepository = categoryRepository;
			this.mapper = mapper;
		}

		public async Task AddAsync(CategoryDto category, CancellationToken cancellationToken = default)
		{
			if (category == null)
				throw new ArgumentNullException(nameof(category));
			await categoryRepository.AddAsync(mapper.Map<Category>(category), cancellationToken);
		}

		public async Task<bool> DeleteAsync(int categoryId, CancellationToken cancellationToken = default)
		{
			if (categoryId <= 0)
				throw new ArgumentOutOfRangeException(nameof(categoryId));
			return await categoryRepository.DeleteAsync(categoryId, cancellationToken);
		}

		public async Task<CategoryDto?> GetAsync(int categoryId, CancellationToken cancellationToken = default)
		{
			var result = await categoryRepository.GetAsync(categoryId, cancellationToken);
			return result == null ? null : mapper.Map<CategoryDto>(result);
		}

		public async Task<IEnumerable<CategoryDto>> ListAsync(CancellationToken cancellationToken = default)
		{
			return mapper.Map<IEnumerable<CategoryDto>>(await categoryRepository.ListAsync(cancellationToken));
		}

		public async Task UpdateAsync(CategoryDto category, CancellationToken cancellationToken = default)
		{
			if (category == null)
				throw new ArgumentNullException(nameof(category));
			await categoryRepository.UpdateAsync(mapper.Map<Category>(category), cancellationToken);
		}
	}
}
