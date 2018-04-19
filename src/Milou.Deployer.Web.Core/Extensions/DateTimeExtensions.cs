using System;

namespace Milou.Deployer.Web.Core.Extensions
{
    public static class DateTimeExtensions
    {
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