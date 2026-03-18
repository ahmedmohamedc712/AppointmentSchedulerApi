using System;

namespace AppointmentScheduler.Data.DTOs;
public record CreateUserRequest(string Email, int VerificationCode);