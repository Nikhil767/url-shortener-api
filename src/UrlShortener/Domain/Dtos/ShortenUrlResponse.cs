namespace UrlShortener.Domain.Dtos;

public class ShortenUrlResponse
{
    public string ShortUrl { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
}
