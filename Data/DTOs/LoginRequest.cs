using System;

namespace AppointmentScheduler.Data.DTOs;

public record LoginRequest(string Email, string Password);