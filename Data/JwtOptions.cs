using System;

namespace AppointmentScheduler.Data;

public class JwtOptions
{
    public int LifeTime { get; set; }
    public string SigningKey { get; set; }
}
