using ArkTracker.Application.Interfaces;
using ArkTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ArkTracker.Infrastructure.Persistence
{
    public class HoldingRepository(AppDbContext context) : IHoldingRepository
    {
        public async Task AddRangeAsync(IEnumerable<HoldingRecord> records)
        {
            await context.Holdings.AddRangeAsync(records);
            _ = await context.SaveChangesAsync();
        }

        public async Task<IEnumerable<DateTime>> GetAvailableDatesAsync()
        {
            return await context.Holdings
                    .Where(x => x.Date.HasValue)
                    .Select(x => x.Date!.Value.Date)
                    .Distinct()
                    .OrderByDescending(d => d)
                    .ToListAsync();
        }

        public async Task<IEnumerable<HoldingRecord>> GetByDateAsync(DateTime date)
        {
            return await context.Holdings
                .Where(x => x.Date.HasValue && x.Date.Value.Date == date.Date)
                .ToListAsync();
        }
    }
}
