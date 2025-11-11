using Asp.Versioning;
using CartService.Core;
using Microsoft.AspNetCore.Mvc;

namespace CartService.WebApi.Controllers
{
	[ApiVersion("1")]
	[ApiVersion("2")]
	[ApiController]
	[Route("api/v{v:apiVersion}/[controller]")]
	public class CartsController : ControllerBase
	{
		ICartService cartService;
		public CartsController(ICartService cartService)
		{
			this.cartService = cartService;
		}

		/// <summary>
		/// Gets cart info.
		/// </summary>
		/// <param name="cartId">Cart unique key</param>
		/// <returns>Cart model</returns>
		/// <response code="200">Request was performed successfully.</response>
		/// <response code="400">If cartId is null or not GUID</response>
		[MapToApiVersion("1")]
		[HttpGet("{cartId}")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		public ActionResult<IEnumerable<CartItemDto>?> Get(string cartId)
		{
			Guid guid;
			if (string.IsNullOrEmpty(cartId) || !Guid.TryParse(cartId, out guid))
			{
				return BadRequest();
			}

			var cartItems = cartService.GetCartItems(guid);
			CartDto cart = new CartDto
			{
				Id = guid,
				Items = cartItems?.ToList() ?? new List<CartItemDto>()
			};
			return Ok(cart);
		}
		
		/// <summary>
		/// Gets cart info.
		/// </summary>
		/// <param name="cartId">Cart unique key</param>
		/// <returns>The list of cart items</returns>
		/// <response code="200">Request was performed successfully.</response>
		/// <response code="400">If cartId is null or not GUID</response>
		[MapToApiVersion("2")]
		[HttpGet("{cartId}")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		public ActionResult<IEnumerable<CartItemDto>?> GetV2(string cartId)
		{
			Guid guid;
			if (string.IsNullOrEmpty(cartId) || !Guid.TryParse(cartId, out guid))
			{
				return BadRequest();
			}

			return Ok(cartService.GetCartItems(guid));
		}

		/// <summary>
		/// Adds an item to a cart.
		/// </summary>
		/// <param name="cartId">Cart unique key</param>
		/// <param name="item">Cart item model to add</param>
		/// <returns></returns>
		/// <remarks>
		/// Sample request:
		///
		///     POST /carts/{cartId}/items
		///     {
		///        "id": 1,
		///        "name": "Item #1",
		///        "price": 9.99,
		///        "quantity": 1
		///     }
		///
		/// </remarks>
		/// <response code="200">Request was performed successfully.</response>
		/// <response code="400">If the item is null or cartId is null or not GUID</response>
		[HttpPost("{cartId}/items")]
		[MapToApiVersion("1")]
		[MapToApiVersion("2")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		public IActionResult Post(string cartId, [FromBody] CartItemDto item)
		{
			if (item == null ||
				item.Id < 0 ||
				string.IsNullOrEmpty(item.Name) ||
				item.Price <= 0 ||
				item.Quantity < 1)
			{
				return BadRequest();
			}

			Guid guid;
			if (string.IsNullOrEmpty(cartId) || !Guid.TryParse(cartId, out guid))
			{
				return BadRequest();
			}

			cartService.AddCartItem(guid, item);

			return Ok();
		}

		/// <summary>
		/// Deletes an item from a cart.
		/// </summary>
		/// <param name="cartId">Cart unique key</param>
		/// <param name="itemId">Id of a cart item to delete</param>
		/// <returns></returns>
		/// <response code="200">Request was performed successfully.</response>
		/// <response code="400">If cartId is null or not GUID</response>
		[HttpDelete("{cartId}/items/{itemId}")]
		[MapToApiVersion("1")]
		[MapToApiVersion("2")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		public IActionResult Delete(string cartId, int itemId)
		{
			Guid guid;
			if (string.IsNullOrEmpty(cartId) || !Guid.TryParse(cartId, out guid))
			{
				return BadRequest();
			}

			return Ok(cartService.RemoveCartItem(guid, itemId));
		}
	}
}
