using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using UrlShortener.API.Endpoints;
using UrlShortener.Application.Services;
using UrlShortener.ApplicationInterface;
using UrlShortener.Infrastructure.Context;
using UrlShortener.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Register services
var apiKey = builder.Configuration["ApiSecretKey"];
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? 
		 builder.Configuration["ConnectionStrings:DefaultConnection"];
var tokenLimit = builder.Configuration.GetValue<int>("TokenLimit");
var tokensPerPeriod = builder.Configuration.GetValue<int>("TokensPerPeriod");
var retryAfter = builder.Configuration["RetryAfter"];
var maxRequestBodySize = builder.Configuration.GetValue<int>("MaxRequestBodySize");

builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));
builder.Services.AddSingleton<IShortCodeService, ShortCodeService>();
builder.Services.AddScoped<IUrlRepository, SqlUrlRepository>();
//builder.Services.AddSingleton<IUrlRepository, InMemoryUrlRepository>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
	options.AddDefaultPolicy(policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

builder.Services.AddRateLimiter(options =>
{
	options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
	options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
	{
		var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
		options.OnRejected = (context, token) =>
		{
			context.HttpContext.Response.Headers["Retry-After"] = retryAfter;
			return ValueTask.CompletedTask;
		};
		return RateLimitPartition.GetTokenBucketLimiter(
			partitionKey: ip,
			factory: _ => new TokenBucketRateLimiterOptions
			{
				TokenLimit = tokenLimit,                     // Max tokens
				TokensPerPeriod = tokensPerPeriod,           // Refill amount
				ReplenishmentPeriod = TimeSpan.FromMinutes(1),
				QueueLimit = 0,
				AutoReplenishment = true
			});
	});
});

// set request size at Kestrel level 
builder.WebHost.ConfigureKestrel(options =>
{
	options.Limits.MaxRequestBodySize = maxRequestBodySize * 1024 * 1024;
});

builder.Services.AddHealthChecks();
var app = builder.Build();

// applies migrations
using (var scope = app.Services.CreateScope())
{
	var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
	await db.Database.MigrateAsync();
}

// Enable Swagger UI in development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseCors();
app.UseRateLimiter();

// MAP LIVENESS ENDPOINT (/alive)
// Returns instantly with HTTP 200 "Healthy" if the app isn't deadlocked.
app.MapHealthChecks("/alive", new HealthCheckOptions
{
	Predicate = check => check.Tags.Contains("live")
});

// Register endpoints
app.MapUrlEndpoints();
app.Run();