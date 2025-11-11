using CatalogService.Core;
using Microsoft.EntityFrameworkCore;

namespace CatalogService.Infrastructure
{
	public partial class CatalogContext : DbContext
	{
		private readonly int _maxImageUrlLength = 2000;

		public CatalogContext()
		{
		}

		public CatalogContext(DbContextOptions<CatalogContext> options) : base(options)
		{
		}

		public virtual DbSet<Category> Categories { get; set; }
		public virtual DbSet<Product> Products { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			modelBuilder.Entity<Category>(entity =>
			{
				entity.HasKey(c => c.Id);

				entity.Property(c => c.Name)
					.IsRequired()
					.HasMaxLength(50);

				entity.Property(c => c.Image)
					.HasMaxLength(_maxImageUrlLength);

				entity.HasOne(c => c.ParentCategory)
					.WithMany()
					.HasForeignKey(c => c.ParentCategoryId)
					.OnDelete(DeleteBehavior.Restrict);
			});

			modelBuilder.Entity<Product>(entity =>
			{
				entity.HasKey(p => p.Id);

				entity.Property(p => p.Name)
					.IsRequired()
					.HasMaxLength(50);

				entity.Property(p => p.Description)
					.HasColumnType("nvarchar(max)")
					.IsRequired(false);

				entity.Property(p => p.Image)
					.HasMaxLength(_maxImageUrlLength);

				entity.HasOne(p => p.Category)
					.WithMany()
					.HasForeignKey(p => p.CategoryId)
					.IsRequired()
					.OnDelete(DeleteBehavior.Restrict);

				entity.Property(p => p.Price)
					.IsRequired()
					.HasColumnType("money");

				entity.ToTable(t => t.HasCheckConstraint("CK_Product_Price_NonNegative", "Price >= 0"));

				entity.Property(p => p.Amount)
					.IsRequired();

				entity.ToTable(t => t.HasCheckConstraint("CK_Product_Amount_Positive", "Amount > 0"));
			});

			OnModelCreatingPartial(modelBuilder);
		}

		partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
	}
}
