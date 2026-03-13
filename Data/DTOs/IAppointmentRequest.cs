using System;

namespace AppointmentScheduler.Data.DTOs;

public interface IAppointmentRequest
{
    public string Title { get; init; }
    public string Description { get; init; }
    public DateTime Date { get; init; }
    public DateTime ReminderDate { get; init; }
}
