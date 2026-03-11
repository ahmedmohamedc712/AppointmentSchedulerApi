using System;

namespace AppointmentScheduler.Data.DTOs;

public record CreateAppointmentRequest(string Title, string Description, DateTime Date, DateTime ReminderDate, bool WantAutoDelete);