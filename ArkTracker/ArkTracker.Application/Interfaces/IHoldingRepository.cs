using ArkTracker.Domain.Entities;

namespace ArkTracker.Application.Interfaces
{
    public interface IHoldingRepository
    {
        Task AddRangeAsync(IEnumerable<HoldingRecord> records);
        Task<IEnumerable<DateTime>> GetAvailableDatesAsync();
        Task<IEnumerable<HoldingRecord>> GetByDateAsync(DateTime date);
    }
}
