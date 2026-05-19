using ArkTracker.Application.Interfaces;
using ArkTracker.Domain.Entities;
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

        return Compare(oldList, newList);
    }

    private CompareHoldingsResult Compare(
        IEnumerable<HoldingRecord> oldList,
        IEnumerable<HoldingRecord> newList)
    {
        CompareHoldingsResult result = new();

        Dictionary<string, HoldingRecord> oldHoldingsDict = ToHoldingDictionary(oldList);
        Dictionary<string, HoldingRecord> newHoldingsDict = ToHoldingDictionary(newList);

        AddNewPositions(result, oldHoldingsDict, newHoldingsDict);
        AddIncreasedPositions(result, oldHoldingsDict, newHoldingsDict);
        AddReducedPositions(result, oldHoldingsDict, newHoldingsDict);

        return result;
    }
    private Dictionary<string, HoldingRecord> ToHoldingDictionary(IEnumerable<HoldingRecord> list)
    {
        return list
            .Where(x => !string.IsNullOrWhiteSpace(x.Ticker) || !string.IsNullOrWhiteSpace(x.Cusip))
            .ToDictionary(x => x.Ticker ?? x.Cusip ?? string.Empty);
    }
    private void AddNewPositions(
        CompareHoldingsResult result,
        Dictionary<string, HoldingRecord> oldDict,
        Dictionary<string, HoldingRecord> newDict)
    {
        foreach (string? key in newDict.Keys.Except(oldDict.Keys))
        {
            HoldingRecord h = newDict[key];

            result.NewPositions.Add(new SimpleHolding
            {
                Company = h.Company ?? string.Empty,
                Ticker = h.Ticker ?? h.Cusip ?? string.Empty,
                Shares = h.Shares ?? 0,
                Weight = h.WeightPercentage ?? 0
            });
        }
    }
    private void AddIncreasedPositions(
        CompareHoldingsResult result,
        Dictionary<string, HoldingRecord> oldDict,
        Dictionary<string, HoldingRecord> newDict)
    {
        foreach (string? key in newDict.Keys.Intersect(oldDict.Keys))
        {
            HoldingRecord oldH = oldDict[key];
            HoldingRecord newH = newDict[key];

            long oldShares = oldH.Shares ?? 0;
            long newShares = newH.Shares ?? 0;

            if (newShares > oldShares)
            {
                result.Increased.Add(new HoldingDelta
                {
                    Company = newH.Company ?? string.Empty,
                    Ticker = newH.Ticker ?? newH.Cusip ?? string.Empty,
                    OldShares = oldShares,
                    NewShares = newShares,
                    NewWeight = newH.WeightPercentage ?? 0
                });
            }
        }
    }
    private void AddReducedPositions(
        CompareHoldingsResult result,
        Dictionary<string, HoldingRecord> oldDict,
        Dictionary<string, HoldingRecord> newDict)
    {
        foreach (string key in oldDict.Keys)
        {
            HoldingRecord oldH = oldDict[key];
            long oldShares = oldH.Shares ?? 0;

            if (!newDict.TryGetValue(key, out HoldingRecord? newH))
            {
                result.Reduced.Add(CreateReduced(oldH, oldShares, 0, 0));
                continue;
            }

            long newShares = newH.Shares ?? 0;

            if (newShares < oldShares)
            {
                result.Reduced.Add(CreateReduced(oldH, oldShares, newShares, newH.WeightPercentage ?? 0));
            }
        }
    }
    private HoldingDelta CreateReduced(
        HoldingRecord h,
        long oldShares,
        long newShares,
        decimal newWeight)
    {
        return new HoldingDelta
        {
            Company = h.Company ?? string.Empty,
            Ticker = h.Ticker ?? h.Cusip ?? string.Empty,
            OldShares = oldShares,
            NewShares = newShares,
            NewWeight = newWeight
        };
    }

}