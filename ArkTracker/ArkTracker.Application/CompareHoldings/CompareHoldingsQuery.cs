using MediatR;

namespace ArkTracker.Application.CompareHoldings;

public record CompareHoldingsQuery(
    DateTime? From,
    DateTime? To
) : IRequest<CompareHoldingsResult>;