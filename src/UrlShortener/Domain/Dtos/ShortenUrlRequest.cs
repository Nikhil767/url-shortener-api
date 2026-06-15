namespace UrlShortener.Domain.Dtos;

public class ShortenUrlRequest
{
    public string Url { get; set; } = string.Empty;
    public string? BaseUrl { get; set; }
}
