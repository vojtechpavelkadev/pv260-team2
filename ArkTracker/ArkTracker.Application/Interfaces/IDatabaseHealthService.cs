namespace ArkTracker.Application.Interfaces;

public interface IDatabaseHealthService
{
    Task<int> GetUserCountAsync(CancellationToken cancellationToken = default);
}
