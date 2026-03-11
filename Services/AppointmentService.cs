using System;
using AppointmentScheduler.BackgroundJobs;
using AppointmentScheduler.Data;
using AppointmentScheduler.Data.DTOs;
using AppointmentScheduler.Exceptions;
using AppointmentScheduler.Models;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Quartz;
using TaskScheduler.Services;

namespace AppointmentScheduler.Services;

public class AppointmentService(AppDbContext context,
    ICurrentUserAccessor currentUserAccessor,
    IUtcLocalConverter utcLocalConverter,
    IBackgroundJobProvider jobProvider) : IAppointmentService
{
    public async Task Create(CreateAppointmentRequest request, string userTimeZone)
    {
        if (string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.Description))
        {
            throw new BadRequestException("You must provide title and description for the appointment.");
        }

        var systemClock = SystemClock.Instance;
        var currentUtc = systemClock.GetCurrentInstant();
        Instant appointmentDate = utcLocalConverter.ConvertLocalToUtc(request.Date, userTimeZone);
        if (appointmentDate <= currentUtc)
        {
            throw new BadRequestException("Appointment date must be in the future.");
        }

        Instant appointmentReminder = utcLocalConverter.ConvertLocalToUtc(request.ReminderDate, userTimeZone);
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

    public async Task<IEnumerable<ReadAppointmentDto>> Get(string userTimeZone)
    {
        int userId = currentUserAccessor.GetCurrentUserId();
        var appointments = await context.Appointments
            .Where(x => x.UserId == userId)
            .ToListAsync();

        IEnumerable<ReadAppointmentDto> readAppointmentDtos = appointments.Select(x => 
        new ReadAppointmentDto(
            x.Title,
            x.Description,
            utcLocalConverter.ConvertUtcToLocal(x.CreatedAt, userTimeZone),
            utcLocalConverter.ConvertUtcToLocal(x.Date, userTimeZone),
            utcLocalConverter.ConvertUtcToLocal(x.ReminderDate, userTimeZone)
            )
        );

        return readAppointmentDtos;
    }

    public async Task<ReadAppointmentDto> GetById(int id, string userTimeZone)
    {
        int userId = currentUserAccessor.GetCurrentUserId();
        var appointment = await context.Appointments
            .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);
        
        if(appointment == null)
            throw new NotFoundException(id);
        
        return new ReadAppointmentDto(
            appointment.Title,
            appointment.Description,
            utcLocalConverter.ConvertUtcToLocal(appointment.CreatedAt, userTimeZone),
            utcLocalConverter.ConvertUtcToLocal(appointment.Date, userTimeZone),
            utcLocalConverter.ConvertUtcToLocal(appointment.ReminderDate, userTimeZone)
        );
    }
}
