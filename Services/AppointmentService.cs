using System;
using AppointmentScheduler.BackgroundJobs;
using AppointmentScheduler.Data;
using AppointmentScheduler.Data.DTOs;
using AppointmentScheduler.Exceptions;
using AppointmentScheduler.Models;
using NodaTime;
using Quartz;
using TaskScheduler.Services;

namespace AppointmentScheduler.Services;

public class AppointmentService(AppDbContext context,
    ICurrentUserAccessor currentUserAccessor,
    IUtcLocalConverter utcLocalConverter,
    IBackgroundJobProvider jobProvider) : IAppointmentService
{
    public async Task Create(CreateAppointmentRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.Description))
        {
            throw new BadRequestException("You must provide title and description for the appointment.");
        }

        var systemClock = SystemClock.Instance;
        var currentUtc = systemClock.GetCurrentInstant();
        Instant appointmentDate = utcLocalConverter.ConvertLocalToUtc(request.Date, request.UserTimeZone);
        if (appointmentDate <= currentUtc)
        {
            throw new BadRequestException("Appointment date must be in the future.");
        }

        Instant appointmentReminder = utcLocalConverter.ConvertLocalToUtc(request.ReminderDate, request.UserTimeZone);
        if (appointmentReminder <= currentUtc)
        {
            throw new BadRequestException("Reminder date must be in the future.");
        }

        if (appointmentReminder >= appointmentDate)
        {
            throw new BadRequestException("Reminder must be before the appointment date.");
        }

        int userId = currentUserAccessor.GetCurrentUserId();

        var appointment = new Appointment()
        {
            Title = request.Title,
            Description = request.Description,
            CreatedAt = currentUtc,
            Date = appointmentDate,
            ReminderDate = appointmentReminder,
            UserId = userId
        };

        await context.Appointments.AddAsync(appointment);
        await context.SaveChangesAsync();

        await jobProvider.CreateReminderJob(appointment,
            currentUserAccessor.GetCurrentUserEmail(),
            currentUserAccessor.GetCurrentUserName());

        if (request.WantAutoDelete)
        {
            await jobProvider.CreateRemoverJob(appointment);
        }
    }
}
