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
}