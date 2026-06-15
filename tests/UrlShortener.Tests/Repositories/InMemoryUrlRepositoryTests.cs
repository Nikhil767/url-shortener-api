using UrlShortener.Domain.Entities;
using UrlShortener.Infrastructure.Repositories;

namespace UrlShortener.Tests.Repositories;

public class InMemoryUrlRepositoryTests
{
    private readonly InMemoryUrlRepository _repository = new();

    [Fact]
    public async Task AddAsync_StoresUrlAndGetByCodeAsync_RetrievesIt()
    {
        // Arrange
        var url = new ShortenedUrl
        {
            OriginalUrl = "https://google.com",
            ShortCode = "goog12",
            ClickCount = 0
        };

        // Act
        await _repository.AddAsync(url);
        var retrieved = await _repository.GetByCodeAsync("goog12");

        // Assert
        Assert.NotNull(retrieved);
        Assert.Equal(url.OriginalUrl, retrieved.OriginalUrl);
        Assert.Equal(url.ShortCode, retrieved.ShortCode);
    }

    [Fact]
    public async Task GetByCodeAsync_WithNonExistentCode_ReturnsNull()
    {
        // Act
        var retrieved = await _repository.GetByCodeAsync("nonexistent");

        // Assert
        Assert.Null(retrieved);
    }

    [Fact]
    public async Task IncrementClickCountAsync_IncreasesClickCountByOne()
    {
        // Arrange
        var url = new ShortenedUrl
        {
            OriginalUrl = "https://example.com",
            ShortCode = "ex1234",
            ClickCount = 0
        };
        await _repository.AddAsync(url);

        // Act
        await _repository.IncrementClickCountAsync("ex1234");
        var retrieved = await _repository.GetByCodeAsync("ex1234");

        // Assert
        Assert.NotNull(retrieved);
        Assert.Equal(1, retrieved.ClickCount);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllUrlsSortedByCreationTimeDescending()
    {
        // Arrange
        var url1 = new ShortenedUrl
        {
            OriginalUrl = "https://first.com",
            ShortCode = "first1",
            CreatedAt = DateTime.UtcNow.AddMinutes(-5)
        };
        var url2 = new ShortenedUrl
        {
            OriginalUrl = "https://second.com",
            ShortCode = "second",
            CreatedAt = DateTime.UtcNow
        };

        await _repository.AddAsync(url1);
        await _repository.AddAsync(url2);

        // Act
        var all = (await _repository.GetAllAsync()).ToList();

        // Assert
        Assert.Equal(2, all.Count);
        Assert.Equal("second", all[0].ShortCode); // Second is newer, so it should be first
        Assert.Equal("first1", all[1].ShortCode);
    }

    [Fact]
    public async Task GetByOriginalUrlAsync_WithExistingUrl_ReturnsCorrectEntity()
    {
        // Arrange
        var url = new ShortenedUrl
        {
            OriginalUrl = "https://findme.com",
            ShortCode = "findme"
        };
        await _repository.AddAsync(url);

        // Act
        var retrieved = await _repository.GetByOriginalUrlAsync("https://findme.com");

        // Assert
        Assert.NotNull(retrieved);
        Assert.Equal("findme", retrieved.ShortCode);
    }

    [Fact]
    public async Task GetByOriginalUrlAsync_WithNonExistentUrl_ReturnsNull()
    {
        // Act
        var retrieved = await _repository.GetByOriginalUrlAsync("https://nonexistent.com");

        // Assert
        Assert.Null(retrieved);
    }
}
