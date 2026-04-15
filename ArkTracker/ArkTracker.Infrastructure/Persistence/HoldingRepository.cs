using ArkTracker.Application.Interfaces;
using ArkTracker.Domain.Entities;

namespace ArkTracker.Infrastructure.Persistence
{
    public class HoldingRepository(AppDbContext context) : IHoldingRepository
    {
        public async Task AddRangeAsync(IEnumerable<HoldingRecord> records)
        {
            await context.Holdings.AddRangeAsync(records);
            _ = await context.SaveChangesAsync();
        }

        public Task<IEnumerable<DateTime>> GetAvailableDatesAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<HoldingRecord>> GetByDateAsync(DateTime date)
        {
            throw new NotImplementedException();
        }
    }
}
