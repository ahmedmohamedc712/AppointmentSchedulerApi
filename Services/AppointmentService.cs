using System;
using System.Drawing;
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
    IBackgroundJobProvider jobProvider,
    IClock clock) : IAppointmentService
{
    public async Task Create(CreateAppointmentRequest request, string userTimeZone)
    {
        var currentUtc = clock.GetCurrentInstant();
        var (appointmentDate, appointmentReminder) = ValidateAppointmentAndConvertAppointmentDates(request, currentUtc, userTimeZone);

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
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .ToListAsync();

        IEnumerable<ReadAppointmentDto> readAppointmentDtos = appointments.Select(x =>
        new ReadAppointmentDto(
            x.Id,
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
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);

        if (appointment == null)
            throw new NotFoundException(id);

        return new ReadAppointmentDto(
            appointment.Id,
            appointment.Title,
            appointment.Description,
            utcLocalConverter.ConvertUtcToLocal(appointment.CreatedAt, userTimeZone),
            utcLocalConverter.ConvertUtcToLocal(appointment.Date, userTimeZone),
            utcLocalConverter.ConvertUtcToLocal(appointment.ReminderDate, userTimeZone)
        );
    }

    public async Task Update(int id, UpdateAppointmentRequest request, string userTimeZone)
    {
        if (id != request.Id)
            throw new BadRequestException("Id mismatch.");

        var userId = currentUserAccessor.GetCurrentUserId();

        var appointmentToUpdate = await context.Appointments
            .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);

        if (appointmentToUpdate == null)
            throw new NotFoundException(id);

        var currentUtc = clock.GetCurrentInstant();
        var (appointmentDate, appointmentReminder) = ValidateAppointmentAndConvertAppointmentDates(request, currentUtc, userTimeZone);

        appointmentToUpdate.Title = request.Title;
        appointmentToUpdate.Description = request.Description;
        appointmentToUpdate.UpdatedAt = currentUtc;
        appointmentToUpdate.Date = appointmentDate;
        appointmentToUpdate.ReminderDate = appointmentReminder;

        await context.SaveChangesAsync();

        var userEmail = currentUserAccessor.GetCurrentUserEmail();
        var userName = currentUserAccessor.GetCurrentUserName();

        await jobProvider.RescheduleAppointmentsJobs(appointmentToUpdate, userEmail, userName, request.WantAutoDelete);
    }

    public async Task Delete(int id)
    {
        int userId = currentUserAccessor.GetCurrentUserId();
        var appointmentToDelete = await context.Appointments
            .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);

        if (appointmentToDelete == null)
            throw new NotFoundException(id);

        context.Appointments.Remove(appointmentToDelete);
        await context.SaveChangesAsync();

        await jobProvider.DeleteJobs(appointmentToDelete.Id);
    }

    private (Instant, Instant) ValidateAppointmentAndConvertAppointmentDates(IAppointmentRequest appointment, Instant currentUtc, string userTimeZone)
    {
        if (string.IsNullOrWhiteSpace(appointment.Title) || string.IsNullOrWhiteSpace(appointment.Description))
        {
            throw new BadRequestException("You must provide title and description for the appointment.");
        }

        Instant appointmentDate = utcLocalConverter.ConvertLocalToUtc(appointment.Date, userTimeZone);
        if (appointmentDate <= currentUtc)
        {
            throw new BadRequestException("Appointment date must be in the future.");
        }

        Instant appointmentReminder = utcLocalConverter.ConvertLocalToUtc(appointment.ReminderDate, userTimeZone);
        if (appointmentReminder <= currentUtc)
        {
            throw new BadRequestException("Reminder date must be in the future.");
        }

        if (appointmentReminder >= appointmentDate)
        {
            throw new BadRequestException("Reminder must be before the appointment date.");
        }

        return (appointmentDate, appointmentReminder);
    }
}
