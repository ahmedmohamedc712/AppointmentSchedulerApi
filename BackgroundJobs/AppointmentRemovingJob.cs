using System;
using AppointmentScheduler.Data;
using AppointmentScheduler.Models;
using Quartz;

namespace AppointmentScheduler.BackgroundJobs;

public class AppointmentRemovingJob(AppDbContext dbContext) : IJob
{
    private const string APPOINTMENT_ID = "AppointmentId";
    public async Task Execute(IJobExecutionContext context)
    {
        var dataMap = context.MergedJobDataMap;
        int appointmentId = dataMap.GetInt(APPOINTMENT_ID);

        dbContext.Appointments.Remove(new Appointment { Id = appointmentId } );
        await dbContext.SaveChangesAsync();
    }
}
