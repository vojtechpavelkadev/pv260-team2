using ArkTracker.Domain.Entities;

namespace ArkTracker.Application.Interfaces
{
    public interface IArkScraperService
    {
        Task<IEnumerable<HoldingRecord>> DownloadHoldingsAsync();
    }
}
