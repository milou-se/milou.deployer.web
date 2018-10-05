namespace Milou.Deployer.Web.IisHost.Areas.Settings
{
    public class ContainerRegistrationInfo
    {
        public string Service { get; }
        public string Scope { get; }

        public ContainerRegistrationInfo(string service, string scope)
        {
            Service = service;
            Scope = scope;
        }
    }
}