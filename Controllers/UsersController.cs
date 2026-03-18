using AppointmentScheduler.Data.DTOs;
using AppointmentScheduler.Exceptions;
using AppointmentScheduler.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;

namespace AppointmentScheduler.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController(IUsersService service) : ControllerBase
    {
        public sealed record Response(string AccessToken, string RefreshToken);
        public sealed record RefreshTokenRequest(string RefreshToken);

        [HttpPost("signup")]
        public async Task<IActionResult> Signup(SignupRequest request)
        {
            await service.Signup(request);
            return Ok();
        }

        [HttpPost("login")]
        public async Task<ActionResult<Response>> Login(LoginRequest request)
        {
            var response = await service.Login(request);
            return Ok(response);
        }

        [HttpPost("refresh-tokens")]
        public async Task<ActionResult<Response>> LoginUserWithRefreshToken(RefreshTokenRequest request)
        {
            var response = await service.LoginUserWithRefreshToken(request.RefreshToken);
            return response;
        }

        [Authorize]
        [HttpDelete("{userId}/refresh-tokens")]
        public async Task<IActionResult> RevokeRefreshTokens(int userId)
        {
            await service.RevokeRefreshTokens(userId);
            return NoContent();
        }

        [HttpPost]
        public async Task<ActionResult<Response>> CreateUser(CreateUserRequest request)
        {
            Response response = await service.Create(request);
            return response;
        }
    }
}
