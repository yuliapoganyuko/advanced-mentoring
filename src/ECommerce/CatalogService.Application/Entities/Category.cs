using System.Text.RegularExpressions;

namespace CatalogService.Core
{
	public class Category
	{
		private int id;
		private string name = string.Empty;
		private string? image;

		public Category() { }

		public Category(string name, string? image = null, int? parentCategoryId = null)
		{
			Name = name;
			Image = image;
			ParentCategoryId = parentCategoryId;
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

		public int? ParentCategoryId { get; set; }

		public virtual Category? ParentCategory { get; set; }
	}
}