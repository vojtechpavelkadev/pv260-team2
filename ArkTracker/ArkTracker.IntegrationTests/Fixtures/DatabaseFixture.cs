using ArkTracker.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ArkTracker.IntegrationTests.Fixtures
{
    public class DatabaseFixture : IAsyncLifetime
    {
        private readonly DbContextOptions<AppDbContext> _options;
        public AppDbContext Context { get; private set; } = null!;

        public DatabaseFixture()
        {
            _options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
        }

        public async Task InitializeAsync()
        {
            Context = new AppDbContext(_options);
            await Context.Database.EnsureCreatedAsync();
        }

        public async Task DisposeAsync()
        {
            await Context.Database.EnsureDeletedAsync();
            await Context.DisposeAsync();
        }
    }
}
