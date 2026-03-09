using System;

namespace AppointmentScheduler.Models;

public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHashed { get; set; } = string.Empty;
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}
