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
                new HoldingRecord(DateTime.UtcNow.Date, null, null, "TEST1", null, 100, null, null),
                new HoldingRecord(DateTime.UtcNow.Date, null, null, "TEST2", null, 200, null, null)
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
            var date1 = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var date2 = new DateTime(2026, 1, 3, 0, 0, 0, DateTimeKind.Utc);

            _fixture.Context.Holdings.AddRange(
                new HoldingRecord(date1, null, null, null, null, null, null, null),
                new HoldingRecord(date2, null, null, null, null, null, null, null),
                new HoldingRecord(date1, null, null, null, null, null, null, null)
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
            var targetDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var otherDate = new DateTime(2026, 1, 2, 0, 0, 0, DateTimeKind.Utc);

            _fixture.Context.Holdings.AddRange(
                new HoldingRecord(targetDate, null, null, "TARGET", null, null, null, null),
                new HoldingRecord(otherDate, null, null, "OTHER", null, null, null, null)
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
