namespace Milou.Deployer.Web.Core.Extensions
{
    public static class ClassExtensions
    {
        public static bool HasValue<T>(this T instance) where T : class
        {
            return instance != null;
        }
    }
}