# Project Rules: URL Shortener

## Core Requirements
- Accept a long URL and return a short code.
- Redirect short code → original URL.
- Track click count.
- Store data in memory first, then EF Core.

## Endpoints
### POST /shorten
- Input: { url, baseUrl }
- Validate URL format. validate the max length can be allowed for an url.
- Generate short code using ShortCodeService.
- Return { shortUrl, code }.

### GET /{code}
- Lookup by code.
- Increment click count.
- Redirect to original URL.

### GET /Admin
- List down all details.

## Services
- ShortCodeService: Generates N-character code (by default n can be 6).
- UrlRepository: InMemory first, EF Core later.

## Constraints
- No over-engineering.
- No unnecessary patterns.
- Keep API minimal and clean.

## Future Enhancements
- Rate limiting
- Analytics endpoint
- EF Core migration