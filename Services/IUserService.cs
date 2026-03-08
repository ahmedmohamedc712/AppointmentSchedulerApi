using System;
using System.Text;
using AppointmentScheduler.Data.DTOs;

namespace AppointmentScheduler.Services;

public interface IUsersService
{
    Task<string> Signup(SignupRequest request);
}
