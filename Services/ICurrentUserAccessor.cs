using System;

namespace AppointmentScheduler.Services;

public interface ICurrentUserAccessor
{
    int GetCurrentUserId();
    string GetCurrentUserEmail();
    string? GetCurrentUserName();
}
