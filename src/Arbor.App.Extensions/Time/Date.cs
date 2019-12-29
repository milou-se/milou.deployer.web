using System;
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Arbor.App.Extensions.Time
{
    public readonly struct Date : IEquatable<Date>, IComparable<Date>
    {
        [PublicAPI]
        [JsonIgnore]
        public DateTime OriginalValue { get; }

        private readonly DateTime _datePart;

        public Date(DateTime date)
        {
            OriginalValue = date;
            _datePart = date.Date;
        }

        public override string ToString() => _datePart.ToString("yyyy-MM-dd");

        public static implicit operator Date(DateTime dateTime) => new Date(dateTime);

        [SuppressMessage("ReSharper", "ImpureMethodCallOnReadonlyValueField")]
        public bool Equals(Date other) => _datePart.Equals(other._datePart);

        public override bool Equals(object? obj)
        {
            if (obj is null)
            {
                return false;
            }

            return obj is Date date && Equals(date);
        }

        public override int GetHashCode() => _datePart.GetHashCode();

        public static bool operator ==(Date left, Date right) => left.Equals(right);

        public static bool operator !=(Date left, Date right) => !left.Equals(right);

        [SuppressMessage("ReSharper", "ImpureMethodCallOnReadonlyValueField")]
        public int CompareTo(Date other) => _datePart.CompareTo(other._datePart);

        public static bool operator <(Date date1, Date date2) => date1._datePart < date2._datePart;

        public static bool operator >(Date date1, Date date2) => date1._datePart > date2._datePart;

        public static bool operator >=(Date date1, Date date2) => date1._datePart >= date2._datePart;

        public static bool operator <=(Date date1, Date date2) => date1._datePart <= date2._datePart;
    }
}