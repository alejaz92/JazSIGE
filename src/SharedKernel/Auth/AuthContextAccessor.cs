using System.Security.Claims;

namespace SharedKernel.Auth;

public static class AuthContextAccessor
{
    public static string? GetUserId(ClaimsPrincipal principal) =>
        principal.FindFirstValue(ClaimTypes.NameIdentifier) ?? principal.FindFirstValue("sub");
}
