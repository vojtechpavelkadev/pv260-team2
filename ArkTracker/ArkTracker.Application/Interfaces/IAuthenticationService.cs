using ArkTracker.Domain.Entities;

namespace ArkTracker.Application.Interfaces;

public interface IAuthenticationService
{
    Task<User?> AuthenticateAsync(string username, string password, CancellationToken cancellationToken = default);
}
