namespace Milou.Deployer.Web.IisHost.Areas.Settings.Controllers
{
    public class ControllerRouteInfo
    {
        public RouteInfo Route { get; }
        public string ControllerType { get; }

        public ControllerRouteInfo(RouteInfo route, string controllerType)
        {
            Route = route;
            ControllerType = controllerType;
        }
    }
}