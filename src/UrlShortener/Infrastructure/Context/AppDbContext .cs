using Microsoft.EntityFrameworkCore;
using UrlShortener.Domain.Entities;

namespace UrlShortener.Infrastructure.Context
{
	public sealed class AppDbContext : DbContext
	{
		public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

		public DbSet<ShortenedUrl> ShortenedUrls => Set<ShortenedUrl>();

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<ShortenedUrl>(entity =>
			{
				entity.ToTable(nameof(ShortenedUrl));
				entity.HasKey(x => x.Id);
				entity.HasIndex(x => x.ShortCode).IsUnique();
				entity.Property(x => x.ShortCode).HasMaxLength(50).IsRequired();
				entity.Property(x => x.OriginalUrl).IsRequired();
				entity.Property(x => x.CreatedAt).IsRequired();
				entity.Property(x => x.ClickCount).HasDefaultValue(0);
			});
		}
	}
}
