namespace Milou.Deployer.Web.Core.Extensions
{
    internal static class CharExtensions
    {
        public static bool IsIntegerValue(this char character)
        {
            return char.IsDigit(character);
        }
    }
}
