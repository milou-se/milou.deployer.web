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

        public static string Checked(this bool value)
        {
            return Checked((bool?)value);
        }

        public static string Selected(this bool? value)
        {
            if (!value.HasValue)
            {
                return string.Empty;
            }

            if (value.Value)
            {
                return "selected=\"selected\"";
            }

            return string.Empty;
        }

        public static string Selected(this bool value)
        {
            return Selected((bool?)value);
        }
    }
}