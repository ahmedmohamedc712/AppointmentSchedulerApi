using System;

namespace AppointmentScheduler.Data.DTOs;

public record SignupRequest(string Name, string Email, string Password, string ConfirmPassword);