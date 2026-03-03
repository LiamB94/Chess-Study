using System.Security.Claims;

namespace ChessStudy.Api.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static int GetUserId(this ClaimsPrincipal user)
    {
        var raw = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(raw))
        {
            throw new Exception("User ID claim is missing.");
        }

        return int.Parse(raw);
    }
}