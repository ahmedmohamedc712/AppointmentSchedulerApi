using AppointmentScheduler.Data.DTOs;
using AppointmentScheduler.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AppointmentScheduler.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AppointmentsController(IAppointmentService service) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> Create(CreateAppointmentRequest request)
        {
            await service.Create(request);
            return Created();
        }
    }
}
