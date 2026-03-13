using System;

namespace AppointmentScheduler.Data.DTOs;

public record UpdateAppointmentRequest(int Id, string Title, string Description, DateTime Date, DateTime ReminderDate, bool WantAutoDelete = false) : IAppointmentRequest;
