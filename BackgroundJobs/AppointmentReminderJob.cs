using System;
using MimeKit;
using Quartz;

namespace AppointmentScheduler.BackgroundJobs;

public class AppointmentReminderJob : IJob
{
    private const string USER_EMAIL = "UserEmail";
    private const string USER_NAME = "UserName";
    private const string APPOINTMENT_TITLE = "AppointmentTitle";
    private const string APPOINTMENT_DESCRIPTION = "AppointmentDescription";
    private const string APPOINTMENT_DATE = "AppointmentDate";

    public async Task Execute(IJobExecutionContext context)
    {
        var dataMap = context.MergedJobDataMap;

        var message = new MimeMessage();
        var from = new MailboxAddress("Appointment Scheduler", "AppointmentScheduler@gmail.com");
        message.From.Add(from);

        var userEmail = dataMap.GetString(USER_EMAIL);
        var userName = dataMap.GetString(USER_NAME);
        var to = new MailboxAddress(userName, userEmail);
        message.To.Add(to);

        message.Subject = $"Appointment Reminder";
        var appointmentTitle = dataMap.GetString(APPOINTMENT_TITLE);
        var appointmentDescription = dataMap.GetString(APPOINTMENT_DESCRIPTION);
        var appointmentDate = dataMap.GetString(APPOINTMENT_DATE);
        message.Body = new TextPart()
        {
            Text = $"""
            Hey {userName},

            This is a reminder for your appointment: '{appointmentTitle}',
            Its due date is '{appointmentDate}'.

            Description: {appointmentDescription}.

            -Appointment Scheduler App
            """
        };

        using var smtp = new MailKit.Net.Smtp.SmtpClient();
        await smtp.ConnectAsync("localhost", 1025);
        await smtp.SendAsync(message);
        await smtp.DisconnectAsync(true);
    }
}
