using ArkTracker.Application.Interfaces;
using ArkTracker.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ArkTracker.Infrastructure.Services;

public class DatabaseHealthService(AppDbContext context) : IDatabaseHealthService
{
    public async Task<int> GetUserCountAsync(CancellationToken cancellationToken = default)
    {
        return await context.Users.CountAsync(cancellationToken);
    }
}
