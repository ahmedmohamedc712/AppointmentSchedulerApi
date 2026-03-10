using NodaTime;

namespace TaskScheduler.Services
{
    public class UtcLocalConverter : IUtcLocalConverter
    {
        public Instant ConvertLocalToUtc(DateTime localTime, string zone)
        {
            var localDateTime = LocalDateTime.FromDateTime(localTime);

            DateTimeZone dateTimeZone = DateTimeZoneProviders.Tzdb[zone];

            ZonedDateTime zoned = localDateTime.InZoneLeniently(dateTimeZone);

            return zoned.ToInstant();
        }

        public DateTime ConvertUtcToLocal(Instant utc, string userZone)
        {
            var zone = DateTimeZoneProviders.Tzdb[userZone];
            var zonedDateTime = utc.InZone(zone);
            return zonedDateTime.ToDateTimeUnspecified();
        }
    }
}
