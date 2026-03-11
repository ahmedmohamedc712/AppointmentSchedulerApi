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
        public const string TIME_ZONE_HEADER = "X-TimeZone" ;
        [HttpPost]
        public async Task<IActionResult> Create(
            CreateAppointmentRequest request,
            [FromHeader(Name = TIME_ZONE_HEADER)] string userTimeZone)
        {
            await service.Create(request, userTimeZone);
            return Created();
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ReadAppointmentDto>>> Get(
            [FromHeader(Name = TIME_ZONE_HEADER)] string userTimeZone
        )
        {
            var appointments = await service.Get(userTimeZone);
            return Ok(appointments);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<IEnumerable<ReadAppointmentDto>>> GetById(
            int id,
            [FromHeader(Name = TIME_ZONE_HEADER)] string userTimeZone
        )
        {
            var appointmentDto = await service.GetById(id, userTimeZone);
            return Ok(appointmentDto);
        }
    }
}
