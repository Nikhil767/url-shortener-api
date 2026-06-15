namespace UrlShortener.ApplicationInterface;

public interface IShortCodeService
{
    string Generate(int length = 6);
}
