using System;
using System.Linq;
using Arbor.KVConfiguration.Core;
using JetBrains.Annotations;

namespace Arbor.App.Extensions.Time
{
    [UsedImplicitly]
    public class CustomSystemClock : ICustomClock
    {
        private readonly TimeZoneInfo _timeZone;

        public CustomSystemClock(
            [CanBeNull] IKeyValueConfiguration keyValueConfiguration = null,
            string timeZoneId = null)
        {
            timeZoneId = !string.IsNullOrWhiteSpace(timeZoneId)
                ? timeZoneId
                : keyValueConfiguration?[
                    TimeConstants.DefaultTimeZoneId];

            if (timeZoneId.HasValue())
            {
                var foundTimeZone = TimeZoneInfo.GetSystemTimeZones()
                    .SingleOrDefault(zone => zone.Id.Equals(timeZoneId, StringComparison.OrdinalIgnoreCase));

                if (foundTimeZone != null)
                {
                    _timeZone = foundTimeZone;
                    return;
                }
            }

            _timeZone = TimeZoneInfo.Utc;
        }

        public DateTimeOffset UtcNow()
        {
            if (ReferenceEquals(_timeZone, TimeZoneInfo.Utc))
            {
                return DateTimeOffset.UtcNow;
            }

            var utcDateTime =
                TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTimeOffset.UtcNow, _timeZone.Id);

            return utcDateTime;
        }

        public DateTime LocalNow()
        {
            if (ReferenceEquals(_timeZone, TimeZoneInfo.Utc))
            {
                return DateTime.UtcNow;
            }

            var localNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _timeZone);

            return localNow;
        }

        public DateTime ToLocalTime(DateTime dateTimeUtc)
        {
            var withKindUtc = new DateTime(dateTimeUtc.Ticks, DateTimeKind.Utc);

            return TimeZoneInfo.ConvertTimeFromUtc(withKindUtc, _timeZone);
        }
    }
}
