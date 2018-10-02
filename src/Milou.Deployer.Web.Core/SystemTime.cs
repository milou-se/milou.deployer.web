using System;
using System.Linq;
using Arbor.KVConfiguration.Core;
using JetBrains.Annotations;
using Milou.Deployer.Web.Core.Extensions;

namespace Milou.Deployer.Web.Core
{
    [UsedImplicitly]
    public class SystemTime : ITime
    {
        private TimeZoneInfo _timeZone;

        public SystemTime([NotNull] IKeyValueConfiguration keyValueConfiguration)
        {
            if (keyValueConfiguration == null)
            {
                throw new ArgumentNullException(nameof(keyValueConfiguration));
            }

            string timeZoneId = keyValueConfiguration[TimeConstants.DefaultTimeZoneId];

            if (timeZoneId.HasValue())
            {
                TimeZoneInfo foundTimeZone = TimeZoneInfo.GetSystemTimeZones()
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

            DateTimeOffset utcDateTime =
                TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTimeOffset.UtcNow, _timeZone.Id);

            return utcDateTime;
        }

        public DateTime LocalNow()
        {
            if (ReferenceEquals(_timeZone, TimeZoneInfo.Utc))
            {
                return DateTime.UtcNow;
            }

            DateTime localNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _timeZone);

            return localNow;
        }

        public DateTime ToLocalTime(DateTime dateTimeValue)
        {
            var withKindUtc = new DateTime(dateTimeValue.Ticks, DateTimeKind.Utc);

            return TimeZoneInfo.ConvertTimeFromUtc(withKindUtc, _timeZone);
        }
    }
}