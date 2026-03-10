using System;
using NodaTime;

namespace TaskScheduler.Services;

public interface IUtcLocalConverter
{
    Instant ConvertLocalToUtc(DateTime localTime, string userZone);
    DateTime ConvertUtcToLocal(Instant utc, string userZone);
}
