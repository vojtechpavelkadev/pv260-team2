using ArkTracker.Application.CompareHoldings;
using ArkTracker.Domain.Entities;
using ArkTracker.Domain.Exceptions;
using ArkTracker.Infrastructure.Persistence;
using ArkTracker.IntegrationTests.Fixtures;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using MediatR;

namespace ArkTracker.IntegrationTests.Tests.Application
{
    public class CompareHoldingsQueryHandlerTests : IAsyncLifetime
    {
        private readonly DatabaseFixture _fixture;
        private CompareHoldingsQueryHandler _handler = null!;
        private AppDbContext _dbContext = null!;

        public CompareHoldingsQueryHandlerTests()
        {
            _fixture = new DatabaseFixture();
        }

        public async Task InitializeAsync()
        {
            await _fixture.InitializeAsync();
            _dbContext = _fixture.Context;

            var repository = new HoldingRepository(_dbContext);
            _handler = new CompareHoldingsQueryHandler(repository);
        }

        public async Task DisposeAsync()
        {
            await _fixture.DisposeAsync();
        }

        [Fact]
        public async Task Handle_WithNoDatesAndNotEnoughData_ThrowsException()
        {
            // Arrange
            var query = new CompareHoldingsQuery(null, null);

            // Act
            Func<Task> act = async () => await _handler.Handle(query, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<InsufficientHoldingsDataException>().WithMessage("Not enough data to compare.");
        }

        [Fact]
        public async Task Handle_WithSpecificDates_ReturnsCorrectDeltas()
        {
            // Arrange
            var date1 = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var date2 = new DateTime(2026, 1, 2, 0, 0, 0, DateTimeKind.Utc);

            _dbContext.Holdings.AddRange(
                new HoldingRecord(date1, null, "Apple", "AAPL", null, 100, null, 1.0m),
                new HoldingRecord(date1, null, "Tesla", "TSLA", null, 50, null, 0.5m),
                new HoldingRecord(date2, null, "Apple", "AAPL", null, 150, null, 1.5m), // Increased
                new HoldingRecord(date2, null, "Tesla", "TSLA", null, 20, null, 0.2m), // Reduced
                new HoldingRecord(date2, null, "Microsoft", "MSFT", null, 100, null, 1.0m) // New
            );
            await _dbContext.SaveChangesAsync();

            var query = new CompareHoldingsQuery(date1, date2);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.NewPositions.Should().HaveCount(1);
            result.NewPositions.First().Ticker.Should().Be("MSFT");

            result.Increased.Should().HaveCount(1);
            result.Increased.First().Ticker.Should().Be("AAPL");
            result.Increased.First().OldShares.Should().Be(100);
            result.Increased.First().NewShares.Should().Be(150);

            result.Reduced.Should().HaveCount(1);
            result.Reduced.First().Ticker.Should().Be("TSLA");
            result.Reduced.First().OldShares.Should().Be(50);
            result.Reduced.First().NewShares.Should().Be(20);
        }
    }
}
