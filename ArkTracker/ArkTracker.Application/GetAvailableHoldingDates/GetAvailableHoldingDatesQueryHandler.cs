using ArkTracker.Application.Interfaces;
using MediatR;

namespace ArkTracker.Application.GetAvailableHoldingDates;

public class GetAvailableHoldingDatesQueryHandler
    : IRequestHandler<GetAvailableHoldingDatesQuery, GetAvailableHoldingDatesResult>
{
    private readonly IHoldingRepository _repository;

    public GetAvailableHoldingDatesQueryHandler(IHoldingRepository repository)
    {
        _repository = repository;
    }

    public async Task<GetAvailableHoldingDatesResult> Handle(
        GetAvailableHoldingDatesQuery request,
        CancellationToken cancellationToken)
    {
        var dates = await _repository.GetAvailableDatesAsync();
        var result = new GetAvailableHoldingDatesResult(dates);
        return result;
    }
}