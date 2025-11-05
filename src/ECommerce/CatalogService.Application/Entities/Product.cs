using System.Text.RegularExpressions;

namespace CatalogService.Core
{
	public class Product
	{
		private int id;
		private string name = string.Empty;
		private string? description;
		private string? image;
		private int categoryId;
		private decimal price;
		private uint amount;
		private Category? category;

		public Product() { }

		public Product(
			string name,
			int categoryId,
			decimal price,
			uint amount,
			string? description = null,
			string? image = null)
		{
			Name = name;
			CategoryId = categoryId;
			Price = price;
			Amount = amount;
			Description = description;
			Image = image;
		}

		public int Id
		{
			get => id;
			set
			{
				if (value < 0)
					throw new ArgumentException("Id must be a non-negative value.", nameof(Id));
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
				if (name.Length > 50)
					throw new ArgumentException("Name must be at most 50 characters.", nameof(Name));
				if (Regex.IsMatch(name, "<[^>]+>"))
					throw new ArgumentException("Name must be plain text without HTML tags.", nameof(Name));
				if (name.Any(char.IsControl))
					throw new ArgumentException("Name contains invalid control characters.", nameof(Name));

				this.name = name;
			}
		}

		public string? Description
		{
			get => description;
			set
			{
				description = string.IsNullOrWhiteSpace(value) ? null : value;
			}
		}

		public string? Image
		{
			get => image;
			set
			{
				if (string.IsNullOrWhiteSpace(value))
				{
					image = null;
					return;
				}

				if (!Uri.TryCreate(value, UriKind.Absolute, out var uri) ||
					!(uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
				{
					throw new ArgumentException("Image must be a valid absolute URL using http or https.", nameof(Image));
				}

				image = value;
			}
		}

		public int CategoryId
		{
			get => categoryId;
			set
			{
				if (value <= 0)
					throw new ArgumentException("CategoryId must be a positive value.", nameof(CategoryId));
				categoryId = value;
			}
		}

		public virtual Category Category
		{
			get => category ?? throw new InvalidOperationException("Category navigation property has not been set.");
			set
			{
				if (value == null)
					throw new ArgumentNullException(nameof(Category));
				CategoryId = value.Id;
				category = value;
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

		public uint Amount
		{
			get => amount;
			set
			{
				if (value <= 0)
					throw new ArgumentException("Amount must be a positive value.", nameof(Amount));
				amount = value;
			}
		}
	}
}