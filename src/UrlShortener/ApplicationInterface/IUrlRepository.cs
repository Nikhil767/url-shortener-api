using UrlShortener.Domain.Entities;

namespace UrlShortener.ApplicationInterface;

public interface IUrlRepository
{
    Task<ShortenedUrl?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<ShortenedUrl?> GetByOriginalUrlAsync(string originalUrl, CancellationToken cancellationToken = default);
    Task AddAsync(ShortenedUrl shortenedUrl, CancellationToken cancellationToken = default);
	/// <summary>
	/// Get all As No Tracking
	/// </summary>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	Task<IEnumerable<ShortenedUrl>> GetAllAsync(CancellationToken cancellationToken = default);
    Task IncrementClickCountAsync(string code, CancellationToken cancellationToken = default);
}
