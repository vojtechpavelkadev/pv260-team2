using ArkTracker.Application.Interfaces;
using ArkTracker.Domain.Entities;
using ArkTracker.Infrastructure.Persistence;
using ArkTracker.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;

namespace ArkTracker.Infrastructure.Services;

public class AuthenticationService(AppDbContext db) : IAuthenticationService
{
    public async Task<User?> AuthenticateAsync(string username, string password, CancellationToken cancellationToken = default)
    {
        User? user = await db.Users.SingleOrDefaultAsync(u => u.Username == username, cancellationToken);

        if (user == null || !PasswordHasher.VerifyPassword(password, user.PasswordHash))
        {
            return null;
        }

        return user;
    }
}
