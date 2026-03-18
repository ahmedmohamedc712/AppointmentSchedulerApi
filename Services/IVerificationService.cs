using System;
using AppointmentScheduler.Data.DTOs;
using AppointmentScheduler.Models;

namespace AppointmentScheduler.Services;

public interface IVerificationService
{
    Task CreateAndSend(SignupRequest request);
    PendingRegistrationModel Get(string key);
}
