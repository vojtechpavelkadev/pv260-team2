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

app.MapControllers();
app.Run();
