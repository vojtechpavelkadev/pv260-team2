using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using ArkTracker.IntegrationTests.Fixtures;
using ArkTracker.IntegrationTests.Helpers;
using ArkTracker.Application.CompareHoldings;
using ArkTracker.Application.GetAvailableHoldingDates;
using ArkTracker.Domain.Entities;
using ArkTracker.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
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
            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await dbContext.Database.EnsureDeletedAsync();
            await dbContext.Database.EnsureCreatedAsync();

            var date1 = new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var date2 = new DateTime(2023, 1, 2, 0, 0, 0, DateTimeKind.Utc);

            dbContext.Holdings.AddRange(
                new HoldingRecord { Date = date1, Ticker = "MSFT", Shares = 100 },
                new HoldingRecord { Date = date2, Ticker = "MSFT", Shares = 200 }
            );
            await dbContext.SaveChangesAsync();
        }

        public Task DisposeAsync() => Task.CompletedTask;

        [Fact]
        public async Task CompleteWorkflow_FetchDates_ThenCompare()
        {
            // Step 1: Unauthenticated request fails
            var datesResponseUnauth = await _client.GetAsync("/api/holdings/dates");
            datesResponseUnauth.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

            // Step 2: Login / Authenticate
            var token = JwtTokenHelper.GenerateTestToken();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Step 3: Fetch Available Dates
            var datesResponse = await _client.GetAsync("/api/holdings/dates");
            datesResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var datesResult = await datesResponse.Content.ReadFromJsonAsync<GetAvailableHoldingDatesResult>(new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            
            datesResult.Should().NotBeNull();
            datesResult!.Dates.Should().HaveCount(2);

            var toDate = datesResult.Dates.ElementAt(0); // 2023-01-02
            var fromDate = datesResult.Dates.ElementAt(1); // 2023-01-01

            // Step 4: Compare Holdings using fetched dates
            var url = $"/api/holdings/compare?from={fromDate:yyyy-MM-ddTHH:mm:ssZ}&to={toDate:yyyy-MM-ddTHH:mm:ssZ}";
            var compareResponse = await _client.GetAsync(url);
            compareResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var compareResult = await compareResponse.Content.ReadFromJsonAsync<CompareHoldingsResult>(new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            
            // Verify comparison result
            compareResult.Should().NotBeNull();
            compareResult!.Increased.Should().HaveCount(1);
            compareResult.Increased[0].Ticker.Should().Be("MSFT");
            compareResult.Increased[0].OldShares.Should().Be(100);
            compareResult.Increased[0].NewShares.Should().Be(200);
        }
    }
}
