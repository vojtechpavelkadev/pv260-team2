using ArkTracker.Domain.Entities;
using ArkTracker.Infrastructure.Persistence;
using ArkTracker.IntegrationTests.Fixtures;
using FluentAssertions;
using Xunit;

namespace ArkTracker.IntegrationTests.Tests.Data
{
    public class HoldingRepositoryTests : IAsyncLifetime
    {
        private readonly DatabaseFixture _fixture;
        private HoldingRepository _repository = null!;

        public HoldingRepositoryTests()
        {
            _fixture = new DatabaseFixture();
        }

        public async Task InitializeAsync()
        {
            await _fixture.InitializeAsync();
            _repository = new HoldingRepository(_fixture.Context);
        }

        public async Task DisposeAsync()
        {
            await _fixture.DisposeAsync();
        }

        [Fact]
        public async Task AddRangeAsync_ShouldAddRecordsToDatabase()
        {
            // Arrange
            var records = new List<HoldingRecord>
            {
                new HoldingRecord { Date = DateTime.UtcNow.Date, Ticker = "TEST1", Shares = 100 },
                new HoldingRecord { Date = DateTime.UtcNow.Date, Ticker = "TEST2", Shares = 200 }
            };

            // Act
            await _repository.AddRangeAsync(records);

            // Assert
            _fixture.Context.Holdings.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetAvailableDatesAsync_ShouldReturnDistinctOrderedDates()
        {
            // Arrange
            var date1 = new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var date2 = new DateTime(2023, 1, 3, 0, 0, 0, DateTimeKind.Utc);

            _fixture.Context.Holdings.AddRange(
                new HoldingRecord { Date = date1 },
                new HoldingRecord { Date = date2 },
                new HoldingRecord { Date = date1 }
            );
            await _fixture.Context.SaveChangesAsync();

            // Act
            var result = await _repository.GetAvailableDatesAsync();

            // Assert
            result.Should().HaveCount(2);
            result.ElementAt(0).Should().Be(date2);
            result.ElementAt(1).Should().Be(date1);
        }

        [Fact]
        public async Task GetByDateAsync_ShouldReturnCorrectRecords()
        {
            // Arrange
            var targetDate = new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var otherDate = new DateTime(2023, 1, 2, 0, 0, 0, DateTimeKind.Utc);

            _fixture.Context.Holdings.AddRange(
                new HoldingRecord { Date = targetDate, Ticker = "TARGET" },
                new HoldingRecord { Date = otherDate, Ticker = "OTHER" }
            );
            await _fixture.Context.SaveChangesAsync();

            // Act
            var result = await _repository.GetByDateAsync(targetDate);

            // Assert
            result.Should().HaveCount(1);
            result.First().Ticker.Should().Be("TARGET");
        }
    }
}
