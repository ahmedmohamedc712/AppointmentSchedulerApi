using System;

namespace AppointmentScheduler.Services;

public interface IPasswordHasher
{
    string HashPassword(string password);
    bool VerifyPassword(string enteredPassword, string hashedPassword);
}
