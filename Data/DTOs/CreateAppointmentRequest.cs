using System;

namespace AppointmentScheduler.Data.DTOs;

public class CreateAppointmentRequest
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool WantAutoDelete { get; set; }
    public DateTime Date { get; set; }
    public DateTime ReminderDate { get; set; }
    public string UserTimeZone { get; set; } = string.Empty;
}
