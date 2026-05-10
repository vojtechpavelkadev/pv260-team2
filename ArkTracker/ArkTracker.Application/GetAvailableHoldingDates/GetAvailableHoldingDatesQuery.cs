using MediatR;

namespace ArkTracker.Application.GetAvailableHoldingDates;

public record GetAvailableHoldingDatesQuery()
    : IRequest<GetAvailableHoldingDatesResult>;