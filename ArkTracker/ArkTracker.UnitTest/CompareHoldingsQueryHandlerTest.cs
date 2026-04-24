using ArkTracker.Application;
using ArkTracker.Application.CompareHoldings;
using ArkTracker.Application.Interfaces;
using ArkTracker.Domain.Entities;
using Moq;
using FluentAssertions;
namespace ArkTracker.UnitTest;

public class CompareHoldingsQueryHandlerTests
{
    private readonly Mock<IHoldingRepository> _repoMock;
    private readonly CompareHoldingsQueryHandler _handler;

    public CompareHoldingsQueryHandlerTests()
    {
        _repoMock = new Mock<IHoldingRepository>();
        _handler = new CompareHoldingsQueryHandler(_repoMock.Object);
    }
    
    [Fact]
    public async Task Should_compare_holdings_when_dates_are_provided()
    {
        var from = new DateTime(2026, 04, 19);
        var to = new DateTime(2026, 04, 20);

        _repoMock.Setup(r => r.GetByDateAsync(from))
            .ReturnsAsync(new List<HoldingRecord>
            {
                new() { Ticker = "TSLA", Company = "Tesla", Shares = 100, WeightPercentage = 5 }
            });

        _repoMock.Setup(r => r.GetByDateAsync(to))
            .ReturnsAsync(new List<HoldingRecord>
            {
                new() { Ticker = "TSLA", Company = "Tesla", Shares = 150, WeightPercentage = 6 }
            });

        var query = new CompareHoldingsQuery(from, to);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Increased.Should().HaveCount(1);
        result.Reduced.Should().BeEmpty();
        result.NewPositions.Should().BeEmpty();
    }
    
    [Fact]
    public async Task Should_detect_new_position()
    {
        var from = new DateTime(2026, 04, 19);
        var to = new DateTime(2026, 04, 20);

        _repoMock.Setup(r => r.GetByDateAsync(from))
            .ReturnsAsync(new List<HoldingRecord>());

        _repoMock.Setup(r => r.GetByDateAsync(to))
            .ReturnsAsync(new List<HoldingRecord>
            {
                new() { Ticker = "NVDA", Company = "Nvidia", Shares = 200 }
            });

        var result = await _handler.Handle(
            new CompareHoldingsQuery(from, to),
            CancellationToken.None);

        result.NewPositions.Should().ContainSingle();
    }
    
    [Fact]
    public async Task Should_detect_reduced_position()
    {
        var from = new DateTime(2026, 04, 19);
        var to = new DateTime(2026, 04, 20);

        _repoMock.Setup(r => r.GetByDateAsync(from))
            .ReturnsAsync(new List<HoldingRecord>
            {
                new() { Ticker = "TSLA", Company = "Tesla", Shares = 200 }
            });

        _repoMock.Setup(r => r.GetByDateAsync(to))
            .ReturnsAsync(new List<HoldingRecord>
            {
                new() { Ticker = "TSLA", Company = "Tesla", Shares = 50 }
            });

        var result = await _handler.Handle(
            new CompareHoldingsQuery(from, to),
            CancellationToken.None);

        result.Reduced.Should().ContainSingle();
    }
    [Fact]
    public async Task Should_detect_full_exit_as_reduced_to_zero()
    {
        var from = new DateTime(2026, 04, 19);
        var to = new DateTime(2026, 04, 20);

        _repoMock.Setup(r => r.GetByDateAsync(from))
            .ReturnsAsync(new List<HoldingRecord>
            {
                new() { Ticker = "TSLA", Company = "Tesla", Shares = 100 }
            });

        _repoMock.Setup(r => r.GetByDateAsync(to))
            .ReturnsAsync(new List<HoldingRecord>());

        var result = await _handler.Handle(
            new CompareHoldingsQuery(from, to),
            CancellationToken.None);

        result.Reduced.Should().ContainSingle();
    }
    
    [Fact]
    public async Task Should_use_latest_two_dates_when_null()
    {
        _repoMock.Setup(r => r.GetAvailableDatesAsync())
            .ReturnsAsync(new List<DateTime>
            {
                new DateTime(2026, 04, 20),
                new DateTime(2026, 04, 19),
                new DateTime(2026, 04, 18)
            });

        _repoMock.Setup(r => r.GetByDateAsync(It.IsAny<DateTime>()))
            .ReturnsAsync(new List<HoldingRecord>());

        var result = await _handler.Handle(
            new CompareHoldingsQuery(null, null),
            CancellationToken.None);
        
        
        _repoMock.Verify(r => r.GetByDateAsync(new DateTime(2026, 04, 19)), Times.Once);
        _repoMock.Verify(r => r.GetByDateAsync(new DateTime(2026, 04, 20)), Times.Once);
        result.Should().NotBeNull();
    }
    [Fact]
    public async Task Should_throw_when_less_than_two_dates_exist()
    {
        _repoMock.Setup(r => r.GetAvailableDatesAsync())
            .ReturnsAsync(new List<DateTime>
            {
                new DateTime(2026, 04, 20)
            });

        await Assert.ThrowsAsync<Exception>(() =>
            _handler.Handle(new CompareHoldingsQuery(null, null), CancellationToken.None));
    }
    
    [Fact]
    public async Task Should_select_latest_two_dates_correctly_regardless_of_order()
    {
        _repoMock.Setup(r => r.GetAvailableDatesAsync())
            .ReturnsAsync(new List<DateTime>
            {
                new DateTime(2026, 04, 18),
                new DateTime(2026, 04, 20),
                new DateTime(2026, 04, 19)
            });

        _repoMock.Setup(r => r.GetByDateAsync(It.IsAny<DateTime>()))
            .ReturnsAsync(new List<HoldingRecord>());

        await _handler.Handle(new CompareHoldingsQuery(null, null), CancellationToken.None);

        _repoMock.Verify(r => r.GetByDateAsync(new DateTime(2026, 04, 19)), Times.Once);
        _repoMock.Verify(r => r.GetByDateAsync(new DateTime(2026, 04, 20)), Times.Once);
    }
    [Fact]
    public async Task Should_return_empty_changes_when_portfolio_is_identical()
    {
        var from = new DateTime(2026, 04, 19);
        var to = new DateTime(2026, 04, 20);

        var holdings = new List<HoldingRecord>
        {
            new() { Ticker = "TSLA", Shares = 100 }
        };

        _repoMock.Setup(r => r.GetByDateAsync(from)).ReturnsAsync(holdings);
        _repoMock.Setup(r => r.GetByDateAsync(to)).ReturnsAsync(holdings);

        var result = await _handler.Handle(new CompareHoldingsQuery(from, to), CancellationToken.None);

        result.NewPositions.Should().BeEmpty();
        result.Increased.Should().BeEmpty();
        result.Reduced.Should().BeEmpty();
    }
}