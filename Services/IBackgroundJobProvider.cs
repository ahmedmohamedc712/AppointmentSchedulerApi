using System;
using AppointmentScheduler.Models;

namespace AppointmentScheduler.Services;

public interface IBackgroundJobProvider
{
    Task CreateReminderJob(Appointment appointment, string userEmail, string userName);
    Task CreateRemoverJob(Appointment appointment);
}
