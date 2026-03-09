using System;
using System.Text;
using AppointmentScheduler.Data.DTOs;
using static AppointmentScheduler.Controllers.UsersController;

namespace AppointmentScheduler.Services;

public interface IUsersService
{
    Task<Response> Signup(SignupRequest request);
    Task<Response> Login(LoginRequest request);
    Task<Response> LoginUserWithRefreshToken(string refreshToken);
    Task RevokeRefreshTokens(int userId);
}
