# Coding Standards & Architecture

## Language & Framework
- .NET 8
- C# 12
- ASP.NET Core Minimal APIs

## Architecture
- Domain: Entities, Dto, Mappers, core models
- Application: Services, business logic, external integrations
- Infrastructure: Repositories, EF Core 
- ApplicationInteface: Interfaces only, used between API & implemented by Application services
- API: Endpoints only, no business logic
- Test: XUnit Test cases, TDD Approach

## Coding Style
- Use PascalCase for classes, methods, properties.
- Use camelCase for variables and parameters.
- Use dependency injection for all services.
- Avoid static classes unless utility-only.
- Keep methods small and single-responsibility.

## Validation Rules
- Validate all incoming URLs.
- Return typed results (e.g., `Results.BadRequest`, `Results.Ok`).
- Never trust client input.
- Use FluentValidation only if explicitly enabled.

## Error Handling
- Use ProblemDetails for structured errors.
- Never expose internal exceptions to clients.
- Handle TaskCancelation, Timeout, and other type exceptions gracefully

## Testing
- Use xUnit.
- Use Moq for moquing class, interfaces & anything required.
- Use Bogus for dummy data for unit test cases
- Test services, not endpoints.
- Mock repositories using interfaces.

## Packages Allowed
- Microsoft.EntityFrameworkCore
- Microsoft.Extensions.Logging
- No external packages unless justified.