using System.Text.RegularExpressions;

namespace CartService.Infrastructure
{
	public class CartItem
	{
		private int id;
		private string name = string.Empty;
		private string? imageUri;
		private string? imageAltText;
		private decimal price;
		private uint quantity;

		public CartItem() { }

		public CartItem(int id, string name, decimal price, uint quantity, string? imageUri = null, string? imageAltText = null)
		{
			Id = id;
			Name = name;
			Price = price;
			Quantity = quantity;
			ImageUri = imageUri;
			ImageAltText = imageAltText;
		}

		public int Id
		{
			get => id;
			set
			{
				if (value <= 0)
					throw new ArgumentException("Id must be a positive value.", nameof(Id));
				id = value;
			}
		}

		public string Name
		{
			get => name;
			set
			{
				var name = (value ?? string.Empty).Trim();
				if (string.IsNullOrEmpty(name))
					throw new ArgumentException("Name is required.", nameof(Name));
				if (Regex.IsMatch(name, "<[^>]+>"))
					throw new ArgumentException("Name must be plain text without HTML tags.", nameof(Name));
				if (name.Any(char.IsControl))
					throw new ArgumentException("Name contains invalid control characters.", nameof(Name));

				this.name = name;
			}
		}

		public string? ImageUri
		{
			get => imageUri;
			set
			{
				if (string.IsNullOrWhiteSpace(value))
				{
					imageUri = null;
					return;
				}

				if (!Uri.TryCreate(value, UriKind.Absolute, out var uri) ||
					!(uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
				{
					throw new ArgumentException("ImageUri must be a valid absolute URL using http or https.", nameof(ImageUri));
				}

				imageUri = value;
			}
		}

		public string? ImageAltText
		{
			get => imageAltText;
			set
			{
				if (string.IsNullOrWhiteSpace(value))
				{
					imageAltText = null;
					return;
				}

				var alt = value.Trim();
				if (Regex.IsMatch(alt, "<[^>]+>"))
					throw new ArgumentException("ImageAltText must be plain text without HTML tags.", nameof(ImageAltText));
				if (alt.Any(char.IsControl))
					throw new ArgumentException("ImageAltText contains invalid control characters.", nameof(ImageAltText));

				imageAltText = alt;
			}
		}

		public decimal Price
		{
			get => price;
			set
			{
				if (value < 0m)
					throw new ArgumentException("Price must be non-negative.", nameof(Price));
				price = value;
			}
		}

		public uint Quantity
		{
			get => quantity;
			set
			{
				if (value <= 0)
					throw new ArgumentException("Quantity must be a positive value.", nameof(Quantity));
				quantity = value;
			}
		}
	}
}