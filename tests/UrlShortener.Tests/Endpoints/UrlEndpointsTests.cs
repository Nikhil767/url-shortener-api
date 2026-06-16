using Castle.Core.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using UrlShortener.API.Endpoints;
using UrlShortener.ApplicationInterface;
using UrlShortener.Domain.Dtos;
using UrlShortener.Domain.Entities;

namespace UrlShortener.Tests.Endpoints;

public class UrlEndpointsTests
{
    private readonly Mock<IShortCodeService> _mockCodeService = new();
    private readonly Mock<IUrlRepository> _mockRepository = new();
	private readonly Mock<ILogger<UrlEndpoints>> _mockLogger = new();
	private readonly DefaultHttpContext _httpContext = new();
    private readonly CancellationToken _cancellationToken = CancellationToken.None;

    public UrlEndpointsTests()
    {
        _httpContext.Request.Scheme = "https";
        _httpContext.Request.Host = new HostString("localhost");
    }

    [Fact]
    public async Task ShortenUrlAsync_WithNullRequest_ReturnsValidationProblem()
    {
        // Act
        var result = await UrlEndpoints.ShortenUrlAsync(
            null!,
            _mockCodeService.Object,
            _mockRepository.Object,
            _httpContext, _mockLogger.Object,
			_cancellationToken);

        // Assert
        var problemResult = Assert.IsAssignableFrom<ProblemHttpResult>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, problemResult.StatusCode);
        var validationDetails = Assert.IsAssignableFrom<HttpValidationProblemDetails>(problemResult.ProblemDetails);
        Assert.Contains("Request body cannot be null.", validationDetails.Errors["Request"]);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ShortenUrlAsync_WithEmptyUrl_ReturnsValidationProblem(string url)
    {
        // Arrange
        var request = new ShortenUrlRequest { Url = url };

        // Act
        var result = await UrlEndpoints.ShortenUrlAsync(
            request,
            _mockCodeService.Object,
            _mockRepository.Object,
            _httpContext, _mockLogger.Object,
			_cancellationToken);

        // Assert
        var problemResult = Assert.IsAssignableFrom<ProblemHttpResult>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, problemResult.StatusCode);
        var validationDetails = Assert.IsAssignableFrom<HttpValidationProblemDetails>(problemResult.ProblemDetails);
        Assert.Contains("URL is required.", validationDetails.Errors["Url"]);
    }

    [Fact]
    public async Task ShortenUrlAsync_WithUrlTooLong_ReturnsValidationProblem()
    {
        // Arrange
        var longUrl = "https://example.com/" + new string('a', 2050);
        var request = new ShortenUrlRequest { Url = longUrl };

        // Act
        var result = await UrlEndpoints.ShortenUrlAsync(
            request,
            _mockCodeService.Object,
            _mockRepository.Object,
            _httpContext, _mockLogger.Object,
			_cancellationToken);

        // Assert
        var problemResult = Assert.IsAssignableFrom<ProblemHttpResult>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, problemResult.StatusCode);
        var validationDetails = Assert.IsAssignableFrom<HttpValidationProblemDetails>(problemResult.ProblemDetails);
        Assert.Contains("URL length cannot exceed 2048 characters.", validationDetails.Errors["Url"]);
    }

    [Theory]
    [InlineData("ftp://google.com")]
    [InlineData("google.com")]
    [InlineData("/relative/path")]
    [InlineData("http://")]
    public async Task ShortenUrlAsync_WithInvalidUrlFormat_ReturnsValidationProblem(string invalidUrl)
    {
        // Arrange
        var request = new ShortenUrlRequest { Url = invalidUrl };

        // Act
        var result = await UrlEndpoints.ShortenUrlAsync(
            request,
            _mockCodeService.Object,
            _mockRepository.Object,
            _httpContext, _mockLogger.Object,
			_cancellationToken);

        // Assert
        var problemResult = Assert.IsAssignableFrom<ProblemHttpResult>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, problemResult.StatusCode);
        var validationDetails = Assert.IsAssignableFrom<HttpValidationProblemDetails>(problemResult.ProblemDetails);
        Assert.Contains("URL must be a well-formed absolute URL with HTTP or HTTPS scheme.", validationDetails.Errors["Url"]);
    }

    [Fact]
    public async Task ShortenUrlAsync_WithValidRequest_ReturnsOkWithResponse()
    {
        // Arrange
        var request = new ShortenUrlRequest { Url = "https://github.com", BaseUrl = "https://custom.lnk" };
        _mockCodeService.Setup(s => s.Generate(It.IsAny<int>())).Returns("git123");
        _mockRepository.Setup(r => r.GetByOriginalUrlAsync("https://github.com", _cancellationToken))
            .ReturnsAsync((ShortenedUrl?)null);
        _mockRepository.Setup(r => r.GetByCodeAsync("git123", _cancellationToken))
            .ReturnsAsync((ShortenedUrl?)null);

        // Act
        var result = await UrlEndpoints.ShortenUrlAsync(
            request,
            _mockCodeService.Object,
            _mockRepository.Object,
            _httpContext, _mockLogger.Object,
			_cancellationToken);

        // Assert
        var okResult = Assert.IsAssignableFrom<Ok<ShortenUrlResponse>>(result);
        Assert.NotNull(okResult.Value);
        Assert.Equal("git123", okResult.Value.Code);
        Assert.Equal("https://custom.lnk/git123", okResult.Value.ShortUrl);
        _mockRepository.Verify(r => r.AddAsync(It.Is<ShortenedUrl>(u => u.OriginalUrl == request.Url && u.ShortCode == "git123"), _cancellationToken), Times.Once);
    }

    [Fact]
    public async Task ShortenUrlAsync_WithAlreadyShortenedUrl_ReturnsExistingShortCode()
    {
        // Arrange
        var request = new ShortenUrlRequest { Url = "https://github.com", BaseUrl = "https://custom.lnk" };
        var existing = new ShortenedUrl { ShortCode = "git123", OriginalUrl = "https://github.com" };
        
        _mockRepository.Setup(r => r.GetByOriginalUrlAsync("https://github.com", _cancellationToken))
            .ReturnsAsync(existing);

        // Act
        var result = await UrlEndpoints.ShortenUrlAsync(
            request,
            _mockCodeService.Object,
            _mockRepository.Object,
            _httpContext, _mockLogger.Object,
			_cancellationToken);

        // Assert
        var okResult = Assert.IsAssignableFrom<Ok<ShortenUrlResponse>>(result);
        Assert.NotNull(okResult.Value);
        Assert.Equal("git123", okResult.Value.Code);
        Assert.Equal("https://custom.lnk/git123", okResult.Value.ShortUrl);
        
        // Ensure no new code is generated and no new entity is added
        _mockCodeService.Verify(s => s.Generate(It.IsAny<int>()), Times.Never);
        _mockRepository.Verify(r => r.AddAsync(It.IsAny<ShortenedUrl>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ShortenUrlAsync_WithCodeCollision_RetriesAndSucceeds()
    {
        // Arrange
        var request = new ShortenUrlRequest { Url = "https://github.com" };
        
        // Setup generator to return duplicate code first, then unique code
        _mockCodeService.SetupSequence(s => s.Generate(It.IsAny<int>()))
            .Returns("dup123")
            .Returns("uniq12");

        _mockRepository.Setup(r => r.GetByOriginalUrlAsync("https://github.com", _cancellationToken))
            .ReturnsAsync((ShortenedUrl?)null);

        // First lookup returns an existing object (collision)
        _mockRepository.Setup(r => r.GetByCodeAsync("dup123", _cancellationToken))
            .ReturnsAsync(new ShortenedUrl { ShortCode = "dup123" });
            
        // Second lookup returns null (no collision)
        _mockRepository.Setup(r => r.GetByCodeAsync("uniq12", _cancellationToken))
            .ReturnsAsync((ShortenedUrl?)null);

        // Act
        var result = await UrlEndpoints.ShortenUrlAsync(
            request,
            _mockCodeService.Object,
            _mockRepository.Object,
            _httpContext, _mockLogger.Object,
			_cancellationToken);

        // Assert
        var okResult = Assert.IsAssignableFrom<Ok<ShortenUrlResponse>>(result);
        Assert.NotNull(okResult.Value);
        Assert.Equal("uniq12", okResult.Value.Code);
        _mockRepository.Verify(r => r.GetByCodeAsync(It.IsAny<string>(), _cancellationToken), Times.Exactly(2));
    }

    [Fact]
    public async Task RedirectUrlAsync_WithNonExistentCode_ReturnsNotFound()
    {
        // Arrange
        _mockRepository.Setup(r => r.GetByCodeAsync("missing", _cancellationToken))
            .ReturnsAsync((ShortenedUrl?)null);

        // Act
        var result = await UrlEndpoints.RedirectUrlAsync("missing", _mockRepository.Object, _mockLogger.Object, _cancellationToken);

        // Assert
        var notFoundResult = Assert.IsAssignableFrom<NotFound<ProblemDetails>>(result);
        Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
    }

    [Fact]
    public async Task RedirectUrlAsync_WithExistingCode_RedirectsAndIncrementsCount()
    {
        // Arrange
        var url = new ShortenedUrl { ShortCode = "findme", OriginalUrl = "https://bing.com" };
        _mockRepository.Setup(r => r.GetByCodeAsync("findme", _cancellationToken))
            .ReturnsAsync(url);

        // Act
        var result = await UrlEndpoints.RedirectUrlAsync("findme", _mockRepository.Object, _mockLogger.Object, _cancellationToken);

        // Assert
        var redirectResult = Assert.IsAssignableFrom<RedirectHttpResult>(result);
        Assert.Equal("https://bing.com", redirectResult.Url);
        _mockRepository.Verify(r => r.IncrementClickCountAsync("findme", _cancellationToken), Times.Once);
    }

    [Fact]
    public async Task GetAdminListAsync_ReturnsAllUrlsFromRepository()
    {
        // Arrange
        var list = new List<ShortenedUrl>
        {
            new() { ShortCode = "code1", OriginalUrl = "https://site1.com" },
            new() { ShortCode = "code2", OriginalUrl = "https://site2.com" }
        };
        _mockRepository.Setup(r => r.GetAllAsync(_cancellationToken)).ReturnsAsync(list);

        // Act
        var result = await UrlEndpoints.GetAdminListAsync(_mockRepository.Object, _mockLogger.Object, _cancellationToken);

        // Assert
        var okResult = Assert.IsAssignableFrom<Ok<IEnumerable<ShortenedUrl>>>(result);
        Assert.NotNull(okResult.Value);
        Assert.Equal(2, okResult.Value.Count());
    }
}
