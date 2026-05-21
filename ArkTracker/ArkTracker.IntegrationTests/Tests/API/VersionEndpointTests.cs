using System.Net;
using System.Net.Http.Json;
using ArkTracker.IntegrationTests.Fixtures;
using FluentAssertions;
using Xunit;

namespace ArkTracker.IntegrationTests.Tests.API
{
    public class VersionEndpointTests : IClassFixture<ApiWebApplicationFactory>
    {
        private readonly ApiWebApplicationFactory _factory;

        public VersionEndpointTests(ApiWebApplicationFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task GetVersion_ReturnsConfiguredVersion()
        {
            Environment.SetEnvironmentVariable("APP_VERSION", "v1.2.3-test");
            using var client = _factory.CreateClient();

            var response = await client.GetAsync("/version");

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var result = await response.Content.ReadFromJsonAsync<VersionResponse>();
            result.Should().NotBeNull();
            result!.Version.Should().Be("v1.2.3-test");
        }

        private sealed class VersionResponse
        {
            public string Version { get; set; } = string.Empty;
        }
    }
}
