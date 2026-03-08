using AppointmentScheduler.Data.DTOs;
using AppointmentScheduler.Exceptions;
using AppointmentScheduler.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AppointmentScheduler.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController(IUsersService service) : ControllerBase
    {
        [HttpPost("signup")] 
        public async Task<IActionResult> Signup(SignupRequest request)
        {
            string token = await service.Signup(request);
            return Ok(token);
        }
    }
}
