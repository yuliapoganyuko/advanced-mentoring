using Newtonsoft.Json;

namespace CartService.Infrastructure
{
	public class Cart
	{
		[JsonIgnore]
		public Guid Id { get; set; }

		[JsonProperty("id")]
		public string IdString
		{
			get => Id.ToString();
			set => Id = Guid.TryParse(value, out var g) ? g : Guid.Empty;
		}

		public ICollection<CartItem> Items { get; set; } = new List<CartItem>();
	}
}
