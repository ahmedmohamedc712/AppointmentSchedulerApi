using System;

namespace AppointmentScheduler.Exceptions;

public class NotFoundException : Exception
{
    public NotFoundException(int id) : base($"Appointment with ID: '{id}' not found.")
    {
        
    }
}
