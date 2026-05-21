using ArkTracker.Api.Jobs;
using ArkTracker.Application;
using ArkTracker.Domain.Exceptions;
using ArkTracker.Application.CompareHoldings;
using ArkTracker.Application.GetAvailableHoldingDates;
using ArkTracker.Application.Interfaces;
using ArkTracker.Infrastructure.Persistence;
using ArkTracker.Infrastructure.Services;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Quartz;
using Scalar.AspNetCore;
using System.Reflection;
using System.Text;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

string? connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));
builder.Services.AddScoped<IHoldingRepository, HoldingRepository>();
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddScoped<IDatabaseHealthService, DatabaseHealthService>();
builder.Services.AddHttpClient<IArkScraperService, ArkScraperService>()
    .AddStandardResilienceHandler();

builder.Services.AddOptions<ArkTracker.Infrastructure.Configuration.ArkScraperOptions>()
    .Bind(builder.Configuration.GetSection("ArkScraper"))
    .ValidateDataAnnotations()
    .ValidateOnStart();

string ingestionCron = builder.Configuration.GetValue<string>("Quartz:IngestionCron") ?? "0 30 11 * * ?";

builder.Services.AddQuartz(q =>
{
    JobKey jobKey = new("ArkHoldingsIngestionJob");

    _ = q.AddJob<ArkHoldingsIngestionJob>(opts => opts.WithIdentity(jobKey));
    _ = q.AddTrigger(opts => opts
        .ForJob(jobKey)
        .WithIdentity("ArkHoldingsIngestionJob-trigger")
        .WithCronSchedule(ingestionCron));
});

builder.Services.AddQuartzHostedService(options => options.WaitForJobsToComplete = true);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

string jwtKey = builder.Configuration["Jwt:Key"] ?? throw new ConfigurationException("JWT Key is missing from configuration.");
string jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? throw new ConfigurationException("JWT Issuer is missing from configuration.");
string jwtAudience = builder.Configuration["Jwt:Audience"] ?? throw new ConfigurationException("JWT Audience is missing from configuration.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddMediatR(cfg =>
{
    _ = cfg.RegisterServicesFromAssembly(typeof(CompareHoldingsQueryHandler).Assembly);
    _ = cfg.RegisterServicesFromAssembly(typeof(GetAvailableHoldingDatesQueryHandler).Assembly);
});

builder.Services.AddValidatorsFromAssemblyContaining<CompareHoldingsQuery>();

builder.Services.AddTransient(
    typeof(IPipelineBehavior<,>),
    typeof(ValidationBehavior<,>));

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
    _ = app.MapOpenApi();
    _ = app.MapScalarApiReference();
}

using (IServiceScope scope = app.Services.CreateScope())
{
    AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    if (app.Environment.EnvironmentName != "Testing")
    {
        await db.Database.MigrateAsync();
    }

    if (app.Environment.IsDevelopment())
    {
        await DbSeeder.SeedAsync(db);
    }
}

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.ContentType = "application/json";

        Exception? exception = context.Features
            .Get<IExceptionHandlerFeature>()?.Error;

        if (exception is not null)
        {
            ILogger<Program> logger = context.RequestServices
                .GetRequiredService<ILogger<Program>>();
            logger.LogError(exception, "Unhandled exception");
        }

        context.Response.StatusCode = exception switch
        {
            FluentValidation.ValidationException => StatusCodes.Status400BadRequest,
            DomainValidationException => StatusCodes.Status400BadRequest,
            InsufficientHoldingsDataException => StatusCodes.Status404NotFound,
            DatabaseConnectionException => StatusCodes.Status503ServiceUnavailable,
            ConfigurationException => StatusCodes.Status500InternalServerError,
            ArkTrackerException => StatusCodes.Status500InternalServerError,
            _ => StatusCodes.Status500InternalServerError
        };

        string errorMessage = exception switch
        {
            ArkTrackerException arkEx => arkEx.Message,
            FluentValidation.ValidationException validationEx => validationEx.Message,
            _ => "An unexpected error occurred."
        };

        await context.Response.WriteAsJsonAsync(new { error = errorMessage });
    });
});

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/version", () =>
{
    string version = Assembly.GetEntryAssembly()?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
        ?? Assembly.GetEntryAssembly()?.GetName().Version?.ToString()
        ?? Environment.GetEnvironmentVariable("APP_VERSION")
        ?? "unknown";

    return Results.Ok(new { version });
})
.AllowAnonymous();

app.MapControllers();
app.Run();

public partial class Program { }
