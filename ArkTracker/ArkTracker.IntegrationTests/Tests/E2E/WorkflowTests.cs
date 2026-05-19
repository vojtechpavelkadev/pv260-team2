using ArkTracker.Application.CompareHoldings;
using ArkTracker.Application.GetAvailableHoldingDates;
using ArkTracker.Domain.Entities;
using ArkTracker.Domain.ValueObjects;
using ArkTracker.Infrastructure.Persistence;
using ArkTracker.IntegrationTests.Fixtures;
using ArkTracker.IntegrationTests.Helpers;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace ArkTracker.IntegrationTests.Tests.E2E
{
    public class WorkflowTests : IClassFixture<ApiWebApplicationFactory>, IAsyncLifetime
    {
        private readonly ApiWebApplicationFactory _factory;
        private HttpClient _client = null!;

        public WorkflowTests(ApiWebApplicationFactory factory)
        {
            _factory = factory;
        }

        public async Task InitializeAsync()
        {
            _client = _factory.CreateClient();

            // Clear db and seed data for E2E workflow
            using IServiceScope scope = _factory.Services.CreateScope();
            AppDbContext dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            _ = await dbContext.Database.EnsureDeletedAsync();
            _ = await dbContext.Database.EnsureCreatedAsync();

            DateTime date1 = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            DateTime date2 = new(2026, 1, 2, 0, 0, 0, DateTimeKind.Utc);

            dbContext.Holdings.AddRange(
                new HoldingRecord(date1, null, null, "MSFT", null, 100, null, null),
                new HoldingRecord(date2, null, null, "MSFT", null, 200, null, null)
            );
            _ = await dbContext.SaveChangesAsync();
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }

        [Fact]
        public async Task CompleteWorkflow_FetchDates_ThenCompare()
        {
            // Step 1: Unauthenticated request fails
            HttpResponseMessage datesResponseUnauth = await _client.GetAsync("/api/holdings/dates");
            _ = datesResponseUnauth.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

            // Step 2: Login / Authenticate
            string token = JwtTokenHelper.GenerateTestToken();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Step 3: Fetch Available Dates
            HttpResponseMessage datesResponse = await _client.GetAsync("/api/holdings/dates");
            _ = datesResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            GetAvailableHoldingDatesResult? datesResult = await datesResponse.Content.ReadFromJsonAsync<GetAvailableHoldingDatesResult>(new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            _ = datesResult.Should().NotBeNull();
            _ = datesResult!.Dates.Should().HaveCount(2);

            DateTime toDate = datesResult.Dates.ElementAt(0); // 2026-01-02
            DateTime fromDate = datesResult.Dates.ElementAt(1); // 2026-01-01

            // Step 4: Compare Holdings using fetched dates
            string url = $"/api/holdings/compare?from={fromDate:yyyy-MM-ddTHH:mm:ssZ}&to={toDate:yyyy-MM-ddTHH:mm:ssZ}";
            HttpResponseMessage compareResponse = await _client.GetAsync(url);
            _ = compareResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            CompareHoldingsResult? compareResult = await compareResponse.Content.ReadFromJsonAsync<CompareHoldingsResult>(new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            // Verify comparison result
            _ = compareResult.Should().NotBeNull();
            _ = compareResult!.Increased.Should().HaveCount(1);
            _ = compareResult.Increased[0].Ticker.Should().Be("MSFT");
            _ = compareResult.Increased[0].OldShares.Should().Be(100);
            _ = compareResult.Increased[0].NewShares.Should().Be(200);
        }
    }
}
