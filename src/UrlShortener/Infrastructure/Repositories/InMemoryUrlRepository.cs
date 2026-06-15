using System.Collections.Concurrent;
using UrlShortener.ApplicationInterface;
using UrlShortener.Domain.Entities;

namespace UrlShortener.Infrastructure.Repositories;

/// <summary>
/// Thread-safe in-memory repository for storing shortened URLs.
/// </summary>
public class InMemoryUrlRepository : IUrlRepository
{
    private readonly ConcurrentDictionary<string, ShortenedUrl> _urls = new(StringComparer.OrdinalIgnoreCase);

    /// <inheritdoc />
    public Task<ShortenedUrl?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        _urls.TryGetValue(code, out var shortenedUrl);
        return Task.FromResult(shortenedUrl);
    }

    /// <inheritdoc />
    public Task<ShortenedUrl?> GetByOriginalUrlAsync(string originalUrl, CancellationToken cancellationToken = default)
    {
        var shortenedUrl = _urls.Values.FirstOrDefault(u => u.IsActive && u.OriginalUrl.Equals(originalUrl, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(shortenedUrl);
    }

    /// <inheritdoc />
    public Task AddAsync(ShortenedUrl shortenedUrl, CancellationToken cancellationToken = default)
    {
        _urls[shortenedUrl.ShortCode] = shortenedUrl;
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<IEnumerable<ShortenedUrl>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IEnumerable<ShortenedUrl>>(_urls.Values.Where(u => u.IsActive).OrderByDescending(u => u.CreatedAt).ToList());
    }

    /// <inheritdoc />
    public Task IncrementClickCountAsync(string code, CancellationToken cancellationToken = default)
    {
        if (_urls.TryGetValue(code, out var shortenedUrl))
        {
            lock (shortenedUrl)
            {
                shortenedUrl.ClickCount++;
            }
        }
        return Task.CompletedTask;
    }
}
