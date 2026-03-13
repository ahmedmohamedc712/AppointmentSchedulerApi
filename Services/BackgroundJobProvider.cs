using System;
using AppointmentScheduler.BackgroundJobs;
using AppointmentScheduler.Models;
using Quartz;
using Quartz.Impl;

namespace AppointmentScheduler.Services;

public class BackgroundJobProvider(ISchedulerFactory schedulerFactory) : IBackgroundJobProvider
{
    private IScheduler? scheduler;
    public async Task CreateReminderJob(Appointment appointment, string userEmail, string userName)
    {
        var scheduler = await GetScheduler();
        var reminderJob = JobBuilder.Create<AppointmentReminderJob>()
            .WithIdentity($"appointmentReminder-{appointment.Id}")
            .UsingJobData("AppointmentTitle", appointment.Title)
            .UsingJobData("AppointmentDescription", appointment.Description)
            .UsingJobData("AppointmentDate", appointment.Date.ToDateTimeOffset().ToString("O"))
            .UsingJobData("UserEmail", userEmail)
            .UsingJobData("UserName", userName)
            .Build();

        var reminderTrigger = TriggerBuilder.Create()
            .WithIdentity($"appointmentReminder-trigger-{appointment.Id}")
            .StartAt(appointment.ReminderDate.ToDateTimeOffset())
            .Build();

        await scheduler.ScheduleJob(reminderJob, reminderTrigger);

    }

    public async Task CreateRemoverJob(Appointment appointment)
    {
        var scheduler = await GetScheduler();
        var removingJob = JobBuilder.Create<AppointmentRemovingJob>()
        .WithIdentity($"appointmentRemoving-{appointment.Id}")
        .UsingJobData("AppointmentId", appointment.Id)
        .Build();

        var removingTrigger = TriggerBuilder.Create()
            .WithIdentity($"appointmentRemoving-trigger-{appointment.Id}")
            .StartAt(appointment.Date.ToDateTimeOffset().AddHours(24))
            .Build();

        await scheduler.ScheduleJob(removingJob, removingTrigger);
    }

    public async Task DeleteJobs(int appointmentId)
    {
        var scheduler = await GetScheduler();

        JobKey jobKey = new JobKey($"appointmentReminder-{appointmentId}");
        await scheduler.DeleteJob(jobKey);

        jobKey = new JobKey($"appointmentRemoving-{appointmentId}");
        await scheduler.DeleteJob(jobKey);
    }

    public async Task RescheduleAppointmentsJobs(Appointment appointmentToUpdate, string userEmail, string userName, bool wantAutoDelete)
    {
        await DeleteJobs(appointmentToUpdate.Id);

        await CreateReminderJob(appointmentToUpdate, userEmail, userName);
        if (wantAutoDelete)
        {
            await CreateRemoverJob(appointmentToUpdate);
        }
    }

    private async Task<IScheduler> GetScheduler()
    {
        return scheduler ??= await schedulerFactory.GetScheduler();
    }
}
