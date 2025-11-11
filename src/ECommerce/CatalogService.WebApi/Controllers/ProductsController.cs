using CatalogService.Core;
using CatalogService.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CatalogService.WebApi.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class ProductsController : ControllerBase
	{
		IProductService productService;

		public ProductsController(IProductService productService)
		{
			this.productService = productService;
		}

		/// <summary>
		/// Lists products.
		/// </summary>
		/// <returns>List of product models</returns>
		/// <response code="200">Request was performed successfully.</response>
		[HttpGet]
		[ProducesResponseType(StatusCodes.Status200OK)]
		public async Task<ActionResult<IEnumerable<ProductDto>>> List(
			[FromQuery] int? pageNumber = null,
			[FromQuery] int? pageSize = null,
			[FromQuery] int? categoryId = null)
		{
			var items = await productService.ListAsync(categoryId);

			if (pageNumber == null || pageSize == null)
			{
				return Ok(items);
			}

			// normalize pagination parameters
			pageNumber = Math.Max(1, pageNumber.Value);
			pageSize = Math.Clamp(pageSize.Value, 1, 100);

			var paged = items
				.Skip((pageNumber.Value - 1) * pageSize.Value)
				.Take(pageSize.Value);

			return Ok(paged);
		}

		/// <summary>
		/// Gets product info.
		/// </summary>
		/// <param name="id">Product id</param>
		/// <returns>Product model</returns>
		/// <response code="200">Request was performed successfully.</response>
		/// <response code="400">If id is not a positive value</response>
		[HttpGet("{id}")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		public async Task<ActionResult<ProductDto>> Get(int id)
		{
			if (id <= 0)
			{
				return BadRequest();
			}

			return Ok(await productService.GetAsync(id));
		}

		/// <summary>
		/// Adds new product.
		/// </summary>
		/// <param name="product">Product model to add</param>
		/// <returns></returns>
		/// <response code="201">Request was performed successfully.</response>
		/// <response code="400">If product model is invalid</response>
		[HttpPost]
		[ProducesResponseType(StatusCodes.Status201Created)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		public async Task<IActionResult> Post([FromBody] ProductDto product)
		{
			if (product == null || 
				product.Id > 0 ||
				string.IsNullOrEmpty(product.Name) ||
				product.CategoryId <= 0 || 
				product.Price < 0 ||
				product.Amount <= 0)
			{
				return BadRequest();
			}

			var added = await productService.AddAsync(product);
			return CreatedAtAction(nameof(Get), new { id = added?.Id }, added);
		}
		
		/// <summary>
		/// Updates existing product.
		/// </summary>
		/// <param name="id">Id of a product to update</param>
		/// <param name="category">Product model to update</param>
		/// <returns></returns>
		/// <response code="200">Request was performed successfully.</response>
		/// <response code="400">If product model is invalid or id is not a positive value</response>
		[HttpPut("{id}")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		public async Task<IActionResult> Put(int id, [FromBody] ProductDto product)
		{
			if (product == null || 
				id <= 0 ||
				product.Id != id ||
				string.IsNullOrEmpty(product.Name) ||
				product.CategoryId <= 0 || 
				product.Price < 0 ||
				product.Amount <= 0)
			{
				return BadRequest();
			}

			await productService.UpdateAsync(product);
			return Ok();
		}
		
		/// <summary>
		/// Deletes product.
		/// </summary>
		/// <param name="id">Id of a product to delete</param>
		/// <returns></returns>
		/// <response code="204">Request was performed successfully.</response>
		/// <response code="400">If id is not a positive value</response>
		[HttpDelete("{id}")]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		public async Task<IActionResult> Delete(int id)
		{
			if (id <= 0)
			{
				return BadRequest();
			}

			await productService.DeleteAsync(id);
			return NoContent();
		}
	}
}
