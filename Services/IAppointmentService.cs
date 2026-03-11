using System;
using AppointmentScheduler.Data.DTOs;

namespace AppointmentScheduler.Services;

public interface IAppointmentService
{
    Task Create(CreateAppointmentRequest request);
    Task<IEnumerable<ReadAppointmentDto>> Get(string userZone);
}
