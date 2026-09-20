using System.Security.Claims;
using API.Extensions;
using Xunit;

namespace Tests.API.Extensions;

public class ClaimsPrincipalExtensionsTests
{
    private static ClaimsPrincipal Principal(params Claim[] claims) => new(new ClaimsIdentity(claims, "test"));

    [Fact]
    public void GetUserId_ReadsTheNameIdentifierClaim()
    {
        var id = Guid.NewGuid();

        Assert.Equal(id, Principal(new Claim(ClaimTypes.NameIdentifier, id.ToString())).GetUserId());
    }

    [Fact]
    public void GetUserId_DoesNotFallBackToTheRawSubClaim()
    {
        var principal = Principal(new Claim("sub", Guid.NewGuid().ToString()));

        Assert.Throws<InvalidOperationException>(() => principal.GetUserId());
    }

    [Fact]
    public void GetUserId_Throws_ForANonGuidValue()
    {
        var principal = Principal(new Claim(ClaimTypes.NameIdentifier, "not-a-guid"));

        Assert.Throws<InvalidOperationException>(() => principal.GetUserId());
    }
}
