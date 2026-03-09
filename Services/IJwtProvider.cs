using System;
using AppointmentScheduler.Models;

namespace AppointmentScheduler.Services;

public interface IJwtProvider
{
    string Create(User user);
    string GenerateRefreshToken();
}
