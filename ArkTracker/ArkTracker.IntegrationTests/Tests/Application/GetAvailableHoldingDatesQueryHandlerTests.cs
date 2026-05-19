using ArkTracker.Application.GetAvailableHoldingDates;
using ArkTracker.Domain.Entities;
using ArkTracker.Infrastructure.Persistence;
using ArkTracker.IntegrationTests.Fixtures;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using MediatR;

namespace ArkTracker.IntegrationTests.Tests.Application
{
    public class GetAvailableHoldingDatesQueryHandlerTests : IAsyncLifetime
    {
        private readonly DatabaseFixture _fixture;
        private GetAvailableHoldingDatesQueryHandler _handler = null!;
        private AppDbContext _dbContext = null!;

        public GetAvailableHoldingDatesQueryHandlerTests()
        {
            _fixture = new DatabaseFixture();
        }

        public async Task InitializeAsync()
        {
            await _fixture.InitializeAsync();
            _dbContext = _fixture.Context;

            var repository = new HoldingRepository(_dbContext);
            _handler = new GetAvailableHoldingDatesQueryHandler(repository);
        }

        public async Task DisposeAsync()
        {
            await _fixture.DisposeAsync();
        }

        [Fact]
        public async Task Handle_ReturnsDistinctDates_OrderedDescending()
        {
            // Arrange
            _dbContext.Holdings.AddRange(
                new HoldingRecord(new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, null, null, null),
                new HoldingRecord(new DateTime(2026, 1, 3, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, null, null, null),
                new HoldingRecord(new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, null, null, null),
                new HoldingRecord(new DateTime(2026, 1, 2, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, null, null, null)
            );
            await _dbContext.SaveChangesAsync();

            var query = new GetAvailableHoldingDatesQuery();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Dates.Should().HaveCount(3);
            result.Dates.Should().BeInDescendingOrder();
            result.Dates.ElementAt(0).Should().Be(new DateTime(2026, 1, 3, 0, 0, 0, DateTimeKind.Utc));
            result.Dates.ElementAt(1).Should().Be(new DateTime(2026, 1, 2, 0, 0, 0, DateTimeKind.Utc));
            result.Dates.ElementAt(2).Should().Be(new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));
        }
    }
}
