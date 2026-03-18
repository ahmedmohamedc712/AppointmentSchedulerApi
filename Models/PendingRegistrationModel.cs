using System;

namespace AppointmentScheduler.Models;

public class PendingRegistrationModel
{
    public int VerificationCode { get; set;}
    public string Email { get; set; } = string.Empty;
    public string PasswordHashed { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}
