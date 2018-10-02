using System;
using Milou.Deployer.Web.IisHost.Areas.Deployment;

namespace Milou.Deployer.Web.Core.Extensions
{
    public static class IntExtensions
    {
        public static string ToStatusColor(this int value)
        {
            if (value == 0)
            {
                return "success";
            }

            return "failure";
        }
    }

    public static class DateTimeExtensions
    {
        public static DeploymentInterval IntervalAgo(this DateTime? dateTimeUtc, ITime time)
        {
            if (!dateTimeUtc.HasValue)
            {
                return DeploymentInterval.Invalid;
            }

            TimeSpan diff = time.LocalNow() - time.ToLocalTime(dateTimeUtc.Value);

            if (diff.TotalSeconds < 0)
            {
                return DeploymentInterval.Invalid;
            }

            return DeploymentInterval.Parse(diff);
        }

        public static string RelativeUtcToLocalTime(this DateTime? dateTime, ITime time)
        {
            if (!dateTime.HasValue)
            {
                return "N/A";
            }

            DateTime localThen = time.ToLocalTime(dateTime.Value);

            DateTime localNow = time.LocalNow();

            return localNow.Since(localThen);
        }

        public static string ToLocalTimeFormatted(this DateTime? dateTime, ITime time)
        {
            if (!dateTime.HasValue)
            {
                return "";
            }

            return ToLocalTimeFormatted(dateTime.Value, time);
        }

        public static string ToLocalTimeFormatted(this DateTime dateTimeUtc, ITime time)
        {
            var utcTime = new DateTime(dateTimeUtc.Ticks, DateTimeKind.Utc);

            return time.ToLocalTime(utcTime).ToString("yyyy-MM-dd HH:mm:ss");
        }

        public static string Since(this DateTime to, DateTime from)
        {
            TimeSpan diff = to - from;

            if (diff.TotalDays > 365)
            {
                return ((int)diff.TotalDays) + " days ago";
            }

            if (diff.TotalDays > 30)
            {
                return ((int)diff.TotalDays / 30) + " months ago";
            }

            if (diff.TotalDays > 1)
            {
                return ((int)diff.TotalDays) + " days ago";
            }

            if (diff.TotalHours > 1)
            {
                return ((int)diff.TotalHours) + " hours ago";
            }

            if (diff.TotalMinutes > 1)
            {
                return ((int)diff.TotalMinutes) + " minutes ago";
            }

            if (diff.TotalSeconds < 0)
            {
                return "N/A";
            }

            return ((int)diff.TotalSeconds) + " seconds ago";
        }
    }
}