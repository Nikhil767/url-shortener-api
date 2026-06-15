using UrlShortener.ApplicationInterface;

namespace UrlShortener.Application.Services;

/// <summary>
/// Service responsible for generating unique short codes for URLs.
/// </summary>
public class ShortCodeService : IShortCodeService
{
    private static readonly char[] Alphabet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789".ToCharArray();

    /// <summary>
    /// Generates a random short code of the specified length using URL-safe alphanumeric characters.
    /// </summary>
    /// <param name="length">The length of the generated short code. Defaults to 6.</param>
    /// <returns>A string representing the generated short code.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the requested length is less than or equal to zero.</exception>
    public string Generate(int length = 6)
    {
        if (length <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(length), "Length must be greater than zero.");
        }

        return string.Create(length, Alphabet, (span, alphabet) =>
        {
            for (int i = 0; i < span.Length; i++)
            {
                span[i] = alphabet[Random.Shared.Next(alphabet.Length)];
            }
        });
    }
}
