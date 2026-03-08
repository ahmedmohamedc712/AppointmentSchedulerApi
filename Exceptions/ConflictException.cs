using System;

namespace AppointmentScheduler.Exceptions;

public class ConflictException : Exception
{
    public ConflictException(string message) : base(message)
    {
        
    }
}
