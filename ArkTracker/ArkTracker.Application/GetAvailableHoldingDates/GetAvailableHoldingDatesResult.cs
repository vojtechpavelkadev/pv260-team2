namespace ArkTracker.Application.GetAvailableHoldingDates;

public record GetAvailableHoldingDatesResult(
    IEnumerable<DateTime> Dates
);