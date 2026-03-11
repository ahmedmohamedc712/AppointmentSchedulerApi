namespace AppointmentScheduler.Data.DTOs;

public record ReadAppointmentDto(int Id, string Title, string Description, DateTime CreatedAt, DateTime Date, DateTime ReminderDate);
