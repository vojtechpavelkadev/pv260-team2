using ArkTracker.Application.CompareHoldings;
using ArkTracker.Application.Interfaces;
using ArkTracker.Domain.Entities;
using FluentAssertions;
using Moq;
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
        DateTime from = new(2026, 04, 19);
        DateTime to = new(2026, 04, 20);

        _ = _repoMock.Setup(r => r.GetByDateAsync(from))
            .ReturnsAsync(
            [
                new() { Ticker = "TSLA", Company = "Tesla", Shares = 100, WeightPercentage = 5 }
            ]);

        _ = _repoMock.Setup(r => r.GetByDateAsync(to))
            .ReturnsAsync(
            [
                new() { Ticker = "TSLA", Company = "Tesla", Shares = 150, WeightPercentage = 6 }
            ]);

        CompareHoldingsQuery query = new(from, to);

        CompareHoldingsResult result = await _handler.Handle(query, CancellationToken.None);

        _ = result.Increased.Should().HaveCount(1);
        _ = result.Reduced.Should().BeEmpty();
        _ = result.NewPositions.Should().BeEmpty();
    }

    [Fact]
    public async Task Should_detect_new_position()
    {
        DateTime from = new(2026, 04, 19);
        DateTime to = new(2026, 04, 20);

        _ = _repoMock.Setup(r => r.GetByDateAsync(from))
            .ReturnsAsync([]);

        _ = _repoMock.Setup(r => r.GetByDateAsync(to))
            .ReturnsAsync(
            [
                new() { Ticker = "NVDA", Company = "Nvidia", Shares = 200 }
            ]);

        CompareHoldingsResult result = await _handler.Handle(
            new CompareHoldingsQuery(from, to),
            CancellationToken.None);

        _ = result.NewPositions.Should().ContainSingle();
    }

    [Fact]
    public async Task Should_detect_reduced_position()
    {
        DateTime from = new(2026, 04, 19);
        DateTime to = new(2026, 04, 20);

        _ = _repoMock.Setup(r => r.GetByDateAsync(from))
            .ReturnsAsync(
            [
                new() { Ticker = "TSLA", Company = "Tesla", Shares = 200 }
            ]);

        _ = _repoMock.Setup(r => r.GetByDateAsync(to))
            .ReturnsAsync(
            [
                new() { Ticker = "TSLA", Company = "Tesla", Shares = 50 }
            ]);

        CompareHoldingsResult result = await _handler.Handle(
            new CompareHoldingsQuery(from, to),
            CancellationToken.None);

        _ = result.Reduced.Should().ContainSingle();
    }
    [Fact]
    public async Task Should_detect_full_exit_as_reduced_to_zero()
    {
        DateTime from = new(2026, 04, 19);
        DateTime to = new(2026, 04, 20);

        _ = _repoMock.Setup(r => r.GetByDateAsync(from))
            .ReturnsAsync(
            [
                new() { Ticker = "TSLA", Company = "Tesla", Shares = 100 }
            ]);

        _ = _repoMock.Setup(r => r.GetByDateAsync(to))
            .ReturnsAsync([]);

        CompareHoldingsResult result = await _handler.Handle(
            new CompareHoldingsQuery(from, to),
            CancellationToken.None);

        _ = result.Reduced.Should().ContainSingle();
    }

    [Fact]
    public async Task Should_use_latest_two_dates_when_null()
    {
        _ = _repoMock.Setup(r => r.GetAvailableDatesAsync())
            .ReturnsAsync(
            [
                new DateTime(2026, 04, 20),
                new DateTime(2026, 04, 19),
                new DateTime(2026, 04, 18)
            ]);

        _ = _repoMock.Setup(r => r.GetByDateAsync(It.IsAny<DateTime>()))
            .ReturnsAsync([]);

        CompareHoldingsResult result = await _handler.Handle(
            new CompareHoldingsQuery(null, null),
            CancellationToken.None);


        _repoMock.Verify(r => r.GetByDateAsync(new DateTime(2026, 04, 19)), Times.Once);
        _repoMock.Verify(r => r.GetByDateAsync(new DateTime(2026, 04, 20)), Times.Once);
        _ = result.Should().NotBeNull();
    }
    [Fact]
    public async Task Should_throw_when_less_than_two_dates_exist()
    {
        _ = _repoMock.Setup(r => r.GetAvailableDatesAsync())
            .ReturnsAsync(
            [
                new DateTime(2026, 04, 20)
            ]);

        _ = await Assert.ThrowsAsync<Exception>(() =>
            _handler.Handle(new CompareHoldingsQuery(null, null), CancellationToken.None));
    }

    [Fact]
    public async Task Should_select_latest_two_dates_correctly_regardless_of_order()
    {
        _ = _repoMock.Setup(r => r.GetAvailableDatesAsync())
            .ReturnsAsync(
            [
                new DateTime(2026, 04, 18),
                new DateTime(2026, 04, 20),
                new DateTime(2026, 04, 19)
            ]);

        _ = _repoMock.Setup(r => r.GetByDateAsync(It.IsAny<DateTime>()))
            .ReturnsAsync([]);

        _ = await _handler.Handle(new CompareHoldingsQuery(null, null), CancellationToken.None);

        _repoMock.Verify(r => r.GetByDateAsync(new DateTime(2026, 04, 19)), Times.Once);
        _repoMock.Verify(r => r.GetByDateAsync(new DateTime(2026, 04, 20)), Times.Once);
    }
    [Fact]
    public async Task Should_return_empty_changes_when_portfolio_is_identical()
    {
        DateTime from = new(2026, 04, 19);
        DateTime to = new(2026, 04, 20);

        List<HoldingRecord> holdings =
        [
            new() { Ticker = "TSLA", Shares = 100 }
        ];

        _ = _repoMock.Setup(r => r.GetByDateAsync(from)).ReturnsAsync(holdings);
        _ = _repoMock.Setup(r => r.GetByDateAsync(to)).ReturnsAsync(holdings);

        CompareHoldingsResult result = await _handler.Handle(new CompareHoldingsQuery(from, to), CancellationToken.None);

        _ = result.NewPositions.Should().BeEmpty();
        _ = result.Increased.Should().BeEmpty();
        _ = result.Reduced.Should().BeEmpty();
    }
}