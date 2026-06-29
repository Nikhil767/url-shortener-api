using Microsoft.AspNetCore.Mvc;
using UrlShortener.ApplicationInterface;
using UrlShortener.Domain.Dtos;
using UrlShortener.Domain.Entities;

namespace UrlShortener.API.Endpoints;

public class UrlEndpoints
{
    private const int MaxUrlLength = 2048;

    public static void MapUrlEndpoints(IEndpointRouteBuilder app)
    {
		var api = app.MapGroup("/api");
		api.MapPost("/shorten", ShortenUrlAsync)
           .WithName("ShortenUrl");

		api.MapGet("/shorten/{code}", GetUrlDetailsAsync)
		.WithName("UrlDetails");

		api.MapGet("/{code}", RedirectUrlAsync)
           .WithName("RedirectUrl");

		api.MapGet("/Admin", GetAdminListAsync)
           .WithName("GetAdminList");

		api.MapDelete("/MasterAdmin/{id}", DeleteUrlAsync)
		   .WithName("DeleteUrl");
	}

    public static async Task<IResult> ShortenUrlAsync(
        [FromBody] ShortenUrlRequest request,
        IShortCodeService codeService,
        IUrlRepository repository,
        HttpContext httpContext,
		ILogger<UrlEndpoints> logger,
		CancellationToken cancellationToken)
    {
        try
        {
            if (request == null)
            {
                return Results.ValidationProblem(
                    new Dictionary<string, string[]> { { "Request", new[] { "Request body cannot be null." } } },
                    title: "Validation Error",
                    statusCode: StatusCodes.Status400BadRequest);
            }

            var errors = new Dictionary<string, string[]>();
            if (string.IsNullOrWhiteSpace(request.Url))
            {
                errors.Add(nameof(request.Url), new[] { "URL is required." });
            }
            else
            {
                if (request.Url.Length > MaxUrlLength)
                {
                    errors.Add(nameof(request.Url), new[] { $"URL length cannot exceed {MaxUrlLength} characters." });
                }

                if (!Uri.TryCreate(request.Url, UriKind.Absolute, out var uriResult) ||
                    (uriResult.Scheme != Uri.UriSchemeHttp && uriResult.Scheme != Uri.UriSchemeHttps))
                {
                    errors.Add(nameof(request.Url), new[] { "URL must be a well-formed absolute URL with HTTP or HTTPS scheme." });
                }
            }
            if (errors.Count > 0)
            {
                return Results.ValidationProblem(
                    errors,
                    title: "Validation Error",
                    statusCode: StatusCodes.Status400BadRequest);
            }

            // Check if URL is already shortened
            var existingUrl = await repository.GetByOriginalUrlAsync(request.Url!, cancellationToken);
            if (existingUrl != null)
            {
                var existingHostUrl = !string.IsNullOrWhiteSpace(request.BaseUrl)
                    ? request.BaseUrl.TrimEnd('/')
                    : $"{httpContext.Request.Scheme}://{httpContext.Request.Host}";

                var existingResponse = new ShortenUrlResponse
                {
                    ShortCode = existingUrl.ShortCode,
                    ShortUrl = $"{existingHostUrl}/{existingUrl.ShortCode}"
                };
                return Results.Ok(existingResponse);
            }

            // Generate a unique code (retry up to 5 times on collision)
            string code = string.Empty;
            const int maxRetries = 5;
            for (int i = 0; i < maxRetries; i++)
            {
                var tempCode = codeService.Generate(6);
                var existing = await repository.GetByCodeAsync(tempCode, cancellationToken);
                if (existing == null)
                {
                    code = tempCode;
                    break;
                }
            }
            if (string.IsNullOrEmpty(code))
            {
                return Results.Problem(
                    detail: "Failed to generate a unique short code. Please try again.",
                    statusCode: StatusCodes.Status500InternalServerError,
                    title: "Generation Failure");
            }
            var shortenedUrl = new ShortenedUrl
            {
                OriginalUrl = request.Url!,
                ShortCode = code,
                CreatedAt = DateTime.UtcNow,
                ClickCount = 0
            };
            await repository.AddAsync(shortenedUrl, cancellationToken);
            var hostUrl = !string.IsNullOrWhiteSpace(request.BaseUrl)
                ? request.BaseUrl.TrimEnd('/')
                : $"{httpContext.Request.Scheme}://{httpContext.Request.Host}";
            var response = new ShortenUrlResponse
            {
                ShortCode = code,
                ShortUrl = $"{hostUrl}/{code}"
            };
            return Results.Ok(response);
        }
        catch (OperationCanceledException)
        {
            return Results.StatusCode(StatusCodes.Status499ClientClosedRequest);
        }
        catch (Exception ex)
        {
			logger.LogError(ex, "An unhandled exception occurred while executing {MethodName}", nameof(ShortenUrlAsync));
			// Do not expose raw internal exception messages to clients
			return Results.Problem(
                detail: "An unexpected error occurred while processing your request.",
                statusCode: StatusCodes.Status500InternalServerError,
                title: "Internal Server Error");
        }
    }

