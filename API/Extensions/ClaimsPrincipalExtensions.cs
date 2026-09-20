using System.Security.Claims;

namespace API.Extensions;

public static class ClaimsPrincipalExtensions
{
    // Eneste stedet bruker-id hentes fra tokenet. Standard JwtBearer omdøper "sub" til ClaimTypes.NameIdentifier,
    // så FindFirst("sub") ville gitt null. Bruker-id sendes videre til Application som eksplisitt parameter -
    // Application skal aldri kjenne til ClaimsPrincipal/HttpContext.
    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        var value = user.FindFirstValue(ClaimTypes.NameIdentifier);

        return Guid.TryParse(value, out var userId)
            ? userId
            : throw new InvalidOperationException("Tokenet mangler en gyldig bruker-id (NameIdentifier).");
    }
}
