using ArkTracker.Api.Jobs;
using ArkTracker.Application.Interfaces;
using ArkTracker.Infrastructure.Persistence;
using ArkTracker.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Quartz;
using Scalar.AspNetCore;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

string? connectionString = builder.Configuration.GetConnectionString("NeonDb");
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
}

app.MapControllers();
app.Run();
