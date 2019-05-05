using System;
using System.Globalization;
using JetBrains.Annotations;

namespace Milou.Deployer.Web.Core.Time
{
    public static class DateTimeExtensions
    {
        public static DeploymentInterval IntervalAgo(this DateTime? dateTimeUtc, [NotNull] ICustomClock customClock)
        {
            if (customClock == null)
            {
                throw new ArgumentNullException(nameof(customClock));
            }

            if (!dateTimeUtc.HasValue)
            {
                return DeploymentInterval.Invalid;
            }

            var diff = customClock.LocalNow() - customClock.ToLocalTime(dateTimeUtc.Value);

            if (diff.TotalSeconds < 0)
            {
                return DeploymentInterval.Invalid;
            }

            return DeploymentInterval.Parse(diff);
        }

        public static string RelativeUtcToLocalTime(this DateTime? dateTimeUtc, [NotNull] ICustomClock customClock)
        {
            if (customClock == null)
            {
                throw new ArgumentNullException(nameof(customClock));
            }

            if (!dateTimeUtc.HasValue)
            {
                return Constants.NotAvailable;
            }

            var localThen = customClock.ToLocalTime(dateTimeUtc.Value);

            var localNow = customClock.LocalNow();

            return localNow.Since(localThen);
        }

        public static string ToLocalTimeFormatted(this DateTime? dateTimeUtc, [NotNull] ICustomClock customClock)
        {
            if (customClock == null)
            {
                throw new ArgumentNullException(nameof(customClock));
            }

            if (!dateTimeUtc.HasValue)
            {
                return "";
            }

            return ToLocalTimeFormatted(dateTimeUtc.Value, customClock);
        }

        public static string ToLocalTimeFormatted(this DateTime dateTimeUtc, [NotNull] ICustomClock customClock)
        {
            if (customClock == null)
            {
                throw new ArgumentNullException(nameof(customClock));
            }

            var utcTime = new DateTime(dateTimeUtc.Ticks, DateTimeKind.Utc);

            return customClock.ToLocalTime(utcTime).ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.CurrentUICulture);
        }

        [PublicAPI]
        public static string Since(this DateTime to, DateTime from)
        {
            string PluralSuffix(int count) => count > 1 ? "s" : "";

            var diff = to - from;

            var diffTotalDays = (int)diff.TotalDays;

            if (diff.TotalDays > 365)
            {
                return $"{diffTotalDays} day{PluralSuffix(diffTotalDays)} ago";
            }

            if (diff.TotalDays > 30)
            {
                return $"{diffTotalDays / 30} month{PluralSuffix(diffTotalDays)} ago";
            }

            if (diff.TotalDays > 1)
            {
                return $"{diffTotalDays} day{PluralSuffix(diffTotalDays)} ago";
            }

            if (diff.TotalHours > 1)
            {
                var diffTotalHours = (int)diff.TotalHours;
                return $"{diffTotalHours} hour{PluralSuffix(diffTotalHours)} ago";
            }

            if (diff.TotalMinutes > 1)
            {
                var diffTotalMinutes = (int)diff.TotalMinutes;
                return $"{diffTotalMinutes} minute{PluralSuffix(diffTotalMinutes)} ago";
            }

            if (diff.TotalSeconds < 0)
            {
                return Constants.NotAvailable;
            }

            var diffTotalSeconds = (int)diff.TotalSeconds;
            return $"{diffTotalSeconds} second{PluralSuffix(diffTotalSeconds)} ago";
        }
    }
}
