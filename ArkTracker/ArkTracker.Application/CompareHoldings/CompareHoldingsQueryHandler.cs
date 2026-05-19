using ArkTracker.Application.Interfaces;
using ArkTracker.Domain.Entities;
using ArkTracker.Domain.Services;
using ArkTracker.Domain.ValueObjects;
using MediatR;

namespace ArkTracker.Application.CompareHoldings;

public class CompareHoldingsQueryHandler
    : IRequestHandler<CompareHoldingsQuery, CompareHoldingsResult>
{
    private readonly IHoldingRepository _repository;
    private const int NumberOfDatesToCompare = 2;

    public CompareHoldingsQueryHandler(IHoldingRepository repository)
    {
        _repository = repository;
    }

    public async Task<CompareHoldingsResult> Handle(
        CompareHoldingsQuery request,
        CancellationToken cancellationToken)
    {
        DateTime from;
        DateTime to;

        if (request.From == null || request.To == null)
        {
            IEnumerable<DateTime> dates = await _repository.GetAvailableDatesAsync();

            List<DateTime> latestTwo = dates
                .OrderByDescending(d => d)
                .Take(NumberOfDatesToCompare)
                .ToList();

            if (latestTwo.Count < NumberOfDatesToCompare)
            {
                throw new Exception("Not enough data to compare.");
            }

            from = latestTwo[1];
            to = latestTwo[0];
        }
        else
        {
            from = request.From.Value;
            to = request.To.Value;
        }

        IEnumerable<HoldingRecord> oldList = await _repository.GetByDateAsync(from);
        IEnumerable<HoldingRecord> newList = await _repository.GetByDateAsync(to);

        var comparisonService = new HoldingComparisonService();
        return comparisonService.Compare(oldList, newList);
    }
}