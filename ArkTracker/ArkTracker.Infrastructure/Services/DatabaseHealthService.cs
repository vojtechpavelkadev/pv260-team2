using ArkTracker.Application.Interfaces;
using ArkTracker.Domain.Exceptions;
using ArkTracker.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ArkTracker.Infrastructure.Services;

public class DatabaseHealthService(AppDbContext context) : IDatabaseHealthService
{
    public async Task<int> GetUserCountAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await context.Users.CountAsync(cancellationToken);
        }
        catch (Exception ex) when (ex is not ArkTrackerException)
        {
            throw new DatabaseConnectionException("Database connection failed.", ex);
        }
    }
}
