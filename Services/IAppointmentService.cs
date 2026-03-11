using System;
using AppointmentScheduler.Data.DTOs;

namespace AppointmentScheduler.Services;

public interface IAppointmentService
{
    Task Create(CreateAppointmentRequest request, string userTimeZone);
    Task<IEnumerable<ReadAppointmentDto>> Get(string userTimeZone);
    Task<ReadAppointmentDto> GetById(int id, string userTimeZone);
}