    public static async Task<IResult> RedirectUrlAsync(
        string code,
        IUrlRepository repository,
		ILogger<UrlEndpoints> logger,
		CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return Results.ValidationProblem(
                    new Dictionary<string, string[]> { { "code", new[] { "Code parameter is required." } } },
                    title: "Validation Error",
                    statusCode: StatusCodes.Status400BadRequest);
            }
            var shortenedUrl = await repository.GetByCodeAsync(code, cancellationToken);
            if (shortenedUrl == null)
            {
                return Results.NotFound(new ProblemDetails
                {
                    Title = "Not Found",
                    Detail = $"Short code '{code}' was not found.",
                    Status = StatusCodes.Status404NotFound
                });
            }
            await repository.IncrementClickCountAsync(code, cancellationToken);
            return Results.Redirect(shortenedUrl.OriginalUrl);
        }
        catch (OperationCanceledException)
        {
            return Results.StatusCode(StatusCodes.Status499ClientClosedRequest);
        }
        catch (Exception ex)
        {
			logger.LogError(ex, "An unhandled exception occurred while executing {MethodName}", nameof(RedirectUrlAsync));
			return Results.Problem(
                detail: "An unexpected error occurred while processing the redirect.",
                statusCode: StatusCodes.Status500InternalServerError,
                title: "Internal Server Error");
        }
    }

    public static async Task<IResult> GetAdminListAsync(
        IUrlRepository repository,
        ILogger<UrlEndpoints> logger,
		CancellationToken cancellationToken)
    {
        try
        {
            var list = await repository.GetAllAsync(cancellationToken);
            return Results.Ok(list);
        }
        catch (OperationCanceledException)
        {
            return Results.StatusCode(StatusCodes.Status499ClientClosedRequest);
        }
        catch (Exception ex)
        {
			logger.LogError(ex, "An unhandled exception occurred while executing {MethodName}", nameof(GetAdminListAsync));
			return Results.Problem(
                detail: "An unexpected error occurred while fetching details.",
                statusCode: StatusCodes.Status500InternalServerError,
                title: "Internal Server Error");
        }
    }

	public static async Task<IResult> GetUrlDetailsAsync(
	string code,
	IUrlRepository repository,
	ILogger<UrlEndpoints> logger,
	CancellationToken cancellationToken)
	{
		try
		{
			if (string.IsNullOrWhiteSpace(code))
			{
				return Results.ValidationProblem(
					new Dictionary<string, string[]> { { "code", new[] { "Code parameter is required." } } },
					title: "Validation Error",
					statusCode: StatusCodes.Status400BadRequest);
			}
			var shortenedUrl = await repository.GetByCodeAsync(code, cancellationToken);
			if (shortenedUrl == null)
			{
				return Results.NotFound(new ProblemDetails
				{
					Title = "Not Found",
					Detail = $"Short code '{code}' was not found.",
					Status = StatusCodes.Status404NotFound
				});
			}
			return Results.Ok(shortenedUrl);
		}
		catch (OperationCanceledException)
		{
			return Results.StatusCode(StatusCodes.Status499ClientClosedRequest);
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "An unhandled exception occurred while executing {MethodName}", nameof(RedirectUrlAsync));
			return Results.Problem(
				detail: "An unexpected error occurred while processing the redirect.",
				statusCode: StatusCodes.Status500InternalServerError,
				title: "Internal Server Error");
		}
	}

	public static async Task<IResult> DeleteUrlAsync(
        Guid id,
        bool? hardDelete,
        IUrlRepository repository,
        ILogger<UrlEndpoints> logger,
        CancellationToken cancellationToken)
    {
        try
        {
            if(id == Guid.Empty)
			{
				return Results.ValidationProblem(
						new Dictionary<string, string[]> { { "Request", new[] { $"Invalid id {id}." } } },
						title: "Validation Error",
						statusCode: StatusCodes.Status400BadRequest);
			}
            bool isDeleted = await repository.DeleteUrlAsync(id, hardDelete ?? false, cancellationToken);
            return Results.Ok(isDeleted);
        }
        catch (OperationCanceledException)
        {
            return Results.StatusCode(StatusCodes.Status499ClientClosedRequest);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unhandled exception occurred while executing {MethodName}", nameof(DeleteUrlAsync));
            return Results.Problem(
                detail: "An unexpected error occurred while fetching details.",
                statusCode: StatusCodes.Status500InternalServerError,
                title: "Internal Server Error");
        }
    }
}