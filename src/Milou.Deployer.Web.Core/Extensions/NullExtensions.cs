namespace Milou.Deployer.Web.Core.Extensions
{
    public static class NullExtensions
    {
        public static bool HasValue<T>(this T item) where T : class
        {
            return !(item is null);
        }

        public static bool IsDefault<T>(this T item) where T : struct
        {
            return !Equals(item, default(T));
        }
    }
}