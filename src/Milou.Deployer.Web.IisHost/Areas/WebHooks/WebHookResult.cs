namespace Milou.Deployer.Web.IisHost.Areas.WebHooks
{
    public class WebHookResult
    {
        public bool Handled { get; }

        public WebHookResult(in bool handled)
        {
            Handled = handled;
        }
    }
}