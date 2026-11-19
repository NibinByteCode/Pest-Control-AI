using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Text;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authorization;
using PestControl.ContractIntelligence.Application.Abstractions.Security;
using PestControl.ContractIntelligence.Application.Features.Authentication;
using PestControl.ContractIntelligence.Application.Features.Users;
using PestControl.ContractIntelligence.Infrastructure.Security;
using PestControl.ContractIntelligence.Domain.Identity;
using PestControl.ContractIntelligence.Application.Common.Exceptions;
using PestControl.ContractIntelligence.Application.Common.Validation;
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

builder.Services.Configure<JwtOptions>(
    builder.Configuration.GetSection(JwtOptions.SectionName));
var jwt = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
    ?? new JwtOptions();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwt.Issuer,
            ValidAudience = jwt.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwt.SigningKey))
        };
    });
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole(nameof(Role.Admin)));
    options.AddPolicy("ManagerOrAdmin", policy =>
        policy.RequireRole(nameof(Role.Manager), nameof(Role.Admin)));
});

builder.Services.AddScoped<IPasswordHasher, SystemPasswordHasher>();
builder.Services.AddScoped<ITokenService, JwtTokenService>();
builder.Services.AddScoped<IAuthenticationService, AuthService>();

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
app.UseAuthentication();
app.UseAuthorization();

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

var auth = app.MapGroup("/api/v1/auth");

auth.MapPost("/bootstrap", async (
    HttpRequest httpRequest,
    BootstrapAdminRequest request,
    IAuthenticationService authentication,
    CancellationToken cancellationToken) =>
{
    var configuredSecret = app.Configuration["Bootstrap:Secret"];
    var suppliedSecret = httpRequest.Headers["X-Bootstrap-Secret"].ToString();
    if (string.IsNullOrWhiteSpace(configuredSecret) ||
        !CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(configuredSecret),
            Encoding.UTF8.GetBytes(suppliedSecret)))
    {
        return Results.NotFound();
    }

    try
    {
        var response = await authentication.BootstrapAdminAsync(request, cancellationToken);
        return Results.Ok(response);
    }
    catch (ValidationException exception)
    {
        return Results.ValidationProblem(
            exception.Errors.Select((error, index) => (Key: $"error{index}", Value: new[] { error }))
                .ToDictionary(pair => pair.Key, pair => pair.Value));
    }
});

auth.MapPost("/login", async (
    LoginRequest request,
    IAuthenticationService authentication,
    CancellationToken cancellationToken) =>
{
    try
    {
        return Results.Ok(await authentication.LoginAsync(request, cancellationToken));
    }
    catch (UnauthorizedAccessException)
    {
        return Results.Unauthorized();
    }
});

auth.MapPost("/refresh", async (
    RefreshRequest request,
    IAuthenticationService authentication,
    CancellationToken cancellationToken) =>
{
    try
    {
        return Results.Ok(await authentication.RefreshAsync(request, cancellationToken));
    }
    catch (UnauthorizedAccessException)
    {
        return Results.Unauthorized();
    }
});

auth.MapPost("/logout", async (
    LogoutRequest request,
    IAuthenticationService authentication,
    CancellationToken cancellationToken) =>
{
    await authentication.LogoutAsync(request, cancellationToken);
    return Results.NoContent();
});

app.MapGet("/api/v1/me", (HttpContext context) =>
{
    var userId = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
        ?? context.User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value;
    var role = context.User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
    var name = context.User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.UniqueName)?.Value;
    return Results.Ok(new { userId, userName = name, role });
}).RequireAuthorization();

var users = app.MapGroup("/api/v1/users").RequireAuthorization("AdminOnly");

users.MapGet("/", async (ApplicationDbContext dbContext, CancellationToken cancellationToken) =>
    Results.Ok(await dbContext.Users
        .Select(user => new UserSummary(
            user.Id,
            user.UserName,
            user.NormalizedEmail,
            (Role)user.Role,
            user.IsActive))
        .ToListAsync(cancellationToken)));

users.MapPost("/", async (
    CreateUserRequest request,
    ApplicationDbContext dbContext,
    IPasswordHasher passwordHasher,
    CancellationToken cancellationToken) =>
{
    var validation = UserRequestValidator.Validate(request);
    if (!validation.IsValid)
    {
        return Results.ValidationProblem(
            validation.Errors.Select((error, index) => (Key: $"error{index}", Value: new[] { error }))
                .ToDictionary(pair => pair.Key, pair => pair.Value));
    }

    var normalizedEmail = request.Email.Trim().ToUpperInvariant();
    if (await dbContext.Users.AnyAsync(x => x.NormalizedEmail == normalizedEmail, cancellationToken))
    {
        return Results.Conflict(new { message = "A user with that email already exists." });
    }

    var user = new PestControl.ContractIntelligence.Infrastructure.Persistence.Models.UserRecord
    {
        Id = Guid.NewGuid(),
        UserName = request.UserName.Trim(),
        NormalizedEmail = normalizedEmail,
        PasswordHash = passwordHasher.Hash(request.Password),
        Role = (int)request.Role,
        IsActive = true,
        CreatedAtUtc = DateTimeOffset.UtcNow
    };
    dbContext.Users.Add(user);
    dbContext.AuditLogs.Add(new()
    {
        Id = Guid.NewGuid(),
        Action = "auth.user.created",
        ResourceType = "User",
        ResourceId = user.Id.ToString(),
        CreatedAtUtc = DateTimeOffset.UtcNow
    });
    await dbContext.SaveChangesAsync(cancellationToken);
    return Results.Created($"/api/v1/users/{user.Id}",
        new UserSummary(user.Id, user.UserName, user.NormalizedEmail, (Role)user.Role, user.IsActive));
});

users.MapPatch("/{id:guid}", async (
    Guid id,
    UpdateUserRequest request,
    ApplicationDbContext dbContext,
    IPasswordHasher passwordHasher,
    CancellationToken cancellationToken) =>
{
    var user = await dbContext.Users.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
    if (user is null) return Results.NotFound();

    if (!string.IsNullOrWhiteSpace(request.Password))
    {
        var passwordValidation = RequestValidation.ValidatePassword(request.Password);
        if (!passwordValidation.IsValid)
        {
            return Results.ValidationProblem(
                passwordValidation.Errors.Select((error, index) => (Key: $"error{index}", Value: new[] { error }))
                    .ToDictionary(pair => pair.Key, pair => pair.Value));
        }
        user.PasswordHash = passwordHasher.Hash(request.Password);
    }
    if (request.Role is { } role) user.Role = (int)role;
    if (request.IsActive is { } isActive) user.IsActive = isActive;

    dbContext.AuditLogs.Add(new()
    {
        Id = Guid.NewGuid(),
        Action = "auth.user.updated",
        ResourceType = "User",
        ResourceId = user.Id.ToString(),
        CreatedAtUtc = DateTimeOffset.UtcNow
    });
    await dbContext.SaveChangesAsync(cancellationToken);
    return Results.Ok(new UserSummary(user.Id, user.UserName, user.NormalizedEmail, (Role)user.Role, user.IsActive));
});

app.Run();
