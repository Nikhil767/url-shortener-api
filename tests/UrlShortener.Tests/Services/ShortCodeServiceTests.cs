using UrlShortener.Application.Services;

namespace UrlShortener.Tests.Services;

public class ShortCodeServiceTests
{
    private readonly ShortCodeService _service = new();

    [Fact]
    public void Generate_WithDefaultLength_ReturnsSixCharacterCode()
    {
        // Act
        var code = _service.Generate();

        // Assert
        Assert.NotNull(code);
        Assert.Equal(6, code.Length);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(100)]
    public void Generate_WithCustomLength_ReturnsCorrectLengthCode(int length)
    {
        // Act
        var code = _service.Generate(length);

        // Assert
        Assert.Equal(length, code.Length);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Generate_WithInvalidLength_ThrowsArgumentOutOfRangeException(int length)
    {
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => _service.Generate(length));
    }

    [Fact]
    public void Generate_ReturnsOnlyValidCharacters()
    {
        // Arrange
        const string validChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        // Act
        var code = _service.Generate(1000);

        // Assert
        Assert.All(code, c => Assert.Contains(c, validChars));
    }

    [Fact]
    public void Generate_MultipleCalls_ReturnsDifferentCodes()
    {
        // Act
        var code1 = _service.Generate();
        var code2 = _service.Generate();

        // Assert
        Assert.NotEqual(code1, code2);
    }
}
