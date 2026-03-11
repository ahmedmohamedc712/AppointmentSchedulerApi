namespace AppointmentScheduler.Data.DTOs;

public record ReadAppointmentDto(string Title, string Description, DateTime CreatedAt, DateTime Date, DateTime ReminderDate);
