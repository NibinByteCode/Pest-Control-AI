using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using PestControl.ContractIntelligence.Infrastructure.Persistence;
using PestControl.ContractIntelligence.Api.Configuration;
using PestControl.ContractIntelligence.Api.Health;

var builder = WebApplication.CreateBuilder(args);

var databaseOptions = builder.Services
    .AddOptions<DatabaseOptions>()
    .Bind(builder.Configuration.GetSection(DatabaseOptions.SectionName));

if (builder.Environment.IsProduction())
{
    databaseOptions
        .Validate(options => !string.IsNullOrWhiteSpace(options.ApplicationDb),
            "ConnectionStrings:ApplicationDb is required.")
        .ValidateOnStart();
}

var databaseConnection =
    builder.Configuration.GetConnectionString(DatabaseOptions.SectionName)
    ?? builder.Configuration[$"{DatabaseOptions.SectionName}:ConnectionString"];

if (string.IsNullOrWhiteSpace(databaseConnection))
{
    databaseConnection =
        "Server=(localdb)\\MSSQLLocalDB;Database=PestControlContractIntelligence;Trusted_Connection=True;TrustServerCertificate=True";
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(databaseConnection));

builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddHealthChecks()
    .AddCheck<DatabaseHealthCheck>("database");

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseHttpsRedirection();

app.Use(async (context, next) =>
{
    var correlationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault()
        ?? Guid.NewGuid().ToString("N");
    context.Response.Headers["X-Correlation-ID"] = correlationId;
    await next();
});

app.MapGet("/health/live", () => Results.Ok(new { status = "ok" }))
    .WithName("LiveHealth");

app.MapHealthChecks("/api/v1/health/ready");

app.Run();
