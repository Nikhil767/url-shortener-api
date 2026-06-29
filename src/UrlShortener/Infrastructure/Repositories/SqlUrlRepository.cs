using Microsoft.EntityFrameworkCore;
using UrlShortener.ApplicationInterface;
using UrlShortener.Domain.Entities;
using UrlShortener.Infrastructure.Context;

namespace UrlShortener.Infrastructure.Repositories
{
	public class SqlUrlRepository : IUrlRepository
	{
		private readonly AppDbContext _db;

		public SqlUrlRepository(AppDbContext db)
		{
			_db = db;
		}

		public Task<ShortenedUrl?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
			=> _db.ShortenedUrls.FirstOrDefaultAsync(x => x.IsActive && x.ShortCode == code, cancellationToken);

		public Task<ShortenedUrl?> GetByOriginalUrlAsync(string originalUrl, CancellationToken cancellationToken = default)
			=> _db.ShortenedUrls.FirstOrDefaultAsync(x => x.IsActive && x.OriginalUrl == originalUrl, cancellationToken);

		public async Task AddAsync(ShortenedUrl shortenedUrl, CancellationToken cancellationToken = default)
		{
			_db.ShortenedUrls.Add(shortenedUrl);
			await _db.SaveChangesAsync(cancellationToken);
		}

		public async Task<IEnumerable<ShortenedUrl>> GetAllAsync(CancellationToken cancellationToken = default)
			=> await _db.ShortenedUrls.Where(x => x.IsActive)
			.OrderByDescending(x => x.CreatedAt)
			.AsNoTracking()
			.ToListAsync(cancellationToken);

		public async Task IncrementClickCountAsync(string code, CancellationToken cancellationToken = default)
		{
			var item = await _db.ShortenedUrls.FirstOrDefaultAsync(x => x.IsActive && x.ShortCode == code, cancellationToken);
			if (item is null) return;
			item.ClickCount++;
			await _db.SaveChangesAsync(cancellationToken);
		}

		public async Task<bool> DeleteUrlAsync(Guid id, bool hardDelete = false, CancellationToken cancellationToken = default)
		{
			bool isDeleted = false;
			var item = await _db.ShortenedUrls.FirstOrDefaultAsync(x => x.IsActive && x.Id == id, cancellationToken);
			if (item is not null)
			{
				if (hardDelete)
				{
					_db.ShortenedUrls.Remove(item);
				}
				else
				{
					item.IsActive = false;
				}
				isDeleted = await _db.SaveChangesAsync(cancellationToken) > 0;
			}
			return isDeleted;
		}
	}
}
