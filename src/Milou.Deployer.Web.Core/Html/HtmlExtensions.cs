namespace Milou.Deployer.Web.Core.Html
{
    public static class HtmlExtensions
    {
        public static string Checked(this bool? value)
        {
            if (!value.HasValue)
            {
                return string.Empty;
            }

            if (value.Value)
            {
                return "checked=\"checked\"";
            }

            return string.Empty;
        }
    }
}