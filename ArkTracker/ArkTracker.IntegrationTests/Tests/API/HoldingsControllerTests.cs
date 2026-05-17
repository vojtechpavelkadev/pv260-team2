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

namespace ArkTracker.IntegrationTests.Tests.API
{
    public class HoldingsControllerTests : IClassFixture<ApiWebApplicationFactory>, IAsyncLifetime
    {
        private readonly ApiWebApplicationFactory _factory;
        private HttpClient _client = null!;

        public HoldingsControllerTests(ApiWebApplicationFactory factory)
        {
            _factory = factory;
        }

        public async Task InitializeAsync()
        {
            _client = _factory.CreateClient();
            var token = JwtTokenHelper.GenerateTestToken();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Clear db and seed
            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await dbContext.Database.EnsureDeletedAsync();
            await dbContext.Database.EnsureCreatedAsync();
        }

        public Task DisposeAsync() => Task.CompletedTask;

        [Fact]
        public async Task Compare_WithoutAuth_ReturnsUnauthorized()
        {
            // Arrange
            var unauthenticatedClient = _factory.CreateClient();

            // Act
            var response = await unauthenticatedClient.GetAsync("/api/holdings/compare");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetDates_WithValidAuth_ReturnsOk()
        {
            // Arrange
            using (var scope = _factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                dbContext.Holdings.Add(new HoldingRecord { Date = new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc) });
                await dbContext.SaveChangesAsync();
            }

            // Act
            var response = await _client.GetAsync("/api/holdings/dates");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<GetAvailableHoldingDatesResult>(new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            result.Should().NotBeNull();
            result!.Dates.Should().HaveCount(1);
        }

        [Fact]
        public async Task Compare_WithSpecificDates_ReturnsOk()
        {
            // Arrange
            using (var scope = _factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var date1 = new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                var date2 = new DateTime(2023, 1, 2, 0, 0, 0, DateTimeKind.Utc);

                dbContext.Holdings.AddRange(
                    new HoldingRecord { Date = date1, Ticker = "AAPL", Shares = 100 },
                    new HoldingRecord { Date = date2, Ticker = "AAPL", Shares = 150 }
                );
                await dbContext.SaveChangesAsync();
            }

            var url = $"/api/holdings/compare?from=2023-01-01T00:00:00Z&to=2023-01-02T00:00:00Z";

            // Act
            var response = await _client.GetAsync(url);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<CompareHoldingsResult>(new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            result.Should().NotBeNull();
            result!.Increased.Should().HaveCount(1);
            result.Increased.First().Ticker.Should().Be("AAPL");
        }
    }
}
