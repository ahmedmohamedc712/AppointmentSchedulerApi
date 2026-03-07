using System;
using NodaTime;

namespace AppointmentScheduler.Models;

public class Appointment
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Instant Date { get; set; }
    public Instant ReminderDate { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = null!;
}
