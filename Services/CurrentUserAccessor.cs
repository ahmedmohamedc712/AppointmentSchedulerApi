using System;
using System.Security.Claims;

namespace AppointmentScheduler.Services;

public class CurrentUserAccessor(IHttpContextAccessor httpContextAccessor) : ICurrentUserAccessor
{
    public int GetCurrentUserId()
    {
        int userId = int.Parse(httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)!.Value!);
        return userId;
    }

    public string GetCurrentUserEmail()
    {
        var httpContext = httpContextAccessor.HttpContext!;
        var claim = httpContext.User.FindFirst(ClaimTypes.Email)!;
        return claim.Value;
    }

    public string GetCurrentUserName()
    {
        var httpContext = httpContextAccessor.HttpContext!;
        var claim = httpContext.User.FindFirst(ClaimTypes.Name)!;
        return claim.Value;
    }
}
