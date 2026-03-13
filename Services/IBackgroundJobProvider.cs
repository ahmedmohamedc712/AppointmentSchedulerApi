using System;
using AppointmentScheduler.Models;

namespace AppointmentScheduler.Services;

public interface IBackgroundJobProvider
{
    Task CreateReminderJob(Appointment appointment, string userEmail, string userName);
    Task CreateRemoverJob(Appointment appointment);
    Task DeleteJobs(int appointmentId);
    Task RescheduleAppointmentsJobs(Appointment appointmentToUpdate, string userEmail, string userName, bool wantAutoDelete);
}
