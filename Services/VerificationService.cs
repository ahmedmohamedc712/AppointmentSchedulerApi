using System;
using AppointmentScheduler.Data.DTOs;
using AppointmentScheduler.Models;
using Microsoft.Extensions.Caching.Memory;
using MimeKit;
namespace AppointmentScheduler.Services;

public class VerificationService(
    IMemoryCache cache,
    IPasswordHasher passwordHasher) : IVerificationService
{
    public async Task CreateAndSend(SignupRequest request)
    {
        int verificationCode = GenerateVerificationCode();
        var pendingRegistrationModel = new PendingRegistrationModel()
        {
            Email = request.Email,
            PasswordHashed = passwordHasher.HashPassword(request.Password),
            Name = request.Name,
            VerificationCode = verificationCode
        };
        cache.Remove(pendingRegistrationModel.Email);

        cache.Set(
            pendingRegistrationModel.Email,
            pendingRegistrationModel,
            TimeSpan.FromMinutes(5)
        );

        await Send(verificationCode, request.Email, request.Name);
    }

    public PendingRegistrationModel Get(string key)
    {
        // check if it exists if not throw an exception
        if(cache.TryGetValue(key, out PendingRegistrationModel? value))
        {
            return value!;
        }
        throw new BadHttpRequestException("Verification code expired.");
    }

    private int GenerateVerificationCode()
    {
        return Random.Shared.Next(100000, 999999);
    }

    private async Task Send(int VerificationCode, string email, string name)
    {
        var message = new MimeMessage();
        var from = new MailboxAddress("Appointment Scheduler", "AppointmentScheduler@gmail.com");
        message.From.Add(from);

        var to = new MailboxAddress(name, email);
        message.To.Add(to);

        message.Subject = $"Verify Account";
        message.Body = new TextPart()
        {
            Text = $"""
            Hey {name},

            This is your verification code: {VerificationCode}
            Don't share it with anyone.

            -Appointment Scheduler App
            """
        };

        using var smtp = new MailKit.Net.Smtp.SmtpClient();
        await smtp.ConnectAsync("localhost", 1025);
        await smtp.SendAsync(message);
        await smtp.DisconnectAsync(true);
    }
}
