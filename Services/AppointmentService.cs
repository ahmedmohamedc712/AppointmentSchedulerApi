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
    ISchedulerFactory schedulerFactory) : IAppointmentService
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
            Date = appointmentDate,
            ReminderDate = appointmentReminder,
            UserId = userId
        };

        await context.Appointments.AddAsync(appointment);
        await context.SaveChangesAsync();

        var scheduler = await schedulerFactory.GetScheduler();

        var remainderJob = JobBuilder.Create<AppointmentReminderJob>()
            .WithIdentity($"appointmentReminder-{appointment.Id}")
            .UsingJobData("AppointmentTitle", appointment.Title)
            .UsingJobData("AppointmentDescription", appointment.Description)
            .UsingJobData("AppointmentDate", $"{appointment.Date}")
            .UsingJobData("UserEmail", currentUserAccessor.GetCurrentUserEmail())
            .UsingJobData("UserName", currentUserAccessor.GetCurrentUserName())
            .Build();

        var remainderTrigger = TriggerBuilder.Create()
            .WithIdentity($"appointmentReminder-trigger-{appointment.Id}")
            .StartAt(appointment.ReminderDate.ToDateTimeOffset())
            .Build();

        await scheduler.ScheduleJob(remainderJob, remainderTrigger);

        if (request.WantAutoDelete)
        {
            var removingJob = JobBuilder.Create<AppointmentRemovingJob>()
            .WithIdentity($"appointmentRemoving-{appointment.Id}")
            .UsingJobData("AppointmentId", appointment.Id)
            .Build();

            var removingTrigger = TriggerBuilder.Create()
                .WithIdentity($"appointmentRemoving-trigger-{appointment.Id}")
                .StartAt(appointment.Date.ToDateTimeOffset())
                .Build();

            await scheduler.ScheduleJob(removingJob, removingTrigger);
        }
    }
}
