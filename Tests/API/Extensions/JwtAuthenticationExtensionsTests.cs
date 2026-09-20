using API.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Tests.API.Extensions;

public class JwtAuthenticationExtensionsTests
{
    [Theory]
    [InlineData(null, "issuer", "audience")]
    [InlineData("key", null, "audience")]
    [InlineData("key", "issuer", null)]
    public void AddJwtAuthentication_Throws_WhenConfigIncomplete(string? key, string? issuer, string? audience)
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = key,
                ["Jwt:Issuer"] = issuer,
                ["Jwt:Audience"] = audience
            })
            .Build();
        var services = new ServiceCollection();

        Assert.Throws<InvalidOperationException>(() => services.AddJwtAuthentication(config));
    }

    [Fact]
    public void AddJwtAuthentication_Succeeds_WhenConfigComplete()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "a-sufficiently-long-test-signing-key-value",
                ["Jwt:Issuer"] = "http://recipe-auth-app/",
                ["Jwt:Audience"] = "recipe-frontend"
            })
            .Build();
        var services = new ServiceCollection();

        var exception = Record.Exception(() => services.AddJwtAuthentication(config));

        Assert.Null(exception);
    }
}
