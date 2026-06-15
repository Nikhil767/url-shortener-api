using System.ComponentModel.DataAnnotations.Schema;

namespace UrlShortener.Domain.Entities;

[Table("ShortenedUrl")]
public class ShortenedUrl
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string OriginalUrl { get; set; } = string.Empty;
    public string ShortCode { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int ClickCount { get; set; }
	public bool IsActive { get; set; } = true;
}
