using ArkTracker.Api.Jobs;
using ArkTracker.Application;
using ArkTracker.Application.CompareHoldings;
using ArkTracker.Application.GetAvailableHoldingDates;
using ArkTracker.Application.Interfaces;
using ArkTracker.Infrastructure.Persistence;
using ArkTracker.Infrastructure.Services;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Quartz;
using Scalar.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
string connectionName = builder.Environment.IsDevelopment()
    ? "DefaultConnection"
    : "NeonDb";

string? connectionString = builder.Configuration.GetConnectionString(connectionName);
builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));
builder.Services.AddScoped<IHoldingRepository, HoldingRepository>();
builder.Services.AddHttpClient<IArkScraperService, ArkScraperService>();

string ingestionCron = builder.Configuration.GetValue<string>("Quartz:IngestionCron") ?? "0 30 11 * * ?";

builder.Services.AddQuartz(q =>
{
    JobKey jobKey = new("ArkHoldingsIngestionJob");

    q.AddJob<ArkHoldingsIngestionJob>(opts => opts.WithIdentity(jobKey));
    q.AddTrigger(opts => opts
        .ForJob(jobKey)
        .WithIdentity("ArkHoldingsIngestionJob-trigger")
        .WithCronSchedule(ingestionCron));
});

builder.Services.AddQuartzHostedService(options => options.WaitForJobsToComplete = true);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key is missing from configuration.");
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? throw new InvalidOperationException("JWT Issuer is missing from configuration.");
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? throw new InvalidOperationException("JWT Audience is missing from configuration.");

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
    cfg.RegisterServicesFromAssembly(typeof(CompareHoldingsQueryHandler).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(GetAvailableHoldingDatesQueryHandler).Assembly);
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
    await db.Database.MigrateAsync();
    
    if (app.Environment.IsDevelopment())
    {
        DbSeeder.Seed(db);
    }
}

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.ContentType = "application/json";

        var exception = context.Features
            .Get<IExceptionHandlerFeature>()?.Error;

        context.Response.StatusCode = exception switch
        {
            FluentValidation.ValidationException => 400,
            _ => 500
        };

        await context.Response.WriteAsJsonAsync(new
        {
            error = exception?.Message
        });
    });
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();
